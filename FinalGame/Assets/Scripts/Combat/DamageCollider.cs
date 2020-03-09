using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class DamageCollider : MonoBehaviour
    {
        public int damage = 10;

        private void Awake()
        {
            if (GetComponentInParent<StateManager>())
            {
                damage = GetComponentInParent<StateManager>().damage;
            }
            else if (GetComponentInParent<Enemy>())
            {
                damage = GetComponentInParent<Enemy>().damage;
            }
        }

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
                    enemyStates.TakeDamage(damage); // Needs to be changed to a variable instead of hard coded value
                }
            }

            if (states != null) // If we hit the player...
            {
                states.TakeDamage(damage, GetComponentInParent<Enemy>().gameObject.transform);
                GetComponentInParent<WeaponHook>().CloseDamageCollider();
            }
        }
    }
}
