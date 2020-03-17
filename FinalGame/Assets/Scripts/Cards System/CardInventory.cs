using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInventory : MonoBehaviour
{
    public static CardInventory instance;
    [SerializeField] private int dungeonCardLimit = 50;
    [SerializeField] private int spiritCardLimit = 20;

    public List<BaseSpirit> spiritCards;
    public List<Card_ScriptableObj> dungeonCards;
    public delegate void SpiritDeckUpdated(BaseSpirit spirit);
    public SpiritDeckUpdated spiritDeckCallback;
    public delegate void DungeonDeckUpdated(Card_ScriptableObj dungeonCard);
    public DungeonDeckUpdated dungeonDeckCallback;

    private void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        foreach (BaseSpirit spirit in spiritCards)
        {
            //print("Spirit in inventory: " + spirit.name);
        }

        foreach (Card_ScriptableObj dungeonCard in dungeonCards)
        {
            //print("Dungeon card in inventory: " + dungeonCard.name);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            RemoveSpiritCardFromInventory(spiritCards[0]);
            RemoveDungeonCardFromInventory(dungeonCards[0]);
        }
    }

    /// <summary>
    /// Attempts to add a spirit to the list of spirits stored.
    /// </summary>
    public bool AddSpiritCardToInventory(BaseSpirit spirit)
    {
        if (spiritCards.Count + 1 <= spiritCardLimit)
        {
            spiritCards.Add(spirit);
            spiritDeckCallback.Invoke(spirit); // Invoke delegate passing the spirit added to the inventory
            return true;
        }

        print("Spirit could not be added to the deck, too many cards!");
        return false;
    }

    /// <summary>
    /// Attempts to add a dungeon card to the list of dungeon cards stored.
    /// </summary>
    public bool AddDungeonCardToInventory(Card_ScriptableObj dungeonCard)
    {
        if (dungeonCards.Count + 1 <= dungeonCardLimit)
        {
            dungeonCards.Add(dungeonCard);
            dungeonDeckCallback.Invoke(dungeonCard); // Invoke delegate passing the dungeonCard added to the inventory
            return true;
        }

        print("Dungeon Card could not be added to the deck, too many cards!");
        return false;
    }

    /// <summary>
    /// Remove the chosen spirit from the inventory.
    /// </summary>
    public void RemoveSpiritCardFromInventory(BaseSpirit spirit)
    {
        spiritCards.Remove(spirit);
        print(spirit.name + " removed from inventory");
    }

    /// <summary>
    /// Remove the chosen dungeon card from the inventory.
    /// </summary>
    public void RemoveDungeonCardFromInventory(Card_ScriptableObj dungeonCard)
    {
        dungeonCards.Remove(dungeonCard);
        print(dungeonCard.name + " removed from inventory");
    }

    /// <summary>
    /// Checks if the size of either list/deck exceeds their intended limit.
    /// </summary>
    private void CheckDeckSize()
    {
        if (spiritCards.Count > spiritCardLimit)
        {
            // TODO: Prompt removal of spirit cards
        }

        if (dungeonCards.Count > dungeonCardLimit)
        {
            // TODO: Prompt removal of dungeon cards
        }
    }
}
