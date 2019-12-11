using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IsThisDarkSouls
{
    public class EnemyStates : MonoBehaviour
    {
        public float health = 100;
        public bool isInvulnerable;
        public Animator charAnim;
        AnimatorHook animHook;
        public bool canMove;
        [HideInInspector] public float delta;
        public Rigidbody rigidBody;
        public bool isDead;

        private void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
            charAnim = GetComponent<Animator>();
            animHook = GetComponentInChildren<AnimatorHook>();

            if (animHook == false)
            {
                //print("Added AnimatorHook.cs to " + gameObject.name);
                //charAnim.gameObject.AddComponent<AnimatorHook>();
            }

            animHook.Initialise(null, this);
        }

        /// <summary>
        /// Deals damage to the NPCs health and plays hurt animations + applies root motion
        /// </summary>
        public void TakeDamage(float value)
        {
            if (isInvulnerable)
            {
                return;
            }

            health -= value;
            //charAnim.Play("hurt");
            //charAnim.applyRootMotion = true;
        }

        public void Update()
        {
            Tick(Time.deltaTime);
            //canMove = charAnim.GetBool("canMove");            

            if (health <= 0)
            {
                if (!isDead)
                {
                    isDead = true;
                    GetComponent<Collider>().enabled = false;
                    rigidBody.isKinematic = true;
                    print(gameObject.name + " died!");
                    //charAnim.Play("death");
                }
            }

            if (isInvulnerable) // If the NPC is currently invulnerable...
            {
                isInvulnerable = !canMove; // Assign invulnerable to the opposite state of 'canMove' - meaning when the character is capable of moving again, they are no longer invulnerable.
            }

            if (!canMove) // If the character can't move...
            {
                //charAnim.applyRootMotion = false; // Toggle root motion
            }
        }

        /// <summary>
        /// What occurs every tick, runs inside Update
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Tick(float deltaTime)
        {
            delta = deltaTime;
        }
    }    
}
