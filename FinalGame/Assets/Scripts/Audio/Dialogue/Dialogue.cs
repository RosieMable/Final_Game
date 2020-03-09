using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public AudioClip audio;
    [TextArea] public string dialogue;
    public bool interactionNeeded;
}
