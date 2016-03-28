﻿using UnityEngine;
using System.Collections.Generic;

public class ChunkMesher : MonoBehaviour
{
    public ChunkData Data;
    int? generation;

    public Mesh FrameMeshTemplate;
    public Mesh[] FaceMeshTemplates = new Mesh[6];
    public MeshFilter OutputMeshFilter;
    public MeshCollider OutputMeshCollider;

    // Use this for initialization
    void Start()
    {
        generation = null;
    }

    void RebuildTargetMesh()
    {
        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();
        var normals = new List<Vector3>();
        var frameIndices = new List<int>();
        var faceIndices = new List<int>();

        var templateMeshVerts = FrameMeshTemplate.vertices;
        var templateMeshIndices = FrameMeshTemplate.triangles;
        var templateMeshUvs = FrameMeshTemplate.uv;
        var templateMeshNormals = FrameMeshTemplate.normals;

        for (int i = 0; i < Constants.ChunkSize; i++)
            for (int j = 0; j < Constants.ChunkSize; j++)
                for (int k = 0; k < Constants.ChunkSize; k++)
                {
                    if (Data.Contents[i, j, k] != 0)
                    {
                        var p = new Vector3(i, j, k);
                        var indexOffset = verts.Count;

                        foreach (var v in templateMeshVerts)
                            verts.Add(p + v);
                        foreach (var uv in templateMeshUvs)
                            uvs.Add(uv);
                        foreach (var n in templateMeshNormals)
                            normals.Add(n);
                        foreach (var ind in templateMeshIndices)
                            frameIndices.Add(indexOffset + ind);
                    }
                }

        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
        {
            templateMeshVerts = FaceMeshTemplates[faceIndex].vertices;
            templateMeshIndices = FaceMeshTemplates[faceIndex].triangles;
            templateMeshUvs = FaceMeshTemplates[faceIndex].uv;
            templateMeshNormals = FaceMeshTemplates[faceIndex].normals;

            for (int i = 0; i < Constants.ChunkSize; i++)
                for (int j = 0; j < Constants.ChunkSize; j++)
                    for (int k = 0; k < Constants.ChunkSize; k++)
                    {
                        var surfaceType = Data.Faces[i, j, k, faceIndex];
                        if (surfaceType != 0)
                        {
                            var p = new Vector3(i, j, k);
                            var indexOffset = verts.Count;

                            foreach (var v in templateMeshVerts)
                                verts.Add(p + v);
                            foreach (var uv in templateMeshUvs)
                                uvs.Add(uv);
                            foreach (var n in templateMeshNormals)
                                normals.Add(n);
                            foreach (var ind in templateMeshIndices)
                                faceIndices.Add(indexOffset + ind);
                        }
                    }
        }

        // rendered mesh vs phys mesh constraints:
        // - rendered mesh MUST have the correct number of submeshes for the number of materials
        //   attached to the MeshRenderer.
        // - phys mesh must NEVER contain an empty submesh, or building will fail.
        //
        // we /could/ take control of the materials array here, but for now just cheat and produce
        // two meshes.
        var m = new Mesh();
        m.SetVertices(verts);
        m.SetUVs(0, uvs);
        m.SetNormals(normals);
        m.subMeshCount = 2;
        m.SetTriangles(frameIndices, 0);
        m.SetTriangles(faceIndices, 1);
        m.UploadMeshData(true);
        OutputMeshFilter.mesh = m;

        m = new Mesh();
        m.SetVertices(verts);
        m.SetUVs(0, uvs);
        m.SetNormals(normals);
        var parts = new List<List<int>>();
        if (frameIndices.Count > 0) parts.Add(frameIndices);
        if (faceIndices.Count > 0) parts.Add(faceIndices);
        m.subMeshCount = parts.Count;
        for (var partIndex = 0; partIndex < parts.Count; partIndex++)
            m.SetTriangles(parts[partIndex], partIndex);
        OutputMeshCollider.sharedMesh = m;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(Constants.HalfChunkSize, Constants.HalfChunkSize, Constants.HalfChunkSize),
            new Vector3(Constants.ChunkSize, Constants.ChunkSize, Constants.ChunkSize));
    }

    void Update()
    {
        if (generation != Data.generation)
        {
            RebuildTargetMesh();
            generation = Data.generation;
            Debug.Log(string.Format("Rebuilt chunk mesh for generation {0}", generation));
        }
    }
}
