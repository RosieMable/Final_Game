using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZaldensGambit
{
    public class RangedEnemy : Enemy
    {
        [SerializeField] private GameObject projectile;
        [SerializeField] private bool stationary;
        private bool movingToPosition;
        private Vector3 desiredPosition;

        protected override void Start()
        {
            base.Start();
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
                    if (!movingToPosition)
                    {
                        agent.isStopped = true;
                        RotateTowardsTarget(player.transform);
                    }

                    bool playerInFront = Physics.Raycast(transform.position + Vector3.up, transform.forward, Mathf.Infinity, playerLayer);
                    Debug.DrawRay(transform.position + Vector3.up, transform.forward * 100);
                    if (playerInFront)
                    {
                        if (attackDelay < Time.time && agent.isStopped && !inAction & !isInvulnerable)
                        {
                            GameObject _projectile = Instantiate(projectile, transform.position + transform.forward + new Vector3(0, 1, 0), Quaternion.identity, null);
                            _projectile.GetComponent<Projectile>().forwardVector = transform.forward;
                            attackDelay = Time.time + attackCooldown - 0.5f + Random.Range(0, 1f);
                            MoveToNewPosition();
                        }
                    }
                    else
                    {
                        RotateTowardsTarget(player.transform);
                    }

                    if (movingToPosition)
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

        private void MoveToNewPosition()
        {
            while (true)
            {
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
            }
        }

        protected override void CombatBehaviour()
        {
            rigidBody.velocity = Vector3.zero; // Reset velocity to ensure no gliding behaviour as navmesh agents do not follow ordinary rigidbody physics
            RotateTowardsTarget(player.transform);

            if (!stationary)
            {
                agent.isStopped = false;
                MoveToTarget();
            }
        }
    }
}


