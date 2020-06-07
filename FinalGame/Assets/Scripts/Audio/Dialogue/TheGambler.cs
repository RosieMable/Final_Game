using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class TheGambler : DialogueNPC
    {
        [SerializeField] private GameObject SpiritCardsArena;
        private bool cardsDealt;

        CardUISystem cardUISystem;

        private void Awake()
        {
            cardUISystem = FindObjectOfType<CardUISystem>();
            SpiritCardsArena = cardUISystem.SpiritCardsUIArena;
            SpiritCardsArena.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();

            if (cardUISystem == null)
            {
                cardUISystem = FindObjectOfType<CardUISystem>();
            }

            if (SpiritCardsArena == null)
            {
                SpiritCardsArena = cardUISystem.DungeonUIChoice;
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
                    SpiritCardsArena.SetActive(true);
                    cardUISystem.SpiritSelection();
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

