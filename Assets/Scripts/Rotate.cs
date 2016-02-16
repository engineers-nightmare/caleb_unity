using UnityEngine;

namespace Assets.Scripts
{
    public class Rotate : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            GetComponent<Rigidbody>().AddTorque(new Vector3(0.1f, 0.3f, 0));
        }

        // Update is called once per frame
        void Update()
        {
            GetComponent<Rigidbody>().WakeUp();
        }
    }
}
