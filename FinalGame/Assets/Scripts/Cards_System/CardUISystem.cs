using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardUISystem : MonoBehaviour
{
    [SerializeField]
    private DungeonCardSystem dungeonCard;

    [SerializeField]
    List<Card_ScriptableObj> currentDeck;

    [SerializeField]
    List<Card_ScriptableObj> drawnCards;

    public List<CardUI> CardsSelected;

    [SerializeField]
    List<GameObject> CardsDealt;

    public int cardsClickedOn = 0;

    public Transform start;  //Location where to start adding my cards
    public Transform HandDeck; //The hand panel reference
    public float howManyAdded; // How many cards I added so far
    public float gapFromOneItemToTheNextOne; //the gap I need between each card

    public Transform CenterPoint;

    [SerializeField]
    int numberOfCards;

    public float totalTwist;

    [SerializeField]
    private Transform HandDeckPos;

    [SerializeField]
    GameObject prefabCard;

    private void OnEnable()
    {
        CardUI.buttonClickDelegate += CheckCardsSelected;
    }
    void Start()
    {
        Init();
    }

    void Init()
    {
        if (dungeonCard == null)
            dungeonCard = FindObjectOfType<DungeonCardSystem>();

        howManyAdded = 0.0f;
        currentDeck = dungeonCard.ownedDungeonCards;
        HandDeckPos = HandDeck;

        foreach (var card in CardsSelected)
        {
            card.selected = false;
        }
        CardsSelected.Clear();
        CardUI.buttonClickDelegate += CheckCardsSelected;
    }


    public void NewDeal()
    {
        Init();
        FitCards();
    }


   public void CheckCardsSelected()
    {
        cardsClickedOn = 0;

            foreach (var card in CardsSelected)
            {
                card.chosenCard = drawnCards[cardsClickedOn++];
                Vector3 parentPos = CenterPoint.position;
                Vector3 newPos = parentPos += new Vector3(200f * cardsClickedOn, 0f, 0f);
                iTween.MoveTo(card.gameObject, newPos, 0.5f);
                iTween.RotateTo(card.gameObject, Vector3.zero, 0.25f);
                card.gameObject.transform.SetParent(CenterPoint);
                card.selected = true;
                print(cardsClickedOn);

            }

            if (CardsSelected.Count == dungeonCard.GlobalAmountCD)
            {
                iTween.MoveTo(HandDeck.gameObject, new Vector3(HandDeck.gameObject.transform.position.x, -300, HandDeck.gameObject.transform.position.z), 1f);
                CardUI.buttonClickDelegate -= CheckCardsSelected;
                 cardsClickedOn = 0;
            }

    }

    private void FitCards()
    {

        if (currentDeck.Count == 0) //if list is null, stop function
            return;


       StartCoroutine(AnimateCardFanning(.25f, currentDeck));
    }


    IEnumerator AnimateCardFanning(float _animSpeed, List<Card_ScriptableObj> _cards)
    {
        drawnCards = dungeonCard.DrawnCards;

        numberOfCards = _cards.Count;
        float twistPerCard = totalTwist / numberOfCards;
        gapFromOneItemToTheNextOne = 450f/numberOfCards;
        float startTwist = -1f * (totalTwist / 2f);
        
        if(CardsDealt.Count == 0) //if cards have never been dealth before
        {
            for (int y = 0; y < _cards.Count; y++)
            {
                GameObject cardGO = Instantiate(prefabCard, HandDeck);
                cardGO.transform.SetParent(HandDeck);
                cardGO.transform.position = start.position; //relocating my card to the Start Position
                float twistForThisCard = startTwist + (howManyAdded * twistPerCard);
                float scalingFactor = 1.75f;
                float nudgeThisCard = Mathf.Abs(twistForThisCard);
                nudgeThisCard *= scalingFactor;

                cardGO.transform.Rotate(0f, 0f, twistForThisCard);
                cardGO.transform.position += new Vector3((howManyAdded * gapFromOneItemToTheNextOne), -nudgeThisCard, 0); // Moving my card 1f to the right
                cardGO.transform.Translate(0f, -nudgeThisCard, 0f);


                CardsDealt.Add(cardGO);
                howManyAdded++;

                yield return new WaitForSeconds(_animSpeed);

            }
        }
        else //If cards have been dealt before
        {
            howManyAdded = 0;
            foreach (var cardGO in CardsDealt)
            {
                cardGO.transform.position = start.position; //relocating my card to the Start Position
                cardGO.transform.SetParent(HandDeck);
                float twistForThisCard = startTwist + (howManyAdded * twistPerCard);
                float scalingFactor = 1.75f;
                float nudgeThisCard = Mathf.Abs(twistForThisCard);
                nudgeThisCard *= scalingFactor;

                cardGO.transform.Rotate(0f, 0f, twistForThisCard);
                cardGO.transform.position += new Vector3((howManyAdded * gapFromOneItemToTheNextOne), -nudgeThisCard, 0); // Moving my card 1f to the right
                cardGO.transform.Translate(0f, -nudgeThisCard, 0f);
                howManyAdded++;
                yield return new WaitForSeconds(_animSpeed);

            }
        }

    }

    private void OnDisable()
    {
        CardUI.buttonClickDelegate -= CheckCardsSelected;
    }
}
