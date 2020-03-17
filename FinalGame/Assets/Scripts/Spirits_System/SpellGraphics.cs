using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellGraphics : MonoBehaviour
{
    public GameObject[] CharacterEffect;
    public GameObject ChosenCharacterEffect;
    public Transform CharacterAttachPoint;
    public float CharacterEffect_DestroyTime = 10;
    [Space]

    public GameObject[] CharacterEffect2;
    public GameObject ChosenCharacterEffect2;
    public Transform CharacterAttachPoint2;
    public float CharacterEffect2_DestroyTime = 10;
    [Space]

    public GameObject[] MainEffect;
    public GameObject ChosenMainEffect;
    public Transform AttachPoint;
    public float Effect_DestroyTime = 10;
    [Space]

    public GameObject[] AdditionalEffect;
    public GameObject ChosenAdditionalEffect;
    public Transform AdditionalEffectAttachPoint;
    public float AdditionalEffect_DestroyTime = 10;
    public delegate void UpdateGraphics(SpellsBaseGraphics[] spellsBases);

    public event UpdateGraphics UpdateAnim;

    private Animator anim;
    private int SkillNumber;

    public SpellsBaseGraphics[] Spells;

    public void OnEnable()
    {
        UpdateAnim += SpellCase;
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    [HideInInspector] public bool IsMobile;
    public void ActivateEffect()
    {
        UpdateAnim(Spells);
        if (ChosenMainEffect == null) return;
        var instance = Instantiate(ChosenMainEffect, AttachPoint.transform.position, AttachPoint.transform.rotation);
        if (Effect_DestroyTime > 0.01f) Destroy(instance, Effect_DestroyTime);
    }

    public void ActivateAdditionalEffect()
    {
        UpdateAnim(Spells);
        if (ChosenAdditionalEffect == null) return;
        if (AdditionalEffectAttachPoint != null)
        {
            var instance = Instantiate(ChosenAdditionalEffect, AdditionalEffectAttachPoint.transform.position, AdditionalEffectAttachPoint.transform.rotation);
            if (AdditionalEffect_DestroyTime > 0.01f) Destroy(instance, AdditionalEffect_DestroyTime);
        }
        else ChosenAdditionalEffect.SetActive(true);
    }

    public void ActivateCharacterEffect()
    {
        UpdateAnim(Spells);
        if (ChosenCharacterEffect == null) return;
        var instance = Instantiate(ChosenCharacterEffect, CharacterAttachPoint.transform.position, CharacterAttachPoint.transform.rotation, CharacterAttachPoint.transform);
        if (CharacterEffect_DestroyTime > 0.01f) Destroy(instance, CharacterEffect_DestroyTime);
    }

    public void ActivateCharacterEffect2()
    {
        UpdateAnim(Spells);
        if (ChosenCharacterEffect2 == null) return;
        var instance = Instantiate(ChosenCharacterEffect2, CharacterAttachPoint2.transform.position, CharacterAttachPoint2.transform.rotation, CharacterAttachPoint2);
        if (CharacterEffect2_DestroyTime > 0.01f) Destroy(instance, CharacterEffect2_DestroyTime);
    }

    void SpellCase(SpellsBaseGraphics[] spellsArray)
    {
        SkillNumber = anim.GetInteger("SkillNumber");

        switch (SkillNumber)
        {
            case 1:
                ChosenAdditionalEffect = spellsArray[0].AdditionalEffect;
                AdditionalEffectAttachPoint = spellsArray[0].AdditionalEffectAttachPoint;
                ChosenCharacterEffect = spellsArray[0].CharacterEffect;
                CharacterAttachPoint = spellsArray[0].CharacterAttachPoint;
                ChosenCharacterEffect2 = spellsArray[0].CharacterEffect2;
                CharacterAttachPoint2 = spellsArray[0].CharacterAttachPoint2;
                ChosenMainEffect = spellsArray[0].MainEffect;
                AttachPoint = spellsArray[0].MainEffectAttachPoint;
                break;

            case 2:
                ChosenAdditionalEffect = spellsArray[1].AdditionalEffect;
                AdditionalEffectAttachPoint = spellsArray[1].AdditionalEffectAttachPoint;
                ChosenCharacterEffect = spellsArray[1].CharacterEffect;
                CharacterAttachPoint = spellsArray[1].CharacterAttachPoint;
                ChosenCharacterEffect2 = spellsArray[1].CharacterEffect2;
                CharacterAttachPoint2 = spellsArray[1].CharacterAttachPoint2;
                ChosenMainEffect = spellsArray[1].MainEffect;
                AttachPoint = spellsArray[1].MainEffectAttachPoint;
                break;

            case 3:
                ChosenAdditionalEffect = spellsArray[2].AdditionalEffect;
                AdditionalEffectAttachPoint = spellsArray[2].AdditionalEffectAttachPoint;
                ChosenCharacterEffect = spellsArray[2].CharacterEffect;
                CharacterAttachPoint = spellsArray[2].CharacterAttachPoint;
                ChosenCharacterEffect2 = spellsArray[2].CharacterEffect2;
                CharacterAttachPoint2 = spellsArray[2].CharacterAttachPoint2;
                ChosenMainEffect = spellsArray[2].MainEffect;
                AttachPoint = spellsArray[2].MainEffectAttachPoint;
                break;

            case 4:
                ChosenAdditionalEffect = spellsArray[3].AdditionalEffect;
                AdditionalEffectAttachPoint = spellsArray[3].AdditionalEffectAttachPoint;
                ChosenCharacterEffect = spellsArray[3].CharacterEffect;
                CharacterAttachPoint = spellsArray[3].CharacterAttachPoint;
                ChosenCharacterEffect2 = spellsArray[3].CharacterEffect2;
                CharacterAttachPoint2 = spellsArray[3].CharacterAttachPoint2;
                ChosenMainEffect = spellsArray[3].MainEffect;
                AttachPoint = spellsArray[3].MainEffectAttachPoint;
                break;

            default:
                print("Incorrect intelligence level.");
                break;
        }
    }

    public void OnCall()
    {
        UpdateAnim.Invoke(Spells);
    }

    private void OnDisable()
    {
        UpdateAnim -= SpellCase;
    }
}

