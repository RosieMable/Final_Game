using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IsThisDarkSouls
{
    public class StateManager : MonoBehaviour
    {
        #region Variables
        public float moveAmount;
        public Vector3 movementDirection;

        [SerializeField] private float moveSpeed = 4;
        [SerializeField] private float rotateSpeed = 5;

        public bool grounded;
        public bool lightAttack, heavyAttack, dodgeRoll, block, specialAttack;
        public bool inAction;
        public bool canMove;
        public bool lockOn;
        public bool listenForCombos;

        public AnimationClip[] lightAttacks;
        public AnimationClip[] heavyAttacks;
        public int animationClipIndex = -1;

        public EnemyStates lockOnTarget;

        public GameObject activeModel;
        [HideInInspector] public Animator charAnim;
        [HideInInspector] public Rigidbody rigidBody;
        [HideInInspector] public float delta;
        [HideInInspector] public LayerMask ignoredLayers;
        [HideInInspector] public AnimatorHook animHook;
        [HideInInspector] public ActionManager actionManager;
        public WeaponHook weaponHook;

        private float actionDelay;
        public float actionLockoutDuration = 1f;
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
            weaponHook.CloseDamageCollider();
            gameObject.layer = 8; // Set to player layer
            ignoredLayers = ~(1 << 8); // Ignore layers 1 to 8
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
        /// Ran alongside Update, records deltaTime, performs ground check and updates animator variables.
        /// </summary>
        public void Tick(float deltaTime)
        {
            delta = deltaTime;
            grounded = IsGrounded();
            charAnim.SetBool("canMove", grounded);

            if (grounded)
            {
                DetectAction(); // Listen for player inputs
            }

            if (inAction) // If an animation is playing...
            {
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
            string desiredAnimation = null;
            Action slot = null;

            if (!lightAttack && !heavyAttack && !dodgeRoll)
            {
                return;
            }

            if (listenForCombos)
            {
                slot = actionManager.GetActionSlot(this);

                if (slot == null)
                {
                    return;
                }
                else
                {
                    desiredAnimation = slot.desiredAnimation;
                }

                if (slot.desiredAnimation == "lightAttack")
                {
                    animationClipIndex++;

                    if (animationClipIndex >= lightAttacks.Length)
                    {
                        animationClipIndex = -1;
                        return;
                    }

                    desiredAnimation = lightAttacks[animationClipIndex].name;
                }
                else if (slot.desiredAnimation == "heavyAttack")
                {
                    animationClipIndex++;

                    if (animationClipIndex >= heavyAttacks.Length)
                    {
                        animationClipIndex = -1;
                        return;
                    }

                    desiredAnimation = heavyAttacks[animationClipIndex].name;
                }

                if (string.IsNullOrEmpty(desiredAnimation)) // If desiredAnimation returns nothing...
                {
                    print("No animation of " + desiredAnimation + " found, is this the correct animation to search for?");
                    return;
                }

                canMove = false;
                inAction = true;
                charAnim.CrossFade(desiredAnimation, 0.2f); // Apply animation crossfade.
                return;
            }
            else
            {
                animationClipIndex = -1;
            }
            
            // -----------

            if (!canMove)
            {
                return;
            }

            slot = actionManager.GetActionSlot(this);

            if (slot == null)
            {
                return;
            }
            else
            {
                desiredAnimation = slot.desiredAnimation;
            }

            if (string.IsNullOrEmpty(desiredAnimation)) // If desiredAnimation returns nothing...
            {
                print("No animation of " + desiredAnimation + " found, is this the correct animation to search for?");
                return;
            }

            canMove = false;
            inAction = true;
            charAnim.CrossFade(desiredAnimation, 0.2f); // Apply animation crossfade.
        }
    }
}


