using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Coffee.UIExtensions;

namespace ZaldensGambit
{
    public class CardUI : MonoBehaviour
    {
        [Header("Cards Details")]
        [SerializeField]
        Sprite backCard;

        [SerializeField]
        Sprite frontCard;

        [SerializeField]
        private CardUISystem UI;

        public Card_ScriptableObj chosenCard;

        public bool selected;


        public delegate void OnSelectCardDelegate();
        public static OnSelectCardDelegate selectCardDelegate;

        public delegate void OnRevealCardDelegate();
        public static OnRevealCardDelegate revealCardDelegate;

        [SerializeField]
        private Image[] suitImages;

        [SerializeField]
        private Text valueText;

        public Image selfImage;

        private UIShadow shadow;


        private void Start()
        {
            if (UI == null)
                UI = FindObjectOfType<CardUISystem>();
            selected = false;

            foreach (var suit in suitImages)
            {
                suit.gameObject.SetActive(false);
            }
            valueText.gameObject.SetActive(false);

            selfImage.sprite = backCard;

            shadow = FindObjectOfType<UIShadow>();

            shadow.effectColor = new Color(shadow.effectColor.r, shadow.effectColor.g, shadow.effectColor.b, 0);
        }


        public void Glowin()
        {
            if (!selected)
            {
                iTween.MoveBy(this.gameObject, new Vector3(0f, 10f, 0f), 0.2f);
            }
            else
            {
                iTween.ScaleTo(this.gameObject, new Vector3(2.2f, 2.2f, 2.2f), .2f);
            }

            shadow.effectColor = new Color(shadow.effectColor.r, shadow.effectColor.g, shadow.effectColor.b, 1);
        }

        public void Glowout()
        {
            if (!selected)
            {
                iTween.MoveBy(this.gameObject, new Vector3(0f, -10f, 0f), 0.2f);
            }
            else
            {
                iTween.ScaleTo(this.gameObject, new Vector3(2f, 2f, 2f), .2f);
            }

            shadow.effectColor = new Color(shadow.effectColor.r, shadow.effectColor.g, shadow.effectColor.b, 0);
        }

        public void OnClickCard()
        {
            if (chosenCard == null)
            {
                if (selectCardDelegate != null)
                {
                    if (selected == false)
                    {
                        UI.CardsSelected.Add(this);
                    }

                    selectCardDelegate();

                }
            }
            else
            {
                iTween.RotateTo(this.gameObject, new Vector3(0, 358, 0), 2f);

                foreach (var suit in suitImages)
                {
                    suit.sprite = chosenCard.CardSprite;
                    suit.gameObject.SetActive(true);
                }

                valueText.text = chosenCard.CardValue.ToString();
                valueText.gameObject.SetActive(true);

                selfImage.sprite = frontCard;

                if (revealCardDelegate != null && valueText.gameObject.activeInHierarchy)
                {
                    revealCardDelegate();
                }

            }


        }

    }
}