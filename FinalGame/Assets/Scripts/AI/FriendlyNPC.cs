using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class FriendlyNPC : MonoBehaviour
    {
        // Proximity Dialogue - What is said when the player walks nearby the NPC
        [SerializeField] private Dialogue[] proximityDialogue;
        [SerializeField] private float proximityDialogueTriggerDistance = 5f;
        private float timeUntilNextProximityDialogue = 0f;
        private float proximityDialogueCooldown = 10f;
        private bool enteredRange;

        // Main Dialogue - What is said when the NPC is interacted with
        [SerializeField] private Dialogue[] mainDialogue;
        [SerializeField] private float interactionRange = 2f;
        private int dialogueIndex = -1;

        private StateManager player;
        private AudioSource audioSource;

        private void Start()
        {
            player = FindObjectOfType<StateManager>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // If the player is within range and enough time has passed for another random dialogue    
            if (distanceToPlayer < proximityDialogueTriggerDistance && timeUntilNextProximityDialogue < Time.time && !enteredRange)
            {
                enteredRange = true;
                PlayProximityDialogue();
            }
            else
            {
                enteredRange = false;
            }

            if (distanceToPlayer < interactionRange)
            {
                print("Within range!");
                if (Input.GetKeyDown(KeyCode.E))
                {
                    print("Interacted with!");
                    player.interacting = true;
                    dialogueIndex++;

                    // If there is dialogue to play...
                    if (dialogueIndex <= mainDialogue.Length - 1)
                    {
                        PlayMainDialogue();
                    }
                    else // End interaction, reset dialogue index for if the player wants to start conversation again.
                    {
                        print("End of main dialogue, looping back to start!");
                        player.interacting = false;
                        CameraManager.instance.ToggleCursorVisibleState(false);
                        dialogueIndex = -1;
                    }
                }
            }

            if (audioSource.isPlaying && dialogueIndex != -1)
            {
                print(mainDialogue[dialogueIndex].dialogue + " should be on screen!");
            }
        }

        private void PlayProximityDialogue()
        {
            timeUntilNextProximityDialogue = Time.time + proximityDialogueCooldown;
            int dialogueToPlay = Random.Range(0, proximityDialogue.Length);
            print(proximityDialogue[dialogueToPlay].dialogue);
            //audioSource.clip = proximityDialogue[dialogueToPlay].audio;
            //audioSource.Play();
        }

        private void PlayMainDialogue()
        {
            print(mainDialogue[dialogueIndex].dialogue);

            if (mainDialogue[dialogueIndex].interactionNeeded)
            {
                CameraManager.instance.ToggleCursorVisibleState(true);
            }
            else
            {
                CameraManager.instance.ToggleCursorVisibleState(false);
            }
            //audioSource.clip = mainDialogue[dialogueIndex].audio;
            //audioSource.Play();     
        }
    }
}
