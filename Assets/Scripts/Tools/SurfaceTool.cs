using UnityEngine;
using System.Collections;
using System;

public class SurfaceTool : MonoBehaviour
{
    public AudioClip PlaceSound = null;
    public AudioClip RemoveSound = null;

    public ChunkMap ChunkMapToEdit = null;
    public BlockShapeSet Shapes = null;

    public Mesh[] SurfaceMeshes = new Mesh[6];
    public Material PreviewMaterial = null;

    public float ToolInputDuration = 0.5f;

    private BuildToolInputAccumulator _inputAccumulator = new BuildToolInputAccumulator();

    static int NormalToFaceIndex(IntVec3 n)
    {
        if (n.x == -1) return 0;
        if (n.x == 1) return 1;
        if (n.y == -1) return 2;
        if (n.y == 1) return 3;
        if (n.z == -1) return 4;
        if (n.z == 1) return 5;

        throw new InvalidOperationException("Bogus face normal");
    }

    void Start()
    {
    }

    int FaceMap(int block, int face)
    {
        // TODO: also remap missing parts of triangular faces to the special face.

        if (block == 2)
        {
            // sloped
            if (face == 1 || face == 5)
                return 6;
        }

        return face;
    }

    private void UpdatePreview(Ray ray, Vector3 rayOriginLocalSpace,
        Vector3 rayDirLocalSpace, float scale)
    {
        var ceTrans = ChunkMapToEdit.transform;

        foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
        {
            var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
            var co = IntVec3.BlockCoordToChunkOffset(fc.pos);
            if (ce != null && ce.Contents[co.x, co.y, co.z] != 0)
            {
                var faceIndex = FaceMap(ce.Contents[co.x, co.y, co.z], NormalToFaceIndex(fc.normal));

                var pv3 = ce.BlockNegativeCornerToWorldSpace(co);
                Graphics.DrawMesh(Shapes.Shapes[ce.Contents[co.x, co.y, co.z]].FaceMeshes[faceIndex], pv3, ceTrans.rotation, PreviewMaterial, 0);
                break;
            }
        }
    }

    private void Update()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));

        var rayOriginLocalSpace = ChunkMapToEdit.transform.InverseTransformPoint(ray.origin);
        var rayDirLocalSpace = ChunkMapToEdit.transform.InverseTransformDirection(ray.direction);

        // first of primary, secondary, tertiary that is active
        BuildToolInputType inputType =
            Input.GetButton("ToolPrimary")
                ? BuildToolInputType.Primary
                : Input.GetButton("ToolSecondary")
                    ? BuildToolInputType.Secondary
                    : Input.GetButton("ToolTertiary")
                        ? BuildToolInputType.Tertiary
                        : BuildToolInputType.None;

        var doTool = false;
        if (inputType != BuildToolInputType.None)
        {
            doTool = _inputAccumulator.Increment(inputType,
                Time.deltaTime, ToolInputDuration);
        }
        else
        {
            _inputAccumulator.Reset();
        }

        if (doTool)
        {
            // this section is kind of gross
            switch (inputType)
            {
                
                case BuildToolInputType.Primary:
                    PlaceSurface(ray, rayOriginLocalSpace, rayDirLocalSpace);
                    break;
                case BuildToolInputType.Secondary:
                    RemoveSurface(ray, rayOriginLocalSpace, rayDirLocalSpace);
                    break;
                case BuildToolInputType.Tertiary:
                    break;
            }
        }

        UpdatePreview(ray, rayOriginLocalSpace, rayDirLocalSpace,
            _inputAccumulator.Duration / ToolInputDuration);
    }

    // Surface placement
    void PlaceSurface(Ray ray, Vector3 rayOriginLocalSpace, Vector3 rayDirLocalSpace)
    {
        foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
        {
            var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
            var co = IntVec3.BlockCoordToChunkOffset(fc.pos);
            if (ce != null && ce.Contents[co.x, co.y, co.z] != 0)
            {
                var faceIndex = FaceMap(ce.Contents[co.x, co.y, co.z], NormalToFaceIndex(fc.normal));
                if (ce.Faces[co.x, co.y, co.z, faceIndex] == 0)
                {
                    ce.Faces[co.x, co.y, co.z, faceIndex] = 1;
                    ce.generation++;

                    AudioSource.PlayClipAtPoint(PlaceSound, ray.origin + fc.t * ray.direction);
                }
                break;
            }
        }
    }

    // Surface removal
    void RemoveSurface(Ray ray, Vector3 rayOriginLocalSpace, Vector3 rayDirLocalSpace)
    {
        foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
        {
            var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
            var co = IntVec3.BlockCoordToChunkOffset(fc.pos);
            if (ce != null && ce.Contents[co.x, co.y, co.z] != 0)
            {
                var faceIndex = FaceMap(ce.Contents[co.x, co.y, co.z], NormalToFaceIndex(fc.normal));
                if (ce.Faces[co.x, co.y, co.z, faceIndex] != 0)
                {
                    ce.Faces[co.x, co.y, co.z, faceIndex] = 0;
                    ce.generation++;

                    AudioSource.PlayClipAtPoint(RemoveSound, ray.origin + fc.t * ray.direction);
                }
                break;
            }
        }
    }
}
