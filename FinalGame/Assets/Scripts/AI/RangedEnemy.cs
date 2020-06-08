using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZaldensGambit
{
    public class RangedEnemy : Enemy
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject projectile;
        [SerializeField] private bool stationary;
        private bool movingToPosition;
        private Vector3 desiredPosition;

        protected override void Start()
        {
            base.Start();
            ignoreCombatCircle = true;
            agent.stoppingDistance = 1;
        }

        protected override void PerformStateBehaviour()
        {
            switch (currentState)
            {
                case State.Idle:
                    Patrol();                    
                    break;
                case State.Pursuing:
                    CombatBehaviour();
                    movingToPosition = false;
                    break;
                case State.Attacking:
                    if (!movingToPosition && agent.isActiveAndEnabled)
                    {
                        agent.isStopped = true;
                        RotateTowardsTarget(player.transform);
                    }

                    bool playerInFront = Physics.Raycast(transform.position + Vector3.up, transform.forward, Mathf.Infinity, playerLayer);
                    Debug.DrawRay(transform.position + Vector3.up, transform.forward * 100);
                    if (playerInFront)
                    {
                        if (agent.enabled)
                        {
                            if (attackDelay < Time.time && agent.isStopped && !inAction & !isInvulnerable)
                            {
                                int animationToPlay = Random.Range(0, attackAnimations.Length);
                                charAnim.CrossFade(attackAnimations[animationToPlay].name, 0.2f);
                                StartCoroutine(FireWithDelay(0.5f));
                            }
                        }
                        
                    }
                    else
                    {
                        RotateTowardsTarget(player.transform);
                    }

                    if (movingToPosition && agent.isActiveAndEnabled)
                    {
                        agent.isStopped = false;
                        RotateTowardsTarget(desiredPosition);                        
                        agent.SetDestination(desiredPosition);
                    }

                    if (Vector3.Distance(transform.position, desiredPosition) < 1)
                    {
                        movingToPosition = false;
                    }
                    break;
            }
        }

        /// <summary>
        /// Calculate a position nearby that is within the NavMesh.
        /// </summary>
        private void MoveToNewPosition()
        {
            if (agent.isActiveAndEnabled)
            {
                int loops = 0;
                while (true)
                {
                    loops++;
                    desiredPosition = Random.insideUnitSphere + transform.position;
                    desiredPosition.x += Random.Range(-3, 4);
                    desiredPosition.y = 0;
                    desiredPosition.z += Random.Range(-3, 4);

                    NavMeshPath path = new NavMeshPath();
                    agent.CalculatePath(desiredPosition, path);

                    if (path.status == NavMeshPathStatus.PathPartial)
                    {
                        print("Path out of bounds - Recalculating");
                    }
                    if (path.status == NavMeshPathStatus.PathInvalid)
                    {
                        print("Path invalid - Recalculating");
                    }
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        print("Valid path found - Moving towards");
                        movingToPosition = true;
                        break;
                    }

                    if (loops == 10)
                    {
                        print("Failed to calculate a path too many times, breaking out of loop.");
                        break;
                    }
                }
            }
        }

        protected override void CombatBehaviour()
        {
            rigidBody.velocity = Vector3.zero; // Reset velocity to ensure no gliding behaviour as navmesh agents do not follow ordinary rigidbody physics
            RotateTowardsTarget(player.transform);

            if (!stationary && agent.enabled)
            {
                agent.isStopped = false;
                MoveToTarget();
            }
        }

        private IEnumerator FireWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            GameObject _projectile = Instantiate(projectile, firePoint.position, transform.rotation, null);
            _projectile.GetComponent<Projectile>().forwardVector = transform.forward;
            attackDelay = Time.time + attackCooldown - 0.5f + Random.Range(0, 1f);
            MoveToNewPosition();
        }
    }
}


