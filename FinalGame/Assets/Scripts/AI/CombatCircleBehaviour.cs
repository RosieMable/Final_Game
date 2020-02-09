using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class CombatCircleBehaviour : MonoBehaviour
    {
        /* Logic:
         * 1) Move towards the player when within aggro range.
         * 2) When within a reasonable range, avoid other AIs unless moving to attack.
         * 3) Move towards the player whilst avoiding other AIs unless moving to attack.
         * 4) When the player is within attack range, check if I'm allowed to attack.
         * 
         * Notes on permission given for attacks:
         * - Cannot attack if there is already a maximum number of allowed attackers
         * - When denied, continue to move aroundthe player and repeat
         * - If the player moves out of attack range, remove from attackers list
         * - If killed, remove from attackers list
         * 
         * Variables Needed:
         * Aggro range - Done
         * Attack range - Done
         * In-range boolean - Done
         * Moving-to-attack boolean - Done
         * Avoid radius - Done
         * List of attackers - Done
         * Maximum number of allowed attackers- Done
        */

        private Enemy AI;
        [SerializeField] private float avoidRadius = 3f;
        private bool withinRangeOfTarget;
        private bool movingToAttack;
        private static List<Enemy> currentAttackers;
        private static int maximumNumberOfAttackers = 3;

        // Start is called before the first frame update
        void Start()
        {
            AI = GetComponent<Enemy>();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}