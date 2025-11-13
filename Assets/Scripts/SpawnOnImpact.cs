using UnityEngine;

public class SpawnOnImpact : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    [Tooltip("El charco de acelerador que aparecerá al chocar.")]
    [SerializeField] private GameObject prefabToSpawn;
    
    [Tooltip("La capa del suelo para detectar el impacto.")]
    [SerializeField] private LayerMask groundLayer;
    
    // --- NUEVO ---
    [Header("Efectos Visuales")]
    [Tooltip("El prefab de la explosión que aparecerá al chocar.")]
    [SerializeField] private GameObject explosionPrefab;


    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Revisamos si lo que tocamos está en la capa "Ground"
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            // --- Lógica de Explosión y Spawn ---
            
            // Primero, crea la explosión en el punto de impacto
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            // Segundo, crea el charco de acelerador
            SpawnPatch(other);

            // Tercero, destruye este proyectil (el Elote)
            Destroy(gameObject);
        }
    }

    private void SpawnPatch(Collider2D groundCollider)
    {
        if (prefabToSpawn != null)
        {
            // Spawnea el prefab en el punto más cercano del suelo
            Vector3 spawnPos = groundCollider.ClosestPoint(transform.position);
            Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        }
    }
}