using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZaldensGambit
{
    public class AudioTriggerZone : MonoBehaviour
    {
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private string[] subtitles;
        private bool played;
        private UIManager UIManager;
        [SerializeField] private float subtitleDuration = 10;

        private void Start()
        {
            UIManager = UIManager.Instance;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<StateManager>() && audioClip != null && played != true)
            {
                CameraManager.instance.GetComponentInChildren<AudioSource>().clip = audioClip;
                CameraManager.instance.GetComponentInChildren<AudioSource>().Play();
                played = true;

                UIManager.DisplayDialogue(subtitles, subtitleDuration);
            }
        }
    }
}
