using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

class CubicBezierCurve
{
    private Vector3[] _controlVerts;

    public CubicBezierCurve(Vector3[] cvs)
    {
        SetControlVerts(cvs);
    }

    public void SetControlVerts(Vector3[] cvs)
    {
        // Cubic Bezier curves require 4 cvs.
        Assert.IsTrue(cvs.Length == 4);
        _controlVerts = cvs;
    }

    public Vector3 GetPoint(float t)
    {
        Assert.IsTrue((t >= 0.0f) && (t <= 1.0f));
        float c = 1.0f - t;
        // The Bernstein polynomials.
        float bb0 = c * c * c;
        float bb1 = 3 * t * c * c;
        float bb2 = 3 * t * t * c;
        float bb3 = t * t * t;

        Vector3 point = _controlVerts[0] * bb0 + _controlVerts[1] * bb1 + _controlVerts[2] * bb2 + _controlVerts[3] * bb3;
        return point;
    }

    public Vector3 GetTangent(float t)
    {
        Assert.IsTrue((t >= 0.0f) && (t <= 1.0f));

        Vector3 q0 = _controlVerts[0] + ((_controlVerts[1] - _controlVerts[0]) * t);
        Vector3 q1 = _controlVerts[1] + ((_controlVerts[2] - _controlVerts[1]) * t);
        Vector3 q2 = _controlVerts[2] + ((_controlVerts[3] - _controlVerts[2]) * t);

        Vector3 r0 = q0 + ((q1 - q0) * t);
        Vector3 r1 = q1 + ((q2 - q1) * t);
        Vector3 tangent = r1 - r0;
        return tangent;
    }
    public Vector3 GetDerivative(float t)
    {


        Vector3 derivative = t * t * (-3f * (_controlVerts[0] - 3f * (_controlVerts[1] - _controlVerts[2]) - _controlVerts[3]));

        derivative += t * (6f * (_controlVerts[0] - 2f * _controlVerts[1] + _controlVerts[2]));

        derivative += -3f * (_controlVerts[0] - _controlVerts[1]);

        return derivative;
    }

    public float GetClosestParam(Vector3 pos, float paramThreshold = 0.000001f)
    {
        return GetClosestParamRec(pos, 0.0f, 1.0f, paramThreshold);
    }

    float GetClosestParamRec(Vector3 pos, float beginT, float endT, float thresholdT)
    {
        float mid = (beginT + endT) / 2.0f;

        if ((endT - beginT) < thresholdT)
            return mid;

        float paramA = (beginT + mid) / 2.0f;
        float paramB = (mid + endT) / 2.0f;

        Vector3 posA = GetPoint(paramA);
        Vector3 posB = GetPoint(paramB);
        float distASq = (posA - pos).sqrMagnitude;
        float distBSq = (posB - pos).sqrMagnitude;

        if (distASq < distBSq)
            endT = mid;
        else
            beginT = mid;

        // The (tail) recursive call.
        return GetClosestParamRec(pos, beginT, endT, thresholdT);
    }
}
/// A CubicBezierPath is made of a collection of cubic Bezier curves. If two points are supplied they become the end
/// points of one CubicBezierCurve and the 2 interior CVs are generated, creating a small straight line. For 3 points
/// the middle point will be on both CubicBezierCurves and each curve will have equal tangents at that point.
public class CubicBezierPath
{
    public enum Type
    {
        Open,
        Closed
    }
    Type type = Type.Open;
    int numCurveSegments = 0;
    int numControlVerts = 0;
    Vector3[] _controlVerts = null;
    public MeshGenerator Generator;

    public CubicBezierPath(Vector3[] anchors, Type t = Type.Open) { InterpolatePoints(anchors, t); }
    public Type GetPathType() { return type; }
    public bool IsClosed() { return (type == Type.Closed) ? true : false; }
    public bool IsValid() { return (numCurveSegments > 0) ? true : false; }
    public void Clear()
    {
        _controlVerts = null;
        type = Type.Open;
        numCurveSegments = 0;
        numControlVerts = 0;
    }

    // A closed path will have one more segment than an open for the same number of interpolated points.
    public int GetNumCurveSegments() { return numCurveSegments; }
    public float GetMaxParam() { return (float)numCurveSegments; }

    // Access to the raw CVs.
    public int GetNumControlVerts() { return numControlVerts; }
    public Vector3[] GetControlVerts() { return _controlVerts; }

    public float ComputeApproxLength()
    {
        if (!IsValid())
            return 0.0f;

        // For a closed path this still works if you consider the last point as separate from the first. That is, a closed
        // path is just like an open except the last interpolated point happens to match the first.
        int numInterpolatedPoints = numCurveSegments + 1;
        if (numInterpolatedPoints < 2)
            return 0.0f;

        float totalDist = 0.0f;
        for (int n = 1; n < numInterpolatedPoints; n++)
        {
            Vector3 a = _controlVerts[(n - 1) * 3];
            Vector3 b = _controlVerts[n * 3];
            totalDist += (a - b).magnitude;
        }

        if (totalDist == 0.0f)
            return 0.0f;

        return totalDist;
    }

