using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EloteroAI : CharacterStats
{
    [Header("Referencias")]
    public Transform player;
    // MODIFICADO: Prefab del proyectil que explota
    [SerializeField] private GameObject eloteBombaPrefab;
    [SerializeField] private Transform firePoint;

    [Header("IA - Configuración de Ataque")]
    [SerializeField] private float attackDistance = 8f;
    [SerializeField] private float attackRate = 3f;
    // NUEVO: Parámetros para el ataque en área
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float explosionDamage = 15f;

    [Header("IA - Configuración General")]
    [SerializeField] private float contactDamage = 1f;
    [SerializeField] private float baseScale = 1.0f;

    [Header("Detección de Entorno")]
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private float checkDistance = 1.0f;
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
                Debug.LogError("EloteroAI: No se encontró al jugador. Asegúrate de que tenga el tag 'Player'.");
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
        if (player == null || isAttacking)
        {
            if (anim != null) anim.SetBool("isMoving", false);
            return;
        }

        float distance = Vector2.Distance(player.position, transform.position);
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
            StartCoroutine(RangedAttack());
        }

        if (directionToPlayer.x > 0.0f)
        {
            transform.localScale = new Vector3(baseScale, baseScale, 1);
        }
        else
        {
            transform.localScale = new Vector3(-baseScale, baseScale, 1);
        }
    }

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

        if (!isAttacking) // El elotero no se mueve mientras prepara el ataque
        {
            rb.linearVelocity = new Vector2(actualMoveDirection * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // MODIFICADO: Corrutina de ataque para lanzar la bomba
    private IEnumerator RangedAttack()
    {
        isAttacking = true;

        if (eloteBombaPrefab != null && firePoint != null && player != null)
        {
            GameObject bomba = Instantiate(eloteBombaPrefab, firePoint.position, Quaternion.identity);
            EloteBomba eloteScript = bomba.GetComponent<EloteBomba>();

            if (eloteScript != null)
            {
                // Le pasamos al proyectil el objetivo, el radio y el daño
                eloteScript.Initialize(player.position, explosionRadius, explosionDamage);
            }
        }

        yield return new WaitForSeconds(attackRate);
        isAttacking = false;
    }

    public void Hit(float damage)
    {
        base.TakeDamage(damage);
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

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

    // MODIFICADO: Gizmos para visualizar rango y área de explosión
    private void OnDrawGizmosSelected()
    {
        // Dibuja el rango de ataque del elotero
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        // Dibuja el detector de suelo
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + (Vector3.down * checkDistance));
        }

        // NUEVO: Dibuja el radio de la explosión en la posición del jugador
        // cuando el elotero está en rango de ataque.
        if (player != null && Vector2.Distance(player.position, transform.position) <= attackDistance)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, explosionRadius);
        }
    }
}