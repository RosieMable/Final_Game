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
        private float actionLockoutDuration = 1f;
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
        /// Gains reference to attached Animator component and gameobject, disabled root motion.
        /// </summary>
        private void SetUpAnimator()
        {
            if (activeModel == null)
            {
                charAnim = GetComponentInChildren<Animator>();

                if (charAnim == null)
                {
                    Debug.Log("No model found for " + gameObject.name);
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
        /// Ran alongside Update, records deltaTime, performs ground check.
        /// </summary>
        public void Tick(float deltaTime)
        {
            delta = deltaTime;
            grounded = IsGrounded();
            charAnim.SetBool("canMove", grounded);
        }

        /// <summary>
        /// Ran alongside FixedUpdate, records fixedDeltaTime, changes rigidbody drag based on movement and grounded status, applies rotation to the player character and movement animations.
        /// </summary>
        public void FixedTick(float fixedDeltaTime)
        {
            delta = fixedDeltaTime;

            if (grounded)
            {
                DetectAction();
            }

            if (inAction)
            {
                charAnim.applyRootMotion = true;
                actionDelay += delta;

                if (actionDelay > actionLockoutDuration)
                {
                    inAction = false;
                    actionDelay = 0;
                }
                else
                {
                    return;
                }           
            }

            canMove = charAnim.GetBool("canMove");

            if (!canMove)
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

            if (targetDirection == Vector3.zero)
            {
                targetDirection = transform.forward;
            }


            if (!lockOn)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection); // Calculate the rotation desired
                Quaternion lerpedRotation = Quaternion.Slerp(transform.rotation, targetRotation, delta * moveAmount * rotateSpeed); // Lerp between current rotation and desired rotation
                transform.rotation = lerpedRotation; // Apply lerped rotation   
                charAnim.SetBool("lockOn", lockOn);
            }
            else
            {
                targetDirection = lockOnTarget.transform.position - transform.position;
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

            if (Physics.Raycast(origin, direction, out hit, distance, ignoredLayers))
            {
                grounded = true;

                Vector3 targetPosition = hit.point;
                transform.position = targetPosition;
            }
            return grounded;
        }

        public void HandleDodgeRoll()
        {
            if (!dodgeRoll)
            {
                return;
            }

            if (movementDirection == Vector3.zero)
            {
                movementDirection = transform.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = targetRotation;

            canMove = false;
            inAction = true;
            charAnim.CrossFade("dodgeRoll", 0.2f);
        }

        public void DetectAction()
        {
            if (!canMove)
            {
                return;
            }

            if (!lightAttack && !heavyAttack && !dodgeRoll)
            {
                return;
            }

            string desiredAnimation = null;

            Action slot = actionManager.GetActionSlot(this);
            if (slot == null)
            {
                return;
            }
            else
            {
                desiredAnimation = slot.desiredAnimation;
            }

            //if (lightAttack)
            //{
            //    desiredAnimation = "lightAttack";
            //    actionLockoutDuration = 0.5f;
            //}
            //if (heavyAttack)
            //{
            //    desiredAnimation = "heavyAttack";
            //    actionLockoutDuration = 0.6f;
            //}
            //if (dodgeRoll)
            //{
            //    HandleDodgeRoll();
            //    actionLockoutDuration = 1f;
            //    return;
            //}

            if (string.IsNullOrEmpty(desiredAnimation))
            {
                print("No animation of " + desiredAnimation + " found, is this the correct animation to search for?");
                return;
            }

            canMove = false;
            inAction = true;
            charAnim.CrossFade(desiredAnimation, 0.2f);
        }
    }
}


