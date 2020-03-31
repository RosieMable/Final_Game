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

        public BaseSpirit spiritEquipped;

        [SerializeField]
        float HealthPlayerBase;

        [SerializeField]
        Transform AbilitySpawnPoint;

        [SerializeField]
        bool canUseAbility;

        ActionManager actionManager;

        private StateManager PlayerCharacter;

        delegate void OnSpiritChanged(BaseSpirit _spirit);
        OnSpiritChanged onSpiritChanged;

        public BaseSpirit[] DemoSpirits;

        RFX4_EffectEvent _EffectEvent;
        private void OnEnable()
        {
            onSpiritChanged += UIManager.Instance.ManageSpiritUI;
            onSpiritChanged += Spirits_PlayerModel.Instance.SetCorrectProps;
        }
        private void Awake()
        {
            actionManager = GetComponent<ActionManager>();
            _EffectEvent = GetComponentInChildren<RFX4_EffectEvent>();
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
        }

        public void ActiveAbility(BaseSpirit _CurrentSpirit)
        {
            if (canUseAbility)
            {
                //Active Ability logic
                UpdateVFXScript(_CurrentSpirit);
                AbilityCooldown(_CurrentSpirit);
            }

        }

        protected void UpdateVFXScript(BaseSpirit _EquippedSpirit)
        {
            //AnimationLogic for the active ability
            //hook up with RFX4 effect event 
            _EffectEvent.MainEffect = _EquippedSpirit.VFXPrefab;
        }

        void AbilityCooldown(BaseSpirit _EquippedSpirit)
        {
            canUseAbility = false;
            StartCoroutine(DoAfter(_EquippedSpirit.ActiveAbilityCooldown, () => canUseAbility = true));
        }

        public bool CheckAbilityCooldown()
        {
            return canUseAbility;
        }

        void OnEquipSpirit(BaseSpirit _SpiritToEquip)
        {
            PlayerCharacter = gameObject.GetComponent<StateManager>();

            if (_SpiritToEquip != null)
            {
                //Update UI
                onSpiritChanged?.Invoke(_SpiritToEquip);

                if (actionManager != null)
                {
                    actionManager.actionSlots[3].desiredAnimation = _SpiritToEquip.activeAbilityAnimation;
                }

                canUseAbility = true;

            }
            else
            {
                //Update UI
                onSpiritChanged?.Invoke(_SpiritToEquip);
            }
        }

        public void DoDamageToEnemy(GameObject enemy)
        {
            enemy.gameObject.GetComponent<Enemy>();
        }

        void PopulateCardStats(BaseSpirit spirit)
        {

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
