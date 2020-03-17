using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    [RequireComponent(typeof(StateManager))]
    public class SpiritSystem : MonoBehaviour
    {
        [SerializeField]
        private BaseSpirit spirit;

        [SerializeField]
        private BaseSpirit spiritEquipped;

        [SerializeField]
        float HealthPlayerBase;

        [SerializeField]
        bool canUseAbility;

        private StateManager PlayerCharacter;

        delegate void OnSpiritChanged(BaseSpirit _spirit);
        OnSpiritChanged onSpiritChanged;

        public BaseSpirit[] DemoSpirits;
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
                spiritEquipped = DemoSpirits[0];
                OnEquipSpirit(spiritEquipped);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                spiritEquipped = DemoSpirits[1];
                OnEquipSpirit(spiritEquipped);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                spiritEquipped = DemoSpirits[2];
                OnEquipSpirit(spiritEquipped);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                spiritEquipped = DemoSpirits[3];
                OnEquipSpirit(spiritEquipped);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                spiritEquipped = DemoSpirits[4];
                OnEquipSpirit(spiritEquipped);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                OnEquipSpirit(spirit);
            }


            if (Input.GetKeyDown(KeyCode.Q))
            {
                ActiveAbility(spiritEquipped);
            }
        }

        protected void ActiveAbility(BaseSpirit _CurrentSpirit)
        {
            if (canUseAbility)
            {
                //Active Ability logic
                AnimationLogic(_CurrentSpirit);
                AbilityCooldown(_CurrentSpirit);
            }

        }

        protected void AnimationLogic(BaseSpirit _EquippedSpirit)
        {
            //AnimationLogic for the active ability
        }

        void AbilityCooldown(BaseSpirit _EquippedSpirit)
        {
            canUseAbility = false;
            StartCoroutine(DoAfter(_EquippedSpirit.ActiveAbilityCooldown, () => canUseAbility = true));
        }



        void OnEquipSpirit(BaseSpirit _SpiritToEquip)
        {
            PlayerCharacter = gameObject.GetComponent<StateManager>();

            if (_SpiritToEquip != null)
            {
                //Update UI
                onSpiritChanged?.Invoke(_SpiritToEquip);

            }
            else
            {
                //Update UI
                onSpiritChanged?.Invoke(_SpiritToEquip);
            }


        }

        IEnumerator DoAfter(float _delayTime, System.Action _actionToDo)
        {
            yield return new WaitForSecondsRealtime(_delayTime);
            _actionToDo();
        }

        private void OnDisable()
        {
            //   UIDelegate -= UIManager.Instance.ManageSpiritUI;
        }
    }
}
