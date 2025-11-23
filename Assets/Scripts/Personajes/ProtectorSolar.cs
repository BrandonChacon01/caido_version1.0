using UnityEngine;

public class ProtectorSolar : MonoBehaviour
{
    [Header("Configuración del Power-Up")]
    [SerializeField] private float duracion = 5f;
    [SerializeField] private float radioAura = 3f;
    [SerializeField] private float danoAura = 2f;

    [Header("Efectos (Opcional)")]
    [SerializeField] private GameObject efectoRecogido; // Partículas al recoger (opcional)
    [SerializeField] private AudioClip sonidoRecogido;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificamos si chocamos con el jugador
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            // Activamos el efecto en el jugador
            player.ActivarProtectorSolar(duracion, danoAura, radioAura);

            // Reproducir sonido si existe
            if (sonidoRecogido != null)
            {
                AudioSource.PlayClipAtPoint(sonidoRecogido, transform.position);
            }

            // Instanciar partículas si existen
            if (efectoRecogido != null)
            {
                Instantiate(efectoRecogido, transform.position, Quaternion.identity);
            }

            // Destruir el objeto del mundo (ya fue consumido)
            Destroy(gameObject);
        }
    }
    
    // Dibujar el radio del aura en el editor para que sepas qué tan grande será
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.4f); // Amarillo transparente
        Gizmos.DrawWireSphere(transform.position, radioAura);
    }
}