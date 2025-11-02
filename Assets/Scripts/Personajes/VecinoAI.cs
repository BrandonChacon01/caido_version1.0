using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VecinoAI : CharacterStats
{
    [Header("Referencias")]
    [SerializeField] public Transform player;
    [SerializeField] private GameObject vecinoPrefab;

    [Header("IA - Configuración")]
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float contactDamage = 2f;
    [SerializeField] private float attackWaitTime = 1.5f;
    [SerializeField] private float baseScale = 1.0f;

    [Header("Detección de Entorno (Ledge Check)")]
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private float checkDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;

    private bool isAttacking = false;
    private float currentMoveDirection = 0f;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        if (player == null)
        {
            try
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }
            catch
            {
                Debug.LogError("VecinoAI: No se encontró al jugador.");
            }
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
    void Update()
    {
        if (player == null) return;

        if (isAttacking)
        {
            currentMoveDirection = 0f;
            if (anim != null) anim.SetBool("isMoving", false);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        Vector3 directionToPlayer = player.position - transform.position;

        if (distance <= detectionRadius)
        {
            currentMoveDirection = Mathf.Sign(directionToPlayer.x);
            if (anim != null) anim.SetBool("isMoving", true);

            if (directionToPlayer.x > 0.0f)
            {
                transform.localScale = new Vector3(baseScale, baseScale, 1);
            }
            else
            {
                transform.localScale = new Vector3(-baseScale, baseScale, 1);
            }
        }
        else
        {
            currentMoveDirection = 0f;
            if (anim != null) anim.SetBool("isMoving", false);
        }
    }
    void FixedUpdate()
    {
        float actualMoveDirection = currentMoveDirection;

        if (actualMoveDirection != 0f)
        {
            bool isGroundedAhead = Physics2D.OverlapCircle(ledgeCheck.position, checkDistance, groundLayer);

            if (!isGroundedAhead)
            {
                actualMoveDirection = 0f;
                if (anim != null) anim.SetBool("isMoving", false);
            }
        }

        rb.linearVelocity = new Vector2(actualMoveDirection * moveSpeed, rb.linearVelocity.y);
    }

    // Método para recibir daño de las balas del jugador
    public void Hit(float damage)
    {
        base.TakeDamage(damage);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    // Método para hacer daño por contacto
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isAttacking)
        {
            PlayerController playerScript = collision.gameObject.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                StartCoroutine(AttackPause(playerScript));
            }
        }
    }

    // Corrutina que aplica el daño, spawnea y espera
    private IEnumerator AttackPause(PlayerController playerScript)
    {
        isAttacking = true;
        playerScript.Hit(contactDamage);

        if (vecinoPrefab != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.0f, 0);

            GameObject newVecinoObj = Instantiate(vecinoPrefab, spawnPosition, Quaternion.identity);

            VecinoAI newVecinoAI = newVecinoObj.GetComponent<VecinoAI>();
            CharacterStats newVecinoStats = newVecinoObj.GetComponent<CharacterStats>();

            if (newVecinoAI != null && newVecinoStats != null)
            {
                newVecinoAI.player = this.player;
                newVecinoStats.moveSpeed = this.moveSpeed * 0.80f;
                newVecinoAI.vecinoPrefab = null;
            }
        }

        yield return new WaitForSeconds(attackWaitTime);
        isAttacking = false;
    }

    // Dibuja los detectores en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (ledgeCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ledgeCheck.position, checkDistance);
        }
    }
}