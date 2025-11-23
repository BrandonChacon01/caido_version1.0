using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CholitoAI : BaseEnemyAI
{
    [Header("Ataque Específico (Cholito)")]
    public GameObject BulletPrefab;
    public float Rate = 0.25f;
    public float Recoil = 2f;
    private float LastShoot;


    protected override void HandleChase()
    {
        currentMoveDirection = Mathf.Sign(directionToPlayer.x);
    }

    // Esto se ejecuta cuando el jugador está CERCA (distance <= attackDistance)
    protected override void HandleAttack()
    {
        // Decide quedarse quieto
        currentMoveDirection = 0f;

        // Y ataca (dispara)
        if (Time.time > LastShoot + Rate)
        {
            Shoot();
            LastShoot = Time.time;
        }
    }

    // Esta función es única del Cholito, así que se queda aquí
    private void Shoot()
    {
        Vector3 direction = new Vector3(transform.localScale.x, 0.0f, 0.0f);
        GameObject bullet = Instantiate(BulletPrefab, transform.position + direction * 0.1f, Quaternion.identity);
        bullet.GetComponent<BulletScript>().SetDirection(direction);

        rb.AddForce(-direction * Recoil, ForceMode2D.Impulse);
    }

    // --- MÉTODO 'Hit' (OPCIONAL) ---
    // El 'Hit' de la clase base ya funciona, pero si quieres que
    // la barra de vida se actualice, necesitamos sobreescribirlo.
    // Si moviste la lógica del slider a 'TakeDamage' en la base, puedes borrar esto.
    public void Hit(float damage)
    {
        base.TakeDamage(damage);
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    // --- GIZMOS (OPCIONAL) ---
    // Podemos añadir un gizmo para la distancia de ataque sobre los gizmos de la base
    private new void OnDrawGizmosSelected()
    {
        // Dibuja el radio de ataque (cian)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position + new Vector3(attackDistance, -1, 0), transform.position + new Vector3(attackDistance, 1, 0));
        Gizmos.DrawLine(transform.position + new Vector3(-attackDistance, -1, 0), transform.position + new Vector3(-attackDistance, 1, 0));
    }

    // --- Lógica de 'FixedUpdate', 'Start', 'Awake', 'UpdateAnimator', 'Flip',
    // 'OnCollisionEnter2D' ya NO son necesarias aquí. Están en 'BaseEnemyAI'. ---
}