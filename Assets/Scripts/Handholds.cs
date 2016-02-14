using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Handhold
    {
        public Vector3 p;       /* position */
        public Vector3 n;       /* normal */
        public GameObject obj;  /* object to which this is attached */
        public bool isBlocked;  /* is this currently blocked by a nonplayer? */

        public Handhold(Vector3 p, Vector3 n, GameObject obj)
        {
            this.p = p;
            this.n = n;
            this.obj = obj;
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = Vector3.one * 0.05f;
            sphere.transform.position = p;
            GameObject.Destroy(sphere.GetComponent<SphereCollider>());
        }
    }

    public class Handholds : MonoBehaviour {

        public List<Handhold> handholds;
        public float DebugNormalLength = 0.5f;
        public float BlockerSize = 0.5f;
        public bool DebugDrawNormal = false;
        public int NumSubdivisions = 24;
        public int SubdivisionsBetweenRuns = 4;

        // Use this for initialization
        void Start()
        {
            handholds = new List<Handhold>();

            /* for now, hardcoded handhold generation:
            three rings around the cylinder */

            for (var i = 0; i < NumSubdivisions; i++)
            {
                var angle = i * Mathf.PI / (NumSubdivisions/2);
                var disp = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            
                if (i % SubdivisionsBetweenRuns == 0)
                {
                    for (var j = 0; j < 9; j++)
                    {
                        handholds.Add(new Handhold(
                            transform.TransformPoint(0.5f * new Vector3(disp.x, 0.25f * 1.7f * j - 1.7f, disp.z)),
                            transform.TransformDirection(disp),
                            gameObject));
                    }
                }
                else
                {
                    for (var j = -1; j < 2; j++)
                    {
                        handholds.Add(new Handhold(
                            transform.TransformPoint(0.5f * new Vector3(disp.x, j * 1.7f, disp.z)),
                            transform.TransformDirection(disp),
                            gameObject));
                    }
                }
            }
        }
    
        // Update is called once per frame
        void Update () {
            if (DebugDrawNormal)
            {
                foreach (var h in handholds)
                {
                    /* update blocked state */
                    h.isBlocked = Physics.CheckSphere(h.p + h.n, BlockerSize);
                    Debug.DrawLine(h.p, h.p + h.n * DebugNormalLength, h.isBlocked ? Color.red : Color.green);
                }
            }
        }
    }
}