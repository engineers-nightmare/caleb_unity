using UnityEngine;

namespace Assets.Scripts
{
    [ExecuteInEditMode]
    public class Wire : MonoBehaviour
    {
        public GameObject AttachA;
        public GameObject AttachB;

        private bool _needsUpdate = true;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var editMode = Application.isEditor && !Application.isPlaying;
            if ((editMode || _needsUpdate) && AttachA != null && AttachB != null)
            {
                var p1 = AttachA.transform.position;
                var p2 = AttachB.transform.position;

                var v = p2 - p1;
                var dist = new Vector3(1, 1, v.magnitude);
                transform.position = p2 - v / 2;
                transform.localScale = dist / 2;
                transform.LookAt(p2);
            }
        }

        void SetNeedsUpdate()
        {
            _needsUpdate = true;
        }
    }
}
