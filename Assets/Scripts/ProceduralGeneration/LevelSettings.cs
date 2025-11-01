using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// LevelSettings - ScriptableObject que define la configuración de cada nivel
/// Permite ajustar dificultad, enemigos, items y apariencia visual por nivel
/// </summary>
[CreateAssetMenu(fileName = "LevelSettings_", menuName = "Procedural Level/Level Settings", order = 1)]
public class LevelSettings : ScriptableObject
{
    [Header("Identificación del Nivel")]
    [Tooltip("Número del nivel (1-5)")]
    public int LevelNumber = 1;

    [Tooltip("Nombre descriptivo del nivel")]
    public string LevelName = "Nivel 1";

    [Header("Dimensiones del Nivel")]
    [Tooltip("Longitud total del nivel en chunks (segmentos)")]
    [Range(10, 50)]
    public int LevelLengthInChunks = 20;

    [Tooltip("Ancho de cada chunk en unidades de Unity")]
    public float ChunkWidth = 10f;

    [Header("Dificultad")]
    [Tooltip("Probabilidad de spawn de enemigos (0 = ninguno, 1 = máximo)")]
    [Range(0f, 1f)]
    public float EnemySpawnRate = 0.3f;

    [Tooltip("Cantidad mínima de enemigos por chunk")]
    [Range(0, 5)]
    public int MinEnemiesPerChunk = 0;

    [Tooltip("Cantidad máxima de enemigos por chunk")]
    [Range(0, 5)]
    public int MaxEnemiesPerChunk = 2;

    [Tooltip("Probabilidad de spawn de items/powerups")]
    [Range(0f, 1f)]
    public float ItemSpawnRate = 0.2f;

    [Header("Prefabs de Enemigos")]
    [Tooltip("Lista de prefabs de enemigos que pueden aparecer en este nivel")]
    public List<GameObject> EnemyPrefabs = new List<GameObject>();

    [Header("Prefabs de Items")]
    [Tooltip("Lista de prefabs de items/powerups disponibles")]
    public List<GameObject> ItemPrefabs = new List<GameObject>();

    [Header("Configuración de Plataformas")]
    [Tooltip("Prefab del bloque de suelo/plataforma")]
    public GameObject GroundPrefab;

    [Tooltip("Altura base del suelo")]
    public float GroundHeight = -3f;

    [Tooltip("Probabilidad de generar gaps (huecos)")]
    [Range(0f, 0.5f)]
    public float GapProbability = 0.1f;

    [Tooltip("Tamaño mínimo de un gap en unidades")]
    [Range(1f, 5f)]
    public float MinGapSize = 2f;

    [Tooltip("Tamaño máximo de un gap en unidades")]
    [Range(2f, 8f)]
    public float MaxGapSize = 4f;

    [Header("Variación Vertical")]
    [Tooltip("Probabilidad de cambiar la altura del terreno")]
    [Range(0f, 0.5f)]
    public float HeightChangeProbability = 0.15f;

    [Tooltip("Altura máxima que puede subir el terreno")]
    [Range(0f, 5f)]
    public float MaxHeightIncrease = 3f;

    [Tooltip("Altura máxima que puede bajar el terreno")]
    [Range(0f, 3f)]
    public float MaxHeightDecrease = 2f;

    [Header("Apariencia Visual")]
    [Tooltip("CAMBIAR ESTO: Color de fondo del nivel - usa SpriteRenderer o Camera.backgroundColor")]
    public Color BackgroundColor = new Color(0.53f, 0.81f, 0.92f); // Azul cielo por defecto

    [Tooltip("CAMBIAR ESTO: Color de las plataformas - aplicar al SpriteRenderer del GroundPrefab")]
    public Color PlatformColor = Color.gray;

    [Tooltip("CAMBIAR ESTO: Sprite del fondo (asignar en el Inspector cuando tengas los assets)")]
    public Sprite BackgroundSprite;

    [Tooltip("CAMBIAR ESTO: Sprites del tilemap (asignar cuando tengas los assets del nivel)")]
    public List<Sprite> TilemapSprites = new List<Sprite>();

    [Header("Referencias Visuales")]
    [TextArea(3, 5)]
    public string VisualNotes = "NOTAS VISUALES:\n- BackgroundColor: Cambiar desde Inspector para cada nivel\n- BackgroundSprite: Arrastrar sprite del fondo aquí\n- TilemapSprites: Agregar tiles específicos del nivel";

    /// <summary>
    /// Obtiene un prefab de enemigo aleatorio de la lista
    /// </summary>
    public GameObject GetRandomEnemy()
    {
        if (EnemyPrefabs.Count == 0)
        {
            UnityEngine.Debug.LogWarning($"[LevelSettings] No hay enemigos configurados para {LevelName}");
            return null;
        }
        return EnemyPrefabs[UnityEngine.Random.Range(0, EnemyPrefabs.Count)];
    }

    /// <summary>
    /// Obtiene un prefab de item aleatorio de la lista
    /// </summary>
    public GameObject GetRandomItem()
    {
        if (ItemPrefabs.Count == 0)
        {
            UnityEngine.Debug.LogWarning($"[LevelSettings] No hay items configurados para {LevelName}");
            return null;
        }
        return ItemPrefabs[UnityEngine.Random.Range(0, ItemPrefabs.Count)];
    }

    /// <summary>
    /// Calcula cuántos enemigos spawnear en un chunk
    /// </summary>
    public int GetEnemyCountForChunk()
    {
        if (UnityEngine.Random.value > EnemySpawnRate)
            return 0;

        return UnityEngine.Random.Range(MinEnemiesPerChunk, MaxEnemiesPerChunk + 1);
    }

    /// <summary>
    /// Determina si debe spawnear un item en esta posición
    /// </summary>
    public bool ShouldSpawnItem()
    {
        return UnityEngine.Random.value <= ItemSpawnRate;
    }

    /// <summary>
    /// Determina si debe generarse un gap en esta posición
    /// </summary>
    public bool ShouldCreateGap()
    {
        return UnityEngine.Random.value <= GapProbability;
    }

    /// <summary>
    /// Obtiene un tamaño aleatorio para un gap
    /// </summary>
    public float GetRandomGapSize()
    {
        return UnityEngine.Random.Range(MinGapSize, MaxGapSize);
    }

    /// <summary>
    /// Determina si debe cambiar la altura del terreno
    /// </summary>
    public bool ShouldChangeHeight()
    {
        return UnityEngine.Random.value <= HeightChangeProbability;
    }

    /// <summary>
    /// Obtiene un cambio de altura aleatorio
    /// </summary>
    public float GetRandomHeightChange()
    {
        float change = UnityEngine.Random.Range(-MaxHeightDecrease, MaxHeightIncrease);
        return change;
    }
}