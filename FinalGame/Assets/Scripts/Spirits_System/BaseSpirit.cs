using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[CreateAssetMenu(fileName = "Spirit", menuName = "SpiritCreation")] 
public class BaseSpirit : ScriptableObject
{
    [SerializeField]
    protected string _spiritName;

    public string SpiritName {  get { return _spiritName; } private set { SpiritName = _spiritName; } }

    [TextArea(3, 10)] [SerializeField]
    protected string _spiritDescription;

    public string SpiritDescription { get { return _spiritDescription; } private set { SpiritDescription = _spiritDescription; } }

    public enum SpiritClass
    {
        Cleric,
        Paladin,
        Ranger,
        Berserker,
        Sellsword,
        Mage
    }

    public enum SpiritTier
    {
        Tier1,
        Tier2,
        Tier3
    }

    public SpiritTier spiritTier;

    public SpiritClass spiritClass;

    public Sprite _spiritSpriteIcon;

    public Sprite SpiritSpriteIcon { get { return _spiritSpriteIcon; } }

    public Sprite _spiritSpriteCardback;

    public Sprite SpiritSpriteCardback { get { return _spiritSpriteCardback; } }

    /// <summary>
    /// Active ability cooldown expressed in seconds.
    /// </summary>
    [SerializeField]
    private float _activeAbilityCooldown;

    public float ActiveAbilityCooldown { get { return _activeAbilityCooldown; } private set { ActiveAbilityCooldown = _activeAbilityCooldown; } }

    public GameObject VFXPrefab;

    public AnimationClip activeAbilityAnimation;

    
}
