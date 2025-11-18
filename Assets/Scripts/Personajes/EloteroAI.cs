using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Hereda de la nueva clase base 'BaseEnemyAI'
public class EloteroAI : BaseEnemyAI
{
    [Header("Ataque Específico (Elotero)")]
    [SerializeField] private GameObject eloteBombaPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackRate = 3f;
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float explosionDamage = 15f; // Este daño se pasa al proyectil

    // --- MÉTODO REQUERIDO ---
    // Esto se ejecuta cuando el jugador está CERCA (distance <= attackDistance)
    protected override void HandleAttack()
    {
        // 1. Decide quedarse quieto
        currentMoveDirection = 0f;

        // 2. Llama a la corrutina de ataque (isAttacking se vuelve true)
        if (!isAttacking)
        {
            StartCoroutine(RangedAttack());
        }
    }

    protected override void HandleChase()
    {
        currentMoveDirection = Mathf.Sign(directionToPlayer.x);
    }

    // Esta corrutina es única del Elotero
    private IEnumerator RangedAttack()
    {
        isAttacking = true; // Pausa la IA (heredado de BaseEnemyAI)

        if (eloteBombaPrefab != null && firePoint != null && player != null)
        {
            // 1. Spawnea la bomba
            GameObject bomba = Instantiate(eloteBombaPrefab, firePoint.position, Quaternion.identity);
            EloteBomba eloteScript = bomba.GetComponent<EloteBomba>();

            // 2. Le pasa al proyectil el objetivo, el radio y el daño
            if (eloteScript != null)
            {
                eloteScript.Initialize(player.position, explosionRadius, explosionDamage);
            }
        }

        // 3. Espera el cooldown
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

}