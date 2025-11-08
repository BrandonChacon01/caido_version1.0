using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))] // Asegura que siempre haya un Collider
public class VisibleDamageZone : MonoBehaviour
{
    [Header("Configuración de Zona")]
    [Tooltip("Daño por segundo que aplica esta zona al Jugador")]
    [SerializeField] private float damagePerSecond = 2f;

    [Header("Visualización en Editor")]
    [Tooltip("Color del área en la vista de Escena")]
    [SerializeField] private Color zoneColor = new Color(1f, 0f, 0f, 0.3f); // Rojo semitransparente

    // Lista para rastrear al jugador si entra
    private PlayerController targetPlayer = null;
    private Collider2D zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
        // Forzamos que sea trigger por si se te olvidó marcarlo
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Revisamos si lo que entró es el jugador
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            targetPlayer = player;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Si el jugador sale, dejamos de rastrearlo
        if (other.GetComponent<PlayerController>() != null)
        {
            targetPlayer = null;
        }
    }

    private void Update()
    {
        // Si tenemos al jugador en la mira, le aplicamos daño constante
        if (targetPlayer != null)
        {
            // Calculamos el daño por frame
            float damageToApply = damagePerSecond * Time.deltaTime;
            targetPlayer.TakeDamage(damageToApply);
        }
    }

    // --- ESTA ES LA MAGIA VISUAL ---
    // Este método dibuja en el editor de Unity, pero NO en el juego final.
    private void OnDrawGizmos()
    {
        // Si no tenemos collider aún (ej. acabas de crear el objeto), intentamos obtenerlo
        if (zoneCollider == null) zoneCollider = GetComponent<Collider2D>();
        if (zoneCollider == null) return;

        Gizmos.color = zoneColor;

        // Dibuja la forma correcta dependiendo del tipo de collider que uses
        if (zoneCollider is BoxCollider2D box)
        {
            // Dibuja un cubo relleno que coincide con el BoxCollider2D
            Gizmos.DrawCube(box.bounds.center, box.bounds.size);
            // Dibuja el borde para que se vea mejor
            Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
            Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
        }
        else if (zoneCollider is CircleCollider2D circle)
        {
            // Dibuja una esfera rellena que coincide con el CircleCollider2D
            Gizmos.DrawSphere(circle.bounds.center, circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y));
             // Dibuja el borde
            Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
            Gizmos.DrawWireSphere(circle.bounds.center, circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y));
        }
    }
}