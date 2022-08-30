using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    private float _t = 0;

    public float Speed;
    // Start is called before the first frame update
    public Vector3 Offset;
    public bool GeneratePathAhead;
    public bool Loop;

    [Tooltip("Starting position (0-1) must be less than second curve if auto generation.")]
    public float StartPosition;


    private void Start()
    {
        _t = StartPosition;
    }
    void Update()
    {
        if (_t < 1)
        {
            transform.position = new Vector3(PathGenerator.Path.GetPointNorm(_t).x, PathGenerator.Path.GetPointNorm(_t).y, PathGenerator.Path.GetPointNorm(_t).z) + Offset;
            Vector3 v3 = PathGenerator.Path.GetPointNorm(_t + 0.01f) - transform.position;
            transform.rotation = Quaternion.LookRotation(v3 + Offset);
            _t += (Speed / 100) * Time.deltaTime;
        }
        else if (Loop)
        {
            _t = 0;
        }
        float tReset = 1.00f / PathGenerator.Path.GetNumCurveSegments();
        if (transform.position.z >= PathGenerator.Path.GetPointNorm(tReset * 2).z && GeneratePathAhead)
        {
            PathGenerator.Instance.AddPoint(PathGenerator.Instance.DistanceBetweenPoints);
            PathGenerator.Instance.RemovePoint(0);
            PathGenerator.Instance.UpdateFloatingOrigin();
            _t = tReset;
        }
    }
}
