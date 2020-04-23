using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZaldensGambit
{
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

        [Header("Dungeon Cards UI Elements")]
        [SerializeField]
        GameObject DungeonCardssUIElements;

        public int cardsRevealed;

        [SerializeField]
        Portal portalToDungeon;
        private void OnEnable()
        {
            //CardUI.selectCardDelegate += CheckCardsSelected;
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
            currentDeck = CardInventory.instance.dungeonCards;
            HandDeckPos = HandDeck;

            foreach (var card in CardsSelected)
            {
                card.selected = false;
            }
            CardsSelected.Clear();
            cardsRevealed = 0;
            CardUI.selectCardDelegate += CheckCardsSelected;
            CardUI.revealCardDelegate += AfterSelection;

            portalToDungeon.gameObject.SetActive(false);
        }


        public void NewDeal()
        {
            Init();
            FitCards();
        }


        public void CheckCardsSelected()
        {
            cardsClickedOn = 0;
          //  CenterPoint.position = GetCentreForCards(dungeonCard.DrawnCards.Count);

            foreach (var card in CardsSelected)
            {
                card.chosenCard = drawnCards[cardsClickedOn++];
                Vector3 parentPos = CenterPoint.position;
                Vector3 newPos = parentPos += new Vector3(GapBasedOnCardsamount(dungeonCard.DrawnCards.Count) * cardsClickedOn, 0f, 0f);
                iTween.MoveTo(card.gameObject, newPos, 0.5f);
                iTween.RotateTo(card.gameObject, Vector3.zero, 0.25f);
                card.gameObject.transform.SetParent(CenterPoint);
                iTween.ScaleTo(card.gameObject, ScaleBasedNumberOfCards(dungeonCard.DrawnCards.Count), 0.2f);
                card.selected = true;
               // print(cardsClickedOn);

            }

            if (CardsSelected.Count == dungeonCard.GlobalAmountCD) //if we drawn the needed cards
            {
                iTween.MoveTo(HandDeck.gameObject, new Vector3(HandDeck.gameObject.transform.position.x, -300, HandDeck.gameObject.transform.position.z), 1f); //Moves the hand out of the way
                CardUI.selectCardDelegate -= CheckCardsSelected;
                cardsClickedOn = 0;
            }
        }

        private void AddToRevealedCards()
        {
           cardsRevealed += 1;

           print(cardsRevealed);


                //close ui system
                //open spirit selection ui
                //saves all selected options to be carried over
                //SpiritSelection();
                 AfterSelection();
        }

        private void SpiritSelection()
        {
            if (CardInventory.instance.spiritCards.Count != 0) //if we have spirit cards
            {
                //show spirit choice UI
            }
            else //we don't have any spirit cards
                return;
        }

        private void AfterSelection()
        {
            //after selction actions
            //ui closes
            //input player system active again
            //portal active

            if (cardsRevealed >= dungeonCard.GlobalAmountCD)
            {
                //close UI
                //after a delay
                //possible animations?
                StartCoroutine(CloseDungeonCardsUI(2f));

                //reset interaction system
                FateWeaver fateWeaver = FindObjectOfType<FateWeaver>();
                fateWeaver.ResetInteraction();

                cardsRevealed = 0;

                //activate portal
                portalToDungeon.gameObject.SetActive(true);
            }
        }

        private void FitCards()
        {

            if (currentDeck.Count == 0) //if list is null, stop function
                return;


            StartCoroutine(AnimateCardFanning(.25f, currentDeck));
        }

        IEnumerator CloseDungeonCardsUI(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            DungeonCardssUIElements.SetActive(false);
            foreach (var cardSelected in CardsSelected)
            {
                Destroy(cardSelected);
            }
        }

        IEnumerator AnimateCardFanning(float _animSpeed, List<Card_ScriptableObj> _cards)
        {
            drawnCards = dungeonCard.DrawnCards;

            numberOfCards = _cards.Count;
            float twistPerCard = totalTwist / numberOfCards;
            gapFromOneItemToTheNextOne = 450f / numberOfCards;
            float startTwist = -1f * (totalTwist / 2f);

            if (CardsDealt.Count == 0) //if cards have never been dealt before
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

        Vector3 GetCentreForCards(float numberOfCards)
        {
            Vector3 val = Vector3.zero;

            if (numberOfCards != 0)
            {
                float x = Screen.width / numberOfCards;
                float y = Screen.height / 2;

                val = new Vector3(x, y, -1);
            }


            return val;
        }

        Vector3 ScaleBasedNumberOfCards(float numberOfCards)
        {
            Vector3 val = Vector3.zero;

            //if (numberOfCards != 0)
            //{
            //    float height = Camera.main.orthographicSize * 2.0f;
            //    float width = height * Screen.height / Screen.width;
            //    val = Vector3.one * width / numberOfCards;

            //}

            if (numberOfCards == 3)
            {
                val = Vector3.one * 2;
            }
            if (numberOfCards == 6)
            {
                val = Vector3.one * 1.75f;
            }
            if (numberOfCards == 9)
            {
                val = Vector3.one * 1.5f;
            }

            return val;
        }

        float GapBasedOnCardsamount(float numberOfCards)
        {
            float val = 0;

            if (numberOfCards == 3)
            {
                val = 200f;
            }
            if (numberOfCards == 6)
            {
                val = 175f;
            }
            if (numberOfCards == 9)
            {
                val = 150f;
            }
            return val;
        }

        private void OnDisable()
        {
            CardUI.selectCardDelegate -= CheckCardsSelected;
            CardUI.revealCardDelegate -= AfterSelection;
        }
    }
}