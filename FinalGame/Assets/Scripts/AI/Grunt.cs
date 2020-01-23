using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class Grunt : Enemy
    {
        private enum CombatPattern { Charge, HitAndRun }
        private CombatPattern combatPattern;
        [SerializeField] private bool attackMade;
        [SerializeField] private bool movingToRetreatPosition;
        public Vector3 retreatPosition;

        // Hit and run Notes
        // On detection, run up to hit
        // After hit, run away
        // After reaching run away location, repeat
        // No timers, only bools to check when hit and when location reached

        protected override void Start()
        {
            base.Start();
            combatPattern = (CombatPattern)Random.Range(0, 2);
            Debug.Log(combatPattern);
        }

        protected override void Update()
        {
            base.Update();

            if (movingToRetreatPosition)
            {
                if (Vector3.Distance(transform.position, retreatPosition) < 2)
                {
                    movingToRetreatPosition = false;
                    attackMade = false;
                    print("Attack Not Made!");
                    print("Position not found!");
                }
            }
        }

        protected override void CombatBehaviour()
        {
            Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up); // Calculate the rotation desired
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

            switch (combatPattern)
            {
                case CombatPattern.Charge:
                    MoveToTarget();
                    break;
                case CombatPattern.HitAndRun:
                    if (!attackMade)
                    {
                        MoveToTarget();
                    }
                    else if(!movingToRetreatPosition)
                    {
                        retreatPosition = (Random.insideUnitSphere + transform.position);
                        retreatPosition.x += Random.Range(-5, 6);
                        retreatPosition.y = 0;
                        retreatPosition.z += Random.Range(-5, 6);
                        movingToRetreatPosition = true;
                        agent.SetDestination(retreatPosition);
                        print("Position found!");
                    }
                    break;
            }
        }

        protected override void PerformStateBehaviour()
        {
            switch (currentState)
            {
                case State.Idle:
                    // Maybe regen health after a delay?
                    Patrol();
                    movingToRetreatPosition = false;
                    attackMade = false;
                    break;
                case State.Attacking:
                    if (!isInvulnerable && !inAction)
                    {
                        charAnim.Play("attack");
                        RotateTowardsTarget(player.transform);
                        attackMade = true;
                        movingToRetreatPosition = false;
                        print("Attack Made!");
                    }
                    else
                    {
                        RotateTowardsTarget(player.transform);
                    }
                    break;
                case State.Pursuing:
                    if (!isInvulnerable && !inAction)
                    {
                        CombatBehaviour();
                    }
                    break;
            }
        }

        protected override State UpdateState()
        {
            if (combatPattern == CombatPattern.HitAndRun)
            {
                if (attackMade)
                {
                    return State.Pursuing;
                }
            }
            return base.UpdateState();
        }
    }
}