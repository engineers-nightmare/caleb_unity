using UnityEngine;

namespace Assets.Scripts
{
    public class CameraController : MonoBehaviour
    {
        public GameObject Target = null;
        public float CameraRotateBlendFactor = 0.1f;
        public float CameraPositionSmoothTime = 2f;
        public float CameraRotationFactor = 8f;
        public float CameraTargetOffset = 1f;

        private Vector3 _positionVelocity;
        private Rigidbody _targetRigidbody;
        private Vector3 _desired;

        void Start()
        {
            var pm = Target.GetComponentInChildren<PlayerInput>();
            _targetRigidbody = pm.Driver;
        }

        // Update is called once per frame
        void Update()
        {
            var targetPos = _targetRigidbody.transform.position - transform.position;
            var rot = Quaternion.LookRotation(targetPos);
            var r = Quaternion.RotateTowards(transform.rotation, rot, CameraRotationFactor);
            CameraRotateBlendFactor = 0.1f;
            transform.rotation = Quaternion.Lerp(transform.rotation, r, CameraRotateBlendFactor);
            
            transform.position = Vector3.SmoothDamp(transform.position, _desired, ref _positionVelocity, CameraPositionSmoothTime);
        }

        public void SetDesiredPosition(Vector3 desired)
        {
            _desired = desired;
        }

        public Vector3 GetDesiredPosition()
        {
            return _desired;
        }
    }
}