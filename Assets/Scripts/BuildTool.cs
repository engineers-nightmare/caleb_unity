using UnityEngine;
using System.Collections;

public class BuildTool : MonoBehaviour
{
    // HACK: we really want to be able to edit any chunks, not prewire them.
    public ChunkData ChunkToEdit = null;

    void Update()
    {
        // Place-block mode.
        if (Input.GetButtonDown("Fire1"))
        {
            // Determine where, if anywhere, to place a block.
            var ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));

            var rayOriginLocalSpace = ChunkToEdit.transform.InverseTransformPoint(ray.origin);
            var rayDirLocalSpace = ChunkToEdit.transform.InverseTransformDirection(ray.direction);

            foreach (var fc in ChunkToEdit.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
            {
                if (ChunkToEdit.Contents[fc.pos.x, fc.pos.y, fc.pos.z] != 0)
                {
                    ChunkToEdit.Contents[fc.pos.x, fc.pos.y, fc.pos.z] = 0;
                    ChunkToEdit.generation++;

                    break;
                }
            }
        }

        else if (Input.GetButtonDown("Fire2"))
        {
            // Determine where, if anywhere, to place a block.
            var ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));

            var rayOriginLocalSpace = ChunkToEdit.transform.InverseTransformPoint(ray.origin);
            var rayDirLocalSpace = ChunkToEdit.transform.InverseTransformDirection(ray.direction);

            foreach (var fc in ChunkToEdit.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
            {
                if (ChunkToEdit.Contents[fc.pos.x, fc.pos.y, fc.pos.z] != 0)
                {

                    // Step back along normal.
                    var x = fc.pos.x + fc.normal.x;
                    var y = fc.pos.y + fc.normal.y;
                    var z = fc.pos.z + fc.normal.z;

                    // Ew.
                    if (x >= 0 && x < 8 && y >= 0 && y < 8 && z >= 0 && z < 8)
                    {
                        // Proposed position is still within the chunk.
                        ChunkToEdit.Contents[x, y, z] = 1;
                        ChunkToEdit.generation++;
                    }

                    break;
                }
            }
        }
    }
}
