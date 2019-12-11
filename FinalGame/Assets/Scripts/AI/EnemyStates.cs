using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace IsThisDarkSouls
{
    public class EnemyStates : MonoBehaviour
    {
        public float health = 100;
        public float stamina = 100;
        public bool isInvulnerable;
        public Animator charAnim;
        AnimatorHook animHook;
        public bool canMove;
        [HideInInspector] public float delta;
        public Rigidbody rigidBody;
        public NavMeshAgent agent;
        private float attackRange = 1.5f;
        private float aggroRange = 10;
        private float speed = 4;
        private GameObject player;
        public bool isTrainingDummy;
        public WeaponHook weaponHook;
        private float actionDelay;
        public bool inAction;
        public float actionLockoutDuration;

        private enum State { Idle, Pursuing, Attacking }
        private State currentState;   
        
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

            if (animHook == false)
            {
                //print("Added AnimatorHook.cs to " + gameObject.name);
                //charAnim.gameObject.AddComponent<AnimatorHook>();
            }
        }

        private void Start()
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

        public void Update()
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

            if (isInvulnerable) // If the NPC is currently invulnerable...
            {
                isInvulnerable = !canMove; // Assign invulnerable to the opposite state of 'canMove' - meaning when the character is capable of moving again, they are no longer invulnerable.
            }

            if (!canMove) // If the character can't move...
            {
                //charAnim.applyRootMotion = false; // Toggle root motion
            }
        }

        /// <summary>
        /// What occurs every tick, runs inside Update
        /// </summary>
        public void Tick(float deltaTime)
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
                    // Maybe regen health later?
                    if (health < 100)
                    {
                        health += 0.1f;
                    }
                    break;
                case State.Attacking:
                    // Attack logic here TBD
                    transform.LookAt(player.transform.position);
                    charAnim.Play("attack");
                    break;
                case State.Pursuing:
                    rigidBody.velocity = Vector3.zero;
                    transform.LookAt(player.transform.position);
                    if (agent.enabled)
                    {
                        agent.SetDestination(player.transform.position);
                    }
                    break;
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
    }    
}
