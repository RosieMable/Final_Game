using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZaldensGambit;

[RequireComponent(typeof(StateManager))]
public class SpiritSystem : MonoBehaviour
{
    [SerializeField]
    private Spirit_ScriptableObj spirit;

    [SerializeField]
    float HealthPlayerBase;

    private StateManager PlayerCharacter;

    delegate void OnSpiritChanged(Spirit_ScriptableObj _spirit);
    OnSpiritChanged onSpiritChanged;

    public Spirit_ScriptableObj[] DemoSpirits;
    private void OnEnable()
    {
        onSpiritChanged += UIManager.Instance.ManageSpiritUI;
        onSpiritChanged += Spirits_PlayerModel.Instance.SetCorrectProps;
    }

    //for now it is going to be in update, but once the inventory system is on, this should happen when "equipping" the spirit
    private void Start()
    {

        OnEquipSpirit(spirit);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnEquipSpirit(DemoSpirits[0]);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnEquipSpirit(DemoSpirits[1]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnEquipSpirit(DemoSpirits[2]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            OnEquipSpirit(DemoSpirits[3]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            OnEquipSpirit(DemoSpirits[4]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            OnEquipSpirit(spirit);
        }
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
            PlayerCharacter.currentHealth = HealthPlayerBase;
            PlayerCharacter.currentHealth = PlayerCharacter.currentHealth + _EquippedSpirit.HealthModifier;
            print(PlayerCharacter.currentHealth);
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
            onSpiritChanged?.Invoke(_SpiritToEquip);

        }
        else
        {
            //Update UI
            onSpiritChanged?.Invoke(_SpiritToEquip);
        }


    }

    private void OnDisable()
    {
     //   UIDelegate -= UIManager.Instance.ManageSpiritUI;
    }
}
