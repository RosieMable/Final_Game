﻿using System.Collections;
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

        UpdateShieldMesh shieldMesh;

        UpdateSwordMesh swordMesh;

        [SerializeField]
        bool DebugTesting;

        private void OnEnable()
        {
            //onSpiritChanged += UIManager.Instance.ManageSpiritUI; // - Moved to start as UIManager Instance reference is not initialised when OnEnable() runs
            onSpiritChanged += Spirits_PlayerModel.Instance.SetCorrectProps;
        }
        private void Awake()
        {
            actionManager = GetComponent<ActionManager>();
            _EffectEvent = GetComponentInChildren<RFX4_EffectEvent>();

            swordMesh = GetComponentInChildren<UpdateSwordMesh>();
            shieldMesh = GetComponentInChildren<UpdateShieldMesh>();
        }

        //for now it is going to be in update, but once the inventory system is on, this should happen when "equipping" the spirit
        private void Start()
        {
            onSpiritChanged += UIManager.Instance.ManageSpiritUI;
            OnEquipSpirit(spirit);
        }

        private void Update()
        {
            if (DebugTesting)
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
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    spiritEquipped = DemoSpirits[5];
                    OnEquipSpirit(spiritEquipped);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    OnEquipSpirit(spirit);
                }
            }
           

            if(spiritEquipped != null)
            {
                if (spiritEquipped.abilityEvoked == false)
                {
                    swordMesh.RevertToOriginalMat();
                    shieldMesh.RevertToOriginalMat();
                }

            }



        }

        public void ActiveAbility(BaseSpirit _CurrentSpirit)
        {
            if (canUseAbility)
            {
                PlayAbilitySound(_CurrentSpirit);
                //Active Ability logic
                UpdateVFXScript(_CurrentSpirit);
                AbilityCooldown(_CurrentSpirit);
                _CurrentSpirit.abilityEvoked = true;
            }

        }

        protected void UpdateVFXScript(BaseSpirit _EquippedSpirit)
        {
            //AnimationLogic for the active ability

            switch (_EquippedSpirit.spiritClass)
            {
                case BaseSpirit.SpiritClass.Cleric:
                    //hook up with RFX4 effect event 
                    swordMesh.RevertToOriginalMat();
                    shieldMesh.RevertToOriginalMat();
                    _EffectEvent.MainEffect = _EquippedSpirit.VFXPrefab;
                    break;
                case BaseSpirit.SpiritClass.Paladin:
                    swordMesh.RevertToOriginalMat();
                    shieldMesh.RevertToOriginalMat();
                    shieldMesh.VFXPrefab = _EquippedSpirit.VFXPrefab;
                    shieldMesh.UpdateMeshEffect();
                    break;
                case BaseSpirit.SpiritClass.Ranger:
                    _EffectEvent.MainEffect = _EquippedSpirit.VFXPrefab;
                    break;
                case BaseSpirit.SpiritClass.Berserker:
                    swordMesh.RevertToOriginalMat();
                    shieldMesh.RevertToOriginalMat();
                    swordMesh.VFXPrefab = _EquippedSpirit.VFXPrefab;
                    _EffectEvent.MainEffect = null;
                    swordMesh.UpdateMeshEffect();
                    break;
                case BaseSpirit.SpiritClass.Sellsword:
                    swordMesh.RevertToOriginalMat();
                    shieldMesh.RevertToOriginalMat();
                    swordMesh.VFXPrefab = _EquippedSpirit.VFXPrefab;
                    swordMesh.UpdateMeshEffect();
                    break;
                case BaseSpirit.SpiritClass.Mage:
                    //hook up with RFX4 effect event 
                    swordMesh.RevertToOriginalMat();
                    shieldMesh.RevertToOriginalMat();
                    _EffectEvent.MainEffect = _EquippedSpirit.VFXPrefab;
                    break;
            }

        }

        void AbilityCooldown(BaseSpirit _EquippedSpirit)
        {
            canUseAbility = false;
            UIManager.Instance.UpdateAbilityBar(_EquippedSpirit.ActiveAbilityCooldown);
            StartCoroutine(DoAfter(_EquippedSpirit.ActiveAbilityCooldown, () => canUseAbility = true));

        }

        public bool CheckAbilityCooldown()
        {
            return canUseAbility;
        }

        public void OnEquipSpirit(BaseSpirit _SpiritToEquip)
        {
            PlayerCharacter = gameObject.GetComponent<StateManager>();

            if (_SpiritToEquip != null)
            {
                //Update UI
                onSpiritChanged?.Invoke(_SpiritToEquip);

                if (actionManager != null)
                {
                    actionManager.actionSlots[2].desiredAnimation = _SpiritToEquip.activeAbilityAnimation;
                }

                canUseAbility = true;

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

        public void ClericHealing()
        {
            PlayerCharacter.RestoreHealth(spiritEquipped.HealthModifier);
            print("Healed for " + spiritEquipped.HealthModifier);
        }


        public void PaladinDamage()
        {
            // Deal damage to characters hit in front, then apply stun

            RaycastHit[] hits = Physics.BoxCastAll(transform.position + transform.forward, transform.localScale, transform.forward, transform.rotation, 5);
            List<Enemy> enemiesHit = new List<Enemy>();

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject.GetComponent<Enemy>())
                {
                    enemiesHit.Add(hit.collider.gameObject.GetComponent<Enemy>());
                }
            }

            foreach (Enemy enemy in enemiesHit)
            {
                enemy.TakeDamage(spiritEquipped.AOEDamageModifier);
                enemy.ApplyStun(spiritEquipped.StunModifier);
            }

            print("Deal Damage Paladin!");
        }

        public void MageDamage(Vector3 position)
        {
            // Deal damage to character hit, then deal AoE damage to everyone else within a radius

            Collider[] collidersInRange = Physics.OverlapSphere(position, 2.5f);
            List<Enemy> enemiesHit = new List<Enemy>();

            foreach (Collider collider in collidersInRange)
            {
                if (collider.GetComponent<Enemy>())
                {
                    enemiesHit.Add(collider.GetComponent<Enemy>());
                }
            }

            foreach (Enemy enemy in enemiesHit)
            {
                enemy.TakeDamage(spiritEquipped.AOEDamageModifier);
            }

            print("Deal Damage Mage!");
        }

        public void RangerDamage(Enemy enemy)
        {
            // Deal damage to character hit and stun
            enemy.TakeDamage(spiritEquipped.DamageModifier);
            enemy.ApplyStun(spiritEquipped.StunModifier);

            print("Deal Damage Ranger!");
        }

        private void PlayAbilitySound(BaseSpirit spirit)
        {
            if(spirit.AbilitySound != null)
            {
                AudioSource source = GetComponentInChildren<AudioSource>();
                source.PlayOneShot(spirit.AbilitySound);
            }
        }
        private void OnDisable()
        {
            //   UIDelegate -= UIManager.Instance.ManageSpiritUI;
        }
    }
}
