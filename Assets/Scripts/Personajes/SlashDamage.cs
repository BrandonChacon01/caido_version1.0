using UnityEngine;

public class SlashDamage : MonoBehaviour
{
    public float damage;
    public float lifeTime = 0.3f;

    // --- NUEVO: Variable para el sonido ---
    [Tooltip("Sonido al golpear al jugador")]
    public AudioClip hitSound;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Hit(damage);

                // --- NUEVO: Reproducir sonido ---
                if (hitSound != null)
                {
                    // Usamos PlayClipAtPoint para crear un sonido temporal en esa posición.
                    // Esto asegura que el sonido se escuche completo aunque el Slash se destruya 0.1 segundos después.
                    AudioSource.PlayClipAtPoint(hitSound, transform.position);
                }
            }
            GetComponent<Collider2D>().enabled = false;
        }
    }
}