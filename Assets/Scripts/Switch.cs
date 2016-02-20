using UnityEngine;

namespace Assets.Scripts
{
    public class Switch : MonoBehaviour
    {
        public Toggleable Target;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0) && Target)
            {
                Target.Toggle();
            }
        }
    }
}
