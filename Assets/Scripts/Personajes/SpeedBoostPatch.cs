using UnityEngine;

[RequireComponent(typeof(AudioSource))] // Asegura que el objeto tenga un AudioSource
public class SpeedBoostPatch : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float multiplicadorVelocidad = 2f;
    [SerializeField] private float lifeTimeInSeconds = 5f;
    [SerializeField] private bool gravedad = false;

    [Header("Audio")]
    [SerializeField] private AudioClip proximitySound; // Arrastra tu clip de audio aquí
    [SerializeField] private float soundVolume = 0.5f;

    private AudioSource audioSource;

    // Se llama cuando el objeto es creado
    private void Start()
    {
        // Configuración inicial del AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.clip = proximitySound;
            audioSource.volume = soundVolume;
            audioSource.loop = true; // Hacemos que se repita mientras estés cerca
            audioSource.playOnAwake = false; // No reproducir al inicio, solo al entrar
            
            // Configuración 3D opcional para que suene más fuerte al acercarse
            audioSource.spatialBlend = 1.0f; // 1.0 = 3D Sound
            audioSource.minDistance = 1.0f;
            audioSource.maxDistance = 5.0f;
        }

        if (gravedad == false)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
            }
        }
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
                
                // Reproducir sonido
                if (audioSource != null && proximitySound != null)
                {
                    if (!audioSource.isPlaying)
                    {
                        audioSource.Play();
                    }
                }
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
                
                // Detener sonido
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
            }
        }
    }
    
    // Asegurarnos de detener el sonido si el objeto se destruye mientras el jugador está dentro
    private void OnDestroy()
    {
         if (audioSource != null)
         {
             audioSource.Stop();
         }
    }
}