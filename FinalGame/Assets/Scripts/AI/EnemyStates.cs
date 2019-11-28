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

            if (isInvulnerable)
            {
                isInvulnerable = !canMove;
            }

            if (!canMove)
            {
                //charAnim.applyRootMotion = false;
            }
        }

        public void Tick(float deltaTime)
        {
            delta = deltaTime;
        }

    }
}
