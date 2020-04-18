using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardInventory : MonoBehaviour
{
    public static CardInventory instance;

    [SerializeField] private int dungeonCardLimit = 50;
    [SerializeField] private int spiritCardLimit = 20;

    public bool inventoryOpen;

    [SerializeField] private Transform[] cardPositions;
    [SerializeField] private Transform cardSelected;
    private TextMeshProUGUI cardInfo;
    private Camera camera;
    private GameObject inventoryPanel;
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

        DontDestroyOnLoad(gameObject);
        Init();
    }

    public void Init()
    {
        cardPositions[0] = GameObject.Find("Card 1").transform;
        cardPositions[1] = GameObject.Find("Card 2").transform;
        cardPositions[2] = GameObject.Find("Card 3").transform;
        cardPositions[3] = GameObject.Find("Card 4").transform;
        cardSelected = GameObject.Find("SelectedCard").transform;
        inventoryPanel = GameObject.Find("InventoryPanel");
        camera = FindObjectOfType<Camera>();
        cardInfo = GameObject.Find("CardInfo").GetComponent<TextMeshProUGUI>();
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

        ToggleInventory();
    }

    private void Update()
    {
        if (cardPositions[0] == null)
        {
            cardPositions[0] = GameObject.Find("Card 1").transform;
            cardPositions[1] = GameObject.Find("Card 2").transform;
            cardPositions[2] = GameObject.Find("Card 3").transform;
            cardPositions[3] = GameObject.Find("Card 4").transform;
            cardSelected = GameObject.Find("SelectedCard").transform;
            inventoryPanel = GameObject.Find("InventoryPanel");
            camera = FindObjectOfType<Camera>();
            cardInfo = GameObject.Find("CardInfo").GetComponent<TextMeshProUGUI>();
            ToggleInventory();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();

            inventoryOpen = !inventoryOpen;

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);

        for (int i = 0; i < cardPositions.Length; i++)
        {
            if (spiritCards[i] != null)
            {
                cardPositions[i].GetComponent<Image>().sprite = spiritCards[i]._spiritSpriteCardback;
                cardPositions[i].GetComponent<InventoryCardUI>().spirit = spiritCards[i];
            }
        }
    }

    public void SelectCard(Transform selectedCardPosition)
    {
        InventoryCardUI cardSelectedsInfo = cardSelected.GetComponent<InventoryCardUI>();

        for (int i = 0; i < cardPositions.Length; i++)
        {
            if (selectedCardPosition == cardPositions[i])
            {
                cardSelectedsInfo.spirit = cardPositions[i].GetComponent<InventoryCardUI>().spirit;
                cardSelected.GetComponent<Image>().sprite = cardSelectedsInfo.spirit._spiritSpriteCardback;

                cardInfo.text = "Name: " + cardSelectedsInfo.spirit.SpiritName
                    + "\n Class: " + cardSelectedsInfo.spirit.spiritClass
                    + "\n \n Bio: " + cardSelectedsInfo.spirit.SpiritDescription;
                return;
            }
        }
    }

    private int minimum = 0;

    public void SortLeft()
    {
        Debug.Log("Minimum beforehand: " + minimum);

        if (minimum > 0)
        {
            minimum--;
        }

        Debug.Log("Minimum after: " + minimum);

        int _cardToDisplay = minimum;

        for (int i = 0; i < cardPositions.Length; i++)
        {
            if (_cardToDisplay <= spiritCards.Count - 1 && spiritCards[_cardToDisplay] != null)
            {
                cardPositions[i].GetComponent<Image>().sprite = spiritCards[_cardToDisplay]._spiritSpriteCardback;
                cardPositions[i].GetComponent<InventoryCardUI>().spirit = spiritCards[_cardToDisplay];
                Debug.Log("Display card: " + _cardToDisplay);
                _cardToDisplay++;
                Debug.Log("Increment to value: " + _cardToDisplay);
            }
        }
    }

    public void SortRight()
    {
        // Check if there is a spiritcard in inventory that is not currently displayed that is greater than the value of the current set
        // If so, readjust display
        Debug.Log("Minimum beforehand: " + minimum);

        if (minimum >= 0 && minimum <= spiritCards.Count - 5)
        {
            minimum++;
        }

        Debug.Log("Minimum after: " + minimum);

        int _cardToDisplay = minimum;

        for (int i = 0; i < cardPositions.Length; i++)
        {
            if (_cardToDisplay <= spiritCards.Count - 1 && spiritCards[_cardToDisplay] != null)
            {
                cardPositions[i].GetComponent<Image>().sprite = spiritCards[_cardToDisplay]._spiritSpriteCardback;
                cardPositions[i].GetComponent<InventoryCardUI>().spirit = spiritCards[_cardToDisplay];
                Debug.Log("Display card: " + _cardToDisplay);
                _cardToDisplay++;
                Debug.Log("Increment to value: " + _cardToDisplay);
            }
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
            //spiritDeckCallback.Invoke(spirit); // Invoke delegate passing the spirit added to the inventory
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
            //dungeonDeckCallback.Invoke(dungeonCard); // Invoke delegate passing the dungeonCard added to the inventory
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
