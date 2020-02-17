using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ZaldensGambit
{
    public class UIManager : Singleton<UIManager>
    {
        private static UIManager _instance;

        private Image healthBar;
        private TextMeshProUGUI healthText;
        private StateManager player;

        #region "Spirit UI Variables"

        public Image SpiritIcon;

        public Image SpiritBar;

        #endregion

        private void Awake()
        {
            base.Awake();
            healthBar = GameObject.Find("HealthBar").GetComponent<Image>();
            healthText = GameObject.Find("HealthText").GetComponent<TextMeshProUGUI>();
            player = FindObjectOfType<StateManager>();
        }

        private void Start()
        {
            UpdateHealthUI();
        }

        private void Update()
        {
            UpdateHealthUI();
        }

        public void UpdateHealthUI()
        {
            if (player.currentHealth <= 0)
            {
                healthBar.fillAmount = 0;
                healthText.text = player.currentHealth.ToString() + " / " + player.maximumHealth.ToString();
            }
            else if (healthBar.fillAmount != player.currentHealth / player.maximumHealth)
            {
                healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, player.currentHealth / player.maximumHealth, Time.deltaTime * 5);
                healthText.text = player.currentHealth.ToString() + " / " + player.maximumHealth.ToString();
            }            
        }

        public void ManageSpiritUI(Spirit_ScriptableObj _spiritToDisplay)
        {
            if (_spiritToDisplay != null)
            {
                SpiritIcon.gameObject.SetActive(true);
                SpiritBar.gameObject.SetActive(true);

                SpiritIcon.sprite = _spiritToDisplay._spiritSprite;
            }
            else
            {
                SpiritIcon.gameObject.SetActive(false);

                SpiritBar.gameObject.SetActive(false);
            }
        }
    }
}
