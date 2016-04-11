using UnityEngine;

namespace Assets.Scripts
{
    public static class UnityExtensions
    {
        public static bool IsDestroyed(this GameObject o)
        {
            return o == null && !Object.ReferenceEquals(o, null);
        }
    }
}
