using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Hereda de la nueva clase base 'BaseEnemyAI'
public class TaqueroAI : BaseEnemyAI
{
    [Header("Ataque Específico (Taquero)")]
    [SerializeField] private GameObject tacoPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackRate = 2f; 

    protected override void HandleChase()
    {
        currentMoveDirection = Mathf.Sign(directionToPlayer.x);
    }

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

    public void Hit(float damage)
    {
        base.TakeDamage(damage);
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    // Esta corrutina es única del Taquero
    private IEnumerator RangedAttack()
    {
        isAttacking = true; // Pausa la IA (heredado de BaseEnemyAI)

        // (Opcional) Aquí podrías poner un anim.SetTrigger("Attack")
        
        if (tacoPrefab != null && firePoint != null)
        {
            // 1. Spawnea el taco
            GameObject taco = Instantiate(tacoPrefab, firePoint.position, Quaternion.identity);
            
            // 2. Calcula la dirección (1 o -1) basándote en la escala
            Vector2 direction = new Vector2(transform.localScale.x, 0).normalized;

            // 3. Pasa la dirección al script del taco
            BulletScript tacoScript = taco.GetComponent<BulletScript>();
            if (tacoScript != null)
            {
                tacoScript.SetDirection(direction);
            }
        }

        // 4. Espera el cooldown
        yield return new WaitForSeconds(attackRate);
        isAttacking = false;
    }

}