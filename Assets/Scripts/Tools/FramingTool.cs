using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Tools;

public class FramingTool : MonoBehaviour
{
    public AudioClip PlaceSound = null;
    public AudioClip RemoveSound = null;

    public ChunkMap ChunkMapToEdit = null;
    public BlockShapeSet Shapes = null;

    public Material PreviewFrameMaterial = null;
    
    public int BlockType = 1;

    public GameObject BuildHelperGameObject;

    private readonly BuildToolInputAccumulator _inputAccumulator = new BuildToolInputAccumulator();

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

        ChunkData chunkDataToEdit = null;
        IntVec3 blockToEdit = new IntVec3(0, 0, 0);
        int mappedFace = 0; 

        foreach (var fc in ChunkData.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
        {
            var ce = ChunkMapToEdit.GetChunk(IntVec3.BlockCoordToChunkCoord(fc.pos));
            var co = IntVec3.BlockCoordToChunkOffset(fc.pos);
            if (ce != null && ce.Contents[co.x, co.y, co.z] != 0)
            {
                chunkDataToEdit = ce;
                blockToEdit = co;

                // adding frame, so adjust position before continuing
                if (inputType == BuildToolInputType.Primary)
                {
                    blockToEdit += fc.normal;
                }

                mappedFace = FaceMap(ce.Contents[co.x, co.y, co.z], NormalToFaceIndex(fc.normal));
                break;
            }
        }

        if (!chunkDataToEdit)
        {
            return;
        }

        var doTool = inputType != BuildToolInputType.None;
        var needHelper = CheckIfNeedHelper(inputType, chunkDataToEdit, blockToEdit, mappedFace);

        if (doTool && needHelper)
        {
            GameObject helperGameObject;

            var newBuildHelper = !buildHelpers.TryGetValue(blockToEdit, out helperGameObject);

            BuildHelper helper = null;
            // can be null if object was Destroy()ed
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
                        helper.StartFrameAction(ChunkMapToEdit, mappedFace, BlockType, blockToEdit,
                            Shapes.Shapes[BlockType].FrameMesh, PreviewFrameMaterial);
                    }
                    else
                    {
                        helper.ActOn();
                    }
                    break;
                case BuildToolInputType.Secondary:
                    if (newBuildHelper)
                    {
                        helper.StartFrameAction(ChunkMapToEdit, mappedFace, 0, blockToEdit,
                            null, null);
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
    }

    private bool CheckIfNeedHelper(BuildToolInputType inputType,
        ChunkData chunkDataToEdit, IntVec3 blockToEdit, int mappedFace)
    {
        var needed = false;
        switch (inputType)
        {
            case BuildToolInputType.Primary:
                needed = mappedFace != Constants.SpecialFace;
                break;
            case BuildToolInputType.Secondary:
                var co = blockToEdit;
                var contents = chunkDataToEdit.Contents[co.x, co.y, co.z];
                needed = contents != 0;
                break;
            case BuildToolInputType.Tertiary:
                break;
        }

        return needed;
    }
}
