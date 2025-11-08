using UnityEngine;

public class SpawnOnImpact : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("El prefab que aparecerá al chocar (ej. IcePatch).")]
    [SerializeField] private GameObject prefabToSpawn;
    
    [Tooltip("La capa del suelo para detectar el impacto.")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("Tiempo de vida de la mancha (0 = infinito).")]
    [SerializeField] private float lifeTime = 5f;

    // Esta función se ejecuta cuando el TRIGGER del taco toca algo
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Revisamos si lo que tocamos está en la capa "Ground"
        // (Usamos una operación de bits para verificar la layer)
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            SpawnPatch(other);
        }
    }

    private void SpawnPatch(Collider2D groundCollider)
    {
        if (prefabToSpawn != null)
        {
            // Buscamos el punto más cercano del suelo a la bala para instanciar ahí
            Vector3 spawnPos = groundCollider.ClosestPoint(transform.position);
            
            // Ajuste opcional: subirlo un poquito para que no quede enterrado en el tilemap
            spawnPos.y += 0.05f; 

            // Creamos la mancha
            GameObject patch = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            // Si tiene tiempo de vida, lo destruimos después de un rato
            if (lifeTime > 0)
            {
                Destroy(patch, lifeTime);
            }
        }
    }
}