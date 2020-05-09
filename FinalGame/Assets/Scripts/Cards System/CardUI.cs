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


        [SerializeField]
        protected CardUISystem UI;

        public Card_ScriptableObj chosenCard;

        public bool selected;

        protected UIShadow shadow;

        protected virtual void Start()
        {
            if (UI == null)
                UI = FindObjectOfType<CardUISystem>();
            selected = false;

           
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


        }

    }