    public float ComputeApproxParamPerUnitLength()
    {
        float length = ComputeApproxLength();
        return (float)numCurveSegments / length;
    }

    public float ComputeApproxNormParamPerUnitLength()
    {
        float length = ComputeApproxLength();
        return 1.0f / length;
    }

    // Interpolates the supplied points. Internally generates any necessary CVs
    public void InterpolatePoints(Vector3[] anchors, Type t)
    {
        int numAnchors = anchors.Length;
        Assert.IsTrue(numAnchors >= 2);
        Clear();
        type = t;
        switch (type)
        {
            case Type.Open:
                {
                    numCurveSegments = numAnchors - 1;
                    numControlVerts = 3 * numAnchors - 2;
                    _controlVerts = new Vector3[numControlVerts];

                    // Place the interpolated CVs.
                    for (int n = 0; n < numAnchors; n++)
                        _controlVerts[n * 3] = anchors[n];

                    // Place the first and last non-interpolated CVs.
                    Vector3 initialPoint = (anchors[1] - anchors[0]) * 0.25f;

                    // Interpolate 1/4 away along first segment.
                    _controlVerts[1] = anchors[0] + initialPoint;
                    Vector3 finalPoint = (anchors[numAnchors - 2] - anchors[numAnchors - 1]) * 0.25f;

                    // Interpolate 1/4 backward along last segment.
                    _controlVerts[numControlVerts - 2] = anchors[numAnchors - 1] + finalPoint;

                    // Now we'll do all the interior non-interpolated CVs.
                    for (int k = 1; k < numCurveSegments; k++)
                    {
                        Vector3 a = anchors[k - 1] - anchors[k];
                        Vector3 b = anchors[k + 1] - anchors[k];
                        float aLen = a.magnitude;
                        float bLen = b.magnitude;

                        if ((aLen > 0.0f) && (bLen > 0.0f))
                        {
                            float abLen = (aLen + bLen) / 8.0f;
                            Vector3 ab = (b / bLen) - (a / aLen);
                            ab.Normalize();
                            ab *= abLen;

                            _controlVerts[k * 3 - 1] = anchors[k] - ab;
                            _controlVerts[k * 3 + 1] = anchors[k] + ab;
                        }
                        else
                        {
                            _controlVerts[k * 3 - 1] = anchors[k];
                            _controlVerts[k * 3 + 1] = anchors[k];
                        }
                    }
                    break;
                }

            case Type.Closed:
                {
                    numCurveSegments = numAnchors;

                    // We duplicate the first point at the end so we have contiguous memory to look of the curve value. That's
                    // what the +1 is for.
                    numControlVerts = 3 * numAnchors + 1;
                    _controlVerts = new Vector3[numControlVerts];

                    // First lets place the interpolated CVs and duplicate the first into the last CV slot.
                    for (int n = 0; n < numAnchors; n++)
                        _controlVerts[n * 3] = anchors[n];

                    _controlVerts[numControlVerts - 1] = anchors[0];

                    // Now we'll do all the interior non-interpolated CVs. We go to k=NumCurveSegments which will compute the
                    // two CVs around the zeroth knot (points[0]).
                    for (int k = 1; k <= numCurveSegments; k++)
                    {
                        int modkm1 = k - 1;
                        int modkp1 = (k + 1) % numCurveSegments;
                        int modk = k % numCurveSegments;

                        Vector3 a = anchors[modkm1] - anchors[modk];
                        Vector3 b = anchors[modkp1] - anchors[modk];
                        float aLen = a.magnitude;
                        float bLen = b.magnitude;
                        int mod3km1 = 3 * k - 1;

                        // Need the -1 so the end point is a duplicated start point.
                        int mod3kp1 = (3 * k + 1) % (numControlVerts - 1);
                        if ((aLen > 0.0f) && (bLen > 0.0f))
                        {
                            float abLen = (aLen + bLen) / 8.0f;
                            Vector3 ab = (b / bLen) - (a / aLen);
                            ab.Normalize();
                            ab *= abLen;

                            _controlVerts[mod3km1] = anchors[modk] - ab;
                            _controlVerts[mod3kp1] = anchors[modk] + ab;
                        }
                        else
                        {
                            _controlVerts[mod3km1] = anchors[modk];
                            _controlVerts[mod3kp1] = anchors[modk];
                        }
                    }
                    break;
                }
        }
    }

    // For a closed path the last CV must match the first.
    public void SetControlVerts(Vector3[] cvs, Type t)
    {
        int numCVs = cvs.Length;
        Assert.IsTrue(numCVs > 0);
        Assert.IsTrue(((t == Type.Open) && (numCVs >= 4)) || ((t == Type.Closed) && (numCVs >= 7)));
        Assert.IsTrue(((numCVs - 1) % 3) == 0);
        Clear();
        type = t;

        numControlVerts = numCVs;
        numCurveSegments = ((numCVs - 1) / 3);
        _controlVerts = cvs;
    }

