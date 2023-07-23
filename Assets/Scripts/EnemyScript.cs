using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    private GameObject player;
    private Rigidbody enemyRb;
    private SpawnManager spawnManager;
    public float speed = 1.5f;
    public float spawnInterval;
    private float nextSpawn;

    public int miniEnemySpawnCount;

    public bool isBoss = false;


    // Start is called before the first frame update
    void Start()
    {

        enemyRb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        if (isBoss)
        {
            spawnManager = FindObjectOfType<SpawnManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Make the enemy move towards the player
        Vector3 lookDirection = (player.transform.position - transform.position).normalized;
        enemyRb.AddForce(lookDirection * speed);

        if (isBoss)
        {
            if (Time.time>nextSpawn)
            {
                nextSpawn = Time.time + spawnInterval;
                spawnManager.SpawnMiniEnemy(miniEnemySpawnCount);
            }
        }

        if (transform.position.y < -10)
        {
            Destroy(gameObject);
        }
    }
}
