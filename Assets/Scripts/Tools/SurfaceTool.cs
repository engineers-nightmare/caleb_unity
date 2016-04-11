using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Tools;

public class SurfaceTool : MonoBehaviour
{
    public AudioClip PlaceSound = null;
    public AudioClip RemoveSound = null;

    public ChunkMap ChunkMapToEdit = null;
    public BlockShapeSet Shapes = null;

    public Mesh[] SurfaceMeshes = new Mesh[6];
    public Material PreviewMaterial = null;
    
    public int SurfaceType = 1;

    public GameObject BuildHelperGameObject;

    private Dictionary<IntVec3, GameObject> buildHelpers = new Dictionary<IntVec3, GameObject>();

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

        ChunkData chunkDataToEdit = null;
        IntVec3 blockToEdit = new IntVec3(0, 0, 0);
        int mappedFace = 0;

        if (!TryGetSurfaceHelperData(rayOriginLocalSpace, rayDirLocalSpace, ref chunkDataToEdit, ref blockToEdit,
            ref mappedFace))
        {
            return;
        }

        var doTool = inputType != BuildToolInputType.None;
    
        if (doTool)
        {
            PerformToolAction(blockToEdit, inputType, chunkDataToEdit, mappedFace);
        }
    }

    private void PerformToolAction(IntVec3 blockToEdit, BuildToolInputType inputType, ChunkData chunkDataToEdit,
        int mappedFace)
    {
        GameObject helperGameObject;

        var newBuildHelper = !buildHelpers.TryGetValue(blockToEdit, out helperGameObject);

        BuildHelper helper = null;
        if (newBuildHelper || helperGameObject.IsDestroyed())
        {
            helperGameObject = Instantiate(BuildHelperGameObject);
            helper = helperGameObject.GetComponent<BuildHelper>();
            helper.enabled = false;

            // stomp old one/insert the new one
            buildHelpers[blockToEdit] = helperGameObject;

            newBuildHelper = true;
        }
        else
        {
            helper = helperGameObject.GetComponent<BuildHelper>();
        }

        switch (inputType)
        {
            case BuildToolInputType.Primary:
                if (newBuildHelper)
                {
                    var co = blockToEdit;
                    var mesh = Shapes.Shapes[chunkDataToEdit.Contents[co.x, co.y, co.z]].FaceMeshes[mappedFace];
                    helper.StartSurfaceAction(ChunkMapToEdit, mappedFace, SurfaceType, blockToEdit,
                        mesh, PreviewMaterial);
                }
                else
                {
                    helper.ActOn();
                }
                break;
            case BuildToolInputType.Secondary:
                if (newBuildHelper)
                {
                    var co = blockToEdit;
                    var mesh = Shapes.Shapes[chunkDataToEdit.Contents[co.x, co.y, co.z]].FaceMeshes[mappedFace];
                    helper.StartSurfaceAction(ChunkMapToEdit, mappedFace, 0, blockToEdit,
                        mesh, PreviewMaterial);
                }
                else
                {
                    helper.ActOn();
                }
                break;
            case BuildToolInputType.Tertiary:
                break;
        }
    }

    private bool TryGetSurfaceHelperData(Vector3 rayOriginLocalSpace,
        Vector3 rayDirLocalSpace, ref ChunkData chunkDataToEdit,
        ref IntVec3 blockToEdit, ref int mappedFace)
    {
        var found = false;
        foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
        {
            var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
            var co = IntVec3.BlockCoordToChunkOffset(fc.pos);

            if (ce == null || ce.Contents[co.x, co.y, co.z] == 0)
            {
                continue;
            }

            chunkDataToEdit = ce;
            blockToEdit = co;

            mappedFace = FaceMap(ce.Contents[co.x, co.y, co.z], NormalToFaceIndex(fc.normal));
            found = true;
            break;
        }

        return found;
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
