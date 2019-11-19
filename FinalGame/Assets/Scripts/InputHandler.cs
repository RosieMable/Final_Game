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
        }

        void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;
            GetInput();
            UpdateStates();
            states.FixedTick(delta);
            cameraManager.Tick(delta);
        }

        private void GetInput()
        {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
            lightAttackInput = Input.GetKeyDown(KeyCode.Mouse0);
            heavyAttackInput = Input.GetKeyDown(KeyCode.Mouse1);
            dodgeRollInput = Input.GetKeyDown(KeyCode.LeftControl);
            lockOnInput = Input.GetKeyDown(KeyCode.Tab);
        }

        /// <summary>
        /// Updates
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

            if (lockOnInput)
            {
                states.lockOn = !states.lockOn;

                if (states.lockOnTarget == null)
                {
                    states.lockOn = false;
                }

                cameraManager.lockOnTarget = states.lockOnTarget.transform;
                cameraManager.lockedOn = states.lockOn;

            }
        }
    }
}

