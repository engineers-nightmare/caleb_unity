using System.Security;
using UnityEngine;

namespace Assets.Scripts
{
    public class TriggerCameraController : MonoBehaviour
    {
        public float LengthScale = 0.8f;

        private Vector3 _spineStartWorld;
        private Vector3 _spineEndWorld;

        // Use this for initialization
        private void Start()
        {
            var pc = GetComponent<BoxCollider>();
            var center = pc.transform.localPosition;
            var size = Vector3.Scale(pc.transform.localScale, pc.size);

            var l = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            var vec = Vector3.zero;
            if (l == size.x)
            {
                vec = Vector3.right;
            }
            else if (l == size.y)
            {
                vec = Vector3.up;
            }
            else if (l == size.z)
            {
                vec = Vector3.forward;
            }

            var spine = vec * l * LengthScale;
            _spineStartWorld = transform.TransformPoint(-(spine / 2f));
            _spineEndWorld = transform.TransformPoint(spine / 2f);
        }

        // Update is called once per frame
        void Update()
        {
        }

        void OnTriggerStay(Collider other)
        {
            var p = GetCameraPoint();

            var c = Camera.main.GetComponent<CameraController>();
            c.SetDesiredPosition(p);
        }

        void OnDrawGizmos()
        {
            var c = Camera.main.GetComponent<CameraController>();
            var p = c.GetDesiredPosition();

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(p, 0.1f);

            Debug.DrawLine(_spineStartWorld, _spineEndWorld, Color.yellow);
        }

        public Vector3[] FindLineSphereIntersections(Vector3 linePoint0,
            Vector3 linePoint1, Vector3 center, float radius)
        {

            var cx = center.x;
            var cy = center.y;
            var cz = center.z;

            var px = linePoint0.x;
            var py = linePoint0.y;
            var pz = linePoint0.z;

            var vx = linePoint1.x - px;
            var vy = linePoint1.y - py;
            var vz = linePoint1.z - pz;

            var a = vx * vx + vy * vy + vz * vz;
            var b = 2 * (px * vx + py * vy + pz * vz - vx * cx - vy * cy - vz * cz);
            var c = px * px - 2 * px * cx + cx * cx + py * py - 2f * py * cy + cy * cy +
                       pz * pz - 2 * pz * cz + cz * cz - radius * radius;

            // discriminant
            var d = b * b - 4 * a * c;

            var t1 = (-b - Mathf.Sqrt(d)) / (2 * a);

            var solution1 = new Vector3(linePoint0.x * (1 - t1) + t1 * linePoint1.x,
                                             linePoint0.y * (1 - t1) + t1 * linePoint1.y,
                                             linePoint0.z * (1 - t1) + t1 * linePoint1.z);

            var t2 = (-b + Mathf.Sqrt(d)) / (2 * a);
            var solution2 = new Vector3(linePoint0.x * (1 - t2) + t2 * linePoint1.x,
                                             linePoint0.y * (1 - t2) + t2 * linePoint1.y,
                                             linePoint0.z * (1 - t2) + t2 * linePoint1.z);

            if (d < 0 || t1 > 1 || t2 > 1)
            {
                return new Vector3[0];
            }
            else if (d == 0)
            {
                return new[] { solution1 };
            }
            else
            {
                return new[] { solution1, solution2 };
            }
        }

        // Find the distance from this point to a line segment (which is not the same as from this 
        //  point to anywhere on an infinite line). Also returns the closest point.
        public float DistanceToLineSegment(Vector3 point, Vector3 lineSegmentPoint1, Vector3 lineSegmentPoint2,
                                            out Vector3 closestPoint)
        {
            return Mathf.Sqrt(DistanceToLineSegmentSquared(point,
                lineSegmentPoint1,
                lineSegmentPoint2,
                out closestPoint));
        }

        // Same as above, but avoid using Sqrt(), saves a new nanoseconds in cases where you only want 
        //  to compare several distances to find the smallest or largest, but don't need the distance
        public float DistanceToLineSegmentSquared(Vector3 point,
            Vector3 lineSegmentPoint1,Vector3 lineSegmentPoint2,
            out Vector3 closestPoint)
        {
            // Compute length of line segment (squared) and handle special case of coincident points
            var segmentLengthSquared = (lineSegmentPoint1 - lineSegmentPoint2).sqrMagnitude;
            if (segmentLengthSquared < 1E-7f)  // Arbitrary "close enough for government work" value
            {
                closestPoint = lineSegmentPoint1;
                return (point - closestPoint).sqrMagnitude;
            }

            // Use the magic formula to compute the "projection" of this point on the infinite line
            var lineSegment = lineSegmentPoint2 - lineSegmentPoint1;
            var t = Vector3.Dot(point - lineSegmentPoint1, lineSegment) / segmentLengthSquared;

            // Handle the two cases where the projection is not on the line segment, and the case where 
            //  the projection is on the segment
            if (t <= 0)
                closestPoint = lineSegmentPoint1;
            else if (t >= 1)
                closestPoint = lineSegmentPoint2;
            else
                closestPoint = lineSegmentPoint1 + (lineSegment * t);
            return (point - closestPoint).sqrMagnitude;
        }

        Vector3 GetCameraPoint()
        {
            var c = Camera.main.GetComponent<CameraController>();
            var cp = c.GetDesiredPosition();
            var pl = c.Target;
            var pi = pl.GetComponentInChildren<PlayerInput>();
            var rb = pi.Driver;
            var p = rb.transform.position;

            var intersects = FindLineSphereIntersections(_spineStartWorld, _spineEndWorld,
                p, c.CameraTargetOffset);

            // camera stays on spine trying to maintain CameraTargetOffset from character
            // if character is too far from spine, stay on spine at closest point
            // if character is close enough that we intersect, find the point that
            // is closest to the current desired camera location
            Vector3 np;
            switch (intersects.Length)
            {
                default:
                case 0:
                    DistanceToLineSegmentSquared(p, _spineStartWorld, _spineEndWorld, out np);
                    break;
                case 1:
                    np = intersects[0];
                    break;
                case 2:
                    var p1 = intersects[0];
                    var p2 = intersects[1];

                    var d1 = (cp - p1).sqrMagnitude;
                    var d2 = (cp - p2).sqrMagnitude;

                    np = d1 < d2 ? p1 : p2;
                    break;
            }

            return np;
        }
    }
}
