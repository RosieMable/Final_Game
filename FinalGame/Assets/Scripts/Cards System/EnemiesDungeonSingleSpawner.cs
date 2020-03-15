using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesDungeonSingleSpawner : MonoBehaviour
{
    public Card_ScriptableObj dungeonCard;

    public int SpawnRadius = 3;

    [SerializeField]
    Card_ScriptableObj.CardSuit enemySuit;

    [SerializeField]
    int numEnemiesToSpawn;

    [SerializeField]
    private List<GameObject> FamineEnemies;

    [SerializeField]
    private List<GameObject> PestilenceEnemies;

    [SerializeField]
    private List<GameObject> WarEnemies;

    [SerializeField]
    private List<GameObject> DeathEnemies;

    public Transform spawnPosition { get { return gameObject.transform; } }

    public void PopulateSpawnerVariables(Card_ScriptableObj newCard)
    {
        dungeonCard = null;
        dungeonCard = newCard;

        numEnemiesToSpawn = 0;
        numEnemiesToSpawn = newCard.CardValue;

        enemySuit = newCard.cardSuit;

        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        switch (enemySuit)
        {
            case Card_ScriptableObj.CardSuit.Famine:
                InstantiateRandomObj(FamineEnemies, spawnPosition, numEnemiesToSpawn, SpawnRadius);
                break;
            case Card_ScriptableObj.CardSuit.War:
                InstantiateRandomObj(WarEnemies, spawnPosition, numEnemiesToSpawn, SpawnRadius);
                break;
            case Card_ScriptableObj.CardSuit.Death:
                InstantiateRandomObj(DeathEnemies, spawnPosition, numEnemiesToSpawn, SpawnRadius);
                break;
            case Card_ScriptableObj.CardSuit.Pestilence:
                InstantiateRandomObj(PestilenceEnemies, spawnPosition, numEnemiesToSpawn, SpawnRadius);
                break;
        }

    }

    private void InstantiateRandomObj(List<GameObject> list, Transform parent, int amount, int radius)
    {
        int index = 0;

        for (int i = index; i < amount; i++)
        {
           Instantiate(list[Random.Range(0, list.Count)].gameObject,new Vector3( (Random.insideUnitSphere.x * radius + parent.position.x),parent.position.y, (Random.insideUnitSphere.z * radius + parent.position.z)), Quaternion.identity);
        }
    }
}