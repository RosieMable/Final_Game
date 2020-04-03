using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class FateWeaver : DialogueNPC
    {
        [SerializeField] private GameObject DungeonChoiceUI;
        [SerializeField] private GameObject DungeonCardSelectionUI;
        [SerializeField] private GameObject SpiritCardSelectionUI;
        private bool cardsDealt;

        // Talk to NPC
        // NPC asks if you want to enter dungeon
        // Player says yes - select cards to be dealt
        // Player says no - dialogue ends
        // Player confirms cards dealt - selects spirit to take into dungeon
        // Portal opens

        protected override void CheckMainDialogue()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (cardsDealt) // If we have already been dealt cards, do not proceed. (Stops redealing cards again if the player doesn't like the dungeon dealt.)
                {
                    return;
                }

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

                    //print("End of main dialogue, looping back to start!");
                    //player.interacting = false;
                    //CameraManager.instance.ToggleCursorVisibleState(false);
                    dialogueIndex = -1;
                }
            }
        }

        public void DisplayCardSelectionUI()
        {
            // Reveal the UI elements needed for selecting what cards you want dealt.

        }

        public void DisplaySpiritSelectionUI()
        {
            // Reveal the UI elements needed for selecting what spirit card you want to bring with you.
            // For the beta, I recommend giving the players every spirit to choose from so we can test them all easily.

            cardsDealt = true;
        }
    }
}
