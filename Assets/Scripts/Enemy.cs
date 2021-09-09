using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] float health = 100f;
    [SerializeField] int scorePerKill = 500;
    [SerializeField] int scorePerHit = 50;

    [Header("Enemy Attacks")]
    [SerializeField] float shotCounter;
    [SerializeField] float minTimeBetweenShots = 0.2f;
    [SerializeField] float maxTimeBetweenShots = 1f;
    [SerializeField] GameObject laserPrefab;
    [SerializeField] float projectileSpeed = 10f;

    [Header("Enemy VFX")]
    [SerializeField] GameObject deathVFX;
    [SerializeField] float explosionDuration = 1f;

    [Header("Enemy SFX")]
    [SerializeField] AudioClip[] enemyDeathSFX;
    [SerializeField] AudioClip[] enemyHitSFX;
    [SerializeField] AudioClip[] enemyAttackSFX;
    [SerializeField] [Range(0,1)] float enemyDeathVolume = .75f;
    [SerializeField] [Range(0, 1)] float enemyHitVolume = .75f;
    [SerializeField] [Range(0, 1)] float enemyAttackVolume = .75f;

    private void Start()
    {
        shotCounter = UnityEngine.Random.Range(minTimeBetweenShots, maxTimeBetweenShots);
    }

    private void Update()
    {
        CountDownAndShoot();
    }

    void CountDownAndShoot()
    {
        shotCounter -= Time.deltaTime;
        if (shotCounter <= 0f)
        {
            Fire();
            shotCounter = UnityEngine.Random.Range(minTimeBetweenShots, maxTimeBetweenShots);

        }
    }
    
    private void Fire()
    {
        AudioClip clip = enemyAttackSFX[UnityEngine.Random.Range(0, enemyAttackSFX.Length)];
        Vector3 startingLaserPos = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        GameObject laser = Instantiate(laserPrefab, startingLaserPos, Quaternion.identity) as GameObject;
        laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -projectileSpeed);
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, enemyAttackVolume);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();
        if (!damageDealer) { return; }
        ProcessHit(damageDealer);
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        AudioClip clip = enemyHitSFX[UnityEngine.Random.Range(0, enemyHitSFX.Length)];
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, enemyHitVolume);

        health -= damageDealer.GetDamage();
        damageDealer.Hit();

        FindObjectOfType<GameSession>().AddToScore(scorePerHit);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        AudioClip clip = enemyDeathSFX[UnityEngine.Random.Range(0, enemyDeathSFX.Length)];
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, enemyDeathVolume);

        Destroy(gameObject);

        GameObject explosion = Instantiate(deathVFX, transform.position, transform.rotation);
        Destroy(explosion, explosionDuration);
        FindObjectOfType<GameSession>().AddToScore(scorePerKill);
    }
}
