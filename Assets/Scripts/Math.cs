using UnityEngine;

public struct IntVec3
{
    public int x, y, z;

    public IntVec3(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static bool operator ==(IntVec3 a, IntVec3 b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(IntVec3 a, IntVec3 b)
    {
        return a.x != b.x || a.y != b.y || a.z != b.z;
    }

    public static IntVec3 operator +(IntVec3 a, IntVec3 b)
    {
        return new IntVec3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static IntVec3 operator -(IntVec3 a, IntVec3 b)
    {
        return new IntVec3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public override bool Equals(object obj)
    {
        return this == (IntVec3)obj;
    }

    public Vector3 NegativeCornersToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector3 PositiveCornersToVector3()
    {
        return new Vector3(x + 1, y + 1, z + 1);
    }

    public Vector3 CenterToVector3()
    {
        var ret = new Vector3(x, y, z);
        return ret + new Vector3(0.5f, 0.5f, 0.5f);
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
    }

    public static IntVec3 BlockCoordToChunkCoord(IntVec3 p)
    {
        return new IntVec3(
            p.x >> Constants.Log2ChunkSize,
            p.y >> Constants.Log2ChunkSize,
            p.z >> Constants.Log2ChunkSize);
    }

    public static IntVec3 BlockCoordToChunkOffset(IntVec3 p)
    {
        var mask = ~(Constants.ChunkSize - 1);
        return new IntVec3(
            p.x - (p.x & mask),
            p.y - (p.y & mask),
            p.z - (p.z & mask));
    }
}

public static class MathExts
{

    public static IntVec3 FloorToInt(this Vector3 p)
    {
        return new IntVec3(
            Mathf.FloorToInt(p.x),
            Mathf.FloorToInt(p.y),
            Mathf.FloorToInt(p.z));
    }

    public static IntVec3 SignToInt(this Vector3 p)
    {
        return new IntVec3(
            (int)Mathf.Sign(p.x),
            (int)Mathf.Sign(p.y),
            (int)Mathf.Sign(p.z));
    }
}