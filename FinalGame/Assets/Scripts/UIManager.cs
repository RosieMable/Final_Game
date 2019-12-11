using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    private static UIManager _instance;


    #region "Spirit UI Variables"

    public Image SpiritIcon;

    public Image SpiritBar;

    #endregion

    public void ManageSpiritUI(Spirit_ScriptableObj _spiritToDisplay)
    {
        if (_spiritToDisplay!=null)
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
