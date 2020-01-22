using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZaldensGambit
{
    public abstract class Enemy : MonoBehaviour
    {
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
            if (isInvulnerable)
            {
                return;
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

            charAnim.SetFloat("speed", Mathf.Abs(agent.velocity.x) + Mathf.Abs(agent.velocity.z));

            if (!isDead)
            {
                currentState = UpdateState();

                if (!isTrainingDummy)
                {
                    PerformStateBehaviour();
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
                        actionDelay = 0; // Reset
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private void PerformStateBehaviour()
        {
            switch (currentState)
            {
                case State.Idle:
                    // Maybe regen health?
                    // Patrol behaviour TBD
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

        private void Patrol()
        {
            if (patrolPoints.Length > 0)
            {
                if (!inAction)
                {
                    agent.SetDestination(patrolPoints[currentPatrolPoint].position);
                    RotateTowardsTarget(patrolPoints[currentPatrolPoint]);

                    if (Vector3.Distance(transform.position, patrolPoints[currentPatrolPoint].position) < 2)
                    {
                        if (currentPatrolPoint + 1 < patrolPoints.Length)
                        {
                            currentPatrolPoint++;
                        }
                        else
                        {
                            currentPatrolPoint = 0;
                        }
                    }
                }                
            }
        }

        private void RotateTowardsTarget(Transform target)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up); // Calculate the rotation desired
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
        }

        protected virtual void CombatBehaviour()
        {
            rigidBody.velocity = Vector3.zero;
            Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up); // Calculate the rotation desired
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

            if (agent.enabled)
            {
                agent.SetDestination(player.transform.position);
            }

        }

        private State UpdateState()
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
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, aggroRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, attackRange);
        }
    }        
}
