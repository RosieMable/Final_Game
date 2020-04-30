using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZaldensGambit
{
    public class AudioTriggerZone : MonoBehaviour
    {
        [SerializeField] private AudioClip audioClip;

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<StateManager>() && audioClip != null)
            {
                CameraManager.instance.GetComponentInChildren<AudioSource>().clip = audioClip;
                CameraManager.instance.GetComponentInChildren<AudioSource>().Play();
            }
        }
    }
}
