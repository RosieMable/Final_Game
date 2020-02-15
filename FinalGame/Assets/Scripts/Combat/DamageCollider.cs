using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class DamageCollider : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Enemy enemyStates = other.transform.GetComponentInParent<Enemy>();
            StateManager states = other.transform.GetComponentInParent<StateManager>();

            if (enemyStates == null && states == null) // If target hit is not a damageable character...
            {
                return;
            }

            if (GetComponentInParent<StateManager>()) // If we (the weapon) are held by the player...
            {
                if (enemyStates != null) // If we hit an enemy...
                {
                    enemyStates.TakeDamage(10); // Needs to be changed to a variable instead of hard coded value
                }
            }

            if (states != null) // If we hit the player...
            {
                states.TakeDamage(10, GetComponentInParent<Enemy>().gameObject.transform);
            }
        }
    }
}
