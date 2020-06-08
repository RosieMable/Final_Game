using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZaldensGambit;
using UnityEngine.UI;
using Coffee.UIExtensions;

namespace ZaldensGambit
{
    public class DungeonCardUI : CardUI
    {

        [Header("Cards Details")]
        [SerializeField]
        Sprite backCard;

        [SerializeField]
        Sprite frontCard;

        [SerializeField]
        private Image[] suitImages;

        [SerializeField]
        private Text valueText;

        public Image selfImage;

        public delegate void OnSelectCardDelegate();
        public static OnSelectCardDelegate selectCardDelegate;

        public delegate void OnRevealCardDelegate();
        public static OnRevealCardDelegate revealCardDelegate;


        [SerializeField]
        bool revealed;

        protected override void Start()
        {
            base.Start();

            foreach (var suit in suitImages)
            {
                suit.gameObject.SetActive(false);
            }
            valueText.gameObject.SetActive(false);

            selfImage.sprite = backCard;


            revealed = false;

        }

        public void OnClickCard()
        {
            if (chosenCard == null)
            {
                if (selectCardDelegate != null)
                {
                    if (selected == false)
                    {
                        UI.DungeonCardsSelected.Add(this);
                    }

                    selectCardDelegate();

                }
            }
            else
            {
                iTween.RotateTo(gameObject, new Vector3(0, 358, 0), 2f);

                foreach (var suit in suitImages)
                {
                    suit.sprite = chosenCard.CardSprite;
                    suit.gameObject.SetActive(true);
                    revealed = true;
                }

                valueText.text = chosenCard.CardValue.ToString();
                valueText.gameObject.SetActive(true);

                selfImage.sprite = frontCard;

                if (revealed)
                {
                    UI.cardsRevealed += 1;
                    print(UI.cardsRevealed);

                    if (revealCardDelegate != null)
                    {
                        revealCardDelegate();
                    }
                }

            }


        }

    }
}
