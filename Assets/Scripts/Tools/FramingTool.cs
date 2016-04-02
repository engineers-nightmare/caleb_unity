using UnityEngine;
using System.Collections;
using System;

public class FramingTool : MonoBehaviour
{
    public AudioClip PlaceSound = null;
    public AudioClip RemoveSound = null;

    public ChunkMap ChunkMapToEdit = null;
    public BlockShapeSet Shapes = null;

    public Material PreviewFrameMaterial = null;

    public float ToolInputDuration = 0.5f;
    public int BlockType = 1;

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
                var pv3 = ce.BlockNegativeCornerToWorldSpace(co + fc.normal);
                Graphics.DrawMesh(Shapes.Shapes[BlockType].FrameMesh, pv3, ceTrans.rotation, PreviewFrameMaterial, 0);
                break;
            }
        }
    }
    
    private void Update()
    {
        if (Input.GetButtonDown("ToolAuxSwitch"))
        {
            BlockType = (BlockType - 1 + 1) % 2 + 1;    // TODO make this proper
        }
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
            switch (inputType)
            {
                case BuildToolInputType.Primary:
                    PlaceFrame(ray, rayOriginLocalSpace, rayDirLocalSpace);
                    break;
                case BuildToolInputType.Secondary:
                    RemoveFrame(ray, rayOriginLocalSpace, rayDirLocalSpace);
                    break;
                case BuildToolInputType.Tertiary:
                    break;
            }
        }

        UpdatePreview(ray, rayOriginLocalSpace, rayDirLocalSpace,
            _inputAccumulator.Duration / ToolInputDuration);
    }

    // Place block tool -- places a block against the block we hit, sharing the face we hit
    void PlaceFrame(Ray ray, Vector3 rayOriginLocalSpace, Vector3 rayDirLocalSpace)
    {
        foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
        {
            var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
            var co = IntVec3.BlockCoordToChunkOffset(fc.pos);
            if (ce != null && ce.Contents[co.x, co.y, co.z] != 0)
            {
                var faceIndex = FaceMap(ce.Contents[co.x, co.y, co.z], NormalToFaceIndex(fc.normal));
                if (faceIndex != Constants.SpecialFace)
                {
                    // Step back along normal.
                    var p = fc.pos + fc.normal;
                    var ce2 = ChunkMapToEdit.EnsureChunk(IntVec3.BlockCoordToChunkCoord(p));
                    var co2 = IntVec3.BlockCoordToChunkOffset(p);

                    ce2.Contents[co2.x, co2.y, co2.z] = (byte)BlockType;
                    ce2.generation++;

                    AudioSource.PlayClipAtPoint(PlaceSound, ray.origin + fc.t * ray.direction);
                }
                break;
            }
        }
    }

    // Remove block tool -- removes the first block we hit
    void RemoveFrame(Ray ray, Vector3 rayOriginLocalSpace, Vector3 rayDirLocalSpace)
    {
        foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
        {
            var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
            var co = IntVec3.BlockCoordToChunkOffset(fc.pos);
            if (ce != null && ce.Contents[co.x, co.y, co.z] != 0)
            {
                ce.Contents[co.x, co.y, co.z] = 0;
                ce.generation++;

                AudioSource.PlayClipAtPoint(RemoveSound, ray.origin + fc.t * ray.direction);
                break;
            }
        }
    }
}
