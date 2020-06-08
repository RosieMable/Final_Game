using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZaldensGambit
{
    public class WagerSystem : MonoBehaviour
    {
        // Start is called before the first frame update
        public static CardInventory instance;

        [SerializeField]
        Transform[] CardsToWagerPosition;

        [SerializeField]
        Transform[] cardsOwnedPosition;

        [SerializeField]
        Transform cardToObtain;

        private int minimum = 1;


        public void Init()
        {
            cardsOwnedPosition[0] = GameObject.Find("SCard 1").transform;
            cardsOwnedPosition[1] = GameObject.Find("SCard 2").transform;
            cardsOwnedPosition[2] = GameObject.Find("SCard 3").transform;
            cardsOwnedPosition[3] = GameObject.Find("SCard 4").transform;
            cardsOwnedPosition[4] = GameObject.Find("SCard 5").transform;

            CardsToWagerPosition[0] = GameObject.Find("SC 1").transform;
            CardsToWagerPosition[1] = GameObject.Find("SC 2").transform;
        }

        public void ToggleInventory()
        {

            for (int i = 0; i < cardsOwnedPosition.Length; i++)
            {
                if (instance.spiritCards[i] != null)
                {
                    cardsOwnedPosition[i].GetComponent<Image>().sprite = instance.spiritCards[i]._spiritSpriteCardback;
                    cardsOwnedPosition[i].GetComponent<InventoryCardUI>().spirit = instance.spiritCards[i];
                }
            }
        }

        public void SortLeft()
        {
            Debug.Log("Minimum beforehand: " + minimum);

            if (minimum > 0)
            {
                minimum--;
            }

            Debug.Log("Minimum after: " + minimum);

            int _cardToDisplay = minimum;

            for (int i = 0; i < cardsOwnedPosition.Length; i++)
            {
                if (_cardToDisplay <= instance.spiritCards.Count - 1 && instance.spiritCards[_cardToDisplay] != null)
                {
                    cardsOwnedPosition[i].GetComponent<Image>().sprite = instance.spiritCards[_cardToDisplay]._spiritSpriteCardback;
                    cardsOwnedPosition[i].GetComponent<InventoryCardUI>().spirit = instance.spiritCards[_cardToDisplay];
                    _cardToDisplay++;
                }
            }
        }

        public void SortRight()
        {
            // Check if there is a spiritcard in inventory that is not currently displayed that is greater than the value of the current set
            // If so, readjust display
            if (minimum >= 0 && minimum <= instance.spiritCards.Count - 5)
            {
                minimum++;
            }

            int _cardToDisplay = minimum;

            for (int i = 0; i < cardsOwnedPosition.Length; i++)
            {
                if (_cardToDisplay <= instance.spiritCards.Count - 1 && instance.spiritCards[_cardToDisplay] != null)
                {
                    cardsOwnedPosition[i].GetComponent<Image>().sprite = instance.spiritCards[_cardToDisplay]._spiritSpriteCardback;
                    cardsOwnedPosition[i].GetComponent<InventoryCardUI>().spirit = instance.spiritCards[_cardToDisplay];
                    _cardToDisplay++;
                }
            }
        }

        public void SelectToWager()
        {
            if (CardsToWagerPosition.Length != 0)
            {

            }
        }


    }

}
