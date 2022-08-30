using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PathGenerator : MonoBehaviour
{

    public static PathGenerator Instance;

    public static CubicBezierPath Path;

    [Header("Generation")]
    public Vector3[] PathPoints;
    public int NumberOfPointsToGenerate;
    public float DistanceBetweenPoints;
    public CubicBezierPath.Type IsClosed;
    public bool GenerateMesh;
    public bool GenerateMeshColllider;
    [Header("Mesh Attributes")]
    [Range(0f, 1000f)]
    public int MeshCount;
    [Range(0f, 100f)]
    public float MeshWidth;
    [Range(0f, 50f)]
    public float MeshDepth;

    public Material TopMaterial;
    public Material BottomMaterial;
    public Material SideMaterial;
    private Material[] _meshMaterials;
    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        Instance = this;
    }

    void Start()
    {
        Path = new CubicBezierPath(PathPoints, IsClosed);
        for (int i = 0; i < NumberOfPointsToGenerate; i++)
        {
            AddPoint(DistanceBetweenPoints);
        }

        if (GenerateMeshColllider)
            gameObject.AddComponent<MeshCollider>();
        GeneratePathMesh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GeneratePathMesh();
        }
    }

    public void UpdateFloatingOrigin()
    {
            Vector3 Distance = PathPoints[0];
            for (int i = 0; i < PathPoints.Length; i++)
            {
                PathPoints[i] = new Vector3(PathPoints[i].x - Distance.x, PathPoints[i].y - Distance.y, PathPoints[i].z - Distance.z);
            }
        Path = new CubicBezierPath(PathPoints, IsClosed);
        GeneratePathMesh();
    }

    public void GeneratePathMesh()
    {
        if (GenerateMesh)
        {
            MeshGenerator gen = new MeshGenerator();
            _meshMaterials = new Material[]
            {
                TopMaterial,
                BottomMaterial,
                SideMaterial
            };
            GetComponent<MeshRenderer>().materials = _meshMaterials;
            GetComponent<MeshFilter>().sharedMesh = gen.GenerateMesh(MeshCount, Path, MeshWidth, MeshDepth);
            if (GenerateMeshColllider)
                GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        }
    }

    public void AddPoint(float distance = 100)
    {
        Vector3 prevPathPoint = PathPoints[PathPoints.Length - 1];
        PathPoints = Path.AddPoint(PathPoints, new Vector3(prevPathPoint.x + Random.Range(-50, 50), prevPathPoint.y + Random.Range(-20, 20), prevPathPoint.z + distance));
        Path = new CubicBezierPath(PathPoints, IsClosed);
    }
    public void RemovePoint(int IndexToRemove)
    {
        PathPoints = Path.RemovePoint(PathPoints, IndexToRemove);
        Path = new CubicBezierPath(PathPoints, IsClosed);
    }
}