using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using ZaldensGambit;

public class DungeonCardSystem : Singleton<DungeonCardSystem>
{
    [Header("Dungeon Cards Section")]
    //public List<Card_ScriptableObj> ownedDungeonCards; //reference to the dungeoncards that the player owns (inventory system ref)

    private CardInventory cardInventory;

    public List<Card_ScriptableObj> allDungeonCards; //Reference to all the cards within the game

    private List<Card_ScriptableObj> _possibleDungeonCardsReward; //reference to the difference between all the cards within the game and the player's owned one, from there the reward is calculated

    [SerializeField]
    private List<Card_ScriptableObj> _cardReward;
    public List<Card_ScriptableObj> DungeonCardsReward { get { return _cardReward; } private set { DungeonCardsReward = _cardReward; } }

    private List<Card_ScriptableObj> _drawnCards;

    public List<Card_ScriptableObj> DrawnCards { get { return _drawnCards; } private set { DrawnCards = _drawnCards; } }

    public int GlobalAmountCD;

    int nClick = 0;

    [Header("Spirit Cards Section")]
   // public List<BaseSpirit> ownedSpirits; //reference to the spirits that the player owns (inventory system ref)

    public List<BaseSpirit> allSpirits; //Reference to all spirits within the game

    [SerializeField]
    private List<BaseSpirit> _possibleSpiritRewards; //reference to the difference between all the cards within the game and the player's owned one, from there the reward is calculated

    private List<BaseSpirit> _spiritReward;
    public List<BaseSpirit> SpiritCardsReward { get { return _spiritReward; } private set { SpiritCardsReward = _spiritReward; } }

    [Header("Dungeon Prefabs Section")]
    [SerializeField]
    private Transform _spawnLocation;

    [SerializeField]
    private List<GameObject> ThreeRoomsDungeonPrefab;

    [SerializeField]
    private List<GameObject> SixRoomsDungeonPrefab;

    [SerializeField]
    private List<GameObject> NineRoomsDungeonPrefab;

    AsyncOperation async;

    public AsyncOperation asyncDungeonScene { get { return async; } private set { asyncDungeonScene = async; } }

    AsyncOperation asyncHub;

    public AsyncOperation asyncHubScene {  get { return asyncHub; } private set { asyncHubScene = asyncHub; } }

    [SerializeField]
    Portal portalToDungeon;

    [SerializeField]
    GameObject portalDungeon, portalArena;

    [SerializeField]
    string mainSceneName = "AlpaHub";

    bool inHubScene;

    [Header("Temp Debug")]
    #region Temp for debug

    public bool isDebug;

    public Text debugCardsDrawn;

    public Text debugCardRewards;

    public Text debugCardOwned;

    public Text debugAllCards;
    #endregion

