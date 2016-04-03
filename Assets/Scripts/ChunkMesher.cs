using UnityEngine;
using System.Collections.Generic;

public class ChunkMesher : MonoBehaviour
{
    public ChunkData Data;
    int? generation;

    public BlockShapeSet Shapes;
    public MeshFilter OutputMeshFilter;
    public MeshCollider OutputMeshCollider;
    public MeshRenderer OutputMeshRenderer;
    public Material[] Materials;

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

        // TODO: turn this inside out again for perf if we need it.
        // Assumptions: there is /never/ a visible face without framing. The mesher won't
        // even consider such things.

        for (int i = 0; i < Constants.ChunkSize; i++)
            for (int j = 0; j < Constants.ChunkSize; j++)
                for (int k = 0; k < Constants.ChunkSize; k++)
                {
                    var shapeIndex = Data.Contents[i, j, k];
                    if (shapeIndex != 0)
                    {
                        var shape = Shapes.Shapes[shapeIndex];
                        var templateMeshVerts = shape.FrameMesh.vertices;
                        var templateMeshIndices = shape.FrameMesh.triangles;
                        var templateMeshUvs = shape.FrameMesh.uv;
                        var templateMeshNormals = shape.FrameMesh.normals;

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

                        for (int faceIndex = 0; faceIndex < shape.FaceMeshes.Length; faceIndex++)
                        {
                            var surfaceType = Data.Faces[i, j, k, faceIndex];
                            if (surfaceType != 0)
                            {
                                var faceMesh = shape.FaceMeshes[faceIndex];
                                templateMeshVerts = faceMesh.vertices;
                                templateMeshIndices = faceMesh.triangles;
                                templateMeshUvs = faceMesh.uv;
                                templateMeshNormals = faceMesh.normals;
                                indexOffset = verts.Count;

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
                }

        // constraints:
        // - must NEVER add empty submeshes to phys
        // - submeshes must be 1:1 with renderer materials

        var parts = new List<List<int>>();
        var mats = new List<Material>();
        if (frameIndices.Count > 0)
        {
            mats.Add(Materials[0]);
            parts.Add(frameIndices);
        }
        if (faceIndices.Count > 0)
        {
            mats.Add(Materials[1]);
            parts.Add(faceIndices);
        }

        var m = new Mesh();
        m.SetVertices(verts);
        m.SetUVs(0, uvs);
        m.SetNormals(normals);

        m.subMeshCount = parts.Count;
        for (var partIndex = 0; partIndex < parts.Count; partIndex++)
            m.SetTriangles(parts[partIndex], partIndex);

        OutputMeshFilter.sharedMesh = m;
        OutputMeshCollider.sharedMesh = m;
        OutputMeshRenderer.sharedMaterials = mats.ToArray();
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
