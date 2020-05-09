using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZaldensGambit
{
    public class TestingAreaEnemySpawner : MonoBehaviour
    {
        public List<GameObject> enemiesToSpawn;
        public List<Transform> spawnPositions;
        private GameObject player;

        private void Start()
        {
            player = FindObjectOfType<StateManager>().gameObject;
        }

        private void Update()
        {
            if (Vector3.Distance(player.transform.position, transform.position) < 3)
            {
                UIManager.Instance.DisplayDialogue("Press E to spawn in enemies!");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    SpawnEnemies();
                }
            }
            else
            {
                UIManager.Instance.HideDialogue();
            }
        }

        private void SpawnEnemies()
        {
            for (int i = 0; i < enemiesToSpawn.Count; i++)
            {
                GameObject enemy = Instantiate(enemiesToSpawn[i], spawnPositions[i].position, Quaternion.identity);
                enemy.GetComponent<Enemy>().aggroRange = 50;
            }
        }

    }
}