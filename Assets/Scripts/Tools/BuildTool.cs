using UnityEngine;
using System.Collections;
using System;

public class BuildTool : MonoBehaviour
{
    // HACK: we really want to be able to edit any chunks, not prewire them.
    public AudioClip PlaceBlockSound = null;
    public AudioClip RemoveBlockSound = null;

    public ChunkMap ChunkMapToEdit = null;

    public Mesh FrameMesh = null;
    public Material PreviewMaterial = null;

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

    protected void UpdatePreview()
    {
        var ray = Camera.main.ScreenPointToRay(
            new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));

        var ceTrans = ChunkMapToEdit.transform;
        var rayOriginLocalSpace = ceTrans.InverseTransformPoint(ray.origin);
        var rayDirLocalSpace = ceTrans.InverseTransformDirection(ray.direction);

        foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
        {
            var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
            var co = IntVec3.BlockCoordToChunkOffset(fc.pos);
            if (ce != null && ce.Contents[co.x, co.y, co.z] != 0)
            {
                var faceIndex = NormalToFaceIndex(fc.normal);
                if ((ce.Faces[co.x, co.y, co.z, faceIndex] & (1 << faceIndex)) == 0)
                {
                    var pv3 = ce.BlockNegativeCornerToWorldSpace(co + fc.normal);
                    Graphics.DrawMesh(FrameMesh, pv3, ceTrans.rotation, PreviewMaterial, 0);
                    break;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        // had to gut this.
    }

    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));

        var rayOriginLocalSpace = ChunkMapToEdit.transform.InverseTransformPoint(ray.origin);
        var rayDirLocalSpace = ChunkMapToEdit.transform.InverseTransformDirection(ray.direction);

        UpdatePreview();

        // Surface placement
        if (Input.GetButtonDown("ToolTertiary"))
        {
            foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
            {
                var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
                var co = IntVec3.BlockCoordToChunkOffset(fc.pos);
                if (ce != null && ce.Contents[co.x, co.y, co.z] != 0)
                {
                    var faceIndex = NormalToFaceIndex(fc.normal);
                    if (ce.Faces[co.x, co.y, co.z, faceIndex] == 0)
                    {
                        ce.Faces[co.x, co.y, co.z, faceIndex] = 1;
                        ce.generation++;

                        AudioSource.PlayClipAtPoint(RemoveBlockSound, ray.origin + fc.t * ray.direction);
                        break;
                    }
                }
            }
        }

        // Remove block tool -- removes the first block we hit
        if (Input.GetButtonDown("ToolPrimary"))
        {
            foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
            {
                var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
                var co = IntVec3.BlockCoordToChunkOffset(fc.pos);
                if (ce != null && ce.Contents[co.x, co.y, co.z] != 0)
                {
                    ce.Contents[co.x, co.y, co.z] = 0;
                    ce.generation++;

                    // Emit noise
                    AudioSource.PlayClipAtPoint(RemoveBlockSound, ray.origin + fc.t * ray.direction);

                    break;
                }
            }
        }

        // Place block tool -- places a block against the block we hit, sharing the face we hit
        if (Input.GetButtonDown("ToolSecondary"))
        {
            foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
            {
                var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
                var co = IntVec3.BlockCoordToChunkOffset(fc.pos);
                if (ce != null && ce.Contents[co.x, co.y, co.z] != 0)
                {
                    // Step back along normal.
                    var p = fc.pos + fc.normal;
                    var ce2 = ChunkMapToEdit.EnsureChunk(IntVec3.BlockCoordToChunkCoord(p));
                    var co2 = IntVec3.BlockCoordToChunkOffset(p);

                    ce2.Contents[co2.x, co2.y, co2.z] = 1;
                    ce2.generation++;

                    // Emit clunk noise
                    AudioSource.PlayClipAtPoint(PlaceBlockSound, ray.origin + fc.t * ray.direction);

                    break;
                }
            }
        }
    }
}
