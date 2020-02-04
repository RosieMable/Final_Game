using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spirit", menuName = "SpiritCreation")] 
public class Spirit_ScriptableObj : ScriptableObject
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

    public enum SpiritLevel
    {
        Lv1,
        Lv2,
        Lv3
    }

    public SpiritLevel spiritLevel;

    public SpiritClass spiritClass;


    public Sprite _spiritSprite;

    public Sprite SpiritSprite { get { return _spiritSprite; } }

    public float _activeAbilityCooldown;

    public int HealthModifier;

    //Implement variables for player's stats? Maybe a struct of player stats?


}
