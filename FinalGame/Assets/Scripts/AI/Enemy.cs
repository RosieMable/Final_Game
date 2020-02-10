using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

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
        private Slider healthSlider;
        private Coroutine healthCoroutine;
        public bool lockedOnto;
        private TextMeshProUGUI damageText;
        private float damageTextValue;
        private Coroutine damageTextCoroutine;
        [SerializeField] protected float attackRange = 1.5f;
        [SerializeField] protected float aggroRange = 10;
        [SerializeField] protected float speed = 4;
        [SerializeField] protected float rotationSpeed = 2;
        protected GameObject player;
        [SerializeField] protected bool isTrainingDummy;
        [HideInInspector] public WeaponHook weaponHook;
        private float actionDelay;
        [HideInInspector] public bool inAction;
        [HideInInspector] public float actionLockoutDuration;
        [SerializeField] private Transform[] patrolPoints;
        private int currentPatrolPoint;
        [SerializeField] protected LayerMask playerLayer;
        private CameraManager cameraManager;
        [SerializeField] protected AnimationClip[] hurtAnimations;

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
            healthSlider = GetComponentInChildren<Slider>();
            healthSlider.value = health;
            healthSlider.gameObject.SetActive(false);
            damageText = GetComponentInChildren<TextMeshProUGUI>();
            damageText.gameObject.SetActive(false);

            //if (animHook == false)
            //{
            //    print("Added AnimatorHook.cs to " + gameObject.name);
            //    charAnim.gameObject.AddComponent<AnimatorHook>();
            //}
        }
        
        protected virtual void Start()
        {
            animHook.Initialise(null, this);
            cameraManager = CameraManager.instance;
        }

        /// <summary>
        /// Deals damage to the NPCs health and plays hurt animations + applies root motion
        /// </summary>
        public void TakeDamage(float damageValue)
        {
            if (isInvulnerable) // If flagged as invulnerable due to taking damage recently...
            {
                return; // Return out of method, take no damage
            }

            float previousHealth = health;
            health -= damageValue;
            healthSlider.value = health;

            if (damageText.IsActive()) // If already showing damage text...
            {
                damageTextValue += damageValue; // Add onto existing damage text displayed                
            }
            else // If not showing damage text...
            {
                damageTextValue = damageValue; // Show initial damage
            }

            damageText.text = damageTextValue.ToString();

            if (healthCoroutine != null)
            {
                StopCoroutine(healthCoroutine);
                healthCoroutine = StartCoroutine(RevealHealthBar(3));
            }
            else
            {
                healthCoroutine = StartCoroutine(RevealHealthBar(3));
            }

            if (damageTextCoroutine != null)
            {
                StopCoroutine(damageTextCoroutine);
                damageTextCoroutine = StartCoroutine(RevealDamageText(3));
            }
            else
            {
                damageTextCoroutine = StartCoroutine(RevealDamageText(3));
            }

            if (currentAttackers.Contains(this))
            {
                //RemoveFromAttackersList();
            }

            int hurtAnimationToPlay = Random.Range(0, hurtAnimations.Length);
            //charAnim.Play(hurtAnimations[hurtAnimationToPlay].name);
            charAnim.CrossFade(hurtAnimations[hurtAnimationToPlay].name, 0.1f);
            charAnim.applyRootMotion = true;
            charAnim.applyRootMotion = true;
        }

        protected virtual void Update()
        {
            //print(currentAttackers.Count);

            Tick(Time.deltaTime);
            canMove = charAnim.GetBool("canMove");            

            if (health <= 0)
            {
                if (!isDead)
                {
                    isDead = true;
                    RemoveFromAttackersList();
                    healthSlider.gameObject.SetActive(false);
                    damageText.gameObject.SetActive(false);
                    GetComponent<Collider>().enabled = false;
                    rigidBody.isKinematic = true;
                    print(gameObject.name + " died!");
                    charAnim.Play("death");
                    agent.enabled = false;
                    Destroy(gameObject, 5);
                }
            }
            else
            {
                healthSlider.transform.LookAt(new Vector3(cameraManager.transform.position.x, healthSlider.transform.position.y, cameraManager.transform.position.z), Vector3.up);
                //damageText.transform.LookAt(new Vector3(cameraManager.transform.position.x, damageText.transform.position.y, cameraManager.transform.position.z), Vector3.up);
                Quaternion rotationToFace = Quaternion.LookRotation(damageText.transform.position - cameraManager.transform.position);
                damageText.transform.rotation = new Quaternion(transform.rotation.x, rotationToFace.y, transform.rotation.z, rotationToFace.w); 
            }

            //if (!canMove) // If the character can't move...
            //{
            //    charAnim.applyRootMotion = false; // Toggle root motion
            //}
        }

        /// <summary>
        /// What occurs every tick, runs inside Update
        /// </summary>
        protected void Tick(float deltaTime)
        {
            delta = deltaTime;
            charAnim.SetFloat("speed", Mathf.Abs(agent.velocity.x) + Mathf.Abs(agent.velocity.z)); // Update animiator with current speed and velocity of the character to understand when to change animation movement states

            if (!isDead)
            {
                currentState = UpdateState(); // Determine state behaviour

                if (!isTrainingDummy) // If not flagged as a training dummy
                {
                    if (!strafing)
                    {
                        PerformStateBehaviour(); // Perform current state behaviour
                    }
                    //CombatCircleBehaviour();
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
                    if (currentAttackers.Contains(this))
                    {
                        RemoveFromAttackersList();
                    }
                    break;
                case State.Attacking:
                    if (!isInvulnerable && !inAction && currentAttackers.Contains(this))
                    {
                        charAnim.CrossFade("attack", 0.1f);
                        Invoke("RemoveFromAttackersList", 1f);
                        RotateTowardsTarget(player.transform);
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

        protected void RotateTowardsTarget(Vector3 location)
        {
            Quaternion targetRotation = Quaternion.LookRotation(location - transform.position, Vector3.up); // Calculate the rotation desired
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed); // Apply rotation
        }

        /// <summary>
        /// The logic behind the characters combat decisions and behaviour, intended to be overridden by derived classes
        /// </summary>
        protected virtual void CombatBehaviour()
        {
            var slotManager = player.GetComponent<AttackSlotManager>();
            rigidBody.velocity = Vector3.zero; // Reset velocity to ensure no gliding behaviour as navmesh agents do not follow ordinary rigidbody physics
            RotateTowardsTarget(player.transform);

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
        }

        /// <summary>
        /// Determines the state of the character based attack & aggro ranges
        /// </summary>
        protected virtual State UpdateState()
        {
            bool canConsiderAttacking = Vector3.Distance(transform.position, player.transform.position) < aggroRange;

            if (canConsiderAttacking)
            {
                //print(gameObject.name + " has asked for permission to attack");


                if (currentAttackers.Count < maximumNumberOfAttackers)
                {
                    //print(gameObject.name + " permission granted");
                    if (!currentAttackers.Contains(this))                       
                    {
                        currentAttackers.Add(this);
                        movingToAttack = true;
                    }
                }
                else
                {
                    //print(gameObject.name + " permission denied");

                }
            }

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

        public void ShowLockOnHealth()
        {
            healthSlider.gameObject.SetActive(true);
        }

        public void HideLockOnHealth()
        {
            healthSlider.gameObject.SetActive(false);
            damageText.gameObject.SetActive(false);
        }

        private IEnumerator RevealHealthBar(float duration)
        {
            healthSlider.gameObject.SetActive(true);

            yield return new WaitForSeconds(duration);

            if (!lockedOnto)
            {
                healthSlider.gameObject.SetActive(false);
                healthCoroutine = null;
            }
        }

        private IEnumerator RevealDamageText(float duration)
        {
            damageText.gameObject.SetActive(true);

            yield return new WaitForSeconds(duration);

            damageText.gameObject.SetActive(false);
            damageTextCoroutine = null;
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

        // Experimental 'Combat Circle' logic attempt below

        /* Logic:
         * 1) Move towards the player when within aggro range. - Done
         * 2) When within a reasonable range, avoid other AIs unless moving to attack. - Done
         * 3) Move towards the player whilst avoiding other AIs unless moving to attack. - Done
         * 4) When the player is within attack range, check if I'm allowed to attack. - Done
         * 
         * Notes on permission given for attacks:
         * - Cannot attack if there is already a maximum number of allowed attackers - Done
         * - When denied, continue to move around the player and repeat
         * - If the player moves out of attack range, remove from attackers list - Done
         * - If killed, remove from attackers list - Done
         * 
         * Variables Needed:
         * Aggro range - Done
         * Attack range - Done
         * In-range boolean - Done
         * Moving-to-attack boolean - Done
         * Avoid radius - Done
         * List of attackers - Done
         * Maximum number of allowed attackers- Done
         * 
         * Possible States:
         * - Idle - Done
         * - Pursuing - Done
         * - LookingToAttack - 
         * - Attacking - Done
        */

        private float avoidRadius = 3;
        protected bool withinRangeOfTarget;
        protected bool movingToAttack;
        [SerializeField] protected static List<Enemy> currentAttackers = new List<Enemy>();
        protected static int maximumNumberOfAttackers = 1;
        protected bool strafing;
        protected int slot = -1;

        private void CombatCircleBehaviour()
        {
            if (withinRangeOfTarget)
            {
                if (!movingToAttack)
                {
                    //print(gameObject.name + " is searching for AIs to avoid!");
                    Collider[] objectsInRange = Physics.OverlapSphere(transform.position, avoidRadius);

                    foreach (Collider collider in objectsInRange)
                    {
                        if (collider.gameObject.GetComponent<Enemy>() && collider.gameObject.GetComponent<Enemy>() != this && !strafing)
                        {
                            ////Transform startTransform = transform;
                            //Vector3 directionFrom = transform.position - collider.transform.position;
                            //Vector3 positionToMoveTo = directionFrom + directionFrom.normalized;
                            ////transform.rotation = Quaternion.LookRotation(transform.position - collider.transform.position);                        
                            ////Vector3 positionToMoveTo = transform.position + transform.forward * 2;
                            ////transform.rotation = startTransform.rotation;
                            //if (agent.enabled)
                            //{
                            //    agent.SetDestination(positionToMoveTo);
                            //    RotateTowardsTarget(positionToMoveTo);
                            //    avoidingOrStarfing = true;
                            //    Invoke("StopAvoidingAI", 1f);
                            //}
                            //Strafe();
                           // print(gameObject.name + " is attempting to avoid " + collider.gameObject.name);
                            break;
                        }
                    }
                }
            }            
        }
        protected void StopStrafing()
        {
            strafing = false;
        }

        protected void Strafe()
        {
            if (agent.enabled)
            {
                Vector3 targetPosition = transform.position;

                int direction = Random.Range(0, 2);
                switch (direction)
                {
                    case 1:
                        targetPosition = transform.position + Vector3.left;
                        break;
                    case 2:
                        targetPosition = transform.position + Vector3.right;
                        break;
                }

                agent.SetDestination(targetPosition);
                Invoke("StopStrafing", 5f);
            }
        }

        protected void RemoveFromAttackersList()
        {
            currentAttackers.Remove(this);
            movingToAttack = false;
           // print(gameObject.name + " is removed from the attackers list.");
        }
    }        
}
