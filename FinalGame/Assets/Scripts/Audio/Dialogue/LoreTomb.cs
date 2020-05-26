using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class LoreTomb : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string[] loreEntries = new string[0];
        private int index = -1;
        [SerializeField] private float interactionRange = 2f;
        [HideInInspector] public bool inRange;

        private float distanceToPlayer = 0;
        private StateManager player;
        private UIManager UIManager;

        private void Start()
        {
            UIManager = UIManager.Instance;
            player = FindObjectOfType<StateManager>();
        }

        private void Update()
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer < interactionRange)
            {
                if (!inRange)
                {
                    inRange = true;
                    UIManager.DisplayDialogue("E to interact.");
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Interact();
                }
            }
            else
            {
                inRange = false;
                index = -1; // Reset index position
                //UIManager.HideDialogue();
            }
        }

        private void Interact()
        {
            index++;
            if (index >= loreEntries.Length)
            {
                UIManager.HideDialogue();
                index = -1;
                return;
            }
            UIManager.DisplayDialogue(loreEntries[index]);
        }
    }
}
