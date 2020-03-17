using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spirits_PlayerModel : Singleton<Spirits_PlayerModel>
{

    [SerializeField]
    private GameObject BerserkerProps, ClericProps, PaladinProps, MageProps, RangerProps, SellswordProps;

    public void SetCorrectProps(BaseSpirit _EquippedSpirit)
    {
        if (_EquippedSpirit!=null)
        {
            switch (_EquippedSpirit.spiritClass)
            {
                case BaseSpirit.SpiritClass.Cleric:
                    BerserkerProps.SetActive(false);
                    ClericProps.SetActive(true);
                    PaladinProps.SetActive(false);
                    MageProps.SetActive(false);
                    RangerProps.SetActive(false);
                    SellswordProps.SetActive(false);
                    print("My class is " + _EquippedSpirit.spiritClass);
                    break;
                case BaseSpirit.SpiritClass.Paladin:
                    BerserkerProps.SetActive(false);
                    ClericProps.SetActive(false);
                    PaladinProps.SetActive(true);
                    MageProps.SetActive(false);
                    RangerProps.SetActive(false);
                    SellswordProps.SetActive(false);
                    print("My class is " + _EquippedSpirit.spiritClass);
                    break;
                case BaseSpirit.SpiritClass.Ranger:
                    BerserkerProps.SetActive(false);
                    ClericProps.SetActive(false);
                    PaladinProps.SetActive(false);
                    MageProps.SetActive(false);
                    RangerProps.SetActive(true);
                    SellswordProps.SetActive(false);
                    print("My class is " + _EquippedSpirit.spiritClass);
                    break;
                case BaseSpirit.SpiritClass.Berserker:
                    BerserkerProps.SetActive(true);
                    ClericProps.SetActive(false);
                    PaladinProps.SetActive(false);
                    MageProps.SetActive(false);
                    RangerProps.SetActive(false);
                    SellswordProps.SetActive(false);
                    print("My class is " + _EquippedSpirit.spiritClass);
                    break;
                case BaseSpirit.SpiritClass.Sellsword:
                    BerserkerProps.SetActive(false);
                    ClericProps.SetActive(false);
                    PaladinProps.SetActive(false);
                    MageProps.SetActive(false);
                    RangerProps.SetActive(false);
                    SellswordProps.SetActive(true);
                    print("My class is " + _EquippedSpirit.spiritClass);
                    break;
                case BaseSpirit.SpiritClass.Mage:
                    BerserkerProps.SetActive(false);
                    ClericProps.SetActive(false);
                    PaladinProps.SetActive(false);
                    MageProps.SetActive(true);
                    RangerProps.SetActive(false);
                    SellswordProps.SetActive(false);
                    print("My class is " + _EquippedSpirit.spiritClass);
                    break;
            }
        }
        else
        {
            BerserkerProps.SetActive(false);
            ClericProps.SetActive(false);
            PaladinProps.SetActive(false);
            MageProps.SetActive(false);
            RangerProps.SetActive(false);
            SellswordProps.SetActive(false);
            print("My class is husk");
        }
    }
}