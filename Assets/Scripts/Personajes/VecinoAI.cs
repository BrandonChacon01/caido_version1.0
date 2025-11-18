using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Hereda de la nueva clase base
public class VecinoAI : BaseEnemyAI
{
    [Header("Ataque Específico (Vecino)")]
    [Tooltip("El prefab del clon que spawnea al atacar.")]
    [SerializeField] private GameObject vecinoPrefab;
    [Tooltip("Tiempo de cooldown después de un ataque por contacto.")]
    [SerializeField] private float attackWaitTime = 1.5f;
    
    // Cuando está en modo de persecución debe moverse hacia el jugador.
    protected override void HandleChase()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    // El Vecino no tiene un "ataque a distancia", así que cuando
    // está en rango de ataque (HandleAttack), simplemente sigue persiguiendo.
    protected override void HandleAttack()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    // --- MÉTODOS DE DAÑO (Sobrescritos) ---
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // Si chocamos con el jugador Y no estamos ya en un cooldown
        if (collision.gameObject.CompareTag("Player") && !isAttacking)
        {
            PlayerController playerScript = collision.gameObject.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                // Iniciamos nuestra corrutina de ataque especial
                StartCoroutine(AttackAndSpawn(playerScript));
            }
        }
    }

    // Esta corrutina es única del Vecino
    private IEnumerator AttackAndSpawn(PlayerController playerScript)
    {
        isAttacking = true;

        // 1. Aplica el daño por contacto (usando la variable 'contactDamage' heredada)
        playerScript.Hit(contactDamage);

        // 2. Lógica de Spawn
        if (vecinoPrefab != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.0f, 0);
            GameObject newVecinoObj = Instantiate(vecinoPrefab, spawnPosition, Quaternion.identity);

            // Obtenemos los scripts del nuevo clon
            VecinoAI newVecinoAI = newVecinoObj.GetComponent<VecinoAI>();
            CharacterStats newVecinoStats = newVecinoObj.GetComponent<CharacterStats>();

            // Configuramos al clon
            if (newVecinoAI != null && newVecinoStats != null)
            {
                newVecinoAI.player = this.player; // Le dice a quién perseguir
                newVecinoStats.moveSpeed = this.moveSpeed * 0.80f; // Es 20% más lento
                newVecinoAI.vecinoPrefab = null; // Evita que el clon spawnee más clones
            }
        }

        // 3. Espera el cooldown
        yield return new WaitForSeconds(attackWaitTime);
        isAttacking = false; // Termina el estado de "atacando"
    }

    // Método para recibir daño (de balas)
    public void Hit(float damage)
    {
        base.TakeDamage(damage); // Llama al método base para restar vida
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
}