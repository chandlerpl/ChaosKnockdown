using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    float timepoint;
    CubicBezierPath BezierPath;
    public Mesh extrudeAlongPath(Vector3[] points, float width, float depth)
    {
        if (points.Length < 2)
            return null;
        Mesh m = new Mesh();
        Vector3 Offset = new Vector3(0, depth, 0);
        int numTris = 2 * (points.Length - 1);
        int[] roadTriangles = new int[numTris * 3];
        int[] underRoadTriangles = new int[numTris * 3];
        int[] sideOfRoadTriangles = new int[numTris * 2 * 3];
        Vector3[] verts = new Vector3[points.Length * 8];
        Vector2[] uvs = new Vector2[verts.Length];
        Vector3[] normals = new Vector3[verts.Length];


        int[] triangleMap = { 1, 8, 0, 9, 8, 1 };
        int[] sidesTriangleMap = { 14, 6, 4, 14, 4, 12, 7, 15, 5, 5, 15, 13 };

        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 perpendicularDirection;
            if (i == points.Length - 1 && (!BezierPath.IsClosed())) 
            {
                perpendicularDirection = new Vector3(-(points[i - 1].z - points[i - 2].z), 0, points[i - 1].x - points[i - 2].x).normalized;
            }
            else if (i == points.Length - 1 && BezierPath.IsClosed())
            {
                perpendicularDirection = new Vector3(-(points[1].z - points[i].z), 0, points[1].x - points[i].x).normalized;
            }
            else
                perpendicularDirection = new Vector3(-(points[i + 1].z - points[i].z), 0, points[i + 1].x - points[i].x).normalized;
            // Find position to left and right of current path vertex 
            Vector3 vertSideA = points[i] + perpendicularDirection * -width;
            Vector3 vertSideB = points[i] + perpendicularDirection * width;

            // Add top of road vertices
            verts[vertIndex + 0] = vertSideA;
            verts[vertIndex + 1] = vertSideB;
            // Add bottom of road vertices
            verts[vertIndex + 2] = vertSideA - Offset;
            verts[vertIndex + 3] = vertSideB - Offset;

            // Duplicate vertices to get flat shading for sides of road
            verts[vertIndex + 4] = verts[vertIndex + 0];
            verts[vertIndex + 5] = verts[vertIndex + 1];
            verts[vertIndex + 6] = verts[vertIndex + 2];
            verts[vertIndex + 7] = verts[vertIndex + 3];
            //TOP UV
            uvs[vertIndex + 0] = new Vector2(0, i * timepoint);
            uvs[vertIndex + 1] = new Vector2(1, i * timepoint);
            //Bottom UV
            uvs[vertIndex + 2] = new Vector2(0, i * timepoint);
            uvs[vertIndex + 3] = new Vector2(1, i * timepoint);
            //SIDE UVS
            uvs[vertIndex + 4] = new Vector2(1, i * timepoint);
            uvs[vertIndex + 5] = new Vector2(1, i * timepoint);

            // Top of road normals
            normals[vertIndex + 0] = Vector3.up;
            normals[vertIndex + 1] = Vector3.up;
            //Bottom of road normals
            normals[vertIndex + 2] = -Vector3.up;
            normals[vertIndex + 3] = -Vector3.up;
            //Sides of road normals
            normals[vertIndex + 4] = -Vector3.right;
            normals[vertIndex + 5] = Vector3.right;
            normals[vertIndex + 6] = -Vector3.right;
            normals[vertIndex + 7] = Vector3.right;


            // Set triangle indices
            if (i < points.Length - 1)
            {
                for (int j = 0; j < triangleMap.Length; j++)
                {
                        roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;
                        // reverse triangle map
                        underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % verts.Length;
                }
                for (int j = 0; j < sidesTriangleMap.Length; j++)
                {
                    sideOfRoadTriangles[triIndex * 2 + j] = (vertIndex + sidesTriangleMap[j]) % verts.Length;
                }

            }

            vertIndex += 8;
            triIndex += 6;
        }

        m.Clear();
        m.vertices = verts;
        m.uv = uvs;
        m.normals = normals;
        m.subMeshCount = 3;
        m.SetTriangles(roadTriangles, 0);
        m.SetTriangles(underRoadTriangles, 1);
        m.SetTriangles(sideOfRoadTriangles, 2);
        m.RecalculateBounds();
        m.name = "pathMesh";
        m.RecalculateBounds();
        return m;
    }
    private static Vector2 ScaledUV(float uv1, float uv2, float scale)
    {
        return new Vector2(uv1 / scale, uv2 / scale);
    }
    public Mesh GenerateMesh(int MeshCount, CubicBezierPath Path, float Width, float depth)
    {
        BezierPath = Path;
        Vector3[] MeshPoints = new Vector3[MeshCount + 1];
        timepoint = 1.0f / MeshCount;
        float t = 0;
        for (int i = 0; i < MeshCount; i++)
        {
            if (BezierPath.IsValid())
            {
                MeshPoints[i] = BezierPath.GetPointNorm(t);
                t += timepoint;
            }
        }
        MeshPoints[MeshPoints.Length - 1] = BezierPath.GetPointNorm(1);
        return extrudeAlongPath(MeshPoints, Width, depth);
    }
}
