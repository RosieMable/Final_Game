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

            if (enemyStates == null)
            {
                return;
            }

            enemyStates.TakeDamage(100);

        }
    }
}
