using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class FriendlyNPC : MonoBehaviour
    {
        // Proximity Dialogue - What is said when the player walks nearby the NPC
        [SerializeField] private Dialogue[] proximityDialogue;
        private int proximityDialogueIndex = -1;
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
        private UIManager UIManager;

        private void Start()
        {
            UIManager = UIManager.Instance;
            player = FindObjectOfType<StateManager>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // If the player is within range and enough time has passed for another random dialogue    
            if (distanceToPlayer < proximityDialogueTriggerDistance)
            {
                if (timeUntilNextProximityDialogue < Time.time && !audioSource.isPlaying)
                {
                    if (!enteredRange)
                    {
                        enteredRange = true;
                        PlayProximityDialogue();
                    }
                }
            }
            else
            {
                enteredRange = false;
            }

            if (!audioSource.isPlaying)
            {
                UIManager.HideDialogue();
            }

            if (distanceToPlayer < interactionRange)
            {
                // Visual indicator needed to be displayed to the player that they can interact, perhaps a '!' or message on screen.

                if (!audioSource.isPlaying)
                {
                    UIManager.DisplayDialogue("E to interact.");
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    player.interacting = true;
                    dialogueIndex++;

                    // If there is dialogue to play...
                    if (dialogueIndex <= mainDialogue.Length - 1)
                    {
                        audioSource.Stop();
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
            proximityDialogueIndex++;

            if (proximityDialogueIndex > proximityDialogue.Length - 1)
            {
                proximityDialogueIndex = 0;
                UIManager.HideDialogue();
            }
            UIManager.DisplayDialogue(proximityDialogue[proximityDialogueIndex]);
            audioSource.clip = proximityDialogue[proximityDialogueIndex].audio;
            audioSource.Play();
        }

        private void PlayMainDialogue()
        {
            timeUntilNextProximityDialogue = Time.time + (proximityDialogueCooldown * 3);
            UIManager.DisplayDialogue(mainDialogue[dialogueIndex]);

            if (mainDialogue[dialogueIndex].interactionNeeded)
            {
                CameraManager.instance.ToggleCursorVisibleState(true);
            }
            else
            {
                CameraManager.instance.ToggleCursorVisibleState(false);
            }
            audioSource.clip = mainDialogue[dialogueIndex].audio;
            audioSource.Play();     
        }
    }
}
