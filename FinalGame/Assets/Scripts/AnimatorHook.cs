﻿using System.Collections;
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

                charAnim.SetFloat("vertical", 0); // Reset vertical float to ensure animator does not return to a running animation afterwards.
                rigidBody.drag = 0;
                float multiplier = 1;

                if (states.grounded)
                {
                    Vector3 delta = charAnim.deltaPosition;
                    delta.y = 0;
                    Vector3 velocity = (delta * multiplier) / states.delta;
                    rigidBody.velocity = velocity;
                }                
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
            // TBD
        }

        /// <summary>
        /// Enables damage colliders
        /// </summary>
        public void OpenDamageCollider()
        {
            if (states == null)
            {
                if (enemyStates == null)
                {
                    return;
                }
                else
                {
                    print("OPEN AI DAMAGE COLLIDER");
                    enemyStates.weaponHook.OpenDamageCollider();
                }
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
                if (enemyStates == null)
                {
                    return;
                }
                else
                {
                    print("CLOSE AI DAMAGE COLLIDER");
                    enemyStates.weaponHook.CloseDamageCollider();
                }

                return;
            }

            states.weaponHook.CloseDamageCollider();
        }

        public void OpenComboPeriod()
        {
            if (states == null)
            {
                return;
            }

            states.listenForCombos = true;
        }

        public void CloseComboPeriod()
        {
            if (states == null)
            {
                return;
            }

            states.listenForCombos = false;
        }

        public void IgnoreInputs()
        {
            if (states == null)
            {
                if (enemyStates == null)
                {
                    return;
                }
                else
                {
                    enemyStates.inAction = true;
                    enemyStates.actionLockoutDuration += 2;
                }
                return;
            }

            states.inAction = true;
            states.actionLockoutDuration += 2;
        }

        public void ListenForInputs()
        {
            if (states == null)
            {
                if (enemyStates == null)
                {
                    return;
                }
                else
                {
                    enemyStates.actionLockoutDuration = 0.3f;
                }
                return;
            }

            states.actionLockoutDuration = 0.3f;
        }
    }
}


