using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerInput : MonoBehaviour
    {
        // player mass is 75
        [Tooltip("Matching rb mass is a good start")]
        public float MoveForce = 75;
        public float BodyDrag = 0.05f;
        public float LimbDrag = 0.5f;
        public float AngularDrag = 0.05f;

        public Rigidbody Driver { get; private set; }

        void Start()
        {
            var player = transform.root.gameObject;
            var rbs = player.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                rb.useGravity = false;
                rb.drag = LimbDrag;
                AngularDrag = 0.05f;
                rb.angularDrag = AngularDrag;
                rb.isKinematic = false;

                if (rb.name == "EthanSpine2")
                {
                    Driver = rb;
                    rb.drag = BodyDrag;
                }
            }
        }

        // We're applying physics updates, so use FixedUpdate
        void FixedUpdate()
        {
            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Normal");
            var z = Input.GetAxis("Vertical");

            var right = Camera.main.transform.right * x;
            var up = Camera.main.transform.up * y;
            var forward = Camera.main.transform.forward * z;
            var dir = (right + up + forward) * MoveForce;
            Driver.AddForce(dir);
        }
    }
}