using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IsThisDarkSouls
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

        }

        public ActionManager()
        {
            for (int i = 0; i < 5; i++)
            {
                Action action = new Action();
                action.input = (ActionInput)i;
                actionSlots.Add(action);
            }
        }

        public Action GetActionSlot(StateManager state)
        {
            ActionInput input = GetActionInput(state);
            return GetAction(input);
        }

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
                return ActionInput.heavyAttack;
            }
            if (state.specialAttack)
            {
                return ActionInput.specialAttack;
            }
            if (state.dodgeRoll)
            {
                state.HandleDodgeRoll();
                return ActionInput.dodgeRoll;
            }

            return ActionInput.lightAttack;
        }
    }

    [System.Serializable]
    public class Action
    {
        public ActionInput input;
        public string desiredAnimation;
    }
}
