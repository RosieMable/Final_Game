﻿using System.Collections;
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
        [SerializeField] private AudioClip spiritRipAudio;

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

        // Teleport Variables
        [SerializeField] private AudioClip teleportAudio;

        // Cloning Variables
        [SerializeField] private GameObject clonePrefab;
        private List<GameObject> activeClones = new List<GameObject>();
        private bool clonesActive;
        [SerializeField] private AudioClip cloneAudio;

        // Summon Variables
        [SerializeField] private GameObject[] summonPrefabs = new GameObject[4];
        private List<GameObject> activeSummons = new List<GameObject>();
        private bool summonsActive;
        [SerializeField] private AudioClip summonAudio;

        // Voice Audio Variables
        [SerializeField] private AudioClip[] HealthBreakPointClips;
        [SerializeField] private AudioClip[] ChargeClips;
        [SerializeField] private AudioClip[] CloneClips;
        [SerializeField] private AudioClip[] LaserClips;
        [SerializeField] private AudioClip[] TeleportClips;
        [SerializeField] private AudioClip[] SpiritRipClips;
        [SerializeField] private AudioClip[] SummonClips;
        [SerializeField] private AudioClip StartClip;
        [SerializeField] private AudioClip EndClip;
        [SerializeField] private AudioClip[] HealthBreakpointClips;
        private bool breakpointOne;
        private bool breakpointTwo;
        private bool breakpointThree;
        private bool startAudioPlayed;
        private AudioSource bossVoicelineAudioSource;

        protected override void Awake()
        {
            base.Awake();
            weaponHook = GetComponentInChildren<WeaponHook>();
            weaponHook.CloseDamageCollider();
            originalSpeed = speed;
            chargeSpeed = speed * 2;
            originalAcceleration = agent.acceleration;
            chargeAcceleration = originalAcceleration * 2;
            characterAudioSource = GetComponent<AudioSource>();
            bossVoicelineAudioSource = GameObject.Find("BossVA").GetComponent<AudioSource>();
        }

        protected override void Update()
        {
            base.Update();

            if (!stunned)
            {
                CalculatePossibleActions();

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
                    charAnim.CrossFade("DodgeRoll", 0.2f);
                    lookingToDodge = false;
                }

                if (lasering)
                {
                    charAnim.SetBool("lasering", true);

                    if (Time.time > laserHitDelay)
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
                else
                {
                    charAnim.SetBool("lasering", false);
                }
            }
        }

        protected override void CombatBehaviour()
        {
            if (abilityCasting)
            {
                RotateTowardsTarget(player.transform);
                agent.isStopped = true;
            }
            else if (charging)
            {
                agent.isStopped = false;
                RotateTowardsTarget(chargePosition);
                agent.speed = chargeSpeed;
                agent.acceleration = chargeAcceleration;
                agent.stoppingDistance = 0;
                agent.SetDestination(chargePosition);
            }
            else if (movingToAttack)
            {
                agent.isStopped = false;
                MoveToTarget();
            }

            if (Vector3.Distance(transform.position, chargePosition) <= 2)
            {
                agent.isStopped = false;
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

            if (isInAttackRange && (!abilityCasting && !charging))
            {
                return State.Attacking;
            }

            bool isInAggroRange = Vector3.Distance(transform.position, player.transform.position) < aggroRange;

            if (isInAggroRange)
            {
                if (!startAudioPlayed)
                {
                    startAudioPlayed = true;
                    bossVoicelineAudioSource.clip = StartClip;
                    bossVoicelineAudioSource.Play();
                }

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
                    if (!isInvulnerable && !inAction && Time.time > attackDelay && !abilityCasting)
                    {
                        agent.isStopped = true;
                        RaycastHit hit;
                        bool playerInFront = false;

                        Physics.Raycast(transform.position + transform.up, transform.forward, out hit, 2, playerLayer);

                        if (hit.collider!= null)
                        {
                            if (hit.collider.gameObject.GetComponent<StateManager>() || hit.collider.gameObject.GetComponentInParent<StateManager>())
                            {
                                playerInFront = true;
                            }
                        }

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

        public override void TakeDamage(float damageValue)
        {
            if (isInvulnerable) // If flagged as invulnerable due to taking damage recently...
            {
                return; // Return out of method, take no damage
            }

            attackDelay = Time.time + attackCooldown; // Add onto the attack delay as we have been hit            
            currentHealth = Mathf.Lerp(currentHealth, currentHealth - damageValue, 1f); // Reduce health by damage value
            healthSlider.value = currentHealth; // Update slider to represent new health total

            if (damageText.IsActive()) // If already showing damage text...
            {
                damageTextValue += damageValue; // Add onto existing damage text displayed                
            }
            else // If not showing damage text...
            {
                damageTextValue = damageValue; // Show initial damage
            }

            damageText.text = damageTextValue.ToString();

            if (healthCoroutine != null) // If the AI health is already visible
            {
                StopCoroutine(healthCoroutine); // Stop coroutine, prevents bar from disappearing before intended
                healthCoroutine = StartCoroutine(RevealHealthBar(3)); // Recall coroutine, ensures bar disappears when intended
            }
            else
            {
                healthCoroutine = StartCoroutine(RevealHealthBar(3)); // Start health coroutine to display AI health
            }

            if (damageTextCoroutine != null) // Same as above but for damage text
            {
                StopCoroutine(damageTextCoroutine);
                damageTextCoroutine = StartCoroutine(RevealDamageText(3));
            }
            else
            {
                damageTextCoroutine = StartCoroutine(RevealDamageText(3));
            }

            int hurtAnimationToPlay = Random.Range(0, hurtAnimations.Length); // Return random animation from list
            charAnim.CrossFade(hurtAnimations[hurtAnimationToPlay].name, 0.1f); // Play animation
            charAnim.applyRootMotion = true;

            int clipToPlay = Random.Range(0, hurtAudioClips.Length);
            base.characterAudioSource.clip = hurtAudioClips[clipToPlay];
            base.characterAudioSource.Play();

            PlayVoiceLines(currentHealth);
        }

        private void PlayVoiceLines(float currentHealth)
        {
            // Needs to flag when a voiceline has been played so as to not repeat.

            if (currentHealth <= 0)
            {
                bossVoicelineAudioSource.clip = EndClip;
                bossVoicelineAudioSource.Play();
            }
            else if (currentHealth < maximumHealth / 4 && !breakpointThree) // 25% health
            {
                // Play Voiceline
                breakpointThree = true;
                bossVoicelineAudioSource.clip = HealthBreakpointClips[2];
                bossVoicelineAudioSource.Play();
            }
            else if (currentHealth < maximumHealth / 2 && !breakpointTwo) // 50% health
            {
                // Play Voiceline
                breakpointTwo = true;
                bossVoicelineAudioSource.clip = HealthBreakpointClips[1];
                bossVoicelineAudioSource.Play();
            }
            else if (currentHealth < maximumHealth / 1.3 && !breakpointOne) // 75% health
            {
                // Play Voiceline
                breakpointOne = true;
                bossVoicelineAudioSource.clip = HealthBreakpointClips[0];
                bossVoicelineAudioSource.Play();
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
                //print("Calculating actions...");
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
                    if (IsPlayerWithinRange(attackRange * 2)) // If the player is within attacking range of the Boss...
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

                if (canSplit && lastAction != Action.Clone && cloneTimer <= Time.time && !clonesActive && !summonsActive)
                {
                    possibleActions.Add(Action.Clone);
                }

                if (canSummon && lastAction != Action.Summon && summonTimer <= Time.time && !summonsActive && !clonesActive)
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
            //print("Choosing an action...");
            //print("I am avoiding the action: " + lastAction);
            //print("My choices are...");
            foreach (Action action in possibleActions)
            {
                //print(action);
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
                    charAnim.CrossFade("SpiritRip", 0.2f);
                    PlayRandomClip(SpiritRipClips);
                    abilityCasting = true;
                    break;
                case Action.Teleport:
                    // Animation cast + voiceline
                    PlayRandomClip(TeleportClips);
                    abilityCasting = true;
                    break;
                case Action.Clone:
                    // Animation cast + voiceline
                    charAnim.CrossFade("Clone", 0.2f);
                    PlayRandomClip(CloneClips);
                    abilityCasting = true;
                    break;
                case Action.Summon:
                    // Animation cast + voiceline
                    charAnim.CrossFade("Summon", 0.2f);
                    PlayRandomClip(SummonClips);
                    abilityCasting = true;
                    break;
                case Action.Charge:
                    // Animation cast + voiceline
                    PlayRandomClip(ChargeClips);
                    abilityCasting = true;
                    break;
                case Action.Laser:
                    // Animation cast + voiceline
                    charAnim.CrossFade("Cast Forward", 0.2f);
                    PlayRandomClip(LaserClips);
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
                    characterAudioSource.clip = cloneAudio;
                    characterAudioSource.Play();
                    break;
                case Action.Teleport:
                    // Teleport to a random position nearby the player, or maybe always behind
                    player.GetComponent<InputHandler>().ClearLockOn();
                    teleportTimer = Time.time + teleportCooldown;
                    print("Perform teleport");
                    transform.position = player.transform.position + -player.transform.forward * 2;
                    characterAudioSource.clip = teleportAudio;
                    characterAudioSource.Play();
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
                    characterAudioSource.clip = cloneAudio;
                    characterAudioSource.Play();
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
                    characterAudioSource.clip = summonAudio;
                    characterAudioSource.Play();
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
                    charAnim.CrossFade("Spin", 0.2f);
                    // Play an animation with a long delay before disabling the damage collider
                    print("Perform spin");
                    break;
            }
            yield return new WaitForSeconds(delayBeforeCast / 2);
            abilityCasting = false;
        }

        private void PlayRandomClip(AudioClip[] audioClips)
        {
            if (!bossVoicelineAudioSource.isPlaying)
            {
                int clipToPlay = Random.Range(0, audioClips.Length);
                bossVoicelineAudioSource.clip = audioClips[clipToPlay];
                bossVoicelineAudioSource.Play();
            }
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
