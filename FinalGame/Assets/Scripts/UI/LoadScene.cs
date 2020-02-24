﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private string sceneName = "N/A";

    public void Load()
    {
        SceneManager.LoadScene(sceneName);
    }
}
