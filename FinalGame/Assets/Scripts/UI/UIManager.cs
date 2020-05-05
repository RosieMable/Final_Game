﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ZaldensGambit
{
    public class UIManager : Singleton<UIManager>
    {
        public static UIManager instance;

        private Image healthBar;
        private TextMeshProUGUI healthText;
        private StateManager player;
        private TextMeshProUGUI dialogueText;
        private GameObject dialoguePanel;

        #region "Spirit UI Variables"

        public Image SpiritIcon;

        public Image SpiritBar;
        [SerializeField] private Sprite huskSpiritSprite;

        #endregion

        private new void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            healthBar = GameObject.Find("HealthBar").GetComponent<Image>();
            healthText = GameObject.Find("HealthText").GetComponent<TextMeshProUGUI>();
            dialoguePanel = GameObject.Find("DialoguePanel");
            dialogueText = GameObject.Find("DialogueText").GetComponent<TextMeshProUGUI>();
            player = FindObjectOfType<StateManager>();
            DontDestroyOnLoad(this);            
        }

        private void Start()
        {
            UpdateHealthUI();
            HideDialogue();
        }

        private void Update()
        {
            UpdateHealthUI();

            //if (Input.GetKeyDown(KeyCode.Escape))
            //{
            //    SceneManager.LoadScene("Title Screen");
            //    Cursor.visible = true;
            //    Cursor.lockState = CursorLockMode.None;
            //}
        }

        public void DisplayDialogue(Dialogue dialogueToShow)
        {
            dialogueText.text = dialogueToShow.dialogue;
            dialoguePanel.SetActive(true);
        }

        public void DisplayDialogue(string dialogueToShow)
        {
            dialogueText.text = dialogueToShow;
            dialoguePanel.SetActive(true);
        }

        public void HideDialogue()
        {
            dialoguePanel.SetActive(false);
            dialogueText.text = string.Empty;
        }

        public void UpdateHealthUI()
        {
            if (player != null)
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
        }

        public void ManageSpiritUI(BaseSpirit _spiritToDisplay)
        {
            if (_spiritToDisplay != null)
            {
                SpiritIcon.gameObject.SetActive(true);
                SpiritBar.gameObject.SetActive(true);
                SpiritBar.color = Color.white;

                SpiritIcon.sprite = _spiritToDisplay._spiritSpriteIcon;
            }
            else
            {
                SpiritIcon.sprite = huskSpiritSprite;

                SpiritBar.color = Color.gray;
            }
        }
    }
}
