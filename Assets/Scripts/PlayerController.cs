using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRb;
    private GameObject focalPoint;
    public GameObject powerupIndicator;
    public GameObject rocketIndicator;
    public GameObject smashIndicator;
    public SpawnManager spawnManager;
    private Vector3 offsetIndicator = new Vector3(0, -0.5f, 0);

    public float speed = 4.0f;
    private float powerUpStrength = 15.0f;
    public float hangTime;
    public float smashSpeed;
    public float explosionForce;
    public float explosionRadius;
    float floorY;

    public bool hasPowerup = false;
    public bool hasRocket = false;
    public bool hasSmash = false;
    public bool smashing = false;
    public bool gameOver = false;
    public bool onGround = true;

    public PowerUpType currentPowerUp = PowerUpType.None;
    public GameObject rocketPrefab;
    private GameObject tmpRocket;
    private Coroutine powerupCountdown;




    // Start is called before the first frame update
    void Start()
    {
        spawnManager = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        playerRb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("Focal Point");
    }

    // Update is called once per frame
    void Update()
    {
        float forwardInput = Input.GetAxis("Vertical");
        playerRb.AddForce(focalPoint.transform.forward * speed * forwardInput);

        powerupIndicator.transform.position = transform.position + offsetIndicator;
        rocketIndicator.transform.position = transform.position + offsetIndicator;
        smashIndicator.transform.position = transform.position + offsetIndicator;

        if (currentPowerUp == PowerUpType.Rocket)
        {
            hasRocket = true;
            rocketIndicator.gameObject.SetActive(true);

        }
        else if (currentPowerUp == PowerUpType.Pushback)
        {
            powerupIndicator.gameObject.SetActive(true);

        }
        else if (currentPowerUp == PowerUpType.Smash && !hasSmash)
        {
            smashing = true;
            hasSmash = true;
            smashIndicator.gameObject.SetActive(true);
        }

        if (hasRocket && Input.GetButtonDown("Fire1"))
        {
            LaunchRockets();
        }
        else if (hasSmash && Input.GetButtonDown("Jump") && onGround)
        {
            smashing = true;
            StartCoroutine(Smash());
        }

        if (transform.position.y < -10)
        {
            spawnManager.waveNumber--;
            gameOver = true;
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            currentPowerUp = other.gameObject.GetComponent<PowerUp>().PowerUpType;
            Destroy(other.gameObject);

            if (powerupCountdown != null)
            {
                StopCoroutine(powerupCountdown);
            }
            powerupCountdown = StartCoroutine(PowerupCountdownRoutine());
        }
        if (other.CompareTag("Ground"))
        {
            onGround = true;

        }
    }

    IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(7);
        hasPowerup = false;
        hasRocket = false;
        hasSmash = false;
        currentPowerUp = PowerUpType.None;
        powerupIndicator.gameObject.SetActive(false);
        rocketIndicator.gameObject.SetActive(false);
        smashIndicator.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && currentPowerUp == PowerUpType.Pushback)
        {
            Rigidbody enemyRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = collision.gameObject.transform.position - transform.position;
            enemyRigidbody.AddForce(awayFromPlayer * powerUpStrength, ForceMode.Impulse);
            Debug.Log("Player Collided with : " + collision.gameObject.name + " with powerup set to " + currentPowerUp.ToString());
        }
    }
    void LaunchRockets()
    {
        foreach (var enemy in FindObjectsOfType<EnemyScript>())
        {
            tmpRocket = Instantiate(rocketPrefab, transform.position + Vector3.up, Quaternion.identity);
            tmpRocket.GetComponent<RocketBehaviour>().Fire(enemy.transform);
        }
    }
    IEnumerator Smash()
    {
        var enemies = FindObjectsOfType<EnemyScript>();
        // Store the y position before taking off
        floorY = transform.position.y;

        // Calculate the amount of time we will go up
        float jumpTime = Time.time + hangTime;

        while (Time.time < jumpTime)
        {
            //Move the player up while still keeping their x velocity.
            playerRb.velocity = new Vector3(playerRb.velocity.x, smashSpeed, playerRb.velocity.z);
            yield return null;
        }
        //Now move the player down
        while (transform.position.y > floorY)
        {
            playerRb.velocity = new Vector3(playerRb.velocity.x, -smashSpeed * 2, playerRb.velocity.z);
            yield return null;
        }

        //Cycle through all enemies.
        for (int i = 0; i < enemies.Length; i++)
        {
            //Apply an explosion force that originates from our position.
            if (enemies[i] != null)
            {
                enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.0f, ForceMode.Impulse);
            }
        }
        //We are no longer smashing, so set the boolean to false
        smashing = false;
        playerRb.velocity = Vector3.zero;
    }
}
