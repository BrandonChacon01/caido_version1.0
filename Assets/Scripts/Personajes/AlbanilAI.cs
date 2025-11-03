using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlbanilAI : CharacterStats
{
    [Header("Referencias")]
    public Transform player;
    [SerializeField] private GameObject slashPrefab; 
    [SerializeField] private Transform attackPoint; 

    [Header("IA - Configuración")]
    [SerializeField] private float attackDistance = 1.0f;
    [SerializeField] private float attackDamage = 2f;
    [SerializeField] private float attackWaitTime = 2f;
    [SerializeField] private float baseScale = 1.0f;

    [Header("Detección de Entorno (Ledge Check)")]
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private float checkDistance = 0.2f;
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
                Debug.LogError("AlbanilAI: No se encontró al jugador.");
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
            StartCoroutine(MeleeAttack()); 
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

        rb.linearVelocity = new Vector2(actualMoveDirection * moveSpeed, rb.linearVelocity.y);
    }

    private IEnumerator MeleeAttack()
    {
        isAttacking = true;

        yield return new WaitForSeconds(0.2f);

        // --- LÓGICA DE SPAWN DE SLASH ---
        if (slashPrefab != null && attackPoint != null)
        {
            // 1. Spawnea el slash en el 'attackPoint'
            GameObject slash = Instantiate(slashPrefab, attackPoint.position, Quaternion.identity);

            // 2. Orienta el slash para que mire en la misma dirección que el albañil
            float myFacingDirection = Mathf.Sign(transform.localScale.x);
            slash.transform.localScale = new Vector3(
                slash.transform.localScale.x * myFacingDirection,
                slash.transform.localScale.y,
                slash.transform.localScale.z
            );

            // 3. Pasa el daño al script del slash
            SlashDamage slashScript = slash.GetComponent<SlashDamage>();
            if (slashScript != null)
            {
                slashScript.damage = this.attackDamage; // Usa el daño de este Albañil
            }
        }

        // Espera el tiempo de "cooldown"
        yield return new WaitForSeconds(attackWaitTime - 0.5f);
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
    private void OnDrawGizmosSelected()
    {
        // --- CAMBIO ---
        // Dibuja el rango de ataque (attackDistance) en lugar del radio de detección
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position + new Vector3(attackDistance, -1, 0), transform.position + new Vector3(attackDistance, 1, 0));
        Gizmos.DrawLine(transform.position + new Vector3(-attackDistance, -1, 0), transform.position + new Vector3(-attackDistance, 1, 0));

        // Dibuja el detector de cornisa (la línea roja)
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + (Vector3.down * checkDistance));
        }
    }
}