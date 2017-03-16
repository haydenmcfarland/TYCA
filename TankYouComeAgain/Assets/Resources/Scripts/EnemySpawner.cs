using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject enemyPrefab;
    private GameObject enemyObj = null;
    public float timer = 0;
    public float timeThreshold = 3.0f;

    private void Update()
    {
        if (!isLocalPlayer) {
            return;
        }
        CmdSpawnEnemy();
    }
    [Command]
    void CmdSpawnEnemy()
    {
        if (enemyObj == null)
        {
            timer += Time.deltaTime;
            if (timer >= timeThreshold)
            {
                timer = 0;
                enemyObj = (GameObject)Instantiate(enemyPrefab, transform.position, Quaternion.identity);
                NetworkServer.Spawn(enemyObj);
            }
        }
    }
}