    protected override void Awake()
    {

        if (SceneManager.GetActiveScene().name == mainSceneName)
        {
            portalToDungeon = portalDungeon.GetComponentInChildren<Portal>();
        }


        base.Awake();
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        cardInventory = CardInventory.instance;

        string temp = "";

        portalDungeon.SetActive(false);


        foreach (var item in cardInventory.dungeonCards)
        {
            Debug.Log(item.CardName);
            temp += item.CardName + "\n";
        }
        if (isDebug)
            debugCardOwned.text = "Dungeon Cards Owned: \n\n" + temp;

        string temp02 = "";

        foreach (var item in allDungeonCards)
        {
            Debug.Log(item.CardName);
            temp02 += item.CardName + "\n";
        }

        if (isDebug)
            debugAllCards.text = "All Dungeon Cards : \n\n" + temp02;

    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == mainSceneName)
        {
            inHubScene = true;
        }
        else
        {
            inHubScene = false;
        }
    }

    public void SetGlobalAmount(Text text)
    {


        nClick++;

        print(nClick);

        
        if (nClick >= 0)
        {
            GlobalAmountCD = 3;
            DrawCards(3);
            text.text = "x3";
            if (inHubScene)
            {
                portalDungeon.SetActive(true);
                portalToDungeon.sceneToLoad = "3_Room_Dungeon";
            }
        }
        if (nClick > 3)
        {
            nClick = 1;
            GlobalAmountCD = 3;
            DrawCards(3);
            text.text = "x3";
            if (inHubScene)
            {
                portalDungeon.SetActive(true);
                portalToDungeon.sceneToLoad = "3_Room_Dungeon";
            }
        }
        if (nClick == 2)
        {
            GlobalAmountCD = 6;
            DrawCards(6);
            text.text = "x6";
            if (inHubScene)
            {
                portalDungeon.SetActive(true);
                portalToDungeon.sceneToLoad = "6_Room_Dungeon";
            }
        }
        if (nClick == 3)
        {
            GlobalAmountCD = 9;
            DrawCards(9);
            text.text = "x9";
            if (inHubScene)
            {
                portalDungeon.SetActive(true);
                portalToDungeon.sceneToLoad = "9_Room_Dungeon";
            }
        }


    }

    public void DrawCards(int amount)
    {
        //From the dungeon cards, draw some randomly based on the amount
        //add them to the drawn cards
        string temp = "";

        _drawnCards = GetRandomItemsFromList<Card_ScriptableObj>(cardInventory.dungeonCards, amount);

        foreach (var item in _drawnCards)
        {
            Debug.Log(item.CardName);
            temp += item.CardName + "\n";
        }
        if(isDebug)
        debugCardsDrawn.text = "Cards drawn: \n\n" + temp;
    }

    public void CreateDungeon()
    {
        //_spawnLocation = GameObject.FindGameObjectWithTag("SpawnPoint").transform;

        //Based on drawn cards, spawn the corrisponding dungeon prefab
        if (_drawnCards.Count != 0 && _spawnLocation != null)
        {
            if (_drawnCards.Count == 3)
            {
                GameObject.Instantiate(ThreeRoomsDungeonPrefab[Random.Range(0, ThreeRoomsDungeonPrefab.Count)], _spawnLocation);
            }
           else if (_drawnCards.Count == 6)
            {
                GameObject.Instantiate(SixRoomsDungeonPrefab[Random.Range(0, SixRoomsDungeonPrefab.Count)], _spawnLocation);
            }
           else if (_drawnCards.Count == 9)
            {
                GameObject.Instantiate(NineRoomsDungeonPrefab[Random.Range(0, NineRoomsDungeonPrefab.Count)], _spawnLocation);
            }
        }
    }

    public void CalculateRewards()
    {
        //Based on drawn cards, spawn the corrisponding dungeon prefab
        if (_drawnCards.Count != 0 && allSpirits.Count != 0)
        {
             if (_drawnCards.Count == 9)
            {
                //Rewards for 9 Rooms Dungeons
                GetDungeonCardsReward(9);
                string temp = "";
                foreach (var item in _cardReward)
                {
                    Debug.Log(item.CardName);
                    temp += item.CardName + "\n";
                }
                // debugCardRewards.text = "Cards Reward: \n\n" + temp;
            }
            else if (_drawnCards.Count == 6)
            {
                //Rewards for 6 Rooms Dungeons
                GetDungeonCardsReward(6);
                string temp = "";
                foreach (var item in _cardReward)
                {
                    Debug.Log(item.CardName);
                    temp += item.CardName + "\n";
                }
                //  debugCardRewards.text = "Cards Reward: \n\n" + temp;
            }
            else if (_drawnCards.Count == 3)
            {
                //Rewards for 3 Rooms Dungeons
                GetDungeonCardsReward(3);
                string temp = "";
                foreach (var item in _cardReward)
                {
                    Debug.Log(item.CardName);
                    temp += item.CardName + "\n";
                }
               // debugCardRewards.text = "Cards Reward: \n\n" + temp;
            }

        }
    }

    public void AddCardRewardToInventory()
    {
        foreach (var dungeonCardReward in _cardReward)
        {
            cardInventory.AddDungeonCardToInventory(dungeonCardReward);
            print("Card Added to Inventory: " + dungeonCardReward.CardName);
        }
    }

    private void GetDungeonCardsReward(int rewardAmount)
    {
        _possibleDungeonCardsReward = GetDifferenceFromTwoLists(allDungeonCards, cardInventory.dungeonCards);

        if (_possibleDungeonCardsReward.Count < rewardAmount) //if there are less possible rewards then the amount asked...
        {
            rewardAmount = _possibleDungeonCardsReward.Count; //...the amount asked is the same as the number of possible rewards
        }
        _cardReward = GetRandomItemsFromList(_possibleDungeonCardsReward, rewardAmount);
    }

        public static List<T> GetRandomItemsFromList<T>(List<T> list, int number)
        {
        //temp list from which we are taking the element
            List<T> tmp = new List<T>(list);

        //list where we will move the elements to
            List<T> newList = new List<T>();

        //make sure temp is not already empty
            while (newList.Count < number && tmp.Count > 0)
            {
             int index = Random.Range(0, tmp.Count);
             newList.Add(tmp[index]);
             tmp.RemoveAt(index);
            }

        return newList;

        }

    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadAsync(sceneName));
        async.allowSceneActivation = false;
    }

    public void LoadHubAsync()
    {
        StartCoroutine(LoadAsyncHub());

    }

    IEnumerator LoadAsyncHub()
    {
        Debug.LogWarning("ASYNC LOAD STARTED - " +
        "DO NOT EXIT PLAY MODE UNTIL SCENE LOADS... UNITY WILL CRASH");
        asyncHub = SceneManager.LoadSceneAsync("AlpaHub");
        asyncHub.allowSceneActivation = false;
        yield return asyncHub;
        CalculateRewards();
    }

    IEnumerator LoadAsync(string sceneName)
    {
        Debug.LogWarning("ASYNC LOAD STARTED - " +
          "DO NOT EXIT PLAY MODE UNTIL SCENE LOADS... UNITY WILL CRASH");
        async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;
        yield return async;
        CreateDungeon();

    }

    /// <summary>
    /// Compares two lists and returns a list of the difference. The two lists need to be of the same type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="minuend">The part you start with.</param>
    /// <param name="subtrahend">The part being taken away.</param>
    /// <returns></returns>
    public static List<T> GetDifferenceFromTwoLists<T>(List<T> minuend, List<T> subtrahend)
        {

             List<T> tmp01 = new List<T>(minuend);

             List<T> temp02 = new List<T>(subtrahend);

             //list where we will move the elements to
            IEnumerable<T> newList = new List<T>();

            newList = minuend.Except(subtrahend);


        return newList.ToList();
    }    
}