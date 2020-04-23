using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class DamageCollider : MonoBehaviour
    {
        public int damage = 10;
        private int critDamage = 0;
        private float critChance = 0;

        private void Awake()
        {
            if (GetComponentInParent<StateManager>())
            {
                damage = GetComponentInParent<StateManager>().damage;
            }
            else if (GetComponentInParent<Enemy>())
            {
                damage = GetComponentInParent<Enemy>().damage;
                critDamage = GetComponentInParent<Enemy>().critDamage;
                critChance = GetComponentInParent<Enemy>().critChance;
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
                int crit = Random.Range(0, 101);

                if (crit <= critChance)
                {
                    Debug.Log("Crit!");
                    states.TakeDamage(critDamage, GetComponentInParent<Enemy>().gameObject.transform);
                    GetComponentInParent<WeaponHook>().CloseDamageCollider();
                }
                else
                {
                    states.TakeDamage(damage, GetComponentInParent<Enemy>().gameObject.transform);
                    GetComponentInParent<WeaponHook>().CloseDamageCollider();
                }
            }
        }
    }
}
