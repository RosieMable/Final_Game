using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class DamageCollider : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Enemy enemyStates = other.transform.GetComponentInParent<Enemy>(); // Finds the parent of the object hit and searches for an 'EnemyStates' reference.
            StateManager states = other.transform.GetComponentInParent<StateManager>();

            if (enemyStates == null && states == null)
            {
                return;
            }

            if (transform.root.GetComponent<StateManager>())
            {
                if (enemyStates != null)
                {
                    enemyStates.TakeDamage(10); // Needs to be changed to a variable instead of hard coded value
                }
            }

            if (states != null)
            {
                states.TakeDamage(10, GetComponentInParent<Enemy>().gameObject.transform);
            }
        }
    }
}
