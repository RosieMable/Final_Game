using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class FateWeaver : DialogueNPC
    {
        [SerializeField] private GameObject DungeonChoiceUI;
        public bool cardsDealt;

        CardUISystem cardUISystem;

        private void Awake()
        {
            cardUISystem = FindObjectOfType<CardUISystem>();
            DungeonChoiceUI = cardUISystem.DungeonUIChoice;
            DungeonChoiceUI.SetActive(false);
        }

        protected override void Update()
        {
            if (cardsDealt) // If we have already been dealt cards, do not proceed. (Stops redealing cards again if the player doesn't like the dungeon dealt.)
            {
                return;
            }

            base.Update();

            if (cardUISystem == null)
            {
                cardUISystem = FindObjectOfType<CardUISystem>();
            }

            if (DungeonChoiceUI == null)
            {
                DungeonChoiceUI = cardUISystem.DungeonUIChoice;
            }
        }

        protected override void CheckMainDialogue()
        {
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
                else // When dialogue is exhausted...
                {
                    DungeonChoiceUI.SetActive(true);
                    print("End of main dialogue, looping back to start!");
                    dialogueIndex = -1;
                }
            }
        }

        public void ResetInteraction()
        {
            player.interacting = false;
            CameraManager.instance.ToggleCursorVisibleState(false);
            dialogueIndex = -1;
        }
    }
}
