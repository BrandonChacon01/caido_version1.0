using UnityEngine;

public class BacheTrap : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Segundos que el jugador se queda atorado.")]
    [SerializeField] private float trapDuration = 2f;
    
    [Tooltip("¿Se destruye después de usarse una vez?")]
    [SerializeField] private bool oneTimeUse = false;

    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si ya se usó y es de un solo uso, ignoramos
        if (isActivated && oneTimeUse) return;

        // Verificamos si lo que entró tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            
            if (player != null)
            {
                isActivated = true;
                // Llamamos al nuevo método del jugador para inmovilizarlo
                player.Immobilize(trapDuration);

                // Opcional: Aquí podrías reproducir un sonido de "caída"
                // AudioSource.PlayClipAtPoint(trapSound, transform.position);

                if (oneTimeUse)
                {
                    // Opción A: Destruir el objeto completo
                    // Destroy(gameObject); 
                    
                    // Opción B: Solo desactivar el collider para que se siga viendo pero no funcione
                    GetComponent<Collider2D>().enabled = false;
                }
            }
        }
    }
}