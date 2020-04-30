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

        private int baseDamage;

        private StateManager playerStateManager;
        private SpiritSystem playerSpiritSystem;
        private void Awake()
        {
            playerStateManager = GetComponentInParent<StateManager>();
            playerSpiritSystem = GetComponentInParent<SpiritSystem>();

            if (playerStateManager)
            {
                damage = playerStateManager.damage;
                baseDamage = damage;
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
                    if (playerSpiritSystem.spiritEquipped != null)
                    {
                        ChooseDamageType(playerSpiritSystem.spiritEquipped, enemyStates);
                    }
                    else
                    {
                        enemyStates.TakeDamage(damage);
                    }
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


        private void ChooseDamageType(BaseSpirit _playerSpirit, Enemy _enemy)
        {
           if (_playerSpirit.abilityEvoked)
                {
                    switch (_playerSpirit.spiritClass)
                    {
                        case BaseSpirit.SpiritClass.Berserker:
                            damage += (int)_playerSpirit.DamageModifier;
                            _enemy.TakeDamage(damage);
                             print("Enemy Damaged by " + damage);
                              damage = baseDamage;
                            _playerSpirit.abilityEvoked = false;
                            return;
                        case BaseSpirit.SpiritClass.Sellsword:
                            damage += (int)_playerSpirit.DamageModifier;
                            _enemy.TakeDamage(damage);
                            playerStateManager.RestoreHealth(_playerSpirit.HealthModifier);
                            print("Enemy Damaged by " + damage);
                            damage = baseDamage;
                            _playerSpirit.abilityEvoked = false;
                            return;
                        default:
                            damage = baseDamage;
                            _enemy.TakeDamage(damage);
                            return;
                    }
                }
            else
            {
                _enemy.TakeDamage(damage);
            }
        }
    }
}