﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ZaldensGambit;

public class DungeonCardSystem : Singleton<DungeonCardSystem>
{
    [Header("Dungeon Cards Section")]
    public List<Card_ScriptableObj> ownedDungeonCards; //reference to the dungeoncards that the player owns (inventory system ref)

    public List<Card_ScriptableObj> allDungeonCards; //Reference to all the cards within the game

    [SerializeField]
    private List<Card_ScriptableObj> _possibleDungeonCardsReward; //reference to the difference between all the cards within the game and the player's owned one, from there the reward is calculated

    private List<Card_ScriptableObj> _cardReward;
    public List<Card_ScriptableObj> DungeonCardsReward { get { return _cardReward; } private set { DungeonCardsReward = _cardReward; } }

    private List<Card_ScriptableObj> _drawnCards;

    public List<Card_ScriptableObj> DrawnCards { get { return _drawnCards; } private set { DrawnCards = _drawnCards; } }


    [Header("Spirit Cards Section")]
    public List<Spirit_ScriptableObj> ownedSpirits; //reference to the spirits that the player owns (inventory system ref)

    public List<Spirit_ScriptableObj> allSpirits; //Reference to all spirits within the game

    [SerializeField]
    private List<Spirit_ScriptableObj> _possibleSpiritRewards; //reference to the difference between all the cards within the game and the player's owned one, from there the reward is calculated

    private List<Spirit_ScriptableObj> _spiritReward;
    public List<Spirit_ScriptableObj> SpiritCardsReward { get { return _spiritReward; } private set { SpiritCardsReward = _spiritReward; } }

    [Header("Dungeon Prefabs Section")]
    [SerializeField]
    private Transform _spawnLocation;

    [SerializeField]
    private List<GameObject> ThreeRoomsDungeonPrefab;

    [SerializeField]
    private List<GameObject> SixRoomsDungeonPrefab;

    [SerializeField]
    private List<GameObject> NineRoomsDungeonPrefab;

    [Header("Temp Debug")]
    #region Temp for debug
    public Text debugCardsDrawn;

    public Text debugCardRewards;

    public Text debugCardOwned;

    public Text debugAllCards;
    #endregion

    private void Start()
    {
        string temp = "";


        foreach (var item in ownedDungeonCards)
        {
            Debug.Log(item.CardName);
            temp += item.CardName + "\n";
        }

        debugCardOwned.text = "Dungeon Cards Owned: \n\n" + temp;

        string temp02 = "";

        foreach (var item in allDungeonCards)
        {
            Debug.Log(item.CardName);
            temp02 += item.CardName + "\n";
        }

        debugAllCards.text = "All Dungeon Cards : \n\n" + temp02;

    }

    public void DrawCards(int amount)
    {
        //From the dungeon cards, draw some randomly based on the amount
        //add them to the drawn cards
        string temp = "";

        _drawnCards = GetRandomItemsFromList<Card_ScriptableObj>(ownedDungeonCards, amount);

        foreach (var item in _drawnCards)
        {
            Debug.Log(item.CardName);
            temp += item.CardName + "\n";
        }

        debugCardsDrawn.text = "Cards drawn: \n\n" + temp;
    }

    public void CreateDungeon()
    {
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
            if (_drawnCards.Count == 3)
            {
                //Rewards for 3 Rooms Dungeons
                GetDungeonCardsReward(3);
                string temp = "";
                foreach (var item in _cardReward)
                {
                    Debug.Log(item.CardName);
                    temp += item.CardName + "\n";
                }
                debugCardRewards.text = "Cards Reward: \n\n" + temp;
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
                debugCardRewards.text = "Cards Reward: \n\n" + temp;
            }
            else if (_drawnCards.Count == 9)
            {
                //Rewards for 9 Rooms Dungeons
                GetDungeonCardsReward(9);
                string temp = "";
                foreach (var item in _cardReward)
                {
                    Debug.Log(item.CardName);
                    temp += item.CardName + "\n";
                }
                debugCardRewards.text = "Cards Reward: \n\n" + temp;
            }
        }
    }

    private void GetDungeonCardsReward(int rewardAmount)
    {
        _possibleDungeonCardsReward = GetDifferenceFromTwoLists(allDungeonCards, ownedDungeonCards);

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