using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * The Boss needs multiple actions that are used in combat.
 * Each action needs an individual cooldown (and duration in some cases) so that the boss does not repeat the same action over and over.
 * Each action also requires its own animation, and method which is called when the action is being performed.
 * The Boss when not performing a special action needs to simply approach and attack.
 * 
 * 
 * 
 * List of possible actions
 * List of suitable actions
 * bools for each action
 * 
 * Calculates what actions are suitable and stores into list
 * Executes action
 * Waits for global cooldown
 * Cannot use same action until action specific cooldown has passed
 */

namespace ZaldensGambit
{
    public class Boss : Enemy
    {
        
        private enum Action { SpiritRip, Teleport, Clone, Summon, Charge, Laser, Dodge, Spin, None }
        [SerializeField] private bool canSpiritRip, canTeleport, canSplit, canSummon, canCharge, canLaser, canDodge, canSpin;
        [SerializeField] private float globalActionCooldown;
        [SerializeField] private float spiritRipCooldown, teleportCooldown, cloneCooldown, summonCooldown, chargeCooldown, laserCooldown, dodgeCooldown, spinCooldown;
        private float spiritRipTimer, teleportTimer, cloneTimer, summonTimer, chargeTimer, laserTimer, dodgeTimer, spinTimer;
        private bool singleAbilityBoss;
        private Action activeAction;
        private Action lastAction = Action.None;
        private float timeUntilNextAction = 0;

        // Dodging Variables
        private bool lookingToDodge;

        // Cloning Variables
        [SerializeField] private GameObject clonePrefab;
        private List<GameObject> activeClones = new List<GameObject>();
        private bool clonesActive;

        // Summon Variables
        [SerializeField] private GameObject[] summonPrefabs = new GameObject[4];
        private List<GameObject> activeSummons = new List<GameObject>();
        private bool summonsActive;

        protected override void Awake()
        {
            base.Awake();
            weaponHook = GetComponentInChildren<WeaponHook>();
            weaponHook.CloseDamageCollider();
        }

        protected override void Update()
        {
            base.Update();
            UpdateBossState();
            //PlayVoiceLines(currentHealth); // Needs further work to function correctly

            if (activeClones.Count == 0)
            {
                clonesActive = false;
            }

            if (activeSummons.Count == 0)
            {
                summonsActive = false;
            }

            if (lookingToDodge && Input.GetMouseButtonDown(0) && IsPlayerWithinRange(3))
            {
                charAnim.CrossFade("dodgeRoll", 0.2f);
                lookingToDodge = false;
            }
        }

        private void PlayVoiceLines(float currentHealth)
        {
            // Needs to flag when a voiceline has been played so as to not repeat.

            if (currentHealth < maximumHealth / 4) // 25% health
            {
                // Play Voiceline
            }
            else if (currentHealth < maximumHealth / 2) // 50% health
            {
                // Play Voiceline
            }
            else if (currentHealth < maximumHealth / 1.3) // 75% health
            {
                // Play Voiceline
            }
        }

        private bool CanSeePlayer()
        {
            bool isPlayerInSight = Physics.Raycast(transform.position + Vector3.up, transform.forward, Mathf.Infinity, playerLayer);
            Debug.DrawRay(transform.position + Vector3.up, transform.forward * 100);
            return isPlayerInSight;
        }

        private bool IsPlayerWithinRange(float range)
        {
            float distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceFromPlayer <= range)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CalculatePossibleActions()
        {
            List<Action> possibleActions = new List<Action>();

            if (timeUntilNextAction <= Time.time) // When the global cooldown period has passed...
            {
                print("Calculating actions...");

                // Check all possible actions that can be done, if they are suitable add them to the list of possible actions.
                possibleActions.Clear(); // Remove all previous possible actions that were recorded, as we are going to recalculate and do not want duplicates.

                if (canSpiritRip && lastAction != Action.SpiritRip && spiritRipTimer <= Time.time) // If we are allowed to perform the action, it was not the last action performed and the cooldown period has passed...
                {
                    if (CanSeePlayer()) // If the player is within sight of the Boss and does not have an obstacle in its path...
                    {
                        possibleActions.Add(Action.SpiritRip);
                    }
                }

                if (canSpin && lastAction != Action.Spin && spinTimer <= Time.time)
                {
                    if (IsPlayerWithinRange(3)) // If the player is within attacking range of the Boss...
                    {
                        possibleActions.Add(Action.Spin);
                    }
                }

                if (canLaser && lastAction != Action.Laser && laserTimer <= Time.time)
                {
                    if (CanSeePlayer()) // If the player is within sight of the Boss and does not have an obstacle in its path...
                    {
                        possibleActions.Add(Action.Laser);
                    }
                }

                if (canDodge && lastAction != Action.Dodge && dodgeTimer <= Time.time)
                {
                    if (IsPlayerWithinRange(attackRange)) // If the player is within attacking range of the boss...
                    {
                        possibleActions.Add(Action.Dodge);
                    }
                }

                if (canCharge && lastAction != Action.Charge && chargeTimer <= Time.time) 
                {
                    if (IsPlayerWithinRange(10)) // If the player is within charging range of the boss
                    {
                        possibleActions.Add(Action.Charge);
                    }
                }

                if (canSplit && lastAction != Action.Clone && cloneTimer <= Time.time && !clonesActive)
                {
                    possibleActions.Add(Action.Clone);
                }

                if (canSummon && lastAction != Action.Summon && summonTimer <= Time.time && !summonsActive)
                {
                    possibleActions.Add(Action.Summon);
                }

                if (canTeleport && lastAction != Action.Teleport && teleportTimer <= Time.time)
                {
                    possibleActions.Add(Action.Teleport);
                }

                if (possibleActions.Count == 1 && !singleAbilityBoss)
                {
                    singleAbilityBoss = true;
                    return;
                }

                if (singleAbilityBoss)
                {
                    lastAction = Action.None;
                }

                if (possibleActions != null && possibleActions.Count != 0)
                {
                    ChooseAction(possibleActions);
                }

            }
        }