    // t E [0, numSegments]. If the type is closed, the number of segments is one more than the equivalent open path.
    public Vector3 GetPoint(float t)
    {
        // Only closed paths accept t values out of range.
        if (type == Type.Closed)
        {
            while (t < 0.0f)
                t += (float)numCurveSegments;

            while (t > (float)numCurveSegments)
                t -= (float)numCurveSegments;
        }
        else
        {
            t = Mathf.Clamp(t, 0.0f, (float)numCurveSegments);
        }

        Assert.IsTrue((t >= 0) && (t <= (float)numCurveSegments));

        // Segment 0 is for t E [0, 1). The last segment is for t E [NumCurveSegments-1, NumCurveSegments].
        // The following 'if' statement deals with the final inclusive bracket on the last segment. The cast must truncate.
        int segment = (int)t;
        if (segment >= numCurveSegments)
            segment = numCurveSegments - 1;

        Vector3[] curveCVs = new Vector3[4];
        curveCVs[0] = _controlVerts[3 * segment + 0];
        curveCVs[1] = _controlVerts[3 * segment + 1];
        curveCVs[2] = _controlVerts[3 * segment + 2];
        curveCVs[3] = _controlVerts[3 * segment + 3];

        CubicBezierCurve bc = new CubicBezierCurve(curveCVs);
        return bc.GetPoint(t - (float)segment);
    }

    // Does the same as GetPoint except that t is normalized to be E [0, 1] over all segments. The beginning of the curve
    // is at t = 0 and the end at t = 1. Closed paths allow a value bigger than 1 in which case they loop.
    public Vector3 GetPointNorm(float t)
    {
        return GetPoint(t * (float)numCurveSegments);
    }

    // Similar to GetPoint but returns the tangent at the specified point on the path. The tangent is not normalized.
    // The longer the tangent the 'more influence' it has pulling the path in that direction.
    public Vector3 GetTangent(float t)
    {
        // Only closed paths accept t values out of range.
        if (type == Type.Closed)
        {
            while (t < 0.0f)
                t += (float)numCurveSegments;

            while (t > (float)numCurveSegments)
                t -= (float)numCurveSegments;
        }
        else
        {
            t = Mathf.Clamp(t, 0.0f, (float)numCurveSegments);
        }

        Assert.IsTrue((t >= 0) && (t <= (float)numCurveSegments));

        // Segment 0 is for t E [0, 1). The last segment is for t E [NumCurveSegments-1, NumCurveSegments].
        // The following 'if' statement deals with the final inclusive bracket on the last segment. The cast must truncate.
        int segment = (int)t;
        if (segment >= numCurveSegments)
            segment = numCurveSegments - 1;

        Vector3[] curveCVs = new Vector3[4];
        curveCVs[0] = _controlVerts[3 * segment + 0];
        curveCVs[1] = _controlVerts[3 * segment + 1];
        curveCVs[2] = _controlVerts[3 * segment + 2];
        curveCVs[3] = _controlVerts[3 * segment + 3];

        CubicBezierCurve bc = new CubicBezierCurve(curveCVs);
        return bc.GetTangent(t - (float)segment);
    }

    public Vector3 GetTangentNorm(float t)
    {
        return GetTangent(t * (float)numCurveSegments);
    }

    public Vector3[] AddPoint(Vector3[] path, Vector3 newPoint)
    {
        Vector3[] finalArray = new Vector3[path.Length + 1];
        for (int i = 0; i < path.Length; i++)
        {
            finalArray[i] = path[i];
        }
        finalArray[finalArray.Length - 1] = newPoint;
        return finalArray;
    }

    public Vector3[] RemovePoint(Vector3[] path, int index)
    {
        for (int a = index; a < path.Length - 1; a++)
        {
            // moving elements downwards, to fill the gap at [index]
            path[a] = path[a + 1];
        }
        System.Array.Resize(ref path, path.Length - 1);
        return path;
    }

    // This function returns a single closest point. There may be more than one point on the path at the same distance.
    public float ComputeClosestParam(Vector3 pos, float paramThreshold)
    {
        float minDistSq = float.MaxValue;
        float closestParam = 0.0f;
        Vector3[] curveCVs = new Vector3[4];
        CubicBezierCurve curve = new CubicBezierCurve(curveCVs);
        for (int startIndex = 0; startIndex < _controlVerts.Length - 1; startIndex += 3)
        {
            for (int i = 0; i < 4; i++)
                curveCVs[i] = _controlVerts[startIndex + i];

            curve.SetControlVerts(curveCVs);
            float curveClosestParam = curve.GetClosestParam(pos, paramThreshold);

            Vector3 curvePos = curve.GetPoint(curveClosestParam);
            float distSq = (curvePos - pos).sqrMagnitude;
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                float startParam = startIndex / 3.0f;
                closestParam = startParam + curveClosestParam;
            }
        }

        return closestParam;
    }

    // Same as above but returns a t value E [0, 1]. You'll need to use a paramThreshold like
    // ComputeApproxParamPerUnitLength() * 0.15f if you want a 15cm tolerance.
    public float ComputeClosestNormParam(Vector3 pos, float paramThreshold)
    {
        return ComputeClosestParam(pos, paramThreshold * (float)numCurveSegments);
    }

}