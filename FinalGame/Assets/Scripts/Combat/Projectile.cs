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
        private bool hitShield;

        void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.velocity = forwardVector * projectileSpeed;
            Destroy(gameObject, 5);
        }

        private void OnCollisionEnter(Collision collision)
        {
            Enemy enemyStates = collision.transform.GetComponentInParent<Enemy>();
            StateManager states = collision.transform.GetComponentInParent<StateManager>();

            if (!hitShield) // If we did NOT hit a shield...
            {
                if (states != null) // If we hit the player...
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
            }

            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject, 1);
        }

        private void OnTriggerEnter(Collider other)
        {
            hitShield = true;           
        }
    }
}
