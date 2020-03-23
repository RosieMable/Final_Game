using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private Sprite[] damageEffects;
    private Image damageEffect;
    private Coroutine damageCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        damageEffect = GameObject.Find("DamageEffect").GetComponent<Image>();
        damageEffect.enabled = false;
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
