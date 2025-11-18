using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Hereda de la nueva clase base 'BaseEnemyAI'
public class AlbanilAI : BaseEnemyAI
{
    [Header("Ataque Específico (Albañil)")]
    [SerializeField] private GameObject slashPrefab; 
    [SerializeField] private Transform attackPoint; 
    [SerializeField] private float attackDamage = 2f;
    [SerializeField] private float attackWaitTime = 2f;

    // Implementación requerida por la clase base: chase behavior cuando el jugador está lejos (distance > attackDistance)
    protected override void HandleChase()
    {
        currentMoveDirection = Mathf.Sign(transform.localScale.x);
    }

    // --- MÉTODO REQUERIDO ---
    // Esto se ejecuta cuando el jugador está CERCA (distance <= attackDistance)
    protected override void HandleAttack()
    {
        // 1. Decide quedarse quieto
        currentMoveDirection = 0f;
        
        // 2. Llama a la corrutina de ataque (isAttacking se vuelve true)
        if (!isAttacking)
        {
            StartCoroutine(MeleeAttack());
        }
    }

    public void Hit(float damage)
    {
        base.TakeDamage(damage);
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    // Esta corrutina es única del Albañil
    private IEnumerator MeleeAttack()
    {
        isAttacking = true;

        // Pausa breve para simular el "swing" de la pala
        yield return new WaitForSeconds(0.2f); 

        if (slashPrefab != null && attackPoint != null)
        {
            // 1. Spawnea el slash
            GameObject slash = Instantiate(slashPrefab, attackPoint.position, Quaternion.identity);

            // 2. Orienta el slash (voltea)
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
                slashScript.damage = this.attackDamage;
            }
        }

        // 4. Espera el cooldown
        yield return new WaitForSeconds(attackWaitTime - 0.2f); // Resta la pausa del "swing"
        isAttacking = false;
    }

    // Método para recibir daño (de balas)
    // Sobrescribimos TakeDamage para actualizar la UI
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage); // Llama al método base para restar vida
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

}