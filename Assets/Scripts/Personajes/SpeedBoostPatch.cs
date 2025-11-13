using UnityEngine;

public class SpeedBoostPatch : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float multiplicadorVelocidad = 2f;
    [SerializeField] private float lifeTimeInSeconds = 5f;

    // Se llama cuando el objeto es creado
    private void Start()
    {
        // Se destruye a sí mismo después de X segundos
        Destroy(gameObject, lifeTimeInSeconds);
    }

    // Se activa cuando el jugador ENTRA
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Inicia un boost "infinito" (o por mucho tiempo)
                player.ActivarPowerUpVelocidad(multiplicadorVelocidad, 999f);
            }
        }
    }

    // Se activa cuando el jugador SALE
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Llama al nuevo método para CANCELAR el boost al instante
                player.CancelSpeedPowerUp();
            }
        }
    }
}