using UnityEngine;

/// <summary>
/// ObstacleSpike - Obstáculo que daña al jugador al tocarlo
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ObstacleSpike : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [Tooltip("Cantidad de daño que hace al jugador")]
    public float Damage = 1f;

    [Header("Configuración Visual")]
    [Tooltip("Color del obstáculo (para identificarlo fácilmente)")]
    public Color ObstacleColor = Color.red;

    [Header("Configuración de Audio")]
    [Tooltip("Sonido al tocar el obstáculo")]
    public AudioClip HitSound;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // Obtener componentes
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Asegurar que el collider sea trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Aplicar color
        spriteRenderer.color = ObstacleColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si es el jugador
        if (other.CompareTag("Player"))
        {
            DamagePlayer(other.gameObject);
        }
    }

    /// <summary>
    /// Aplica daño al jugador
    /// </summary>
    private void DamagePlayer(GameObject player)
    {
        // Buscar el script PlayerController (actualizado)
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Llamar al método Hit del jugador
            playerController.Hit(Damage);
            UnityEngine.Debug.Log($"[ObstacleSpike] Jugador recibió {Damage} de daño");

            // Reproducir sonido
            PlayHitSound();
        }
        else
        {
            UnityEngine.Debug.LogWarning("[ObstacleSpike] PlayerController no encontrado en el jugador");
        }
    }

    /// <summary>
    /// Reproduce el sonido de golpe
    /// </summary>
    private void PlayHitSound()
    {
        if (HitSound != null && Camera.main != null)
        {
            AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(HitSound);
            }
        }
    }

    /// <summary>
    /// Dibuja el obstáculo en el editor
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}