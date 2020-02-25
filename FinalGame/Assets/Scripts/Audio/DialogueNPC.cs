using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class DialogueNPC : MonoBehaviour
    {
        // Proximity Dialogue - What is said when the player walks nearby the NPC
        [SerializeField] private Dialogue[] proximityDialogue;
        private int proximityDialogueIndex = -1;
        [SerializeField] private float proximityDialogueTriggerDistance = 5f;
        private float timeUntilNextProximityDialogue = 0f;
        private float proximityDialogueCooldown = 10f;
        private static bool isSomeoneInRange;
        private static bool isDialoguePlaying;
        private static DialogueNPC[] NPCs;

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
            NPCs = FindObjectsOfType<DialogueNPC>();
        }

        // If the player is outside of our range, do nothing
        // If the player enters our range, speak - Done
        // If the player leaves our range, stop speaking

        private void Update()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // Flag when the player is within range, if enough time has passed, play proximity dialogue
            if (distanceToPlayer < proximityDialogueTriggerDistance)
            {
                isSomeoneInRange = true;

                if (timeUntilNextProximityDialogue < Time.time && !audioSource.isPlaying)
                {
                    PlayProximityDialogue();
                }
            }
            else // Check if any other NPC is in range
            {
                bool inRange = false;
                foreach (DialogueNPC npc in NPCs)
                {
                    if (Vector3.Distance(npc.transform.position, player.transform.position) < proximityDialogueTriggerDistance)
                    {
                        inRange = true;
                        break; // Exit loop
                    }
                    else
                    {
                        inRange = false;
                    }
                }
                isSomeoneInRange = inRange; // Assign result from loop
            }
            
            // Check if any other NPC is playing their dialogue
            bool dialoguePlaying = false;
            foreach (DialogueNPC npc in NPCs)
            {
                if (npc.audioSource.isPlaying)
                {
                    dialoguePlaying = true;
                    break; // Exit loop
                }
                else
                {
                    dialoguePlaying = false;
                }
            }
            isDialoguePlaying = dialoguePlaying; // Assign result from loop        

            // If no NPCs dialogue is playing, remove dialogue box from UI
            if (!isDialoguePlaying)
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
