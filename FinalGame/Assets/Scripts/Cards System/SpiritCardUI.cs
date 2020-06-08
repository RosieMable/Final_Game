using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZaldensGambit;

namespace ZaldensGambit
{
    public class SpiritCardUI : CardUI
    {

        public Image SpiritCardImage;

        public BaseSpirit chosenSpirit;

        public delegate void OnSelectSpiritDelegate();
        public static OnSelectSpiritDelegate selectCardDelegate;

        // Start is called before the first frame update

        private void Awake()
        {

           SpiritCardImage = this.GetComponent<Image>();
        }
        protected override void Start()
        {
            base.Start();

        }

        public void OnSpiritClick()
        {
            if (chosenSpirit != null)
            {
                spiritSystem.spiritEquipped = chosenSpirit;
                spiritSystem.OnEquipSpirit(chosenSpirit);

                GameManager.instance.spiritToEquipInDungeon = chosenSpirit;

                if (selectCardDelegate != null) 
                { 
                    selectCardDelegate();

                }
            }
        }
    }

}
