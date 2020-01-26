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
        [SerializeField] private float minimumDistance = 8;

        protected override void PerformStateBehaviour()
        {
            switch (currentState)
            {
                case State.Idle:
                    Patrol();
                    break;
                case State.Pursuing:
                    CombatBehaviour();
                    break;
                case State.Attacking:
                    if (Vector3.Distance(transform.position, player.transform.position) < minimumDistance)
                    {
                        // Run away to safe distance
                        agent.isStopped = false;
                        agent.SetDestination(transform.position - transform.forward * 3);
                        RotateTowardsTarget(transform.position - transform.forward * 3);
                        print("Retreat");
                    }
                    else
                    {
                        agent.isStopped = true;
                        RotateTowardsTarget(player.transform);

                        if (timeToFire < Time.time)
                        {
                            GameObject _projectile = Instantiate(projectile, transform.position + transform.forward + new Vector3(0, 1, 0), Quaternion.identity, null);
                            _projectile.GetComponent<Projectile>().forwardVector = transform.forward;
                            timeToFire = Time.time + fireCooldown;
                        }
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
                MoveToTarget();
                print("Move");
            }
        }
    }
}


