using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IsThisDarkSouls
{
    public class DamageCollider : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            EnemyStates enemyStates = other.transform.GetComponentInParent<EnemyStates>(); // Finds the parent of the object hit and searches for an 'EnemyStates' reference.
            StateManager states = other.transform.GetComponentInParent<StateManager>();

            if (enemyStates == null && states == null)
            {
                return;
            }

            if (enemyStates != null)
            {
                enemyStates.TakeDamage(10); // Needs to be changed to a variable instead of hard coded value
            }

            if (states != null)
            {
                states.TakeDamage(10, transform.root);
            }

        }
    }
}
