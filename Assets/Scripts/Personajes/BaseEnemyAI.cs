using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Esta clase hereda de CharacterStats y añade toda la lógica de IA común
public abstract class BaseEnemyAI : CharacterStats
{
    [Header("Referencias de IA")]
    public Transform player; 
    [SerializeField] protected Slider healthSlider;

    [Header("Configuración de IA")]
    [Tooltip("Distancia a la que el enemigo deja de perseguir y empieza a atacar.")]
    [SerializeField] protected float attackDistance = 3f;
    [Tooltip("Daño que hace el enemigo al tocar al jugador.")]
    [SerializeField] protected float contactDamage = 1f; // Daño por contacto estándar
    [Tooltip("Escala base del sprite para voltear.")]
    [SerializeField] protected float baseScale = 1.0f;

    [Header("Detección de Entorno")]
    [SerializeField] protected Transform ledgeCheck;
    [SerializeField] protected float checkDistance = 1.0f; 
    [SerializeField] protected LayerMask groundLayer;

    // Variables de estado protegidas para los hijos
    protected bool isAttacking = false;
    protected float currentMoveDirection = 0f;
    protected float distanceToPlayer;
    protected Vector3 directionToPlayer;

    // --- MÉTODOS BASE (Comunes para todos) ---

    protected override void Start()
    {
        base.Start(); // Llama a Start() de CharacterStats (setea la vida)

        // Asigna la barra de vida
        if (healthSlider != null) 
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Busca al jugador si no está asignado
        if (player == null)
        {
            try
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }
            catch
            {
                Debug.LogError(gameObject.name + ": No se pudo encontrar al Jugador. Asegúrate de que tenga el Tag 'Player'.");
            }
        }
    }

    // Update es el "cerebro" que delega a los hijos
    protected virtual void Update()
    {
        if (player == null) return;

        // Si estamos en medio de un ataque (ej. cooldown), no hacemos nada más
        if (isAttacking)
        {
            currentMoveDirection = 0f;
            if (anim != null) anim.SetBool("isMoving", false);
            return;
        }

        // 1. Calcular distancia y dirección
        directionToPlayer = player.position - transform.position;
        distanceToPlayer = Mathf.Abs(directionToPlayer.x);

        // 2. Tomar Decisiones (Abstractas)
        if (distanceToPlayer > attackDistance)
        {
            HandleChase();
        }
        else
        {
            HandleAttack();
        }

        // 3. Acciones Comunes
        Flip();
        UpdateAnimator();
    }

    // FixedUpdate maneja la física y el ledge check (común para todos)
    protected virtual void FixedUpdate()
    {
        float actualMoveDirection = currentMoveDirection;

        // --- Chequeo de Seguridad (Ledge Check) ---
        if (actualMoveDirection != 0f)
        {
            bool isGroundedAhead = Physics2D.Raycast(ledgeCheck.position, Vector2.down, checkDistance, groundLayer);

            if (!isGroundedAhead)
            {
                actualMoveDirection = 0f; 
            }
        }
        // --- Movimiento ---
        rb.linearVelocity = new Vector2(actualMoveDirection * moveSpeed, rb.linearVelocity.y);
    }

    // Qué hacer cuando el jugador está lejos (persiguiendo)
    protected abstract void HandleChase();

    // Qué hacer cuando el jugador está cerca (atacando)
    protected abstract void HandleAttack();

    // Método para recibir daño (heredado de CharacterStats, pero lo modificamos para la UI)
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    // Lógica de volteo
    protected virtual void Flip()
    {
        if (directionToPlayer.x > 0.0f)
        {
            transform.localScale = new Vector3(baseScale, baseScale, 1);
        }
        else if (directionToPlayer.x < 0.0f) 
        {
            transform.localScale = new Vector3(-baseScale, baseScale, 1);
        }
    }

    // Lógica de animación
    protected virtual void UpdateAnimator()
    {
        if (anim != null)
        {
            anim.SetBool("isMoving", currentMoveDirection != 0);
        }
    }

    // Lógica de daño por contacto con el jugador
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isAttacking)
        {
            PlayerController playerScript = collision.gameObject.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.Hit(contactDamage);
                
                // (Opcional) Iniciar un cooldown de daño por contacto
                // StartCoroutine(ContactCooldown()); 
            }
        }
    }

    // Dibuja los Gizmos de la IA en el editor
    // Lo marcamos como 'virtual' para que los hijos (PerroAI) puedan sobreescribirlo
    protected virtual void OnDrawGizmosSelected()
    {
        // Dibuja el LedgeCheck (rojo)
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + (Vector3.down * checkDistance));
        }

        // Dibuja el Radio de Detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkDistance);

        // Dibuja el Radio de Ataque (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
