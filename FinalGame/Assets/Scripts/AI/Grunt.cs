using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class Grunt : Enemy
    {
        private enum CombatPattern { Charge, HitAndRun }
        [SerializeField] private CombatPattern combatPattern;
        private float attackDelay;
        [SerializeField] private float attacksCooldown = 0.5f;
        private bool attackMade;
        private bool movingToRetreatPosition;
        private Vector3 retreatPosition;
        [SerializeField] AnimationClip[] attackAnimations;


        protected override void Start()
        {
            base.Start();
            //combatPattern = (CombatPattern)Random.Range(0, 2);
            //Debug.Log(combatPattern);
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
                }
            }
        }

        protected override void CombatBehaviour()
        {
            var slotManager = player.GetComponent<AttackSlotManager>();
            Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up); // Calculate the rotation desired
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

            switch (combatPattern)
            {
                case CombatPattern.Charge:
                    if (movingToAttack)
                    {
                        MoveToTarget();
                    }
                    else
                    {
                        if (agent.enabled)
                        {
                            //Vector3 targetPosition = Random.insideUnitSphere;
                            //targetPosition += player.transform.position;
                            //targetPosition.y = 0;
                            //agent.SetDestination(targetPosition);

                            if (slot == -1)
                            {
                                slot = slotManager.ReserveSlot(this);
                            }

                            if (slot == -1)
                            {
                                return;
                            }
                            agent.destination = slotManager.GetSlotPosition(slot);
                        }
                    }
                    break;
                case CombatPattern.HitAndRun:
                    if (!attackMade)
                    {
                        MoveToTarget();
                    }
                    else if(!movingToRetreatPosition)
                    {
                        retreatPosition = Random.insideUnitSphere + transform.position;
                        retreatPosition.x += Random.Range(-5, 6);
                        retreatPosition.y = 0;
                        retreatPosition.z += Random.Range(-5, 6);
                        movingToRetreatPosition = true;
                        RotateTowardsTarget(retreatPosition);
                        agent.SetDestination(retreatPosition);
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
                    withinRangeOfTarget = false;
                    if (currentAttackers.Contains(this))
                    {
                        //RemoveFromAttackersList();
                    }
                    break;
                case State.Attacking:
                    if (!isInvulnerable && !inAction && Time.time > attackDelay && currentAttackers.Contains(this))
                    {
                        agent.isStopped = true;
                        bool playerInFront = Physics.Raycast(transform.position, transform.forward, 2, playerLayer);

                        if (playerInFront)
                        {
                            attackDelay = Time.time + attacksCooldown;
                            int animationToPlay = Random.Range(0, attackAnimations.Length);
                            charAnim.CrossFade(attackAnimations[animationToPlay].name, 0.2f);
                            RotateTowardsTarget(player.transform);
                            attackMade = true;
                            movingToRetreatPosition = false;
                            Invoke("RemoveFromAttackersList", 1f);
                        }
                        else
                        {
                            RotateTowardsTarget(player.transform);
                        }                        
                    }
                    else
                    {
                        RotateTowardsTarget(player.transform);
                    }
                    break;
                case State.Pursuing:
                    withinRangeOfTarget = true;
                    if (!isInvulnerable && !inAction)
                    {
                        agent.isStopped = false;

                        if (!movingToRetreatPosition)
                        {
                            CombatBehaviour();
                        }
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