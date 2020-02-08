using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZaldensGambit
{
    public class AmbientAudio : MonoBehaviour
    {
        private AudioSource audioSource;
        private StateManager player;
        [SerializeField] private AudioClip[] audioClips;
        private bool inRange;

        private void Start()
        {
            player = FindObjectOfType<StateManager>();
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = audioClips[0];
        }

        private void Update()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer < audioSource.maxDistance && !inRange)
            {
                inRange = true;
                int clipToPlay = Random.Range(0, audioClips.Length - 1);
                audioSource.clip = audioClips[clipToPlay];
                audioSource.Play();
            }
            else if (distanceToPlayer > audioSource.maxDistance)
            {
                inRange = false;
                audioSource.Stop();
            }

            if (!audioSource.isPlaying && inRange)
            {
                int clipToPlay = Random.Range(0, audioClips.Length - 1);
                audioSource.clip = audioClips[clipToPlay];
                audioSource.Play();
            }
        }
    }
}
