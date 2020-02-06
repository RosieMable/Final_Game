using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class FriendlyNPC : MonoBehaviour
    {
        [SerializeField] private AudioClip[] dialoguePhrases;
        [SerializeField] private float dialogueTriggerDistance;
        private bool enteredRange;
        private float timeUntilNextDialogue = 0f;
        private float dialogueCooldown = 5f;
        private StateManager player;
        private AudioSource audioSource;

        private void Start()
        {
            player = FindObjectOfType<StateManager>();
        }

        private void Update()
        {
            PlayAudioOnPlayerEntry();
        }

        private void PlayAudioOnPlayerEntry()
        {
            // If the player is within range and enough time has passed for another random dialogue
            if (Vector3.Distance(transform.position, player.transform.position) < dialogueTriggerDistance && timeUntilNextDialogue < Time.time && !enteredRange)
            {
                enteredRange = true;
                timeUntilNextDialogue = Time.time + dialogueCooldown;
                int dialogueToPlay = Random.Range(0, dialoguePhrases.Length);
                audioSource.clip = dialoguePhrases[dialogueToPlay];
                audioSource.Play();
            }
            else
            {
                enteredRange = false;
            }
        }
    }
}
