using UnityEngine;

namespace Assets.Scripts
{
    public class CameraController : MonoBehaviour
    {
        public float OffsetDistance = 6;
        public float MinOffset = 1;
        public float MaxOffset = 10;

        private float _azimuth = 0;
        private float _elevation = 0;
        private float _dampVelocity;
        private float _targetOffset;
        public Rigidbody TargetRigidbody;

        void Start()
        {
            _targetOffset = OffsetDistance;
            //var pm = Target.GetComponentInChildren<PlayerInput>();
            //_targetRigidbody = pm.Driver;
        }

        // Update is called once per frame
        void Update()
        {
            var az = Input.GetAxis("Mouse X");
            var el = Input.GetAxis("Mouse Y");
            var zm = Input.GetAxis("Mouse ScrollWheel") * 10f;

            _targetOffset = Mathf.Clamp(_targetOffset - zm, MinOffset, MaxOffset);

            OffsetDistance = Mathf.SmoothDamp(OffsetDistance, _targetOffset, ref _dampVelocity, 0.3f);

            _azimuth += az;
            _elevation += el;

            var rot = Quaternion.identity * Quaternion.Euler(_elevation, _azimuth, 0);
            var pos = rot * new Vector3(0, 0, -OffsetDistance) + TargetRigidbody.position;
            transform.rotation = rot;
            transform.position = pos;
        }
    }
}