using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class RangedEnemy : Enemy
    {
        [SerializeField] private GameObject projectile;
        [SerializeField] private bool stationary;
        [SerializeField] private float fireCooldown = 1.5f;
        private float timeToFire;
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
                        if (timeToFire < Time.time && agent.isStopped)
                        {
                            GameObject _projectile = Instantiate(projectile, transform.position + transform.forward + new Vector3(0, 1, 0), Quaternion.identity, null);
                            _projectile.GetComponent<Projectile>().forwardVector = transform.forward;
                            timeToFire = Time.time + fireCooldown;

                            desiredPosition = Random.insideUnitSphere + transform.position;
                            desiredPosition.x += Random.Range(-3, 4);
                            desiredPosition.y = 0;
                            desiredPosition.z += Random.Range(-3, 4);
                            movingToPosition = true;
                        }
                    }
                    else
                    {
                        RotateTowardsTarget(player.transform);
                    }

                    if (movingToPosition)
                    {
                        print("Moving to position");
                        agent.isStopped = false;
                        RotateTowardsTarget(desiredPosition);                        
                        agent.SetDestination(desiredPosition);
                    }

                    if (Vector3.Distance(transform.position, desiredPosition) < 1)
                    {
                        print("Near position");
                        movingToPosition = false;
                    }

                    break;
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
                print("Move");
            }
        }
    }
}


