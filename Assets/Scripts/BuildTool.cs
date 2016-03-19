using UnityEngine;
using System.Collections;

public class BuildTool : MonoBehaviour
{
    // HACK: we really want to be able to edit any chunks, not prewire them.
    public ChunkData ChunkToEdit = null;
    public AudioClip PlaceBlockSound = null;
    public AudioClip RemoveBlockSound = null;

    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));

        var rayOriginLocalSpace = ChunkToEdit.transform.InverseTransformPoint(ray.origin);
        var rayDirLocalSpace = ChunkToEdit.transform.InverseTransformDirection(ray.direction);

        // Remove block tool -- removes the first block we hit
        if (Input.GetButtonDown("Fire1"))
        {
            foreach (var fc in ChunkToEdit.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
            {
                if (ChunkToEdit.Contents[fc.pos.x, fc.pos.y, fc.pos.z] != 0)
                {
                    ChunkToEdit.Contents[fc.pos.x, fc.pos.y, fc.pos.z] = 0;
                    ChunkToEdit.generation++;

                    // Emit noise
                    AudioSource.PlayClipAtPoint(RemoveBlockSound, ray.origin + fc.t * ray.direction);

                    break;
                }
            }
        }

        // Place block tool -- places a block against the block we hit, sharing the face we hit
        if (Input.GetButtonDown("Fire2"))
        {
            foreach (var fc in ChunkToEdit.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
            {
                if (ChunkToEdit.Contents[fc.pos.x, fc.pos.y, fc.pos.z] != 0)
                {
                    // Step back along normal.
                    var x = fc.pos.x + fc.normal.x;
                    var y = fc.pos.y + fc.normal.y;
                    var z = fc.pos.z + fc.normal.z;

                    if (x >= 0 && x < Constants.ChunkSize &&
                        y >= 0 && y < Constants.ChunkSize &&
                        z >= 0 && z < Constants.ChunkSize)
                    {
                        // Proposed position is still within the chunk.
                        ChunkToEdit.Contents[x, y, z] = 1;
                        ChunkToEdit.generation++;

                        // Emit clunk noise
                        AudioSource.PlayClipAtPoint(PlaceBlockSound, ray.origin + fc.t * ray.direction);
                    }

                    break;
                }
            }
        }
    }
}
