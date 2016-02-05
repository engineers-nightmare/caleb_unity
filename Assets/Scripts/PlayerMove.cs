using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Utility;

namespace Assets.Scripts
{
    public class PlayerMove : NetworkBehaviour
    {
        public GameObject BulletPrefab;

        public float LinearPower = 100;
        public float RotationPower = 0.0001f;
        public float RollPower = 1;

        private Vector3 _linearInput = Vector3.zero;

        public override void OnStartLocalPlayer()
        {
            gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

            Camera.main.enabled = false;
            var cam = GetComponentInChildren<Camera>();
            cam.enabled = true;

            var ni = gameObject.GetComponent<NetworkIdentity>();
            gameObject.name = "Player " + ni.netId;
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            var lx = Input.GetAxis("Lateral");
            var ly = Input.GetAxis("Normal");
            var lz = Input.GetAxis("Longitudinal");

            _linearInput.x = lx;
            _linearInput.y = ly;
            _linearInput.z = lz;
        }
        
        // Update is called once per frame
        private void FixedUpdate()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            
            var t = LinearPower * Time.fixedDeltaTime;

            var thrust = _linearInput * t;
            
            var rb = gameObject.GetComponent<Rigidbody>();

            rb.AddRelativeForce(thrust);

            if (Input.GetMouseButtonDown(0))
            {
                CmdFire(gameObject, rb.velocity + (transform.rotation * Vector3.forward * 6));
            }
        }
        
        [Command]
        private void CmdFire(GameObject go, Vector3 velocity)
        {
            // create the bullet object from the bullet prefab
            var bullet = (GameObject)Instantiate(
                BulletPrefab,
                transform.position,
                Quaternion.identity);

            // make the bullet move away in front of the player
            bullet.GetComponent<Rigidbody>().velocity = velocity;
            bullet.GetComponent<Bullet>().SetParent(go);

            NetworkServer.Spawn(bullet);
            
            Destroy(bullet, 15.0f);
        }
    }
}
