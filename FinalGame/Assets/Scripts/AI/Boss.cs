using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZaldensGambit
{
    public class Boss : Enemy
    {        
        private enum Action { SpiritRip, Teleport, Clone, Summon, Charge, Laser, Dodge, Spin, Nothing }
        [SerializeField] private bool canSpiritRip, canTeleport, canSplit, canSummon, canCharge, canLaser, canDodge, canSpin;        
        [SerializeField] private float spiritRipCooldown, teleportCooldown, cloneCooldown, summonCooldown, chargeCooldown, laserCooldown, dodgeCooldown, spinCooldown;
        private float spiritRipTimer, teleportTimer, cloneTimer, summonTimer, chargeTimer, laserTimer, dodgeTimer, spinTimer;
        private bool singleAbilityBoss;
        private Action activeAction;
        private Action lastAction = Action.Nothing;

        // Global Variables
        private float timeUntilNextAction = 0;
        [SerializeField] private float globalActionCooldown;
        private bool abilityCasting;
        [SerializeField] private GameObject abilityChargingParticleEffect;

        // Spirit Rip Variables
        [SerializeField] private GameObject spiritPickUp;

        // Laser Variables
        private bool lasering;
        private float laserHitDelay;
        private float laserHitCooldown = 0.05f;

        // Charging Variables
        private bool charging;
        private Vector3 chargePosition;
        private float originalSpeed;
        private float originalAcceleration;
        private float chargeSpeed;
        private float chargeAcceleration;

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
            originalSpeed = speed;
            chargeSpeed = speed * 2;
            originalAcceleration = agent.acceleration;
            chargeAcceleration = originalAcceleration * 2;
        }

        protected override void Update()
        {
            base.Update();
            CalculatePossibleActions();
            print(currentState);
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

            if (lasering && Time.time > laserHitDelay)
            {
                RaycastHit hit;
                Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, Mathf.Infinity);

                if (hit.collider != null)
                {
                    if (hit.collider.gameObject == player || hit.collider.gameObject.GetComponentInParent<StateManager>())
                    {
                        player.GetComponent<StateManager>().TakeDamage(1, transform, false, false);
                        laserHitDelay = laserHitCooldown + Time.time;
                    }
                }                
            }
        }

        protected override void CombatBehaviour()
        {
            if (abilityCasting)
            {
                RotateTowardsTarget(player.transform);
                agent.Stop();
            }
            else if (charging)
            {
                RotateTowardsTarget(chargePosition);
                agent.speed = chargeSpeed;
                agent.acceleration = chargeAcceleration;
                agent.stoppingDistance = 0;
                agent.SetDestination(chargePosition);
            }
            else if (movingToAttack)
            {
                MoveToTarget();
            }

            if (Vector3.Distance(transform.position, chargePosition) <= 2)
            {
                agent.speed = originalSpeed;
                agent.acceleration = originalAcceleration;
                agent.stoppingDistance = 1;
                charging = false;
            }
        }

        protected override State UpdateState()
        {
            bool canConsiderAttacking = Vector3.Distance(transform.position, player.transform.position) < aggroRange;

            if (canConsiderAttacking)
            {
                movingToAttack = true;
            }

            bool isInAttackRange = Vector3.Distance(transform.position, player.transform.position) < attackRange;

            if (isInAttackRange && (!abilityCasting || !charging))
            {
                return State.Attacking;
            }

            bool isInAggroRange = Vector3.Distance(transform.position, player.transform.position) < aggroRange;

            if (isInAggroRange)
            {
                return State.Pursuing;
            }

            return State.Idle;
        }

        protected override void PerformStateBehaviour()
        {
            switch (currentState)
            {
                case State.Idle:
                    // Maybe regen health after a delay?
                    Patrol();
                    withinRangeOfTarget = false;
                    break;
                case State.Attacking:
                    if (!isInvulnerable && !inAction && Time.time > attackDelay)
                    {
                        agent.isStopped = true;
                        bool playerInFront = Physics.Raycast(transform.position, transform.forward, 2, playerLayer);

                        if (playerInFront)
                        {
                            attackDelay = Time.time + attackCooldown;
                            int animationToPlay = Random.Range(0, attackAnimations.Length);
                            charAnim.CrossFade(attackAnimations[animationToPlay].name, 0.2f);
                            RotateTowardsTarget(player.transform);
                        }
                        else
                        {
                            RotateTowardsTarget(player.transform);
                        }
                    }
                    else
                    {
                        RotateTowardsTarget(player.transform);
                    }
                    break;
                case State.Pursuing:
                    withinRangeOfTarget = true;
                    if (!isInvulnerable && !inAction)
                    {
                        agent.isStopped = false;
                        CombatBehaviour();
                    }
                    break;
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

            if (timeUntilNextAction <= Time.time && (currentState == State.Pursuing || currentState == State.Attacking)) // When the global cooldown period has passed...
            {
                print("Calculating actions...");
                // Check all possible actions that can be done, if they are suitable add them to the list of possible actions.
                possibleActions.Clear(); // Remove all previous possible actions that were recorded, as we are going to recalculate and do not want duplicates.

                if (canSpiritRip && lastAction != Action.SpiritRip && spiritRipTimer <= Time.time && player.GetComponent<SpiritSystem>().spiritEquipped != null) // If we are allowed to perform the action, it was not the last action performed and the cooldown period has passed...
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
                    lastAction = Action.Nothing;
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
            print("I am avoiding the action: " + lastAction);
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
                    StartCoroutine(PerformAction(activeAction, 3));
                    break;
                case Action.Teleport:
                    StartCoroutine(PerformAction(activeAction, 2));
                    break;
                case Action.Clone:
                    StartCoroutine(PerformAction(activeAction, 3));
                    break;
                case Action.Summon:
                    StartCoroutine(PerformAction(activeAction, 3));
                    break;
                case Action.Charge:
                    StartCoroutine(PerformAction(activeAction, 3));
                    break;
                case Action.Laser:
                    StartCoroutine(PerformAction(activeAction, 3));
                    break;
                case Action.Dodge:
                    StartCoroutine(PerformAction(activeAction, 0));
                    break;
                case Action.Spin:
                    StartCoroutine(PerformAction(activeAction, 1f));
                    break;
                default:
                    print("Waiting...");
                    break;
            }

            if (activeAction != Action.Nothing)
            {
                lastAction = activeAction;
                activeAction = Action.Nothing;
                timeUntilNextAction = Time.time + globalActionCooldown;
            }
        }

        private IEnumerator PerformAction(Action action, float delayBeforeCast)
        {
            switch (action)
            {
                case Action.SpiritRip:
                    // Animation cast + voiceline
                    abilityCasting = true;
                    break;
                case Action.Teleport:
                    // Animation cast + voiceline
                    abilityCasting = true;
                    break;
                case Action.Clone:
                    // Animation cast + voiceline
                    abilityCasting = true;
                    break;
                case Action.Summon:
                    // Animation cast + voiceline
                    abilityCasting = true;
                    break;
                case Action.Charge:
                    // Animation cast + voiceline
                    abilityCasting = true;
                    break;
                case Action.Laser:
                    // Animation cast + voiceline
                    abilityCasting = true;
                    break;
                case Action.Dodge:
                    // ???
                    break;
                case Action.Spin:
                    // ???
                    break;
            }

            if (abilityCasting)
            {                
                GameObject particleEffect = Instantiate(abilityChargingParticleEffect, transform.position + Vector3.up, abilityChargingParticleEffect.transform.rotation, transform);
            }

            if (action == Action.Teleport)
            {
                GameObject particleEffect = Instantiate(abilityChargingParticleEffect, player.transform.position + -player.transform.forward * 2 + Vector3.up, abilityChargingParticleEffect.transform.rotation, player.transform);
            }

            if (action == Action.Summon)
            {
                GameObject summonEffect = null;
                for (int i = 0; i < 4; i++)
                {
                    switch (i)
                    {
                        case 0:
                            summonEffect = Instantiate(abilityChargingParticleEffect, transform.position + transform.right, Quaternion.identity, transform);                            
                            break;
                        case 1:
                            summonEffect = Instantiate(abilityChargingParticleEffect, transform.position + -transform.right, Quaternion.identity, transform);                            
                            break;
                        case 2:
                            summonEffect = Instantiate(abilityChargingParticleEffect, transform.position + transform.forward, Quaternion.identity, transform);                            
                            break;
                        case 3:
                            summonEffect = Instantiate(abilityChargingParticleEffect, transform.position + -transform.forward, Quaternion.identity, transform);      
                            break;
                    }
                }
            }

            if (action == Action.Clone)
            {
                GameObject cloneEffect = null;
                for (int i = 0; i < 2; i++)
                {
                    switch (i)
                    {
                        case 0:
                            cloneEffect = Instantiate(abilityChargingParticleEffect, transform.position + transform.right * 2, Quaternion.identity, transform);
                            break;
                        case 1:
                            cloneEffect = Instantiate(abilityChargingParticleEffect, transform.position + -transform.right * 2, Quaternion.identity, transform);
                            break;
                    }
                }
            }

            yield return new WaitForSeconds(delayBeforeCast); // Cast time, allows animation to play before ability effect activates

            switch (action)
            {
                case Action.SpiritRip:
                    // Projectile attack
                    spiritRipTimer = Time.time + spiritRipCooldown;
                    print("Perform spirit rip");
                    bool isPlayerInSight = Physics.Raycast(transform.position + Vector3.up, transform.forward, Mathf.Infinity, playerLayer);
                    if (isPlayerInSight)
                    {
                        GameObject _spiritPickUp = Instantiate(spiritPickUp, transform.position + Vector3.up, Quaternion.identity);
                        _spiritPickUp.GetComponent<SpiritPickUp>().spiritPickUp = player.GetComponent<SpiritSystem>().spiritEquipped;

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
                    chargePosition = player.transform.position;
                    charging = true;
                    // Charge towards position
                    print("Perform charge");
                    break;
                case Action.Laser:
                    // Constant projectile attack
                    laserTimer = Time.time + laserCooldown;
                    // Coroutine that deals damage continuously
                    lasering = true;
                    print("Perform laser");
                    yield return new WaitForSeconds(4);
                    lasering = false;
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
            }
            abilityCasting = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (charging)
            {
                if (collision.gameObject.GetComponent<StateManager>())
                {
                    collision.gameObject.GetComponent<StateManager>().TakeDamage(30, transform, false, true);
                    charging = false;
                }
            }
        }
    }    
}
