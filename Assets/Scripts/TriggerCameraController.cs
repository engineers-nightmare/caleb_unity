using UnityEngine;

namespace Assets.Scripts
{
    public class TriggerCameraController : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnTriggerEnter(Collider other)
        {
            
        }

        void OnTriggerStay(Collider other)
        {
            var c = Camera.main.GetComponent<CameraController>();
            c.SetDesiredPosition(transform.position);
        }

        void OnTriggerExit(Collider other)
        {
            
        }
    }
}
