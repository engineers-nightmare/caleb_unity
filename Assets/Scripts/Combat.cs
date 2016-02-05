using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Assets.Scripts
{
    public class Combat : NetworkBehaviour
    {
        public const int MaxHealth = 100;

        [SyncVar]
        public int Health = MaxHealth;

        public bool DestroyOnDeath;

        [ClientRpc]
        public void RpcRespawn()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            transform.position = Vector3.zero;
        }

        public void TakeDamage(int amount)
        {
            if (!isServer)
            {
                return;
            }

            Health -= amount;

            if (Health <= 0)
            {
                Health = 0;

                if (DestroyOnDeath)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Health = MaxHealth;
                    RpcRespawn();
                }
            }
        }
    }
}
