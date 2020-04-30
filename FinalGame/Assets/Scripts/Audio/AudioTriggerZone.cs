using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZaldensGambit
{
    public class AudioTriggerZone : MonoBehaviour
    {
        [SerializeField] private AudioClip audioClip;
        private bool played;

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<StateManager>() && audioClip != null && played != true)
            {
                CameraManager.instance.GetComponentInChildren<AudioSource>().clip = audioClip;
                CameraManager.instance.GetComponentInChildren<AudioSource>().Play();
                played = true;
            }
        }
    }
}