        private void ChooseAction(List<Action> possibleActions)
        {
            print("Choosing an action...");
            print("I am avoiding " + lastAction);
            print("My choices are...");
            foreach (Action action in possibleActions)
            {
                print(action);
            }

            int selection = Random.Range(0, possibleActions.Count); // Generate a random number based on the total number of possibleActions

            activeAction = possibleActions[selection]; // Set activeAction to the number generated in the list of possibleActions

            ExecuteAction();
        }

        private void ExecuteAction()
        {           
            switch (activeAction)
            {
                case Action.SpiritRip:
                    // Projectile attack
                    spiritRipTimer = Time.time + spiritRipCooldown;
                    print("Perform spirit rip");
                    bool isPlayerInSight = Physics.Raycast(transform.position + Vector3.up, transform.forward, Mathf.Infinity, playerLayer);
                    if (isPlayerInSight)
                    {
                        player.GetComponent<SpiritSystem>().spiritEquipped = null;
                        player.GetComponent<SpiritSystem>().OnEquipSpirit(null);
                    }
                    break;
                case Action.Teleport:
                    // Teleport to a random position nearby the player, or maybe always behind
                    teleportTimer = Time.time + teleportCooldown;
                    print("Perform teleport");
                    transform.position = player.transform.position + -player.transform.forward * 2;
                    break;
                case Action.Clone:
                    // Summon two duplicates with lower health values and no boss actions
                    cloneTimer = Time.time + cloneCooldown;
                    print("Perform clone");
                    GameObject clone = null;
                    for (int i = 0; i < 2; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                clone = Instantiate(clonePrefab, transform.position + transform.right * 2, Quaternion.identity, null);
                                activeClones.Add(clone);
                                break;
                            case 1:
                                clone = Instantiate(clonePrefab, transform.position + -transform.right * 2, Quaternion.identity, null);
                                activeClones.Add(clone);
                                break;
                        }
                    }
                    clonesActive = true;
                    break;
                case Action.Summon:
                    // Summon a group of minions, remain stationary and invulnerable to damage unless shieldbashed
                    summonTimer = Time.time + summonTimer;
                    print("Perform summon");
                    GameObject summon = null;
                    for (int i = 0; i < 4; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                summon = Instantiate(summonPrefabs[0], transform.position + transform.right, Quaternion.identity, null);
                                activeSummons.Add(summon);
                                break;
                            case 1:
                                summon = Instantiate(summonPrefabs[1], transform.position + -transform.right, Quaternion.identity, null);
                                activeSummons.Add(summon);
                                break;
                            case 2:
                                summon = Instantiate(summonPrefabs[2], transform.position + transform.forward, Quaternion.identity, null);
                                activeSummons.Add(summon);
                                break;
                            case 3:
                                summon = Instantiate(summonPrefabs[3], transform.position + -transform.forward, Quaternion.identity, null);
                                activeSummons.Add(summon);
                                break;
                        }
                    }
                    summonsActive = true;
                    break;
                case Action.Charge:
                    // Charge towards the player and deal damage on contact + knockback, destroy obstacle if collided with
                    chargeTimer = Time.time + chargeCooldown;
                    // Unsure how to handle this right now
                    print("Perform charge");
                    break;
                case Action.Laser:
                    // Constant projectile attack
                    laserTimer = Time.time + laserCooldown;
                    // Coroutine that deals damage continuously
                    print("Perform laser");
                    break;
                case Action.Dodge:
                    // Look to dodge the players incoming attacks
                    dodgeTimer = Time.time + dodgeCooldown;
                    lookingToDodge = true;
                    print("Perform dodge");
                    break;
                case Action.Spin:
                    // Spin and move towards the player at the same time
                    spinTimer = Time.time + spinCooldown;
                    // Play an animation with a long delay before disabling the damage collider
                    print("Perform spin");
                    break;
                default:
                    print("Waiting...");
                    break;
            }

            if (activeAction != Action.None)
            {
                lastAction = activeAction;
                activeAction = Action.None;
                timeUntilNextAction = Time.time + globalActionCooldown;
            }
        }

        private void UpdateBossState()
        {
            CalculatePossibleActions();
        }
    }

    //[System.Serializable]
    //public class BossAction
    //{
    //    public float cooldown;
    //    public float duration;
    //    public string desiredAnimation;
    //}
}
