﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZaldensGambit
{
    public class StateManager : MonoBehaviour
    {
        #region Variables
        [HideInInspector] public bool interacting;

        [HideInInspector] public float moveAmount;
        [HideInInspector] public Vector3 movementDirection;

        [SerializeField] private float moveSpeed = 4;
        [SerializeField] private float rotateSpeed = 5;

        [SerializeField] private float maxStepHeight = 0.4f;        // The maximum a player can step upwards in units when they hit a wall that's potentially a step
        [SerializeField] private float stepSearchOvershoot = 0.01f; // How much to overshoot into the direction a potential step in units when testing. High values prevent player from walking up tiny steps but may cause problems.
        private List<ContactPoint> contactPoints = new List<ContactPoint>();

        public float maximumHealth = 100;
        public float currentHealth = 100;
        private int level = 1;
        private int experience = 0;
        private int experienceForNextLevel = 1000;
        public int damage = 10;
        private bool isDead = false;
        private Collider collider;
        [HideInInspector] public bool isInvulnerable;
        [HideInInspector] public bool grounded;
        [HideInInspector] public bool lightAttack, heavyAttack, dodgeRoll, block, specialAttack;
        [HideInInspector] public bool inAction;
        [HideInInspector] public bool canMove;
        [HideInInspector] public bool lockOn;
        [HideInInspector] public bool isBlocking;
        [HideInInspector] public bool listenForCombos;
        [HideInInspector] public bool shieldBashing;
        private float shieldBashCooldown = 1.5f;
        private float shieldBashTimer;

        [SerializeField] private AnimationClip[] lightAttacksChain;
        [SerializeField] private AnimationClip[] heavyAttacksChain;
        private int animationClipIndex = 0;
        [SerializeField] private AnimationClip[] hurtAnimations;

        [HideInInspector] public Enemy lockOnTarget;
        [HideInInspector] public GameObject activeModel;
        [HideInInspector] public Animator charAnim;
        [HideInInspector] public Rigidbody rigidBody;
        [HideInInspector] public float delta;
        [HideInInspector] public LayerMask ignoredLayers;
        [HideInInspector] public AnimatorHook animHook;
        [HideInInspector] public ActionManager actionManager;
        [HideInInspector] public WeaponHook weaponHook;
        private AudioSource characterAudioSource;
        private AudioSource weaponAudioSource;
        private AudioSource shieldAudioSource;
        [SerializeField] private AudioClip[] hurtAudioClips;
        [SerializeField] private AudioClip[] attackBlockedAudioClips;

        private float actionDelay;
        [HideInInspector] public float actionLockoutDuration = 1f;
        #endregion

        /// <summary>
        /// Performs set up references for variables needed for class methods.
        /// </summary>
        public void Initialise()
        {
            SetUpAnimator();
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.angularDrag = 999;
            rigidBody.drag = 4;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            animHook = GetComponentInChildren<AnimatorHook>();
            collider = GetComponent<Collider>();
            characterAudioSource = GetComponent<AudioSource>();
            shieldAudioSource = GameObject.Find("Shield").GetComponent<AudioSource>();


            if (animHook == false)
            {
                //print("Added AnimatorHook.cs to " + gameObject.name);
                animHook = activeModel.AddComponent<AnimatorHook>();
            }

            animHook.Initialise(this, null);
            actionManager = GetComponent<ActionManager>();
            actionManager.Initialise();
            weaponHook = GetComponentInChildren<WeaponHook>();
            weaponHook.CloseDamageCollider();
            gameObject.layer = 9; // Set to player layer
            ignoredLayers = ~(1 << 10); // Ignore layers 1 to 10
        }

        /// <summary>
        /// Gains reference to attached Animator component and gameobject, disables root motion.
        /// </summary>
        private void SetUpAnimator()
        {
            if (activeModel == null)
            {
                charAnim = GetComponentInChildren<Animator>();

                if (charAnim == null)
                {
                    Debug.Log("No animator found for " + gameObject.name);
                }
                else
                {
                    activeModel = charAnim.gameObject;
                }
            }

            if (charAnim == null)
            {
                charAnim = activeModel.GetComponentInChildren<Animator>();
            }

            charAnim.applyRootMotion = false;
        }

        /// <summary>
        /// Deals damage to the player unless invulnerable or the damageSource is directly in front of the player whilst they are blocking.
        /// </summary>
        public void TakeDamage(float value, Transform damageSource)
        {
            Enemy enemy = damageSource.GetComponent<Enemy>();

            if (isInvulnerable || isDead)
            {
                print("Cannot take damage whilst invulnerable, or dead!");
                return;
            }

            if (isBlocking && !shieldBashing)
            {
                RaycastHit[] hits = Physics.BoxCastAll(transform.position + transform.forward, transform.localScale / 2, transform.forward, transform.rotation, 5, ignoredLayers);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform == damageSource)
                    {
                        charAnim.CrossFade("BlockShieldHit", 0.1f);
                        int audioToPlay = Random.Range(0, attackBlockedAudioClips.Length);
                        shieldAudioSource.clip = attackBlockedAudioClips[audioToPlay];
                        shieldAudioSource.Play();

                        if (enemy)
                        {
                            enemy.attackDelay = Time.time + enemy.attackCooldown * 2f;
                        }
                        return;
                    }
                }
            }

            float previousHealth = currentHealth;
            currentHealth -= value;
            canMove = false;

            GameManager.instance.PlayDamageEffect();

            if (currentHealth <= 0 && !isDead)
            {
                isDead = true;
                charAnim.SetBool("isDead", isDead);
                print("Player died!");
                collider.enabled = false;
                rigidBody.isKinematic = true;
                charAnim.Play("Death");
                GameManager.instance.GameOver();
            }

            int audioClipToPlay = Random.Range(0, hurtAudioClips.Length);
            characterAudioSource.clip = hurtAudioClips[audioClipToPlay];
            characterAudioSource.Play();

            int hurtAnimationToPlay = Random.Range(0, hurtAnimations.Length);
            charAnim.CrossFade(hurtAnimations[hurtAnimationToPlay].name, 0.1f);
            charAnim.applyRootMotion = true;
        }

        /// <summary>
        /// Ran alongside Update, records deltaTime, performs ground check and updates animator variables.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (isDead)
            {
                return;
            }

            CheckHealth();
            delta = deltaTime;

            if (!canMove) // If the character can't move...
            {
                //charAnim.applyRootMotion = false; // Toggle root motion
            }

            if (!block)
            {
                isBlocking = false;
            }

            if (isBlocking)
            {
                moveSpeed = 4;
            }
            else
            {
                charAnim.SetBool("blocking", isBlocking);
                moveSpeed = 6;
            }

            grounded = IsGrounded();
            charAnim.SetBool("grounded", grounded);

            if (grounded && !isInvulnerable)
            {
                if (!interacting)
                {
                    DetectAction(); // Listen for player inputs
                }
            }

            if (inAction) // If an animation is playing...
            {
                isBlocking = false;
                moveAmount = 0;
                charAnim.applyRootMotion = true;
                actionDelay += delta;

                if (actionDelay > actionLockoutDuration) // After the duration that the animation locks the character out of performing other actions has passed...
                {
                    inAction = false; // Flag animation no longer playing
                    actionDelay = 0; // Reset
                }
                else
                {
                    return;
                }
            }

            canMove = charAnim.GetBool("canMove");
        }

        /// <summary>
        /// Ran alongside FixedUpdate, records fixedDeltaTime, changes rigidbody drag based on movement and grounded status, applies rotation to the player character and movement animations.
        /// </summary>
        public void FixedTick(float fixedDeltaTime)
        {
            delta = fixedDeltaTime;

            // Do stair step
            ContactPoint groundCP;
            bool grounded = FindGround(out groundCP, contactPoints);
            Vector3 stepUpOffset = Vector3.zero;
            Vector3 currentVelocity = rigidBody.velocity;
            bool stepUp = false;
            if (grounded)
                stepUp = FindStep(out stepUpOffset, contactPoints, groundCP, out currentVelocity);

            if (stepUp)
            {
                //Take the RigidBody and apply the stepUpOffset to its position
                transform.position += stepUpOffset;
                //When it hit the stair, it stopped our player, so reapply their last velocity
                rigidBody.velocity = currentVelocity; //You'll need to store this from the last physics frame...
            }
            contactPoints.Clear();

            //if (grounded)
            //{
            //    DetectAction(); // Listen for player inputs
            //}

            //if (inAction) // If an animation is playing...
            //{
            //    charAnim.applyRootMotion = true;
            //    actionDelay += delta;

            //    if (actionDelay > actionLockoutDuration) // After the duration that the animation locks the character out of performing other actions has passed...
            //    {
            //        inAction = false; // Flag animation no longer playing
            //        actionDelay = 0; // Reset
            //    }
            //    else
            //    {
            //        return;
            //    }           
            //}

            //canMove = charAnim.GetBool("canMove");

            if (!canMove) // If the player can't move, return out of the method to allow root motion to continue being applied.
            {
                return;
            }

            charAnim.applyRootMotion = false;

            if (moveAmount > 0 || !grounded) // If there is any movement or if the player is not grounded...
            {
                rigidBody.drag = 0;
            }
            else
            {
                rigidBody.drag = 4;                
            }

            if (grounded)
            {
                rigidBody.velocity = movementDirection * (moveSpeed * moveAmount); // Apply force in the direction the player is heading
                rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y * 200, rigidBody.velocity.z);
            }

            Vector3 targetDirection = movementDirection;
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero && !lockOn) // If there is no recorded target direction (No input)...
            {
                targetDirection = transform.forward; // Target direction is equivalent to the current forward vector of the character
            }

            if (!lockOn) // If not locking onto an enemy...
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection); // Calculate the rotation desired
                Quaternion lerpedRotation = Quaternion.Slerp(transform.rotation, targetRotation, delta * moveAmount * rotateSpeed); // Lerp between current rotation and desired rotation
                transform.rotation = lerpedRotation; // Apply lerped rotation   
                charAnim.SetBool("lockOn", lockOn);
            }
            else // If locked onto an enemy...
            {
                targetDirection = lockOnTarget.transform.position - transform.position; // Calculate the direction based on the position of the player and the enemy.
                targetDirection.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection); // Calculate the rotation desired
                Quaternion lerpedRotation = Quaternion.Slerp(transform.rotation, targetRotation, delta * moveAmount * rotateSpeed); // Lerp between current rotation and desired rotation
                transform.rotation = lerpedRotation; // Apply lerped rotation
                charAnim.SetBool("lockOn", lockOn);
            }

            if (!lockOn)
            {
                HandleMovementAnimations();
            }
            else
            {
                HandleLockOnAnimations(movementDirection);
            }
        }

        /// <summary>
        /// Checks the players health and clamps between the maximum and minimum values.
        /// </summary>
        private void CheckHealth()
        {
            if (currentHealth >= maximumHealth)
            {
                currentHealth = maximumHealth;
            }
            else if (currentHealth <= 0)
            {
                currentHealth = 0;
            }
        }

        /// <summary>
        /// Updates animator movement variables.
        /// </summary>
        private void HandleMovementAnimations()
        {
            charAnim.SetFloat("horizontal", moveAmount, 0.1f, delta);
            charAnim.SetFloat("vertical", moveAmount, 0.1f, delta);
        }

        /// <summary>
        /// Updates the animator movement variables based on the direction to the locked on target.
        /// </summary>
        private void HandleLockOnAnimations(Vector3 movementDirection)
        {
            Vector3 relativeDirection = transform.InverseTransformDirection(movementDirection);
            float h = relativeDirection.x;
            float v = relativeDirection.z;

            charAnim.SetFloat("vertical", v, 0.4f, delta);
            charAnim.SetFloat("horizontal", h, 0.4f, delta);
        }

        /// <summary>
        /// Performs a ground check from the bottom centre of the player, checking if there is collision with any layers.
        /// </summary>
        public bool IsGrounded()
        {
            bool grounded = false;

            Vector3 origin = transform.position + (Vector3.up * 0.5f);
            Vector3 direction = Vector3.down;
            float distance = 0.8f;

            RaycastHit hit;

            Debug.DrawRay(origin, direction * distance);

            if (Physics.Raycast(origin, direction, out hit, distance, ignoredLayers)) // Possibly change to a boxcast later if raycast seems too inaccurate on ledges etc.
            {
                grounded = true;
                Vector3 targetPosition = hit.point;
                transform.position = targetPosition;
            }
            return grounded;
        }

        /// <summary>
        /// Rotates the character in the direction of the current movement direction.
        /// </summary>
        public void HandleDodgeRoll()
        {
            if (!dodgeRoll) // If there is no input from the player to dodge roll return out of the method.
            {
                return;
            }

            if (movementDirection == Vector3.zero) // If there is no recorded input...
            {
                movementDirection = transform.forward; // Movement direction is the current forward vector.
            }

            Quaternion targetRotation = Quaternion.LookRotation(movementDirection); // Calculate rotation/direction of the roll based on movementDirection.
            transform.rotation = targetRotation;

            canMove = false;
            inAction = true;
            charAnim.CrossFade("dodgeRoll", 0.2f); // Apply animation crossfade
        }

        /// <summary>
        /// Rotates the character towards another transform smoothly, based on character rotation speed
        /// </summary>
        protected void RotateTowardsTarget(Transform target)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position, Vector3.up); // Calculate the rotation desired
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed); // Apply rotation
        }

        /// <summary>
        /// Listens for inputs from the player if no other inputs are already true, returns and applies corresponding action based on input received.
        /// </summary>
        public void DetectAction()
        {
            AnimationClip desiredAnimation = null;
            Action slot = null;

            if (!lightAttack && !heavyAttack && !dodgeRoll && !block) // If there are no actions detected...
            {
                return;
            }

            if (block && lightAttack && !inAction && shieldBashTimer < Time.time)
            {
                shieldBashTimer = Time.time + shieldBashCooldown;
                animationClipIndex = 0; // Reset array position
                listenForCombos = false;
                charAnim.CrossFade("ParryShield", 0.2f); // Apply animation crossfade.
                return;
            }

            if (listenForCombos) // If we are listening for a combo input and are not in the middle of a combo...
            {
                slot = actionManager.GetActionSlot(this); // Return action that matches input

                if (slot == null) // If there is nothing to return...
                {
                    return;
                }
                else
                {
                    listenForCombos = false;
                    desiredAnimation = slot.desiredAnimation;           
                }

                // Light Attack Combo
                if (slot.desiredAnimation == lightAttacksChain[0])
                {
                    animationClipIndex++;

                    if (animationClipIndex >= lightAttacksChain.Length) // Array bounds check
                    {
                        animationClipIndex = 0; // Reset array position
                    }

                    desiredAnimation = lightAttacksChain[animationClipIndex]; // Set animation to call
                }
                // Heavy Attack Combo
                else if (slot.desiredAnimation == heavyAttacksChain[0])
                {
                    animationClipIndex++;

                    if (animationClipIndex >= heavyAttacksChain.Length) // Array bounds check
                    {
                        animationClipIndex = 0; // Reset array position
                    }

                    desiredAnimation = heavyAttacksChain[animationClipIndex]; // Set animation to call
                }
                // Block mid combo
                else if (desiredAnimation.name == "block")
                {
                    animationClipIndex = 0; // Reset array position
                    isBlocking = true;
                    charAnim.SetBool("blocking", isBlocking);
                    charAnim.SetBool("canMove", true);
                    canMove = true;
                    //actionLockoutDuration = 0.3f;
                    return;
                }
                else if (desiredAnimation.name == "DodgeRoll")
                {
                    HandleDodgeRoll();
                    animationClipIndex = 0;
                    return;
                }

                if (desiredAnimation == null) // If desiredAnimation returns nothing...
                {
                    print("No animation of " + desiredAnimation + " found, is this the correct animation to search for?");
                    return;
                }
                
                canMove = false;
                inAction = true;

                if (lockOn)
                {
                    RotateTowardsTarget(lockOnTarget.transform);
                }

                charAnim.CrossFade(desiredAnimation.name, 0.2f); // Crossfade from current animation to the desired animation.
                return;
            }
            
            // -----------

            if (!canMove) // If the player can't move (Such as mid roll, staggered from a hit, etc.)
            {
                return;
            }

            slot = actionManager.GetActionSlot(this); // Return action that matches input

            if (slot == null) // If there is nothing to return...
            {
                return;
            }
            else
            {
                desiredAnimation = slot.desiredAnimation;
            }

            if (desiredAnimation.name == "block")
            {
                isBlocking = true;
                charAnim.SetBool("blocking", isBlocking);
                canMove = true;
                charAnim.SetBool("canMove", true);
                //actionLockoutDuration = 0.3f;
                return;
            }
            else if (desiredAnimation.name == "DodgeRoll")
            {
                HandleDodgeRoll();
                animationClipIndex = 0;
                return;
            }

            if (string.IsNullOrEmpty(desiredAnimation.name)) // If desiredAnimation returns nothing...
            {
                print("No animation of " + desiredAnimation + " found, is this the correct animation to search for?");
                return;
            }
            
            //canMove = false;
            //inAction = true;            

            if (!inAction)
            {
                if (lockOn)
                {
                    RotateTowardsTarget(lockOnTarget.transform);
                }

                charAnim.CrossFade(desiredAnimation.name, 0.2f); // Apply animation crossfade.
            }
            
        }

        private bool FindGround(out ContactPoint groundCP, List<ContactPoint> allCPs)
        {
            groundCP = default(ContactPoint);
            bool found = false;
            foreach (ContactPoint cp in allCPs)
            {
                //Pointing with some up direction
                if (cp.normal.y > 0.0001f && (found == false || cp.normal.y > groundCP.normal.y))
                {
                    groundCP = cp;
                    found = true;
                }
            }
            //print("Grounded");
            return found;
        }

        private bool FindStep(out Vector3 stepUpOffset, List<ContactPoint> allCPs, ContactPoint groundCP, out Vector3 currentVelocity)
        {
            stepUpOffset = default(Vector3);
            currentVelocity = rigidBody.velocity;

            //No chance to step if the player is not moving
            Vector2 velocityXZ = new Vector2(currentVelocity.x, currentVelocity.z);
            if (velocityXZ.sqrMagnitude < 0.0001f)
            {
               // print("Player not moving");
                return false;
            }

            foreach (ContactPoint cp in allCPs)
            {
                bool test = ResolveStepUp(out stepUpOffset, cp, groundCP);
                if (test)
                    //print("Found step");
                return test;
            }
            //print("Step not found");
            return false;
        }

        private bool ResolveStepUp(out Vector3 stepUpOffset, ContactPoint stepTestCP, ContactPoint groundCP)
        {
            stepUpOffset = default(Vector3);
            Collider stepCol = stepTestCP.otherCollider; //You'll need the collider of the potential step for this
                                                         //Determine if stepTestCP is a stair...
            //( 1 ) Check if the contact point normal matches that of a step (y close to 0)
            if (Mathf.Abs(stepTestCP.normal.y) <= 0.01f)
            {
               // print("Failed 1");
                return false;
            }

            //( 2 ) Make sure the contact point is low enough to be a step
            if (!(stepTestCP.point.y - groundCP.point.y < maxStepHeight))
            {
                //print("Failed 2");
                return false;
            }

            //( 3 ) Check to see if there's actually a place to step in front of us
            RaycastHit hitInfo;
            float stepHeight = groundCP.point.y + maxStepHeight + 0.001f;
            Vector3 stepTestInvDir = new Vector3(-stepTestCP.normal.x, 0, -stepTestCP.normal.z).normalized;
            Vector3 origin = new Vector3(stepTestCP.point.x, stepHeight, stepTestCP.point.z) + (stepTestInvDir * stepSearchOvershoot) + transform.forward / 2;
            Vector3 direction = Vector3.down;
            //Debug.DrawRay(origin, direction * maxStepHeight, Color.red, 5);

            if (!stepCol.Raycast(new Ray(origin, direction), out hitInfo, maxStepHeight))
            {
                if (hitInfo.collider == null)
                {
                    //Debug.Log("null");
                }
                //print("Failed 3");
                return false;
            }
            else
            {
                //Debug.Log(hitInfo.collider.gameObject.name);
            }

            //We have enough info to calculate the points
            Vector3 stepUpPoint = new Vector3(stepTestCP.point.x, hitInfo.point.y + 0.0001f, stepTestCP.point.z) + (stepTestInvDir * stepSearchOvershoot);
            Vector3 stepUpPointOffset = stepUpPoint - new Vector3(stepTestCP.point.x, groundCP.point.y, stepTestCP.point.z);

           // print("Stepping up");
            stepUpOffset = stepUpPointOffset;
            return true; //We're going to step up!
        }

        /// <summary>
        /// Checks if the player has enough experience to levle up, if so increases their level and sets experienced needed for the next level up.
        /// </summary>
        public void LevelUp()
        {
            if (experience > experienceForNextLevel)
            {
                level++;
                experienceForNextLevel = 1000 * level; // 1000, 2000, 3000, 4000, 5000, etc... Update later to be a curve that becomes steeper over time.
                damage += 5;
                maximumHealth += 10;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            contactPoints.AddRange(collision.contacts);
        }

        private void OnCollisionStay(Collision collision)
        {
            contactPoints.AddRange(collision.contacts);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Projectile>())
            {
                if (isBlocking && !shieldBashing)
                {
                    charAnim.CrossFade("BlockShieldHit", 0.1f);
                }
                else
                {
                    TakeDamage(other.GetComponent<Projectile>().damageValue, other.transform);
                }
            }

            if (other.GetComponent<Enemy>() && shieldBashing)
            {
                other.GetComponent<Enemy>().TakeDamage(5);
                //other.GetComponent<Enemy>().attackDelay = Time.time + other.GetComponent<Enemy>().attackCooldown * 4f;
                other.GetComponent<Enemy>().ApplyStun(2.5f);
            }
        }

        IEnumerator PlayAudioAfterDelay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            source.Play();
        }
    }    
}


