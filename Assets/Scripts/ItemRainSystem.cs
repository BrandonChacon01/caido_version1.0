using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema que hace llover items desde el cielo en zonas específicas
/// </summary>
public class ItemRainSystem : MonoBehaviour
{
    [Header("⚙️ Configuración de Items")]
    [Tooltip("Prefabs de items que pueden caer (arrastra aquí los prefabs)")]
    [SerializeField] private GameObject[] itemPrefabs;

    [Tooltip("Tiempo entre caídas de items (segundos)")]
    [SerializeField] private float dropInterval = 20f;

    [Tooltip("Máximo de items en pantalla a la vez")]
    [SerializeField] private int maxItemsActive = 3;

    [Header("📍 Zona de Caída")]
    [Tooltip("Posición X mínima donde pueden caer items")]
    [SerializeField] private float minX = -5f;

    [Tooltip("Posición X máxima donde pueden caer items")]
    [SerializeField] private float maxX = 5f;

    [Tooltip("Altura Y desde donde caen los items")]
    [SerializeField] private float spawnHeight = 10f;

    [Tooltip("¿Usar la posición del GameObject como centro de la zona?")]
    [SerializeField] private bool useTransformAsCenter = true;

    [Header("🎯 Configuración de Caída")]
    [Tooltip("Velocidad de caída de los items")]
    [SerializeField] private float fallSpeed = 2f;

    [Tooltip("¿Los items tienen gravedad? (Si no, caen a velocidad constante)")]
    [SerializeField] private bool useGravity = true;

    [Tooltip("Tiempo antes de que un item se autodestruya si no se recoge (0 = nunca)")]
    [SerializeField] private float itemLifetime = 10f;

    [Header("🎨 Efectos Visuales")]
    [Tooltip("Efecto visual al aparecer el item (opcional)")]
    [SerializeField] private GameObject spawnEffect;

    [Tooltip("¿Hacer que los items parpadeen antes de desaparecer?")]
    [SerializeField] private bool blinkBeforeDestroy = true;

    [Tooltip("Tiempo de parpadeo antes de destruirse")]
    [SerializeField] private float blinkDuration = 2f;

    [Header("🔊 Audio (Opcional)")]
    [Tooltip("Sonido al aparecer un item")]
    [SerializeField] private AudioClip spawnSound;

    // Variables privadas
    private List<GameObject> activeItems = new List<GameObject>();
    private float nextDropTime = 0f;
    private bool isActive = true;

    private void Start()
    {
        // Configurar el primer drop
        nextDropTime = Time.time + dropInterval;

        UnityEngine.Debug.Log($"[ItemRain] Sistema iniciado. Zona: X({minX} a {maxX}), Altura: {spawnHeight}");
    }

    private void Update()
    {
        if (!isActive) return;

        // Limpiar items destruidos de la lista
        activeItems.RemoveAll(item => item == null);

        // Verificar si es tiempo de dropear un item
        if (Time.time >= nextDropTime && activeItems.Count < maxItemsActive)
        {
            DropRandomItem();
            nextDropTime = Time.time + dropInterval;
        }
    }

