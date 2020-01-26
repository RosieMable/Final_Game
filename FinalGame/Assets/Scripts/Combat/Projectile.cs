using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class Projectile : MonoBehaviour
    {
        public float projectileSpeed;
        public float damageValue;
        private Rigidbody rigidBody;
        public Vector3 forwardVector;

        void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.velocity = forwardVector * projectileSpeed;
            Destroy(gameObject, 5);
        }

        private void OnCollisionEnter(Collision collision)
        {
            Enemy enemyStates = collision.transform.GetComponentInParent<Enemy>(); // Finds the parent of the object hit and searches for an 'EnemyStates' reference.
            StateManager states = collision.transform.GetComponentInParent<StateManager>();

            if (states != null)
            {
                RaycastHit hitInfo;
                Physics.Raycast(transform.position, forwardVector, out hitInfo, 0.5f);

                if (hitInfo.collider != null)
                {
                    if (hitInfo.collider.gameObject.name != "Shield")
                    {
                        states.TakeDamage(damageValue, transform);
                    }
                }
            }

            if (enemyStates != null)
            {
                enemyStates.TakeDamage(damageValue);
            }

            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject, 1);
        }
    }
}
