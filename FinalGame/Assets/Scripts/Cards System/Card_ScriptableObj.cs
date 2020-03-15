using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseCard", menuName = "CardCreation")]
public class Card_ScriptableObj : ScriptableObject
{
    [SerializeField]
    protected string _cardName;

    public string CardName { get { return _cardName; } private set { CardName = _cardName; } }

    [SerializeField]
    protected int _cardValue;

    public int CardValue { get { return _cardValue; } private set { CardValue = _cardValue; } }

    [TextArea(3, 10)]
    [SerializeField]
    protected string _cardDescription;

    public string CardDescription { get { return _cardDescription; } private set { CardDescription = _cardDescription; } }

    public enum CardSuit
    {
        Famine,
        War,
        Death,
        Pestilence
    }

    public CardSuit cardSuit;

    public Sprite _cardSprite;

    public Sprite CardSprite { get { return _cardSprite; } }
}
