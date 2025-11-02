using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CholitoAI : CharacterStats
{
    public Transform Player;
    public GameObject BulletPrefab;
    public float Rate = 0.25f;
    public float Recoil = 2f;
    private float LastShoot;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;

    [Header("IA de Movimiento")]
    [SerializeField] private float stoppingDistance = 1.5f;

    [Header("Detección de Entorno")]
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private float checkDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private float currentMoveDirection = 0f;

    protected override void Awake()
    {
        base.Awake(); 
    }

    protected override void Start()
    {
        base.Start(); 

        if (healthSlider != null) 
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }


    void Update()
    {
        if (Player == null) return;

        Vector3 directionToPlayer = Player.position - transform.position;

        if (directionToPlayer.x >= 0.0f)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
        else
        {
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }

        float distance = Mathf.Abs(Player.position.x - transform.position.x);

        if (distance > stoppingDistance)
        {
            currentMoveDirection = Mathf.Sign(directionToPlayer.x);
        }
        else
        {
            currentMoveDirection = 0f;

            if (Time.time > LastShoot + Rate)
            {
                Shoot();
                LastShoot = Time.time;
            }
        }

        if (anim != null)
        {
            anim.SetBool("isMoving", currentMoveDirection != 0);
        }
    }

    private void Shoot()
    {
        Vector3 direction = new Vector3(transform.localScale.x, 0.0f, 0.0f);
        GameObject bullet = Instantiate(BulletPrefab, transform.position + direction * 0.1f, Quaternion.identity);
        bullet.GetComponent<BulletScript>().SetDirection(direction);

        rb.AddForce(-direction * Recoil, ForceMode2D.Impulse);
    }

    public void Hit(float damage)
    {
        base.TakeDamage(damage);
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    private void OnDrawGizmos()
    {
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + (Vector3.down * checkDistance));
        }
    }

    private void FixedUpdate()
    {
        float actualMoveDirection = currentMoveDirection;

        if (actualMoveDirection != 0f)
        {
            bool isGroundedAhead = Physics2D.Raycast(ledgeCheck.position, Vector2.down, checkDistance, groundLayer);

            if (!isGroundedAhead)
            {
                actualMoveDirection = 0f;
            }
        }
        rb.linearVelocity = new Vector2(actualMoveDirection * moveSpeed, rb.linearVelocity.y);
    }
}