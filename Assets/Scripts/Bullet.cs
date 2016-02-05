using UnityEngine;
using System.Collections;

namespace Assets.Scripts
{
    public class Bullet : MonoBehaviour
    {
        private GameObject _parentGameObject;

        public void SetParent(GameObject parent)
        {
            _parentGameObject = parent;
            Physics.IgnoreCollision(_parentGameObject.GetComponent<Collider>(), GetComponent<Collider>());
        }

        private void OnCollisionEnter(Collision collision)
        {
            var hit = collision.gameObject;

            if (hit == _parentGameObject)
            {
                return;
            }

            var hitCombat = hit.GetComponent<Combat>();

            if (hitCombat == null)
            {
                return;
            }
            
            hitCombat.TakeDamage(10);

            Destroy(gameObject);
        }
    }
}