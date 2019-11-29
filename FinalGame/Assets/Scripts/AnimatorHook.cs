using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IsThisDarkSouls
{
    public class AnimatorHook : MonoBehaviour
    {
        private Animator charAnim;
        private StateManager states;
        private EnemyStates enemyStates;
        private Rigidbody rigidBody;

        /// <summary>
        /// Performs initial setup based on if the character is a player or enemy.
        /// </summary>
        public void Initialise(StateManager stateManager, EnemyStates enemyStateManager)
        {
            states = stateManager;
            enemyStates = enemyStateManager;

            if (states != null)
            {
                charAnim = stateManager.charAnim;
                rigidBody = states.rigidBody;
            }

            if (enemyStates != null)
            {
                charAnim = enemyStates.charAnim;
                rigidBody = enemyStates.rigidBody;
            }
        }

        /// <summary>
        /// Runs when animated character is in motion, applies rootmotion physics.
        /// </summary>
        private void OnAnimatorMove()
        {
            if (states == null && enemyStates == null)
            {
                Debug.Log("No statemanager found for AnimiatorHook.cs !");
                return;
            }

            if (rigidBody == null)
            {
                return;
            }

            if (states != null)
            {
                if (states.canMove)
                {
                    return;
                }

                rigidBody.drag = 0;
                float multiplier = 1;

                Vector3 delta = charAnim.deltaPosition;
                delta.y = 0;
                Vector3 velocity = (delta * multiplier) / states.delta;
                rigidBody.velocity = velocity;
            }

            if (enemyStates != null)
            {
                if (enemyStates.canMove)
                {
                    return;
                }

                rigidBody.drag = 0;
                float multiplier = 1;

                Vector3 delta = charAnim.deltaPosition;
                delta.y = 0;
                Vector3 velocity = (delta * multiplier) / enemyStates.delta;
                rigidBody.velocity = velocity;
            }            
        }

        public void LateTick()
        {

        }

        /// <summary>
        /// Enables damage colliders
        /// </summary>
        public void OpenDamageCollider()
        {
            if (states == null)
            {
                return;
            }

            states.weaponHook.OpenDamageCollider();
        }

        /// <summary>
        /// Disables damage colliders
        /// </summary>
        public void CloseDamageCollider()
        {
            if (states == null)
            {
                return;
            }

            states.weaponHook.CloseDamageCollider();
        }

        public void IgnoreInputs()
        {
            if (states == null)
            {
                return;
            }

            states.inAction = true;
            states.actionLockoutDuration += 10;
            print("Ignore");
        }

        public void ListenForInputs()
        {
            if (states == null)
            {
                return;
            }

            states.actionLockoutDuration = 0.3f;
            print("Listen");
        }
    }
}


