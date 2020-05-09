using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

namespace ZaldensGambit
{
    /// <summary>
    /// Base class for enemy behaviour, contains information on enemy type, health, animation/UI references, base combat logic, etc.
    /// </summary>
    public abstract class Enemy : MonoBehaviour
    {
        public enum Type { Famine, War, Death, Pestilence } // Famine = Lifeleech on hit, War = More damage, Death = Delayed explosian on death, Pestilence = DoT effect
        public Type enemyType;
        [SerializeField] public float currentHealth = 50;
        protected float maximumHealth;
        [HideInInspector] public bool isInvulnerable;
        [HideInInspector] public Animator charAnim;
        private AnimatorHook animHook;
        [HideInInspector] public bool canMove;
        [HideInInspector] public float delta;
        [HideInInspector] public Rigidbody rigidBody;
        [HideInInspector] public NavMeshAgent agent;
        protected Slider healthSlider;
        protected Coroutine healthCoroutine;
        public bool lockedOnto;
        protected TextMeshProUGUI damageText;
        protected float damageTextValue;
        protected Coroutine damageTextCoroutine;
        public float attackDelay;
        public float attackCooldown = 0.5f;
        public int damage = 10;
        public int critDamage = 15;
        public float critChance = 5;
        [SerializeField] protected float attackRange = 1.5f;
        public float aggroRange = 10;
        [SerializeField] protected float speed = 4;
        [SerializeField] protected float rotationSpeed = 2;
        protected bool stunned;
        private Coroutine stunnedCoroutine;

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
        [SerializeField] protected AnimationClip[] attackAnimations;

        protected AudioSource characterAudioSource;
        [SerializeField] protected AudioClip[] deathAudioClips;
        [SerializeField] protected AudioClip[] hurtAudioClips;

        protected bool withinRangeOfTarget;
        protected bool movingToAttack;
        [SerializeField] protected static List<Enemy> currentAttackers = new List<Enemy>();
        protected static int maximumNumberOfAttackers = 2;
        protected int currentSlot = -1;
        protected bool ignoreCombatCircle;

        protected enum State { Idle, Pursuing, Attacking }
        protected State currentState;

        public bool isDead;

        protected virtual void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.angularDrag = 999;
            rigidBody.drag = 4;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            charAnim = GetComponentInChildren<Animator>();
            animHook = GetComponentInChildren<AnimatorHook>();
            agent = GetComponent<NavMeshAgent>();
            agent.speed = speed;
            agent.stoppingDistance = attackRange;
            healthSlider = GetComponentInChildren<Slider>();
            healthSlider.maxValue = currentHealth;
            healthSlider.value = currentHealth;
            healthSlider.gameObject.SetActive(false);
            damageText = GetComponentInChildren<TextMeshProUGUI>();
            damageText.gameObject.SetActive(false);
            characterAudioSource = GetComponent<AudioSource>();
            maximumHealth = currentHealth;

            //if (animHook == false)
            //{
            //    print("Added AnimatorHook.cs to " + gameObject.name);
            //    charAnim.gameObject.AddComponent<AnimatorHook>();
            //}
        }
        
        protected virtual void Start()
        {
            player = FindObjectOfType<StateManager>().gameObject;
            animHook.Initialise(null, this);
            cameraManager = CameraManager.instance;
            currentAttackers.Clear();
        }

        /// <summary>
        /// Reduces the AI's health and plays hurt animations + applies root motion.
        /// </summary>
        public virtual void TakeDamage(float damageValue)
        {
            if (isInvulnerable) // If flagged as invulnerable due to taking damage recently...
            {
                return; // Return out of method, take no damage
            }

            attackDelay = Time.time + attackCooldown; // Add onto the attack delay as we have been hit            
            currentHealth = Mathf.Lerp(currentHealth, currentHealth - damageValue, 1f); // Reduce health by damage value
            healthSlider.value = currentHealth; // Update slider to represent new health total

            if (damageText.IsActive()) // If already showing damage text...
            {
                damageTextValue += damageValue; // Add onto existing damage text displayed                
            }
            else // If not showing damage text...
            {
                damageTextValue = damageValue; // Show initial damage
            }

            damageText.text = damageTextValue.ToString();

            if (healthCoroutine != null) // If the AI health is already visible
            {
                StopCoroutine(healthCoroutine); // Stop coroutine, prevents bar from disappearing before intended
                healthCoroutine = StartCoroutine(RevealHealthBar(3)); // Recall coroutine, ensures bar disappears when intended
            }
            else
            {
                healthCoroutine = StartCoroutine(RevealHealthBar(3)); // Start health coroutine to display AI health
            }

            if (damageTextCoroutine != null) // Same as above but for damage text
            {
                StopCoroutine(damageTextCoroutine);
                damageTextCoroutine = StartCoroutine(RevealDamageText(3));
            }
            else
            {
                damageTextCoroutine = StartCoroutine(RevealDamageText(3));
            }

            if (!ignoreCombatCircle)
            {
                if (!currentAttackers.Contains(this)) // If the AI is not within the list of attackers...
                {
                    currentAttackers[Random.Range(0, currentAttackers.Count - 1)].GetComponent<Enemy>().RemoveFromAttackersList(); // Remove a random AI from the list
                    currentAttackers.Add(this); // Add self to the list
                    movingToAttack = true; // Flag as moving to attack to no longer be bound to the combat circle positions
                }
            }

            int hurtAnimationToPlay = Random.Range(0, hurtAnimations.Length); // Return random animation from list
            charAnim.CrossFade(hurtAnimations[hurtAnimationToPlay].name, 0.1f); // Play animation
            charAnim.applyRootMotion = true;

            int clipToPlay = Random.Range(0, hurtAudioClips.Length);
            characterAudioSource.clip = hurtAudioClips[clipToPlay];
            characterAudioSource.Play();
        }

