using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpline : MonoBehaviour
{
    public int pointCount = 10;

    private int position = 0;
    private List<Vector3> _splinePoints = new List<Vector3>();
    private Quaternion previousDirection;
    private Quaternion currentDirection;
    private Quaternion nextDirection;

    public Quaternion RotationOffset { get; set; }
    public Vector3 CameraOffset { get; set; }
    public float Speed { get; set; }

    void Awake()
    {
        for(int i = 0; i < transform.childCount - 1; i++) {
            Transform point = transform.GetChild(i);
            Transform secondPoint = transform.GetChild(i + 1);
            _splinePoints.Add(point.position);
            if(point.childCount > 0) {
                //List<Vector3> points = GenerateBezierCurve(pointCount, point.position, point.GetChild(0).position, secondPoint.position);

                _splinePoints.Add(point.GetChild(0).position);
            }
        }
        _splinePoints.Add(transform.GetChild(transform.childCount - 1).position);
    }

    public IEnumerator MoveAlongSpline(Rigidbody tracker, Transform camera) {
        Vector3 pos = GenerateBezierPoint(0, _splinePoints[position], _splinePoints[position + 1], _splinePoints[position + 2]);
        Vector3 nextPos = GenerateBezierPoint(0.0125f, _splinePoints[position], _splinePoints[position + 1], _splinePoints[position + 2]);
        
        tracker.MovePosition(pos);

        camera.rotation = RotationOffset;
        camera.position = pos;
        camera.Translate(CameraOffset);
        while (position < _splinePoints.Count - 2) {
            float startTime = Time.time;
            float distanceCovered = 0;
            float distanceFraction = 0;

            float movementLength = Vector3.Distance(_splinePoints[position], _splinePoints[position + 2]);
            camera.Translate(CameraOffset);
            while(distanceFraction < 1)
            {
                if (GameManager.instance.IsFinished)
                    yield break;

                distanceCovered += (Time.time - startTime) * Speed;
                startTime = Time.time;
                if (Speed == 0)
                {
                    camera.position = pos;
                    camera.Translate(CameraOffset);
                    camera.rotation = Quaternion.Lerp(camera.rotation, Quaternion.LookRotation((pos - nextPos).normalized * 360, Vector3.up) * RotationOffset, Time.deltaTime);
                    yield return new WaitForFixedUpdate();
                    continue;
                }

                distanceFraction = distanceCovered / movementLength;
                
                if(distanceFraction > 1) {
                    distanceFraction = 1;
                    break;
                }
                pos = nextPos;

                nextPos = GenerateBezierPoint(distanceFraction, _splinePoints[position], _splinePoints[position + 1], _splinePoints[position + 2]);
                tracker.MovePosition(pos);

                camera.rotation = Quaternion.Lerp(camera.rotation, Quaternion.LookRotation((pos - nextPos).normalized * 360, Vector3.up) * RotationOffset, Time.deltaTime);
                camera.position = pos;
                camera.Translate(CameraOffset);
                yield return new WaitForFixedUpdate();
            }
            
            position += 2;
            yield return new WaitForFixedUpdate();
        }
    }

    public Vector3 GetPreviousPoint() {
        return _splinePoints[position - 1];
    }

    public Vector3 GetCurrentPoint() {
        return _splinePoints[position];
    }

    public Vector3 GetNextPoint() {
        return _splinePoints[position + 1];
    }

    public Quaternion GetPreviousDirection() {
        return previousDirection;
    }

    public Quaternion GetCurrentDirection() {
        return currentDirection;
    }
    
    public Quaternion GetNextDirection() {
        return nextDirection;
    }

    public void AdvancePoint() {
        previousDirection = currentDirection;
        position += 2;
        currentDirection = nextDirection;
        nextDirection = Quaternion.LookRotation((_splinePoints[position] - _splinePoints[position + 2]).normalized * 360, Vector3.up);
    }

    public void ReversePoint() {
        previousDirection = currentDirection;
        --position;
        currentDirection = nextDirection;
        nextDirection = Quaternion.LookRotation((_splinePoints[position - 1] - _splinePoints[position]).normalized * 360, Vector3.up);

    }

    public int GetNearestPoint(Vector3 pos) {
        float minDist = float.MaxValue;
        int child = 0;
        for(int i = 0; i < _splinePoints.Count; i++) {
            float dist = Vector3.Distance(pos, _splinePoints[i]); 
            if(dist < minDist) { 
                minDist = dist;
                child = i;
            }
        }

        return child;
    }
    
    public void UpdatePosition(Vector3 pos) {
        int currPos = GetNearestPoint(pos);
        position = currPos > 0 ? currPos : 0;

        previousDirection = currentDirection;
        currentDirection = Quaternion.LookRotation((_splinePoints[position] - _splinePoints[position + 2]).normalized * 360, Vector3.up);
    }

    public int GetPosition() {
        return position;
    }

    private void OnDrawGizmos() {
        for(int i = 0; i < transform.childCount - 1; i++) {
            Transform point = transform.GetChild(i);
            Transform secondPoint = transform.GetChild(i + 1);
            if(point.childCount > 0) {
                List<Vector3> points = GenerateBezierCurve(pointCount, point.position, point.GetChild(0).position, secondPoint.position);

                Gizmos.DrawLine(point.position, points[0]);
                for(int j = 0; j < points.Count - 1; ++j) {
                    Gizmos.DrawLine(points[j], points[j + 1]);
                }

                Gizmos.DrawLine(points[points.Count - 1], secondPoint.position);
            } else {
                Gizmos.DrawLine(point.position, secondPoint.position);
            }
        }
    }

    private static Vector3 GenerateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2) {
        float a = (1-t)*(1-t);
        float b = 2*(1-t)*t;
        float c = t*t;
 
        Vector3 p = (p0 * a + p1 * b + p2 * c);
        return p;
    }

    public static List<Vector3> GenerateBezierCurve(int segmentCount, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        List<Vector3> points = new List<Vector3>();
        for(int i = 1; i < segmentCount; i++)
        {
            float t = i / (float) segmentCount;
            points.Add(GenerateBezierPoint(t, p0, p1, p2));
        }
        return points;
    }
}
