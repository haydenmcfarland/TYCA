using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject enemyPrefab;
    NetworkStartPosition[] spawnPoints;

    private void Start()
    {
        if (isServer) {
            Invoke("SpawnEnemies", 5);
        }
    }
    void SpawnEnemy(int index)
    {
            GameObject enemyObj = (GameObject)Instantiate(enemyPrefab, spawnPoints[index].gameObject.transform.position, Quaternion.identity);
            NetworkServer.Spawn(enemyObj);
    }

    void SpawnEnemies() {
        spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        int numAI = Game.MAX_PLAYERS - Game.instance.GetNumPlayers();
        for (int i = Game.MAX_PLAYERS - numAI; i < Game.MAX_PLAYERS; ++i) {
            SpawnEnemy(i);
        }
    }
}
