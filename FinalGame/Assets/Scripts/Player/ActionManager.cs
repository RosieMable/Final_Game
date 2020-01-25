using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZaldensGambit
{
    public enum ActionInput
    {
        lightAttack, heavyAttack, block, specialAttack, dodgeRoll
    }

    public class ActionManager : MonoBehaviour
    {
        public List<Action> actionSlots = new List<Action>();

        public void Initialise()
        {
            // TBD
        }

        /// <summary>
        /// Constructor that sets up all action slots and inputs when added in the IDE.
        /// </summary>
        public ActionManager()
        {
            for (int i = 0; i < 5; i++)
            {
                Action action = new Action();
                action.input = (ActionInput)i;
                actionSlots.Add(action);
            }
        }

        /// <summary>
        /// Retrieves the input recorded in the StateManager and returns the corresponding actions data.
        /// </summary>
        public Action GetActionSlot(StateManager state)
        {
            ActionInput input = GetActionInput(state); // Record player input from StateManager
            return GetAction(input); // Retrieve corresponding action information based on input
        }

        /// <summary>
        /// Returns the corresponding action based on input.
        /// </summary>
        private Action GetAction(ActionInput input)
        {
            for (int i = 0; i < actionSlots.Count; i++)
            {
                if (actionSlots[i].input == input)
                {
                    return actionSlots[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an action input based on the values of the StateManager.
        /// </summary>
        public ActionInput GetActionInput(StateManager state)
        {
            if (state.lightAttack)
            {
                return ActionInput.lightAttack;
            }
            if (state.heavyAttack)
            {
                return ActionInput.heavyAttack;
            }
            if (state.block)
            {
                return ActionInput.block;
            }
            if (state.specialAttack)
            {
                return ActionInput.specialAttack;
            }
            if (state.dodgeRoll)
            {
                return ActionInput.dodgeRoll;
            }

            return ActionInput.lightAttack;
        }
    }

    [System.Serializable]
    public class Action
    {
        public ActionInput input; // Input that triggers this action.
        public AnimationClip desiredAnimation; //  Animation that the actioninput maps to.
    }
}
