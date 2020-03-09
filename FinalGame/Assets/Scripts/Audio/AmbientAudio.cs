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
        private float timer;
        private float cooldown = 2f;
        private float distanceToPlayer;

        private void Start()
        {
            player = FindObjectOfType<StateManager>();
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = audioClips[0];
        }

        private void Update()
        {
            if (timer < Time.time)
            {
                timer = Time.time + cooldown;
                distanceToPlayer = Vector3.Distance(transform.position, player.transform.position); // Record distance from the player
            }            

            if (distanceToPlayer < audioSource.maxDistance && !inRange) // If the player is within range of the audioSource...
            {
                // Play a random clip
                inRange = true;
                int clipToPlay = Random.Range(0, audioClips.Length - 1);
                audioSource.clip = audioClips[clipToPlay];
                audioSource.Play();
            }
            else if (distanceToPlayer > audioSource.maxDistance + 5) // Otherwise...
            {
                // Stop playing
                inRange = false;
                audioSource.Pause();
            }

            if (!audioSource.isPlaying && inRange) // If the audioSource is NOT playing whilst the player is in range...
            {
                // Play a random clip
                int clipToPlay = Random.Range(0, audioClips.Length - 1);
                audioSource.clip = audioClips[clipToPlay];
                audioSource.Play();
            }
        }
    }
}
