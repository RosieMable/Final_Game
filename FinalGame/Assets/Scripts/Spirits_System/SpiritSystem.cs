using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IsThisDarkSouls;

[RequireComponent(typeof(StateManager))]
public class SpiritSystem : MonoBehaviour
{
    [SerializeField]
    private Spirit_ScriptableObj spirit;

    private StateManager PlayerCharacter;

    delegate void UIChecks(Spirit_ScriptableObj _spirit);
    UIChecks UIDelegate;


    private void OnEnable()
    {
        UIDelegate += UIManager.Instance.ManageSpiritUI;
    }

    //for now it is going to be in update, but once the inventory system is on, this should happen when "equipping" the spirit
    private void Start()
    {

        OnEquipSpirit(spirit);
    }

    protected void ActiveAbility()
    {
        //Active Ability logic
        AnimationLogic();
    }

    protected void AnimationLogic()
    {
        //AnimationLogic for the active ability
    }

    protected void PassiveAbility(Spirit_ScriptableObj _EquippedSpirit)
    {
        if (PlayerCharacter)
        {
            PlayerCharacter.health = PlayerCharacter.health + _EquippedSpirit.HealthModifier;
            print(PlayerCharacter.health);
        }
        else
        {
            throw new System.Exception("No PlayerCharacter found!");
        }
    }

    void OnEquipSpirit(Spirit_ScriptableObj _SpiritToEquip)
    {
        PlayerCharacter = gameObject.GetComponent<StateManager>();

        if (_SpiritToEquip != null)
        {
            PassiveAbility(_SpiritToEquip);

            //Update UI
            UIDelegate?.Invoke(_SpiritToEquip);

        }
        else
        {
            //Update UI
            UIDelegate?.Invoke(_SpiritToEquip);
        }


    }

    private void OnDisable()
    {
     //   UIDelegate -= UIManager.Instance.ManageSpiritUI;
    }
}
