using UnityEngine;

namespace Assets.Scripts
{
    [ExecuteInEditMode]
    public class Toggleable : MonoBehaviour
    {
        public GameObject Target;
        public ToggleState State = ToggleState.On;

        public enum ToggleState
        {
            On,
            Off,
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Target)
            {
                Target.SetActive(State == ToggleState.On);
            }
        }

        public void Toggle()
        {
            State = State == ToggleState.On ? ToggleState.Off : ToggleState.On;
        }

        public void ToggleOn()
        {
            State = ToggleState.On;
        }

        public void ToggleOff()
        {
            State = ToggleState.Off;
        }
    }
}
