using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Hereda de la nueva clase base 'BaseEnemyAI'
// Asegúrate de agregar un componente AudioSource al objeto del perro en Unity
[RequireComponent(typeof(AudioSource))] 
public class PerroAI : BaseEnemyAI
{
    [Header("Ataque Específico (Perro)")]
    [Tooltip("El punto en los pies para revisar si puede saltar.")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float jumpForce = 2f;
    [SerializeField] private float jumpInterval = 1.5f;
    [SerializeField] private float checkRadius = 0.2f;
    [Tooltip("Tiempo de cooldown después de un ataque por contacto.")]
    [SerializeField] private float attackWaitTime = 3f;

    // --- NUEVO: Variables de Audio ---
    [Header("Efectos de Sonido")]
    [SerializeField] private AudioClip attackSound; // Arrastra el sonido de mordida/ataque aquí
    [SerializeField] private AudioClip hurtSound;   // Arrastra el sonido de herido aquí
    private AudioSource audioSource;

    // Variables de estado para el salto
    private float jumpTimer;
    private bool isGrounded;

    // --- MÉTODOS BASE (Sobrescritos) ---

    // Prepara el temporizador de salto y obtiene el AudioSource
    protected override void Start()
    {
        base.Start();
        jumpTimer = jumpInterval;
        
        // --- NUEVO: Obtener referencia al AudioSource ---
        audioSource = GetComponent<AudioSource>();
    }
    
    // Sobrescribimos 'HandleChase': le decimos que se mueva Y que salte
    protected override void HandleChase()
    {
        // Implementamos la lógica de persecución aquí mismo.
        currentMoveDirection = Mathf.Sign(directionToPlayer.x);
        
        HandleJumping();    // Llama a la lógica de salto del perro
    }

    // Sobrescribimos 'HandleAttack': le decimos que se mueva Y que salte
    protected override void HandleAttack()
    {
        // Implementamos la lógica de persecución de nuevo.
        currentMoveDirection = Mathf.Sign(directionToPlayer.x);

        HandleJumping();    // Llama a la lógica de salto del perro
    }

    // Sobrescribimos 'OnCollisionEnter2D' para usar nuestro cooldown de ataque Y SONIDO
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isAttacking)
        {
            PlayerController playerScript = collision.gameObject.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                // --- NUEVO: Reproducir sonido de ataque ---
                if (audioSource != null && attackSound != null)
                {
                    audioSource.PlayOneShot(attackSound);
                }

                // Inicia la corrutina de pausa específica del perro
                StartCoroutine(AttackPause(playerScript));
            }
        }
    }

    // --- MÉTODOS ESPECIALES (Perro) ---

    // Esta es la lógica de salto única del perro
    private void HandleJumping()
    {
        // Revisa si está en el suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (anim != null) anim.SetBool("isGrounded", isGrounded);

        // Temporizador de salto
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0 && isGrounded)
        {
            Jump();
            jumpTimer = jumpInterval;
        }
    }

    // Aplica la fuerza de salto y activa la animación
    private void Jump()
    {
        if (anim != null) anim.SetTrigger("Jump");

        float horizontalDirection = Mathf.Sign(player.position.x - transform.position.x);
        Vector2 jumpDirection = new Vector2(horizontalDirection, 1.5f).normalized;
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
    }

    // Método Hit modificado para incluir SONIDO DE DAÑO
    public void Hit(float damage)
    {
        base.TakeDamage(damage);
        
        // --- NUEVO: Reproducir sonido de herido ---
        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    // Corrutina de pausa de ataque (usa 'attackWaitTime' de este script)
    private IEnumerator AttackPause(PlayerController playerScript)
    {
        isAttacking = true;
        playerScript.Hit(contactDamage);
        yield return new WaitForSeconds(attackWaitTime);
        isAttacking = false;
    }

    // Dibuja los detectores (los de la base + el nuevo 'groundCheck')
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // Dibuja el radio de detección y el ledge check

        // Dibuja el detector de suelo para el salto
        if (groundCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}