    /// <summary>
    /// Dropear un item aleatorio
    /// </summary>
    private void DropRandomItem()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            UnityEngine.Debug.LogWarning("[ItemRain] No hay prefabs de items asignados");
            return;
        }

        // Seleccionar prefab aleatorio
        GameObject itemPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

        if (itemPrefab == null)
        {
            UnityEngine.Debug.LogWarning("[ItemRain] Prefab de item nulo en el array");
            return;
        }

        // Calcular posición de spawn
        Vector3 spawnPosition = CalculateSpawnPosition();

        // Instanciar item
        GameObject droppedItem = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);

        // Configurar física del item
        SetupItemPhysics(droppedItem);

        // Agregar a la lista
        activeItems.Add(droppedItem);

        // Efecto visual de spawn
        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, spawnPosition, Quaternion.identity);
        }

        // Sonido de spawn
        PlaySound(spawnSound);

        // Autodestrucción programada
        if (itemLifetime > 0f)
        {
            StartCoroutine(DestroyItemAfterTime(droppedItem, itemLifetime));
        }

        UnityEngine.Debug.Log($"[ItemRain] Item dropeado: {itemPrefab.name} en {spawnPosition}");
    }

    /// <summary>
    /// Calcular posición aleatoria de spawn
    /// </summary>
    private Vector3 CalculateSpawnPosition()
    {
        float randomX;
        float spawnY;

        if (useTransformAsCenter)
        {
            // Usar la posición de este GameObject como centro
            randomX = transform.position.x + Random.Range(minX, maxX);
            spawnY = transform.position.y + spawnHeight;
        }
        else
        {
            // Usar coordenadas absolutas
            randomX = Random.Range(minX, maxX);
            spawnY = spawnHeight;
        }

        return new Vector3(randomX, spawnY, 0f);
    }

    /// <summary>
    /// Configurar la física del item que cae
    /// </summary>
    private void SetupItemPhysics(GameObject item)
    {
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = item.AddComponent<Rigidbody2D>();
        }

        // Configurar Rigidbody
        if (useGravity)
        {
            rb.gravityScale = 1f;
        }
        else
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.down * fallSpeed;
        }

        // Asegurar que tenga collider como trigger
        Collider2D col = item.GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    /// <summary>
    /// Destruir item después de un tiempo
    /// </summary>
    private IEnumerator DestroyItemAfterTime(GameObject item, float delay)
    {
        if (item == null) yield break;

        // Esperar el tiempo menos el tiempo de parpadeo
        float waitTime = blinkBeforeDestroy ? delay - blinkDuration : delay;
        yield return new WaitForSeconds(Mathf.Max(0f, waitTime));

        // Parpadeo antes de destruir
        if (blinkBeforeDestroy && item != null)
        {
            yield return StartCoroutine(BlinkItem(item, blinkDuration));
        }

        // Destruir item
        if (item != null)
        {
            Destroy(item);
        }
    }

    /// <summary>
    /// Hacer parpadear el item
    /// </summary>
    private IEnumerator BlinkItem(GameObject item, float duration)
    {
        SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float elapsedTime = 0f;
        bool isVisible = true;

        while (elapsedTime < duration && item != null)
        {
            isVisible = !isVisible;
            sr.enabled = isVisible;
            yield return new WaitForSeconds(0.2f);
            elapsedTime += 0.2f;
        }

        // Asegurar que esté visible al final
        if (sr != null)
        {
            sr.enabled = true;
        }
    }

    /// <summary>
    /// Reproducir sonido
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (Camera.main != null)
        {
            AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    /// <summary>
    /// Activar/Desactivar el sistema
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        UnityEngine.Debug.Log($"[ItemRain] Sistema {(active ? "activado" : "desactivado")}");
    }

    /// <summary>
    /// Forzar drop de un item inmediatamente
    /// </summary>
    public void ForceDropItem()
    {
        DropRandomItem();
    }

    /// <summary>
    /// Limpiar todos los items activos
    /// </summary>
    public void ClearAllItems()
    {
        foreach (GameObject item in activeItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        activeItems.Clear();
    }

    /// <summary>
    /// Gizmos para visualizar la zona de caída
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 center;
        float gizmoMinX, gizmoMaxX, gizmoHeight;

        if (useTransformAsCenter)
        {
            center = transform.position;
            gizmoMinX = center.x + minX;
            gizmoMaxX = center.x + maxX;
            gizmoHeight = center.y + spawnHeight;
        }
        else
        {
            gizmoMinX = minX;
            gizmoMaxX = maxX;
            gizmoHeight = spawnHeight;
            center = new Vector3((minX + maxX) / 2f, spawnHeight, 0f);
        }

        // Dibujar línea horizontal de la zona de spawn
        Vector3 leftPoint = new Vector3(gizmoMinX, gizmoHeight, 0f);
        Vector3 rightPoint = new Vector3(gizmoMaxX, gizmoHeight, 0f);

        Gizmos.DrawLine(leftPoint, rightPoint);

        // Dibujar líneas verticales en los extremos
        Gizmos.DrawLine(leftPoint, leftPoint + Vector3.down * 2f);
        Gizmos.DrawLine(rightPoint, rightPoint + Vector3.down * 2f);

        // Dibujar zona de caída
        Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
        Vector3 size = new Vector3(gizmoMaxX - gizmoMinX, 0.5f, 0f);
        Gizmos.DrawCube(center, size);
    }

#if UNITY_EDITOR
    [ContextMenu("Test - Dropear Item Ahora")]
    private void TestDropItem()
    {
        if (Application.isPlaying)
        {
            ForceDropItem();
        }
        else
        {
            UnityEngine.Debug.LogWarning("Solo funciona en modo Play");
        }
    }

    [ContextMenu("Test - Limpiar Todos los Items")]
    private void TestClearItems()
    {
        if (Application.isPlaying)
        {
            ClearAllItems();
        }
    }
#endif
}