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
                        var p = new Vector3(i - Constants.ChunkSize / 2 + 1,
                            j - Constants.ChunkSize / 2,
                            k - Constants.ChunkSize / 2);       // HACK for broken content.
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

        for (int faceBit = 0; faceBit < 6; faceBit++)
        {
            templateMeshVerts = FaceMeshTemplates[faceBit].vertices;
            templateMeshIndices = FaceMeshTemplates[faceBit].triangles;
            templateMeshUvs = FaceMeshTemplates[faceBit].uv;
            templateMeshNormals = FaceMeshTemplates[faceBit].normals;

            for (int i = 0; i < Constants.ChunkSize; i++)
                for (int j = 0; j < Constants.ChunkSize; j++)
                    for (int k = 0; k < Constants.ChunkSize; k++)
                    {
                        var face = Data.Faces[i, j, k];
                        if ((face & (1 << faceBit)) != 0)
                        {
                            var p = new Vector3(i - Constants.ChunkSize / 2 + 1,
                            j - Constants.ChunkSize / 2,
                            k - Constants.ChunkSize / 2);       // HACK for broken content.
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

        var m = new Mesh();
        m.SetVertices(verts);
        m.SetUVs(0, uvs);
        m.SetNormals(normals);
        m.subMeshCount = 2;
        m.SetTriangles(frameIndices, 0);
        m.SetTriangles(faceIndices, 1);
        OutputMeshFilter.mesh = m;
        OutputMeshCollider.sharedMesh = m;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(0, 0, 0),
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
