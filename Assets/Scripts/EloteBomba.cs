using UnityEngine;

public class EloteBomba : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 8f;
    
    [Header("Efectos Visuales y Sonoros")]
    [SerializeField] private GameObject explosionVFXPrefab; // Opcional: para un efecto visual
    [Tooltip("Sonido al explotar")]
    [SerializeField] private AudioClip explosionSound; // --- NUEVO ---

    private Vector2 targetPosition;
    private float explosionRadius;
    private float damage;
    private bool isInitialized = false;

    // El EloteroAI llamará a este método para configurar la bomba
    public void Initialize(Vector2 target, float radius, float dmg)
    {
        this.targetPosition = target;
        this.explosionRadius = radius;
        this.damage = dmg;
        this.isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // Mueve el proyectil hacia la posición objetivo
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Si llega al objetivo (o muy cerca), explota
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // 1. Efecto Visual
        if (explosionVFXPrefab != null)
        {
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
        }

        // 2. --- NUEVO: Efecto de Sonido ---
        if (explosionSound != null)
        {
            // Reproduce el sonido en el lugar de la explosión antes de destruir el objeto
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // 3. Detecta todos los colliders dentro del radio de explosión
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D col in objectsInRange)
        {
            // Si encuentra al jugador, le hace daño
            if (col.CompareTag("Player"))
            {
                PlayerController player = col.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.Hit(damage);
                }
                // Rompemos el bucle si solo quieres que dañe al jugador una vez
                break;
            }
        }

        // 4. Destruye el proyectil
        Destroy(gameObject);
    }

    // Dibuja el radio de la explosión en el editor para que sea fácil de ver
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}