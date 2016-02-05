using UnityEngine;

namespace Assets.Scripts
{
    public class CameraRotator : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            var rb = GetComponent<Rigidbody>();
            rb.AddTorque(0, 5, 0);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
