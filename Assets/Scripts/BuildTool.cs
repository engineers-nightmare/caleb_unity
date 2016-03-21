using UnityEngine;
using System.Collections;
using System;

public class BuildTool : MonoBehaviour
{
    // HACK: we really want to be able to edit any chunks, not prewire them.
    public ChunkData ChunkToEdit = null;
    public AudioClip PlaceBlockSound = null;
    public AudioClip RemoveBlockSound = null;

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

    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));

        var rayOriginLocalSpace = ChunkToEdit.transform.InverseTransformPoint(ray.origin);
        var rayDirLocalSpace = ChunkToEdit.transform.InverseTransformDirection(ray.direction);

        // Surface placement
        if (Input.GetButtonDown("Fire3"))
        {
            foreach (var fc in ChunkToEdit.BlockCrossingsLocalSpace(rayOriginLocalSpace, rayDirLocalSpace, 5.0f))
            {
                if (ChunkToEdit.Contents[fc.pos.x, fc.pos.y, fc.pos.z] != 0)
                {
                    var faceIndex = NormalToFaceIndex(fc.normal);
                    if (ChunkToEdit.Faces[fc.pos.x, fc.pos.y, fc.pos.z, faceIndex] == 0)
                    {
                        ChunkToEdit.Faces[fc.pos.x, fc.pos.y, fc.pos.z, faceIndex] = 1;
                        ChunkToEdit.generation++;

                        AudioSource.PlayClipAtPoint(RemoveBlockSound, ray.origin + fc.t * ray.direction);
                        break;
                    }
                }
            }
        }

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
