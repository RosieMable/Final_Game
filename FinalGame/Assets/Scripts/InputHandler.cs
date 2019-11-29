using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IsThisDarkSouls
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
        CameraManager cameraManager;
        private float delta;

        void Start()
        {
            states = GetComponent<StateManager>();
            states.Initialise();
            cameraManager = CameraManager.instance;
            cameraManager.Initialse(this.transform);
        }

        private void Update()
        {
            delta = Time.deltaTime;
            states.Tick(delta);

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
            GetInput();
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
            heavyAttackInput = Input.GetKeyDown(KeyCode.Mouse1);
            dodgeRollInput = Input.GetKeyDown(KeyCode.LeftControl);
            lockOnInput = Input.GetKeyDown(KeyCode.Tab);
            specialAttackInput = Input.GetKeyDown(KeyCode.Mouse2);
            // blockInput = // What input needs to be decided, perhaps replace heavy attack
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
            states.moveAmount = Mathf.Clamp01(desiredMovement); // Clamp between 0-1 and update StateManager 

            states.lightAttack = lightAttackInput;
            states.heavyAttack = heavyAttackInput;
            states.dodgeRoll = dodgeRollInput;
            states.block = blockInput;
            states.specialAttack = specialAttackInput;

            if (lockOnInput) // When the lock on key is pressed...
            {
                states.lockOn = !states.lockOn; // Toggle lock on state

                if (states.lockOnTarget == null) // If there is no target to lock onto...
                {
                    states.lockOn = false; // Toggle lock on state
                }

                cameraManager.lockOnTarget = states.lockOnTarget.transform; // Update CameraManager values to match with the StateManager
                cameraManager.lockedOn = states.lockOn;

            }
        }
    }
}

