using UnityEngine;
using System.Collections;
using System;

public class BuildTool : MonoBehaviour
{
    public AudioClip PlaceBlockSound = null;
    public AudioClip RemoveBlockSound = null;

    public ChunkMap ChunkMapToEdit = null;

    public Mesh FrameMesh = null;
    public Mesh[] SurfaceMeshes = new Mesh[6];
    public Material PreviewFrameMaterial = null;
    public Material PreviewSurfaceMaterial = null;

    public BuildToolMode ToolMode = BuildToolMode.Frame;

    public enum BuildToolMode
    {
        Frame,
        Surface,
    }

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

    private void UpdatePreview(Ray ray, Vector3 rayOriginLocalSpace, Vector3 rayDirLocalSpace)
    {
        var ceTrans = ChunkMapToEdit.transform;

        foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
        {
            var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
            var co = IntVec3.BlockCoordToChunkOffset(fc.pos);
            if (ce != null && ce.Contents[co.x, co.y, co.z] != 0)
            {
                var faceIndex = NormalToFaceIndex(fc.normal);

                if ((ce.Faces[co.x, co.y, co.z, faceIndex] & (1 << faceIndex)) != 0)
                {
                    continue;
                }

                var pv3 = ce.BlockNegativeCornerToWorldSpace(co);

                switch (ToolMode)
                {
                    case BuildToolMode.Frame:
                        Graphics.DrawMesh(SurfaceMeshes[faceIndex], pv3, ceTrans.rotation, PreviewFrameMaterial, 0);

                        pv3 = ce.BlockNegativeCornerToWorldSpace(co + fc.normal);
                        Graphics.DrawMesh(FrameMesh, pv3, ceTrans.rotation, PreviewFrameMaterial, 0);

                        return;
                    case BuildToolMode.Surface:
                        Graphics.DrawMesh(SurfaceMeshes[faceIndex], pv3, ceTrans.rotation, PreviewSurfaceMaterial, 0);

                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), ToolMode.ToString());
    }

    void OnDrawGizmos()
    {
        // had to gut this.
    }

    private void Update()
    {
        if (Input.GetButtonDown("ToolModeSwitch"))
        {
            if (ToolMode == BuildToolMode.Frame)
            {
                ToolMode = BuildToolMode.Surface;
            }
            else if (ToolMode == BuildToolMode.Surface)
            {
                ToolMode = BuildToolMode.Frame;
            }
        }

        var ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));

        var rayOriginLocalSpace = ChunkMapToEdit.transform.InverseTransformPoint(ray.origin);
        var rayDirLocalSpace = ChunkMapToEdit.transform.InverseTransformDirection(ray.direction);


        if (Input.GetButtonDown("ToolPrimary"))
        {
            switch (ToolMode)
            {
                case BuildToolMode.Frame:
                    PlaceFrame(ray, rayOriginLocalSpace, rayDirLocalSpace);
                    break;
                case BuildToolMode.Surface:
                    PlaceSurface(ray, rayOriginLocalSpace, rayDirLocalSpace);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (Input.GetButtonDown("ToolSecondary"))
        {
            switch (ToolMode)
            {
                case BuildToolMode.Frame:
                    RemoveFrame(ray, rayOriginLocalSpace, rayDirLocalSpace);
                    break;
                case BuildToolMode.Surface:
                    RemoveSurface(ray, rayOriginLocalSpace, rayDirLocalSpace);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (Input.GetButtonDown("ToolTertiary"))
        {
        }

        UpdatePreview(ray, rayOriginLocalSpace, rayDirLocalSpace);
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

    // Surface removal
    void RemoveSurface(Ray ray, Vector3 rayOriginLocalSpace, Vector3 rayDirLocalSpace)
    {
        foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
        {
            var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
            var co = IntVec3.BlockCoordToChunkOffset(fc.pos);
            if (ce != null && ce.Contents[co.x, co.y, co.z] != 0)
            {
                var faceIndex = NormalToFaceIndex(fc.normal);
                if (ce.Faces[co.x, co.y, co.z, faceIndex] != 0)
                {
                    ce.Faces[co.x, co.y, co.z, faceIndex] = 0;
                    ce.generation++;

                    AudioSource.PlayClipAtPoint(RemoveBlockSound, ray.origin + fc.t * ray.direction);
                    break;
                }
            }
        }
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

                // Emit noise
                AudioSource.PlayClipAtPoint(RemoveBlockSound, ray.origin + fc.t * ray.direction);

                break;
            }
        }
    }
}
