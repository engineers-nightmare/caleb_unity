using UnityEngine;

namespace Assets.Scripts
{
    public class CameraController : MonoBehaviour
    {
        public GameObject Target = null;
        public float CameraRotateBlendFactor = 0.1f;
        public float CameraPositionSmoothTime = 2f;
        
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
            CameraRotateBlendFactor = 0.1f;
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, CameraRotateBlendFactor);

            CameraPositionSmoothTime = 2;
            transform.position = Vector3.SmoothDamp(transform.position, _desired, ref _positionVelocity, CameraPositionSmoothTime);
        }

        public void SetDesiredPosition(Vector3 desired)
        {
            _desired = desired;
        }
    }
}