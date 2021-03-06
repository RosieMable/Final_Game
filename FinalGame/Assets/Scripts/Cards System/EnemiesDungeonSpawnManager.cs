﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit 
{
    public class EnemiesDungeonSpawnManager : MonoBehaviour
    {

        private DungeonCardSystem dungeonSystem;

        [SerializeField]
        List<EnemiesDungeonSingleSpawner> Spawners;

        [SerializeField]
        Transform playerSpawnPoint;

        [SerializeField]
        GameObject PlayerPrefab, CameraHolder, PortalToHub;

        [SerializeField]
        Enemy[] enemies;

        [SerializeField]
        AudioClip BattleMusic;

        private void Awake()
        {
            if (dungeonSystem == null)
                dungeonSystem = FindObjectOfType<DungeonCardSystem>();

            PortalToHub.SetActive(false);
        }

        private void Start()
        {

            int i = 0;

            foreach (var spawn in Spawners)
            {
                spawn.dungeonCard = dungeonSystem.DrawnCards[i++];
                spawn.PopulateSpawnerVariables(spawn.dungeonCard);
            }

            GameObject player = GameObject.Instantiate(PlayerPrefab);
            GameObject camera = Instantiate(CameraHolder);
            camera.GetComponentInChildren<AudioSource>().clip = BattleMusic;
            camera.GetComponentInChildren<AudioSource>().Play();
            player.transform.position = playerSpawnPoint.position;
            camera.transform.position = playerSpawnPoint.position;

        }

        private void Update()
        {
            enemies = FindObjectsOfType<Enemy>();
            if (enemies.Length <= 0)
                PortalToHub.SetActive(true);
        }
    }
}
