using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))] 
[RequireComponent(typeof(AudioSource))] // --- NUEVO: Requiere AudioSource
public class VisibleDamageZone : MonoBehaviour
{
    [Header("Configuración de Zona")]
    [Tooltip("Daño por segundo que aplica esta zona al Jugador")]
    [SerializeField] private float damagePerSecond = 2f;

    [Header("Visualización en Editor")]
    [Tooltip("Color del área en la vista de Escena")]
    [SerializeField] private Color zoneColor = new Color(1f, 0f, 0f, 0.3f); 

    [Header("Configuración de Audio")] // --- NUEVO ---
    [Tooltip("Sonido que se reproduce mientras el jugador recibe daño")]
    [SerializeField] private AudioClip damageSound;
    [Tooltip("Cada cuántos segundos se reproduce el sonido (para no saturar)")]
    [SerializeField] private float soundInterval = 0.5f; 
    
    private float nextSoundTime = 0f;
    private AudioSource audioSource;

    // Lista para rastrear al jugador si entra
    private PlayerController targetPlayer = null;
    private Collider2D zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
        // Forzamos que sea trigger por si se te olvidó marcarlo
        zoneCollider.isTrigger = true;

        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.5f; // Un poco de sonido 3D para que sepa de dónde viene
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Revisamos si lo que entró es el jugador
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            targetPlayer = player;
            // Opcional: Reproducir sonido inmediatamente al entrar
            // PlayDamageSound(); 
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
            // 1. Aplicar Daño
            float damageToApply = damagePerSecond * Time.deltaTime;
            targetPlayer.TakeDamage(damageToApply);

            // 2. Reproducir Sonido (con intervalo)
            if (Time.time >= nextSoundTime)
            {
                PlayDamageSound();
                nextSoundTime = Time.time + soundInterval;
            }
        }
    }

    private void PlayDamageSound()
    {
        if (damageSound != null && audioSource != null)
        {
            // Usamos PlayOneShot para que se puedan superponer si hay varios sonidos
            // y variar un poco el tono (pitch) para que no suene robótico
            audioSource.pitch = Random.Range(0.9f, 1.1f); 
            audioSource.PlayOneShot(damageSound);
        }
    }

    // --- ESTA ES LA MAGIA VISUAL ---
    private void OnDrawGizmos()
    {
        if (zoneCollider == null) zoneCollider = GetComponent<Collider2D>();
        if (zoneCollider == null) return;

        Gizmos.color = zoneColor;

        if (zoneCollider is BoxCollider2D box)
        {
            Gizmos.DrawCube(box.bounds.center, box.bounds.size);
            Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
            Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
        }
        else if (zoneCollider is CircleCollider2D circle)
        {
            Gizmos.DrawSphere(circle.bounds.center, circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y));
            Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
            Gizmos.DrawWireSphere(circle.bounds.center, circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y));
        }
    }
}