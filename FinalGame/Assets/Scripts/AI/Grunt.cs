using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZaldensGambit
{
    public class Grunt : Enemy
    {
        private enum CombatPattern { Charge }
        [SerializeField] private CombatPattern combatPattern;
        private Vector3 retreatPosition;
        [SerializeField] AnimationClip[] attackAnimations;
        private float slotMovementTimer;
        private float slotMovementCooldown = 1.5f;    

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
                        if (Time.time > slotMovementTimer)
                        {
                            slotMovementTimer = Time.time + slotMovementCooldown + Random.Range(0.1f, 1.5f);

                            if (agent.enabled)
                            {
                                currentSlot = slotManager.ReserveSlot(this, currentSlot, true);

                                if (currentSlot == -1)
                                {
                                    return;
                                }

                                NavMeshPath path = new NavMeshPath();
                                agent.CalculatePath(slotManager.GetSlotPosition(currentSlot), path);

                                if (path.status == NavMeshPathStatus.PathPartial)
                                {
                                    print("Path out of bounds");
                                    RemoveFromAttackersList();
                                    return;
                                }
                                if (path.status == NavMeshPathStatus.PathInvalid)
                                {
                                    print("Path invalid");
                                    RemoveFromAttackersList();
                                    return;
                                }
                                if (path.status == NavMeshPathStatus.PathComplete)
                                {
                                   // print("Path complete");
                                }
                                agent.destination = slotManager.GetSlotPosition(currentSlot);
                            }
                        }
                        else if (Vector3.Distance(transform.position, slotManager.GetSlotPosition(currentSlot)) > 3f)
                        {
                            currentSlot = slotManager.ReserveSlot(this, currentSlot, false);
                        }
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
                    withinRangeOfTarget = false;
                    RemoveFromAttackersList();                    
                    break;
                case State.Attacking:
                    if (!isInvulnerable && !inAction && Time.time > attackDelay)
                    {
                        agent.isStopped = true;
                        bool playerInFront = Physics.Raycast(transform.position, transform.forward, 2, playerLayer);

                        if (playerInFront)
                        {
                            attackDelay = Time.time + attackCooldown;
                            int animationToPlay = Random.Range(0, attackAnimations.Length);
                            charAnim.CrossFade(attackAnimations[animationToPlay].name, 0.2f);
                            RotateTowardsTarget(player.transform);
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
                        CombatBehaviour();
                    }
                    break;
            }
        }
    }
}