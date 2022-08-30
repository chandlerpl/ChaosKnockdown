using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RaceTrackGenerator : MonoBehaviour
{
    public static CubicBezierPath RaceTrack;

    [Header("Generation")]
    public Vector3[] PathPoints;
    [Range(10f, 1000f)]
    public float SizeOfTrack;
    [Range(4f, 50)]
    public int NumberOfCorners;
    [Range(0f, 50)]
    public float HeightVariation;

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


    void Start()
    {
        if (PathPoints.Length <= 1)
            GenerateTrackPoints();
        else
            RaceTrack = new CubicBezierPath(PathPoints, CubicBezierPath.Type.Closed);
        gameObject.AddComponent<MeshCollider>();
        transform.position = Vector3.zero;
        GeneratePathMesh();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GenerateTrackPoints();
            GeneratePathMesh();
        }
    }

    public void GenerateTrackPoints()
    {
        if (NumberOfCorners == 4)
        {
            PathPoints = new Vector3[]
            {
                new Vector3(0,0,0),
                new Vector3(-SizeOfTrack / 2,Random.Range(-HeightVariation,HeightVariation),SizeOfTrack/2),
                new Vector3(0,Random.Range(-HeightVariation,HeightVariation),SizeOfTrack),
                new Vector3(SizeOfTrack / 2,Random.Range(-HeightVariation,HeightVariation),SizeOfTrack/2),
            };
        }
        else
        {
            PathPoints = new Vector3[NumberOfCorners + 1];
            PathPoints[0] = Vector3.zero;
            float ZDistance = SizeOfTrack / (NumberOfCorners / 2);
            float ZDistanceCount = ZDistance;
            for (int i = 1; i <= NumberOfCorners; i++)
            {
                Vector3 prevPathPoint = PathPoints[PathPoints.Length - 1];
                if (i < NumberOfCorners / 2)
                {
                    PathPoints[i] = new Vector3(Random.Range(0, -SizeOfTrack), Random.Range(-HeightVariation, HeightVariation), Random.Range(ZDistanceCount * 0.8f, ZDistanceCount * 1.2f));
                    ZDistanceCount += ZDistance;
                }
                else if (i == NumberOfCorners / 2)
                {
                    PathPoints[i] = new Vector3(Random.Range(0, -SizeOfTrack), Random.Range(-HeightVariation, HeightVariation), SizeOfTrack);
                }
                else if (i == NumberOfCorners)
                {
                    PathPoints[i] = new Vector3(Random.Range(SizeOfTrack / 2, SizeOfTrack), Random.Range(-HeightVariation, HeightVariation), ZDistanceCount);
                }
                else
                {
                    PathPoints[i] = new Vector3(Random.Range(10, SizeOfTrack), Random.Range(-HeightVariation, HeightVariation), Random.Range(ZDistanceCount * 1.2f, ZDistanceCount * 0.8f));
                    ZDistanceCount -= ZDistance;
                }
            }
        }
        RaceTrack = new CubicBezierPath(PathPoints, CubicBezierPath.Type.Closed);
    }

    public void GeneratePathMesh()
    {
        MeshGenerator gen = new MeshGenerator();
        _meshMaterials = new Material[]
        {
                TopMaterial,
                BottomMaterial,
                SideMaterial
        };
        GetComponent<MeshRenderer>().materials = _meshMaterials;
        GetComponent<MeshFilter>().sharedMesh = gen.GenerateMesh(MeshCount, RaceTrack, MeshWidth, MeshDepth);
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }


    public void AddPoint(bool LeftSide)
    {
        Vector3 prevPathPoint = PathPoints[PathPoints.Length - 1];
        PathPoints = RaceTrack.AddPoint(PathPoints, new Vector3());
        RaceTrack = new CubicBezierPath(PathPoints, CubicBezierPath.Type.Closed);
    }
}
