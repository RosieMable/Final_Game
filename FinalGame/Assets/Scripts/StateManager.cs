using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZaldensGambit
{
    public class StateManager : MonoBehaviour
    {
        #region Variables
        [HideInInspector] public float moveAmount;
        [HideInInspector] public Vector3 movementDirection;

        [SerializeField] private float moveSpeed = 4;
        [SerializeField] private float rotateSpeed = 5;

        public float health = 100;
        public float stamina = 100;
        [HideInInspector] public bool isInvulnerable;
        [HideInInspector] public bool grounded;
        [HideInInspector] public bool lightAttack, heavyAttack, dodgeRoll, block, specialAttack;
        [HideInInspector] public bool inAction;
        [HideInInspector] public bool canMove;
        [HideInInspector] public bool lockOn;
        [HideInInspector] public bool isBlocking;
        [HideInInspector] public bool listenForCombos;
        private bool comboActive;

        [SerializeField] private AnimationClip[] lightAttacks;
        [SerializeField] private AnimationClip[] heavyAttacks;
        private int animationClipIndex = 0;

        public Enemy lockOnTarget;

        [HideInInspector] public GameObject activeModel;
        [HideInInspector] public Animator charAnim;
        [HideInInspector] public Rigidbody rigidBody;
        [HideInInspector] public float delta;
        [HideInInspector] public LayerMask ignoredLayers;
        [HideInInspector] public AnimatorHook animHook;
        [HideInInspector] public ActionManager actionManager;
        [HideInInspector] public WeaponHook weaponHook;

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

            if (animHook == false)
            {
                print("Added AnimatorHook.cs to " + gameObject.name);
                animHook = activeModel.AddComponent<AnimatorHook>();
            }

            animHook.Initialise(this, null);
            actionManager = GetComponent<ActionManager>();
            actionManager.Initialise();
            weaponHook = GetComponentInChildren<WeaponHook>();
            //weaponHook.CloseDamageCollider();
            gameObject.layer = 9; // Set to player layer
            ignoredLayers = ~(1 << 9); // Ignore layers 1 to 9
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

        public void TakeDamage(float value, Transform location)
        {
            if (isInvulnerable)
            {
                print("Cannot take damage while invulnerable!");
                return;
            }

            if (isBlocking)
            {
                RaycastHit[] hits = Physics.BoxCastAll(transform.position + transform.forward, transform.localScale / 2, transform.forward, transform.rotation, 5, ignoredLayers);

                foreach (RaycastHit hit in hits)
                {
                    print(hit.collider.gameObject.name);
                    if (hit.transform == location)
                    {
                        print("Blocked attack!");
                        return;
                    }
                }
            }

            health -= value;
            //isInvulnerable = true;
            canMove = false;
            charAnim.Play("hurt");
            charAnim.applyRootMotion = true;
        }

        /// <summary>
        /// Ran alongside Update, records deltaTime, performs ground check and updates animator variables.
        /// </summary>
        public void Tick(float deltaTime)
        {
            delta = deltaTime;

            if (health <= 0)
            {
                // TBD
            }

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
                DetectAction(); // Listen for player inputs
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
            }

            Vector3 targetDirection = movementDirection;
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero) // If there is no recorded target direction (No input)...
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
        /// Updates animator movement variables.
        /// </summary>
        private void HandleMovementAnimations()
        {
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
            float distance = (0.5f + 0.3f);

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

            if (listenForCombos) // If we are listening for a combo input and are not in the middle of a combo...
            {
                slot = actionManager.GetActionSlot(this); // Return action that matches input
                comboActive = true; // Flag that a combo is now active

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
                if (slot.desiredAnimation.name == "lightAttack")
                {
                    animationClipIndex++;

                    if (animationClipIndex >= lightAttacks.Length) // Array bounds check
                    {
                        animationClipIndex = 0; // Reset array position
                    }

                    desiredAnimation = lightAttacks[animationClipIndex]; // Set animation to call
                }
                // Heavy Attack Combo
                else if (slot.desiredAnimation.name == "heavyAttack")
                {
                    animationClipIndex++;

                    if (animationClipIndex >= heavyAttacks.Length) // Array bounds check
                    {
                        animationClipIndex = 0; // Reset array position
                    }

                    desiredAnimation = heavyAttacks[animationClipIndex]; // Set animation to call
                }
                // Block mid combo
                else if (desiredAnimation.name == "block")
                {
                    animationClipIndex = 0; // Reset array position
                    isBlocking = true;
                    charAnim.SetBool("blocking", isBlocking);
                    charAnim.SetBool("canMove", true);
                    canMove = true;
                    actionLockoutDuration = 0.5f;
                    return;
                }

                if (desiredAnimation == null) // If desiredAnimation returns nothing...
                {
                    print("No animation of " + desiredAnimation + " found, is this the correct animation to search for?");
                    return;
                }

                if (!listenForCombos) // If we aren't listening for combos...
                {
                    //return;
                }

                canMove = false;
                inAction = true;
                comboActive = false;
                charAnim.CrossFade(desiredAnimation.name, 0.2f); // Crossfade from current animation to the desired animation.
                return;
            }
            else
            {
                //animationClipIndex = 0; // Reset array position
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
                actionLockoutDuration = 0.5f;
                return;
            }

            if (string.IsNullOrEmpty(desiredAnimation.name)) // If desiredAnimation returns nothing...
            {
                print("No animation of " + desiredAnimation + " found, is this the correct animation to search for?");
                return;
            }

            comboActive = false;
            canMove = false;
            inAction = true;
            charAnim.CrossFade(desiredAnimation.name, 0.2f); // Apply animation crossfade.
        }
    }
}


