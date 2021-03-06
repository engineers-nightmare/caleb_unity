﻿using UnityEngine;
using System.Collections.Generic;

public struct BlockHit
{
    public IntVec3 pos;
    public IntVec3 normal;
    public float t;

    public BlockHit(IntVec3 pos, IntVec3 normal, float t)
    {
        this.pos = pos;
        this.normal = normal;
        this.t = t;
    }
}

public class ChunkData : MonoBehaviour
{
    public byte[,,] Contents = new byte[Constants.ChunkSize, Constants.ChunkSize, Constants.ChunkSize];
    public byte[,,,] Faces = new byte[Constants.ChunkSize, Constants.ChunkSize, Constants.ChunkSize, 7];
    public int generation;

    ChunkData()
    {
        generation = 0;
    }

    static float MaxAlongAxis(float a, float d)
    {
        if (d > 0)
        {
            return Mathf.Abs((Mathf.Ceil(a) - a) / d);
        }
        else
        {
            return Mathf.Abs((Mathf.Floor(a) - a) / d);
        }
    }

    public static IEnumerable<BlockHit> BlockCrossingsLocalSpace(Vector3 start, Vector3 dir, float maxDistance)
    {
        // Produces a sequence of block face crossings. This is more or less DDA with some extra tracking.
        var t = 0.0f;
        var bl = start.FloorToInt();
        var n = new IntVec3();
        var step = dir.SignToInt();
        // Distance in t between successive crossings on each axis
        var delta = new Vector3(
            Mathf.Abs(1.0f / dir.x),
            Mathf.Abs(1.0f / dir.y),
            Mathf.Abs(1.0f / dir.z));
        // First crossings in t on each axis
        var max = new Vector3(
            MaxAlongAxis(start.x, dir.x),
            MaxAlongAxis(start.y, dir.y),
            MaxAlongAxis(start.z, dir.z));

        // Repeatedly cross the next closest face.
        while (t < maxDistance)
        {
            if (max.x <= max.y && max.x <= max.z)
            {
                bl.x += step.x;
                n = new IntVec3(-step.x, 0, 0);
                t = max.x;
                max.x += delta.x;
            }
            else if (max.y < max.z)
            {
                bl.y += step.y;
                n = new IntVec3(0, -step.y, 0);
                t = max.y;
                max.y += delta.y;
            }
            else
            {
                bl.z += step.z;
                n = new IntVec3(0, 0, -step.z);
                t = max.z;
                max.z += delta.z;
            }

            yield return new BlockHit(bl, n, t);
        }
    }

    public Vector3 BlockNegativeCornerToWorldSpace(IntVec3 block)
    {
        var pos = transform.localToWorldMatrix * (block.NegativeCornersToVector3());
        var pv3 = transform.position + new Vector3(pos.x, pos.y, pos.z);

        return pv3;
    }
}
