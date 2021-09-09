using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //params
    [Header("Player Movement")]
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] float padding = 1f;
    [SerializeField] int health = 300;

    [Header("Projectile")]
    [SerializeField] GameObject fireballPrefab;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float projectileFiringPeriod = 0.2f;

    [Header("SFX")]
    [SerializeField] AudioClip[] playerDeathSFX;
    [SerializeField] AudioClip[] playerHitSFX;
    [SerializeField] AudioClip[] playerAttackSFX;
    [SerializeField] [Range(0, 1)] float playerDeathVolume = .5f;
    [SerializeField] [Range(0, 1)] float playerHitVolume = .5f;
    [SerializeField] [Range(0, 1)] float playerAttackVolume = .5f;

    [Header("VFX")]
    [SerializeField] GameObject deathVFX;
    [SerializeField] float explosionDuration = 1f;



    Coroutine firingCoroutine;

    float xMin;
    float xMax;
    float yMin;
    float yMax;

    // Start is called before the first frame update
    void Start()
    {
        SetUpMoveBoundaries();
    }

    private void SetUpMoveBoundaries()
    {
        Camera gameCamera = Camera.main;

        xMin = gameCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x + padding;
        xMax = gameCamera.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x - padding;
        yMin = gameCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y + padding;
        yMax = gameCamera.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)).y - padding;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Fire();
    }

    private void Fire()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            firingCoroutine = StartCoroutine(FireContinuously());
        }
        if (Input.GetButtonUp("Fire1"))
        {
            StopCoroutine(firingCoroutine);
        }
    }

    IEnumerator FireContinuously()
    {
        while (true)
        {
            GameObject fireball = Instantiate(
                fireballPrefab,
                transform.position,
                Quaternion.identity) as GameObject;
            fireball.GetComponent<Rigidbody2D>().velocity = new Vector2(0, projectileSpeed);

            AudioClip clip = playerAttackSFX[UnityEngine.Random.Range(0, playerAttackSFX.Length)];
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, playerAttackVolume);

            yield return new WaitForSeconds(projectileFiringPeriod);
        }
    }

    private void Move()
    {
        var deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        var deltaY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        var newXPos = Mathf.Clamp(transform.position.x + deltaX, xMin, xMax);
        var newYPos = Mathf.Clamp(transform.position.y + deltaY, yMin, yMax);

        transform.position = new Vector2(newXPos, newYPos);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();
        if (!damageDealer) { return; }
        ProcessHit(damageDealer);
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        AudioClip clip = playerHitSFX[UnityEngine.Random.Range(0, playerHitSFX.Length)];
        health -= damageDealer.GetDamage();
        damageDealer.Hit();
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, playerHitVolume);

        if (health <= 0)
        {
            Die();
        }

    }

    private void Die()
    {
        GameObject explosion = Instantiate(deathVFX, transform.position, transform.rotation);
        Destroy(explosion, explosionDuration);
        FindObjectOfType<Level>().LoadGameOver();
        AudioClip clip = playerDeathSFX[UnityEngine.Random.Range(0, playerDeathSFX.Length)];
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, playerDeathVolume);
        Destroy(gameObject);
    }

    public int GetHealth()
    {
        return health;
    }

}
