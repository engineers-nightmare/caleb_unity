using UnityEngine;
using System.Collections;

public static class Constants
{
    // Note: ChunkSize must be a power of 2.
    public static readonly int ChunkSize = 8;
    public static readonly int HalfChunkSize = ChunkSize / 2;
    public static readonly int Log2ChunkSize = 3;
    public static readonly int SpecialFace = 6;     // The extra face on oddly-shaped blocks

    public static readonly int PlayerInventorySize = 5;
}
