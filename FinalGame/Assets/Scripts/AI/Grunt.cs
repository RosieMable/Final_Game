using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class Grunt : Enemy
    {
        private enum Type { Fire, Poison, Leech, Other }
        [SerializeField] private Type enemyType;

        private enum CombatPattern { Charge, HitAndRun }
        private CombatPattern combatPattern;

        protected override void Start()
        {
            base.Start();
            combatPattern = (CombatPattern)Random.Range(0, 2);
        }

        protected override void CombatBehaviour()
        {
            switch (combatPattern)
            {
                case CombatPattern.Charge:

                    Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up); // Calculate the rotation desired
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

                    if (agent.enabled)
                    {
                        agent.SetDestination(player.transform.position);
                    }
                    break;

                case CombatPattern.HitAndRun:

                    break;

                default:
                    base.CombatBehaviour();
                    break;
            }
        }
    }
}