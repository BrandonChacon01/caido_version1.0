using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerroAI : CharacterStats
{
    [Header("Referencias")]
    public Transform player;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    [Header("IA - Configuración")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float contactDamage = 1f;
    [SerializeField] private float attackWaitTime = 3f;
    [SerializeField] private float baseScale = 0.15f;

    [Header("IA - Salto")]
    [SerializeField] private float jumpForce = 2f;
    [SerializeField] private float jumpInterval = 1.5f;
    [SerializeField] private float checkRadius = 0.2f;
    private float jumpTimer;
    private bool isGrounded;

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
                Debug.LogError("PerroAI: No se encontró al jugador. Asegúrate de que el jugador tenga el Tag 'Player'.");
            }
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        jumpTimer = jumpInterval;
    }

    void Update()
    {
        if (player == null) return;

        if (isAttacking)
        {
            currentMoveDirection = 0f;
            anim.SetBool("isMoving", false);
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (anim != null) anim.SetBool("isGrounded", isGrounded);

        float distance = Vector2.Distance(transform.position, player.position);
        Vector3 directionToPlayer = player.position - transform.position;

        if (distance <= detectionRadius)
        {
            currentMoveDirection = Mathf.Sign(directionToPlayer.x);
            anim.SetBool("isMoving", true);

            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0 && isGrounded)
            {
                Jump();
                jumpTimer = jumpInterval;
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
        else
        {
            currentMoveDirection = 0f;
            anim.SetBool("isMoving", false);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(currentMoveDirection * moveSpeed, rb.linearVelocity.y);
    }

    // Aplica la fuerza de salto y activa la animación
    private void Jump()
    {
        if (anim != null) anim.SetTrigger("Jump");

        float horizontalDirection = Mathf.Sign(player.position.x - transform.position.x);
        Vector2 jumpDirection = new Vector2(horizontalDirection, 1.5f).normalized;
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
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

    // Corrutina que aplica el daño y la espera
    private IEnumerator AttackPause(PlayerController playerScript)
    {
        isAttacking = true;

        playerScript.Hit(contactDamage);

        yield return new WaitForSeconds(attackWaitTime);

        isAttacking = false;
    }

    // Dibuja los detectores en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (groundCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}