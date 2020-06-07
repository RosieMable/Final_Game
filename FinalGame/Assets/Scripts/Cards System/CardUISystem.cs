using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        public List<DungeonCardUI> DungeonCardsSelected;

        [SerializeField]
        List<GameObject> CardsDealt;

        [SerializeField]
        private List<BaseSpirit> spirits;

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
        private Transform HandDeckPos, SpiritCardsPosition;

        [SerializeField]
        GameObject prefabDungeonCard, prefabSpiritCard;

        [Header("Dungeon Cards UI Elements")]
        [SerializeField]
        GameObject DungeonCardssUIElements;

        [SerializeField]
        GameObject SpiritCardsUIElements;

        public GameObject DungeonUIChoice;

        public GameObject SpiritCardsUIArena;

        public int cardsRevealed;

        public bool spiritSelected;

        [SerializeField]
        Portal portalToDungeon;
        private void OnEnable()
        {
            //CardUI.selectCardDelegate += CheckCardsSelected;
        }
        void Start()
        {
            Init();
            DungeonUIChoice.SetActive(false);
        }

        void Init()
        {
            if (dungeonCard == null)
                dungeonCard = FindObjectOfType<DungeonCardSystem>();

            howManyAdded = 0.0f;
            currentDeck = CardInventory.instance.dungeonCards;
            HandDeckPos = HandDeck;

            foreach (var card in DungeonCardsSelected)
            {
                card.selected = false;
            }
            DungeonCardsSelected.Clear();
            cardsRevealed = 0;
            DungeonCardUI.selectCardDelegate += CheckCardsSelected;
            DungeonCardUI.revealCardDelegate += AfterSelection;

            SpiritCardUI.selectCardDelegate += AfterSpiritSelection;

            spirits = CardInventory.instance.spiritCards;

            if (portalToDungeon != null)
            {
                portalToDungeon.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (portalToDungeon == null && SceneManager.GetActiveScene().name == "BetaHub")
            {
                portalToDungeon = GameObject.Find("PortalToDungeon").GetComponent<Portal>();
            }
        }

        public void NewDeal()
        {
            Init();
            FitCards();
        }


        public void CheckCardsSelected()
        {
            cardsClickedOn = 0;

            foreach (var card in DungeonCardsSelected)
            {
                card.gameObject.transform.SetParent(CenterPoint);
                iTween.RotateTo(card.gameObject, Vector3.zero, 0.25f);
                iTween.ScaleTo(card.gameObject, ScaleBasedNumberOfCards(dungeonCard.DrawnCards.Count), 0.2f);
                card.chosenCard = drawnCards[cardsClickedOn++];
                card.selected = true;
                // print(cardsClickedOn);

            }

            if (DungeonCardsSelected.Count == dungeonCard.GlobalAmountCD) //if we drawn the needed cards
            {

                iTween.MoveTo(HandDeck.gameObject, new Vector3(HandDeck.gameObject.transform.position.x, -300, HandDeck.gameObject.transform.position.z), 1f); //Moves the hand out of the way
                DungeonCardUI.selectCardDelegate -= CheckCardsSelected;
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

        public void SpiritSelection()
        {
            if (spirits.Count != 0) //if we have spirit cards
            {
                SpiritCardsUIElements.SetActive(true);
                //show spirit choice UI
                    for (int i = 0; i <= spirits.Count -1; i++)
                    {
                        GameObject spiritCard = Instantiate(prefabSpiritCard, SpiritCardsPosition);
                        SpiritCardUI spirit = spiritCard.GetComponent<SpiritCardUI>();
                        spirit.SpiritCardImage.sprite = spirits[i]._spiritSpriteCardback;
                        spirit.chosenSpirit = spirits[i];

                    }
            }
            else //we don't have any spirit cards
                CloseSystem();
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

                SpiritSelection();
            }
        }

        private void AfterSpiritSelection()
        {
            StartCoroutine(CloseSpiritCardsUI(2f));

            CloseSystem();
        }

        private void CloseSystem()
        {
            //reset interaction system
            FateWeaver fateWeaver = FindObjectOfType<FateWeaver>();
            fateWeaver.ResetInteraction();

            cardsRevealed = 0;

            //activate portal
            portalToDungeon.gameObject.SetActive(true);
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
            foreach (var cardSelected in DungeonCardsSelected)
            {
                Destroy(cardSelected);
            }
        }

        IEnumerator CloseSpiritCardsUI(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            SpiritCardsUIElements.SetActive(false);
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
                    GameObject cardGO = Instantiate(prefabDungeonCard, HandDeck);
                    cardGO.transform.position = start.transform.position; //relocating my card to the Start Position
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
                    cardGO.transform.position = start.transform.position; //relocating my card to the Start Position
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

        Vector3 ScaleBasedNumberOfCards(float numberOfCards)
        {
            Vector3 val = Vector3.zero;

            if (numberOfCards == 3)
            {
                val = Vector3.one * 2;
            }
            if (numberOfCards == 6)
            {
                val = Vector3.one * 1f;
            }
            if (numberOfCards == 9)
            {
                val = Vector3.one * .7f;
            }

            return val;
        }

        private void OnDisable()
        {
            DungeonCardUI.selectCardDelegate -= CheckCardsSelected;
            DungeonCardUI.revealCardDelegate -= AfterSelection;

            SpiritCardUI.selectCardDelegate -= AfterSpiritSelection;
        }
    }
}