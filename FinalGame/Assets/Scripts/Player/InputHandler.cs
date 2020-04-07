using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class InputHandler : MonoBehaviour
    {
        private float vertical;
        private float horizontal;
        private bool lightAttackInput;
        private bool heavyAttackInput;
        private bool dodgeRollInput;
        private bool lockOnInput;
        private bool specialAttackInput;
        private bool blockInput;

        private StateManager states;
        private CameraManager cameraManager;
        private float delta;

        void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            states = GetComponent<StateManager>();
            states.Initialise();
            cameraManager = CameraManager.instance;
            cameraManager.Initialse(transform);
        }

        private void Update()
        {
            states.Tick(delta);

            if (CardInventory.instance.inventoryOpen)
            {
                vertical = 0;
                horizontal = 0;
                return;
            }

            if (!states.interacting)
            {
                GetInput();
            }
            else
            {
                vertical = 0;
                horizontal = 0;
            }
            SearchForLockOnTarget();
            delta = Time.deltaTime;

            if (states.lockOnTarget != null)
            {
                if (states.lockOnTarget.isDead)
                {
                    states.lockOn = false;
                    states.lockOnTarget = null;
                    cameraManager.lockOnTarget = null;
                }
            }            
        }

        void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;            
            UpdateStates();
            states.FixedTick(delta);
            cameraManager.Tick(delta);
        }

        /// <summary>
        /// Listens for and records user inputs.
        /// </summary>
        private void GetInput()
        {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");  
            lightAttackInput = Input.GetKeyDown(KeyCode.Mouse0);
            //heavyAttackInput = // Needs new input, probably when controller support added?
            dodgeRollInput = Input.GetKeyDown(KeyCode.Space);
            specialAttackInput = Input.GetKeyDown(KeyCode.R);
            blockInput = Input.GetKey(KeyCode.Mouse1);
        }

        /// <summary>
        /// Updates 'StateManager' class with the values recorded, applies movement and actions to the player character through the StateManager.
        /// </summary>
        private void UpdateStates()
        {
            Vector3 v = vertical * cameraManager.transform.forward;
            Vector3 h = horizontal * cameraManager.transform.right;
            states.movementDirection = (v + h).normalized; // Return normalized vector

            float desiredMovement = Mathf.Abs(horizontal) + Mathf.Abs(vertical); // Add both values together

            if (!states.inAction) // If the StateManager isn't currently performing an animation...
            {
                states.moveAmount = Mathf.Clamp01(desiredMovement); // Clamp between 0-1 and update StateManager 
            }

            states.lightAttack = lightAttackInput;
            states.heavyAttack = heavyAttackInput;
            states.dodgeRoll = dodgeRollInput;
            states.block = blockInput;
            states.specialAttack = specialAttackInput;           
        }


        /// <summary>
        /// Searches the nearby area for the closest target that can be locked onto if one is present. Resets locked on target is Q key is pressed, cycles to second closest target if search is attempted whilst already locked on to another target.
        /// </summary>
        private void SearchForLockOnTarget()
        {
            lockOnInput = Input.GetKeyDown(KeyCode.Tab);

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (states.lockOn) // If we are already locked onto something...
                {
                    // Reset
                    states.lockOnTarget.lockedOnto = false;
                    states.lockOnTarget.HideLockOnHealth();
                    states.lockOnTarget = null;
                    cameraManager.lockOnTarget = null;
                    states.lockOn = false;
                    cameraManager.lockedOn = false;
                }
            }

            if (lockOnInput) // When the lock on key is pressed...
            {
                //states.lockOn = !states.lockOn; // Toggle lock on state
                states.lockOn = true;

                Transform currentTarget = cameraManager.lockOnTarget;
                Transform targetToReturn = null;
                float closestDistance = Mathf.Infinity;

                Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 10);
                List<Transform> validTargets = new List<Transform>();

                foreach (Collider collider in nearbyColliders)
                {
                    if (collider.GetComponent<Enemy>())
                    {
                        validTargets.Add(collider.transform);
                    }
                }

                if (validTargets.Count <= 0)
                {
                    states.lockOn = false;
                    cameraManager.lockedOn = false;
                    return;
                }

                foreach (Transform target in validTargets) // Loop through all valid targets
                {
                    if (target != currentTarget) // If the target is not the current target...
                    {
                        float distance = Vector3.Distance(target.position, transform.position); // Calculate distance between target and player

                        if (distance < closestDistance) // If the calculated distance is less than the current closest...
                        {
                            closestDistance = distance; // Set new closest
                            targetToReturn = target; // Set new target to return
                        }
                    }
                }

                if (targetToReturn != null)
                {
                    states.lockOnTarget = targetToReturn.GetComponent<Enemy>();
                    states.lockOnTarget.ShowLockOnHealth();
                    states.lockOnTarget.lockedOnto = true;
                }

                if (states.lockOnTarget == null)
                {
                    states.lockOn = false;
                }
                else
                {
                    cameraManager.lockOnTarget = states.lockOnTarget.transform;
                    cameraManager.lockedOn = states.lockOn;
                }
            }
        }
    }
}

