using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class Grunt : Enemy
    {
        private enum CombatPattern { Charge, HitAndRun }
        private CombatPattern combatPattern;
        private Coroutine hitAndRunCoroutine;

        protected override void Start()
        {
            base.Start();
            combatPattern = (CombatPattern)Random.Range(0, 2);
            Debug.Log(combatPattern);
        }

        protected override void CombatBehaviour()
        {
            Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up); // Calculate the rotation desired
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);



            switch (combatPattern)
            {
                case CombatPattern.Charge:
                    if (agent.enabled)
                    {
                        agent.SetDestination(player.transform.position);
                    }
                    break;

                case CombatPattern.HitAndRun:
                    if (hitAndRunCoroutine == null)
                    {
                        hitAndRunCoroutine = StartCoroutine(HitAndRun(3));
                    }
                    break;

                default:
                    base.CombatBehaviour();
                    break;
            }
        }

        private IEnumerator HitAndRun(float delay)
        {
            if (agent.enabled)
            {
                agent.SetDestination(player.transform.position);
            }
            yield return new WaitForSeconds(delay);
            if (agent.enabled)
            {
                agent.SetDestination(Random.insideUnitSphere + transform.position * 5);
            }
            yield return new WaitForSeconds(delay);
            hitAndRunCoroutine = null;

        }
    }
}