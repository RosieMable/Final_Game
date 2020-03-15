using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Coffee.UIExtensions;

public class CardUI : MonoBehaviour
{
    [Header("Cards Detais=ls")]
    [SerializeField]
    Sprite backCard;

    [SerializeField]
    Sprite frontCard;

    [SerializeField]
    private CardUISystem UI;

    public Card_ScriptableObj chosenCard;

    public bool selected;


    public delegate void OnButtonClickDelegate();
    public static OnButtonClickDelegate buttonClickDelegate;

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


    public void Glouwin()
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
        if(chosenCard == null)
        {
            if (buttonClickDelegate != null)
            {
                if (selected == false)
                {
                    UI.CardsSelected.Add(this);
                }

                buttonClickDelegate();

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

        }
     

    }

}
