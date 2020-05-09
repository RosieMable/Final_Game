using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ZaldensGambit
{
    public class UIManager : Singleton<UIManager>
    {
        public static UIManager Instance;

        private Image healthBar;
        private TextMeshProUGUI healthText;
        private StateManager player;
        private TextMeshProUGUI dialogueText;
        private GameObject dialoguePanel;

        private Image spiritBar;

        #region "Spirit UI Variables"

        public Image SpiritIcon;

        public Image SpiritBar;
        [SerializeField] private Sprite huskSpiritSprite;

        #endregion

        private new void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            healthBar = GameObject.Find("HealthBar").GetComponent<Image>();
            healthText = GameObject.Find("HealthText").GetComponent<TextMeshProUGUI>();
            dialoguePanel = GameObject.Find("DialoguePanel");
            dialogueText = GameObject.Find("DialogueText").GetComponent<TextMeshProUGUI>();
            spiritBar = GameObject.Find("SpiritBar").GetComponent<Image>();
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
            if (!player)
            {
                player = FindObjectOfType<StateManager>();
            }

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

        public void DisplayDialogue(string[] dialogueToShow, float duration)
        {
            StartCoroutine(DisplayTextForDuration(dialogueToShow, duration));
        }

        private IEnumerator DisplayTextForDuration(string[] text, float duration)
        {
            for (int i = 0; i < text.Length; i++)
            {
                dialogueText.text = text[i];
                dialoguePanel.SetActive(true);
                yield return new WaitForSeconds(duration);
                HideDialogue();
            }
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

        public void UpdateAbilityBar(float abilityCooldown)
        {
            StopCoroutine(AbilityCooldown(abilityCooldown));
            StartCoroutine(AbilityCooldown(abilityCooldown));
        }

        IEnumerator AbilityCooldown(float countdownValue)
        {
            float currentValue = 0;
            spiritBar.fillAmount = 0;
            while (currentValue < countdownValue)
            {
                yield return new WaitForSecondsRealtime(1.0f);
                currentValue++;
                spiritBar.fillAmount = 1 / countdownValue * currentValue;
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