        private void CheckHealth()
        {
            if (currentHealth >= maximumHealth)
            {
                currentHealth = maximumHealth;
                healthSlider.value = currentHealth; // Update slider to represent new health total
            }
            else if (currentHealth <= 0)
            {
                currentHealth = 0;
                healthSlider.value = currentHealth; // Update slider to represent new health total
            }
        }

        public void RestoreHealth(float amountToRestore)
        {
            currentHealth = Mathf.Lerp(currentHealth, currentHealth + amountToRestore, 1f);
            healthSlider.value = currentHealth; // Update slider to represent new health total
        }

        protected virtual void Update()
        {
            if (!player)
            {
                player = FindObjectOfType<StateManager>().gameObject;
            }


            CheckHealth();

            if (!stunned)
            {
                Tick(Time.deltaTime);
                canMove = charAnim.GetBool("canMove");

                if (currentHealth <= 0)
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
                        int clipToPlay = Random.Range(0, deathAudioClips.Length);
                        characterAudioSource.clip = deathAudioClips[clipToPlay];
                        characterAudioSource.Play();
                    }
                    else
                    {
                        transform.Translate(new Vector3(0, -0.1f, 0) * Time.fixedDeltaTime); // Sink into the ground whilst dead, should allow the model to submerge before being deleted.
                    }
                }
                else
                {
                    healthSlider.transform.LookAt(new Vector3(cameraManager.transform.position.x, healthSlider.transform.position.y, cameraManager.transform.position.z), Vector3.up);
                    //damageText.transform.LookAt(new Vector3(cameraManager.transform.position.x, damageText.transform.position.y, cameraManager.transform.position.z), Vector3.up);
                    Quaternion rotationToFace = Quaternion.LookRotation(damageText.transform.position - cameraManager.transform.position);
                    damageText.transform.rotation = new Quaternion(transform.rotation.x, rotationToFace.y, transform.rotation.z, rotationToFace.w);
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
        protected void Tick(float deltaTime)
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
                else
                {
                    rigidBody.isKinematic = true;
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

        /// <summary>
        /// Actions performed by the AI dependent on their current state.
        /// </summary>
        protected virtual void PerformStateBehaviour()
        {
            switch (currentState)
            {
                case State.Idle:
                    // Maybe regen health after a delay?
                    Patrol();
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
                        CombatBehaviour();
                    }
                    break;
            }
        }

        /// <summary>
        /// Moves the AI towards the players position.
        /// </summary>
        protected void MoveToTarget()
        {
            if (agent.enabled)
            {
                agent.SetDestination(player.transform.position);
            }
        }

        /// <summary>
        /// Loops over list of patrol points in order, moving towards each point before moving along to the next.
        /// </summary>
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
        /// Rotates the character towards another position smoothly, based on character rotation speed
        /// </summary>
        protected void RotateTowardsTarget(Transform target)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up); // Calculate the rotation desired
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed); // Apply rotation
        }

        /// <summary>
        /// Rotates the character towards another position smoothly, based on character rotation speed
        /// </summary>
        protected void RotateTowardsTarget(Vector3 location)
        {
            Quaternion targetRotation = Quaternion.LookRotation(location - transform.position, Vector3.up); // Calculate the rotation desired
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed); // Apply rotation
        }

        /// <summary>
        /// The logic behind the AIs combat decisions and behaviour, intended to be overridden by derived classes
        /// </summary>
        protected virtual void CombatBehaviour()
        {
            if (!ignoreCombatCircle)
            {



                var slotManager = player.GetComponent<AttackSlotManager>();
                rigidBody.velocity = Vector3.zero; // Reset velocity to ensure no gliding behaviour as navmesh agents do not follow ordinary rigidbody physics
                RotateTowardsTarget(player.transform);

                if (movingToAttack && !inAction) // If the AI is flagged as moving to attack...
                {
                    MoveToTarget();
                }
                else // Otherwise if not moving to attack...
                {
                    if (agent.enabled) // If Navmeshagent is active...
                    {
                        if (currentSlot == -1) // If not assigned a slot...
                        {
                            currentSlot = slotManager.ReserveSlot(this, currentSlot, false);
                        }

                        if (currentSlot == -1) // If still not assigned a slot...
                        {
                            return;
                        }

                        NavMeshPath path = new NavMeshPath();
                        agent.CalculatePath(slotManager.GetSlotPosition(currentSlot), path); // Calculate if the position of the slot is on the NavMesh

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
                            print("Path complete");
                        }

                        agent.destination = slotManager.GetSlotPosition(currentSlot);
                    }
                }
            }
        }

        /// <summary>
        /// Determines the state of the character based on aggro and attack ranges.
        /// </summary>
        protected virtual State UpdateState()
        {
            if (!ignoreCombatCircle)
            {
                bool canConsiderAttacking = Vector3.Distance(transform.position, player.transform.position) < aggroRange;

                if (canConsiderAttacking)
                {
                    if (currentAttackers.Count < maximumNumberOfAttackers)
                    {
                        if (!currentAttackers.Contains(this))
                        {
                            currentAttackers.Add(this);
                            movingToAttack = true;
                        }
                    }
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

        /// <summary>
        /// Show AI UI when locked on.
        /// </summary>
        public void ShowLockOnHealth()
        {
            healthSlider.gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide AI UI when locked onto.
        /// </summary>
        public void HideLockOnHealth()
        {
            healthSlider.gameObject.SetActive(false);
            damageText.gameObject.SetActive(false);
        }

        public void ApplyStun(float duration)
        {
            if (stunnedCoroutine != null)
            {
                StopCoroutine(stunnedCoroutine);
                stunnedCoroutine = StartCoroutine(StunEffect(duration));
            }
            else
            {
                stunnedCoroutine = StartCoroutine(StunEffect(duration));
            }
        }

        /// <summary>
        /// Show AI UI for the duration specified.
        /// </summary>
        protected IEnumerator RevealHealthBar(float duration)
        {
            healthSlider.gameObject.SetActive(true);

            yield return new WaitForSeconds(duration);

            if (!lockedOnto)
            {
                healthSlider.gameObject.SetActive(false);
                healthCoroutine = null;
            }
        }

        /// <summary>
        /// Show AI UI for the duration specified
        /// </summary>
        protected IEnumerator RevealDamageText(float duration)
        {
            damageText.gameObject.SetActive(true);

            yield return new WaitForSeconds(duration);

            damageText.gameObject.SetActive(false);
            damageTextCoroutine = null;
        }

        protected IEnumerator StunEffect(float duration)
        {
            stunned = true;
            charAnim.SetBool("stunned", true);
            charAnim.CrossFade("Stunned", 0.2f);
            if (agent.enabled)
            {
                agent.isStopped = true;
            }

            yield return new WaitForSeconds(duration);

            if (agent.enabled)
            {
                agent.isStopped = false;
            }
            charAnim.SetBool("stunned", false);
            stunned = false;
            stunnedCoroutine = null;
        }

        /// <summary>
        /// Draw the aggro & attack ranges in the scene view.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Draw a yellow wire sphere into the scene editor viewport to match aggro range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, aggroRange);

            // Draw a red wire sphere into the scene editor viewport to match attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, attackRange);
        }

        /// <summary>
        /// Removes the AI from the list of attackers and flags them as no longer moving to attack.
        /// </summary>
        protected void RemoveFromAttackersList()
        {
            if (!ignoreCombatCircle)
            {
                if (currentAttackers.Contains(this))
                {
                    currentAttackers.Remove(this);
                }

                movingToAttack = false;
                var slotManager = player.GetComponent<AttackSlotManager>();

                if (currentSlot != -1) // If the AI has a valid slot assigned...
                {
                    slotManager.ClearSlot(currentSlot); // Clear slot
                    currentSlot = -1; // Assign invalid slot - will be recalculated when they attempt to reserve another.
                }
            }
        }
    }        
}
