using UnityEngine;

/// <summary>
/// LevelEndFlag - Script para el objeto que finaliza el nivel
/// Detecta cuando el jugador lo toca y notifica al GameManager
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelEndFlag : MonoBehaviour
{
    [Header("Configuración Visual")]
    [Tooltip("CAMBIAR ESTO: Sprite de la bandera (arrastrar cuando tengas el asset)")]
    public Sprite FlagSprite;

    [Tooltip("Color temporal de la bandera")]
    public Color FlagColor = Color.yellow;

    [Tooltip("Tamaño de la bandera")]
    public Vector2 FlagSize = new Vector2(1f, 2f);

    [Header("Configuración de Audio")]
    [Tooltip("CAMBIAR ESTO: Sonido al completar el nivel")]
    public AudioClip CompletionSound;

    [Header("Efectos Visuales")]
    [Tooltip("¿Animar la bandera? (rotación constante)")]
    public bool AnimateFlag = true;

    [Tooltip("Velocidad de rotación de la animación")]
    public float RotationSpeed = 50f;

    private SpriteRenderer spriteRenderer;
    private Collider2D triggerCollider;
    private bool levelCompleted = false;

    private void Awake()
    {
        // Configurar el SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Configurar el Collider2D como trigger
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider == null)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = FlagSize;
            triggerCollider = boxCollider;
        }
        triggerCollider.isTrigger = true;

        // Configurar apariencia visual
        SetupVisuals();
    }

    private void Start()
    {
        UnityEngine.Debug.Log("[LevelEndFlag] Bandera de nivel inicializada");
    }

    private void Update()
    {
        // Animación simple de rotación
        if (AnimateFlag && !levelCompleted)
        {
            transform.Rotate(Vector3.forward * RotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Configura la apariencia visual de la bandera
    /// </summary>
    private void SetupVisuals()
    {
        if (FlagSprite != null)
        {
            // Si hay un sprite asignado, usarlo
            spriteRenderer.sprite = FlagSprite;
        }
        else
        {
            // Crear un sprite temporal simple
            CreateTemporarySprite();
        }

        spriteRenderer.color = FlagColor;
        spriteRenderer.sortingOrder = 10; // Asegurar que se vea encima de otros objetos
    }

    /// <summary>
    /// Crea un sprite temporal hasta que se asigne el real
    /// </summary>
    private void CreateTemporarySprite()
    {
        // NOTA: Este es un sprite temporal. Reemplazar con tu asset de bandera
        // Por ahora usamos un cuadrado simple
        Texture2D texture = new Texture2D(32, 64);
        Color[] pixels = new Color[32 * 64];

        // Llenar con el color de la bandera
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = FlagColor;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        spriteRenderer.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, 32, 64),
            new Vector2(0.5f, 0f), // Pivot en la parte inferior central
            32
        );
    }

    /// <summary>
    /// Detecta cuando el jugador toca la bandera
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si es el jugador
        if (other.CompareTag("Player") && !levelCompleted)
        {
            CompleteLevel();
        }
    }

    /// <summary>
    /// Completa el nivel actual
    /// </summary>
    private void CompleteLevel()
    {
        levelCompleted = true;
        UnityEngine.Debug.Log("[LevelEndFlag] ¡Nivel completado! Tocaste la bandera");

        // Reproducir sonido si existe
        PlayCompletionSound();

        // Efecto visual de completado
        StartCoroutine(CompletionEffect());

        // Notificar al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteLevel();
        }
        else
        {
            UnityEngine.Debug.LogError("[LevelEndFlag] No se encontró GameManager en la escena");
        }
    }

    /// <summary>
    /// Reproduce el sonido de completado
    /// </summary>
    private void PlayCompletionSound()
    {
        if (CompletionSound != null && Camera.main != null)
        {
            AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(CompletionSound);
            }
            else
            {
                UnityEngine.Debug.LogWarning("[LevelEndFlag] No hay AudioSource en la cámara principal");
            }
        }
    }

    /// <summary>
    /// Efecto visual al completar el nivel
    /// </summary>
    private System.Collections.IEnumerator CompletionEffect()
    {
        // Efecto de parpadeo
        for (int i = 0; i < 6; i++)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.15f);
        }
        spriteRenderer.enabled = true;

        // Efecto de escala (hacer la bandera más grande)
        Vector3 originalScale = transform.localScale;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 1.5f, elapsed / duration);
            transform.localScale = originalScale * scale;
            yield return null;
        }
    }

    /// <summary>
    /// Dibuja gizmos en el editor para visualizar la posición de la bandera
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * (FlagSize.y / 2), FlagSize);

        // Dibujar una línea hacia arriba para indicar que es la bandera
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * FlagSize.y);
    }
}