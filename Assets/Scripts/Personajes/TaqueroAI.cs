using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaqueroAI : CharacterStats
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject tacoPrefab;
    [SerializeField] private Transform firePoint;

    [Header("IA - Configuración")]
    [SerializeField] private float attackDistance = 6f;
    [SerializeField] private float attackRate = 2f;
    [SerializeField] private float contactDamage = 1f; 
    [SerializeField] private float baseScale = 1.0f;

    [Header("Detección de Entorno (Ledge Check)")]
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private float checkDistance = 1.0f;
    [SerializeField] private LayerMask groundLayer;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;

    // Variables de estado internas
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
                Debug.LogError("TaqueroAI: No se encontró al jugador.");
            }
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }


    // Decide qué hacer (moverse o disparar)
    void Update()
    {
        if (player == null) return;

        // Si está en el cooldown del ataque de DISPARO, no hace nada
        if (isAttacking)
        {
            currentMoveDirection = 0f;
            if (anim != null) anim.SetBool("isMoving", false);
            return;
        }

        float distance = Mathf.Abs(player.position.x - transform.position.x);
        Vector3 directionToPlayer = player.position - transform.position;

        if (distance > attackDistance)
        {
            currentMoveDirection = Mathf.Sign(directionToPlayer.x);
            if (anim != null) anim.SetBool("isMoving", true);
        }
        else
        {
            currentMoveDirection = 0f;
            if (anim != null) anim.SetBool("isMoving", false);
            StartCoroutine(RangedAttack()); // Llama a la corrutina de disparo
        }

        // Voltea el sprite para mirar al jugador
        if (directionToPlayer.x > 0.0f)
        {
            transform.localScale = new Vector3(baseScale, baseScale, 1);
        }
        else
        {
            transform.localScale = new Vector3(-baseScale, baseScale, 1);
        }
    }

    // Aplica el movimiento y revisa las cornisas
    void FixedUpdate()
    {
        float actualMoveDirection = currentMoveDirection;

        if (actualMoveDirection != 0f)
        {
            bool isGroundedAhead = Physics2D.Raycast(ledgeCheck.position, Vector2.down, checkDistance, groundLayer);

            if (!isGroundedAhead)
            {
                actualMoveDirection = 0f;
                if (anim != null) anim.SetBool("isMoving", false);
            }
        }

        rb.linearVelocity = new Vector2(actualMoveDirection * moveSpeed, rb.linearVelocity.y);
    }

    // Corrutina que spawnea el taco y espera
    private IEnumerator RangedAttack()
    {
        isAttacking = true; // Previene que se mueva y dispare más

        if (tacoPrefab != null && firePoint != null)
        {
            GameObject taco = Instantiate(tacoPrefab, firePoint.position, Quaternion.identity);
            Vector2 direction = new Vector2(transform.localScale.x, 0).normalized;
            BulletScript tacoScript = taco.GetComponent<BulletScript>();
            if (tacoScript != null)
            {
                tacoScript.SetDirection(direction);
            }
        }

        yield return new WaitForSeconds(attackRate); // Espera el cooldown del DISPARO
        isAttacking = false;
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

    // Método para hacer daño POR CONTACTO
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerScript = collision.gameObject.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.Hit(contactDamage);
            }
        }
    }

    // Dibuja los detectores en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position + new Vector3(attackDistance, -1, 0), transform.position + new Vector3(attackDistance, 1, 0));
        Gizmos.DrawLine(transform.position + new Vector3(-attackDistance, -1, 0), transform.position + new Vector3(-attackDistance, 1, 0));

        if (ledgeCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + (Vector3.down * checkDistance));
        }
    }
}