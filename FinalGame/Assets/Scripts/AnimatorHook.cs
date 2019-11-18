using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IsThisDarkSouls
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(StateManager))]
    public class AnimatorHook : MonoBehaviour
    {
        private Animator charAnim;
        private StateManager states;

        public void Initialise(StateManager stateManager)
        {
            states = stateManager;
            charAnim = stateManager.charAnim;
        }

        //private void OnAnimatorMove()
        //{
        //    if (states.canMove)
        //    {
        //        return;
        //    }

        //    states.rigidBody.drag = 0;
        //    float multiplier = 1;

        //    Vector3 delta = charAnim.deltaPosition;
        //    delta.y = 0;
        //    Vector3 velocity = (delta * multiplier) / states.delta;
        //    states.rigidBody.velocity = velocity;
        //}

        public void LateTick()
        {

        }
    }
}


