using UnityEngine;

public class Salsita : MonoBehaviour
{
    [Header("Configuración del Power-Up")]
    // Multiplicador: 1.5 significa 50% más rápido, 2 significa el doble de rápido.
    public float multiplicadorDeVelocidad = 1.5f;

    // La duración del efecto en segundos.
    public float duracionDelPowerUp = 5.0f; // 5 segundos

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobamos que el objeto que nos toca es el jugador
        if (other.CompareTag("Player"))
        {
            // Buscamos el script del jugador
            PlayerMovement player = other.GetComponent<PlayerMovement>();

            if (player != null)
            {
                // Llamamos a la función pública del jugador y le pasamos nuestros valores
                player.ActivarPowerUpVelocidad(multiplicadorDeVelocidad, duracionDelPowerUp);

                // Destruimos la salsita para que no se pueda volver a usar
                Destroy(gameObject);
            }
        }
    }
}