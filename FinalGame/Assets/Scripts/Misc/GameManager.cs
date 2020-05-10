using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZaldensGambit;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private Sprite[] damageEffects;
    private Image damageEffect;
    private Coroutine damageCoroutine;

    public BaseSpirit spiritToEquipInDungeon;

    SpiritSystem spiritSystem;

    private Scene currentScene;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(gameObject);

        damageEffect = GameObject.Find("DamageEffect").GetComponent<Image>();
        damageEffect.enabled = false;

        currentScene = SceneManager.GetActiveScene();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    SceneManager.LoadScene("Title Screen");
        //    Cursor.visible = true;
        //    Cursor.lockState = CursorLockMode.None;
        //}

        if(SceneManager.GetActiveScene() != currentScene)
        {
            if(spiritSystem== null)
            {
                spiritSystem = FindObjectOfType<SpiritSystem>();
                if (spiritToEquipInDungeon != null)
                {
                    spiritSystem.spiritEquipped = spiritToEquipInDungeon;
                    spiritSystem.OnEquipSpirit(spiritToEquipInDungeon);
                }
            }
        }
    }

    public void GameOver()
    {
        StartCoroutine(GameOverAfterDelay(3));
    }

    public void InstantGameOver()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        CardInventory.instance.Init();
        //CardInventory.instance.ToggleInventory();
    }

    public void PlayDamageEffect()
    {
        int effectToPlay = (int)Random.Range(0f, damageEffects.Length);
        damageEffect.sprite = damageEffects[effectToPlay];

        if (damageCoroutine == null)
        {
            damageCoroutine = StartCoroutine(DamageEffect(1f));
        }
        else
        {
            StopCoroutine(damageCoroutine);
            damageEffect.color = new Color(damageEffect.color.r, damageEffect.color.g, damageEffect.color.b, 1);
            damageCoroutine = StartCoroutine(DamageEffect(1f));
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

    private IEnumerator GameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        CardInventory.instance.Init();
        //CardInventory.instance.ToggleInventory();
    }

    private IEnumerator DamageEffect(float duration)
    {
        damageEffect.enabled = true;

        yield return new WaitForSeconds(duration);

        while (damageEffect.color.a > 0f)
        {
            damageEffect.color = new Color(damageEffect.color.r, damageEffect.color.g, damageEffect.color.b, damageEffect.color.a - Time.deltaTime);
            yield return null;
        }

        damageEffect.enabled = false;
        damageEffect.color = new Color(damageEffect.color.r, damageEffect.color.g, damageEffect.color.b, 1);
    }
}
