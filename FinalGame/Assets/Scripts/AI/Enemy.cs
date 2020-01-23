using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZaldensGambit
{
    public abstract class Enemy : MonoBehaviour
    {
        private enum Type { Famine, War, Death, Pestilence } // Famine = Lifeleech on hit, War = More damage, Death = Delayed explosian on death, Pestilence = DoT effect
        [SerializeField] private Type enemyType;
        [SerializeField] protected float health = 100;
        [HideInInspector] public bool isInvulnerable;
        [HideInInspector] public Animator charAnim;
        private AnimatorHook animHook;
        [HideInInspector] public bool canMove;
        [HideInInspector] public float delta;
        [HideInInspector] public Rigidbody rigidBody;
        [HideInInspector] public NavMeshAgent agent;
        [SerializeField] protected float attackRange = 1.5f;
        [SerializeField] protected float aggroRange = 10;
        [SerializeField] protected float speed = 4;
        [SerializeField] protected float rotationSpeed = 2;
        protected GameObject player;
        protected bool isTrainingDummy;
        [HideInInspector] public WeaponHook weaponHook;
        private float actionDelay;
        [HideInInspector] public bool inAction;
        [HideInInspector] public float actionLockoutDuration;
        [SerializeField] private Transform[] patrolPoints;
        private int currentPatrolPoint;
        [SerializeField] private LayerMask playerLayer;

        protected enum State { Idle, Pursuing, Attacking }
        protected State currentState;

        public bool isDead;

        private void Awake()
        {
            player = FindObjectOfType<StateManager>().gameObject;
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.angularDrag = 999;
            rigidBody.drag = 4;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            charAnim = GetComponentInChildren<Animator>();
            animHook = GetComponentInChildren<AnimatorHook>();
            agent = GetComponent<NavMeshAgent>();
            agent.speed = speed;
            agent.stoppingDistance = attackRange;
            weaponHook = GetComponentInChildren<WeaponHook>();
            weaponHook.CloseDamageCollider();

            //if (animHook == false)
            //{
            //    print("Added AnimatorHook.cs to " + gameObject.name);
            //    charAnim.gameObject.AddComponent<AnimatorHook>();
            //}
        }
        
        protected virtual void Start()
        {
            animHook.Initialise(null, this);
        }

        /// <summary>
        /// Deals damage to the NPCs health and plays hurt animations + applies root motion
        /// </summary>
        public void TakeDamage(float value)
        {
            if (isInvulnerable) // If flagged as invulnerable due to taking damage recently...
            {
                return; // Return out of method, take no damage
            }

            health -= value;
            charAnim.Play("hurt");
            charAnim.applyRootMotion = true;
        }

        protected virtual void Update()
        {
            Tick(Time.deltaTime);
            canMove = charAnim.GetBool("canMove");            

            if (health <= 0)
            {
                if (!isDead)
                {
                    isDead = true;
                    GetComponent<Collider>().enabled = false;
                    rigidBody.isKinematic = true;
                    print(gameObject.name + " died!");
                    charAnim.Play("death");
                    agent.enabled = false;
                    Destroy(gameObject, 5);
                }
            }

            //if (!canMove) // If the character can't move...
            //{
            //    charAnim.applyRootMotion = false; // Toggle root motion
            //}
        }

        /// <summary>
        /// What occurs every tick, runs inside Update
        /// </summary>
        protected virtual void Tick(float deltaTime)
        {
            delta = deltaTime;

            charAnim.SetFloat("speed", Mathf.Abs(agent.velocity.x) + Mathf.Abs(agent.velocity.z)); // Update animiator with current speed and velocity of the character to understand when to change animation movement states

            if (!isDead)
            {
                currentState = UpdateState(); // Determine state behaviour

                if (!isTrainingDummy) // If not flagged as a training dummy
                {
                    PerformStateBehaviour(); // Perform current state behaviour
                }

                if (inAction) // If an animation is playing...
                {
                    agent.enabled = false;
                    charAnim.applyRootMotion = true;
                    actionDelay += delta;

                    if (actionDelay > actionLockoutDuration) // After the duration that the animation locks the character out of performing other actions has passed...
                    {
                        agent.enabled = true;
                        inAction = false; // Flag animation no longer playing
                        actionDelay = 0; // Reset delay
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        protected virtual void PerformStateBehaviour()
        {
            switch (currentState)
            {
                case State.Idle:
                    // Maybe regen health after a delay?
                    Patrol();
                    break;
                case State.Attacking:
                    if (!isInvulnerable && !inAction)
                    {
                        charAnim.Play("attack");
                        RotateTowardsTarget(player.transform);
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

        protected void MoveToTarget()
        {
            if (agent.enabled)
            {
                agent.SetDestination(player.transform.position);
            }
        }

        protected void Patrol()
        {
            if (patrolPoints.Length > 0) // If there are any patrol points allocated...
            {
                if (!inAction) // If not performing an action...
                {
                    agent.SetDestination(patrolPoints[currentPatrolPoint].position); // Move to patrol point
                    RotateTowardsTarget(patrolPoints[currentPatrolPoint]); // Rotate to face patrol point

                    if (Vector3.Distance(transform.position, patrolPoints[currentPatrolPoint].position) < 2) // If close to destination...
                    {
                        if (currentPatrolPoint + 1 < patrolPoints.Length) // If we are not at the end of the patrol path...
                        {
                            currentPatrolPoint++; // Look to next point
                        }
                        else // If we are at the end of the patrol path...
                        {
                            currentPatrolPoint = 0; // Reset
                        }
                    }
                }                
            }
        }

        /// <summary>
        /// Rotates the character towards another transform smoothly, based on character rotation speed
        /// </summary>
        protected void RotateTowardsTarget(Transform target)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up); // Calculate the rotation desired
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed); // Apply rotation
        }

        /// <summary>
        /// The logic behind the characters combat decisions and behaviour, intended to be overridden by derived classes
        /// </summary>
        protected virtual void CombatBehaviour()
        {
            rigidBody.velocity = Vector3.zero; // Reset velocity to ensure no gliding behaviour as navmesh agents do not follow ordinary rigidbody physics
            RotateTowardsTarget(player.transform);

            if (agent.enabled) // If the navmesh agent is active (Not performing an action...)
            {
                agent.SetDestination(player.transform.position); // Move towards target
            }

        }

        /// <summary>
        /// Determines the state of the character based attack & aggro ranges
        /// </summary>
        protected virtual State UpdateState()
        {
            bool isInAttackRange = Vector3.Distance(transform.position, player.transform.position) < attackRange;

            if (isInAttackRange)
            {
                return State.Attacking;
            }

            bool isInAggroRange = Vector3.Distance(transform.position, player.transform.position) < aggroRange;

            if (isInAggroRange)
            {
                return State.Pursuing;
            }

            return State.Idle;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw a yellow wire sphere into the scene editor viewport to match aggro range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, aggroRange);

            // Draw a red wire sphere into the scene editor viewport to match attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, attackRange);
        }
    }        
}
