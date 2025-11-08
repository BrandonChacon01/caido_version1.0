using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Cantidad de balas que recarga este objeto")]
    [SerializeField] private int ammoAmount = 3; // Por defecto recarga 3 (toda la barra)

    [Tooltip("Sonido opcional al recoger")]
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificamos si lo que entró es el jugador
        if (other.CompareTag("Player"))
        {
            // Buscamos el nuevo script del jugador
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                // --- Lógica de Recarga ---
                // Llamamos al nuevo método público que creamos
                player.RechargeAmmo(ammoAmount);

                // Destruimos este objeto para que no se pueda volver a recoger
                Destroy(gameObject);
            }
        }
    }
}