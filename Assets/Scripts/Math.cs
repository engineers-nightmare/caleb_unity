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

    public override bool Equals(object obj)
    {
        return this == (IntVec3)obj;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
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