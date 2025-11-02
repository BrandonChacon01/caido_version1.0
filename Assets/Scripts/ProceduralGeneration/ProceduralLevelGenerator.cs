using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ProceduralLevelGenerator - Genera niveles 2D procedurales estilo Super Mario Bros
/// Crea plataformas, enemigos, items y la bandera de finalización
/// </summary>
public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Configuración del Nivel")]
    [Tooltip("Settings del nivel actual (arrastrar desde Project)")]
    public LevelSettings CurrentLevelSettings;

    [Header("Prefabs Requeridos")]
    [Tooltip("Prefab de la bandera de finalización")]
    public GameObject LevelEndFlagPrefab;

    [Header("Referencias de la Escena")]
    [Tooltip("Transform contenedor para las plataformas")]
    public Transform PlatformsContainer;

    [Tooltip("Transform contenedor para los enemigos")]
    public Transform EnemiesContainer;

    [Tooltip("Transform contenedor para los items")]
    public Transform ItemsContainer;

    [Header("Prefabs de Obstáculos")]
    [Tooltip("Prefab de obstáculos dañinos (picos, etc.)")]
    public GameObject ObstaclePrefab;

    [Tooltip("Probabilidad de spawner obstáculos (0-1)")]
    [Range(0f, 0.5f)]
    public float ObstacleSpawnRate = 0.15f;

    [Tooltip("Cámara principal (se ajustará el color de fondo)")]
    public Camera MainCamera;

    [Header("Estado de Generación")]
    [Tooltip("¿Generar el nivel automáticamente al Start?")]
    public bool GenerateOnStart = true;

    [Tooltip("Seed para generación (0 = aleatorio, >0 = reproducible)")]
    public int GenerationSeed = 0;

    // Variables internas
    private float currentXPosition = 0f;
    private float currentGroundHeight = 0f;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    private Transform ObstaclesContainer;


    private void Awake()
    {
        // Crear contenedores si no existen
        SetupContainers();
    }

    private void Start()
    {
        if (GenerateOnStart)
        {
            GenerateLevel();
        }
    }

    /// <summary>
    /// Crea los contenedores para organizar los objetos del nivel
    /// </summary>
    private void SetupContainers()
    {
        if (PlatformsContainer == null)
        {
            GameObject platformsObj = new GameObject("--- PLATFORMS ---");
            PlatformsContainer = platformsObj.transform;
            PlatformsContainer.SetParent(transform);
        }

        if (EnemiesContainer == null)
        {
            GameObject enemiesObj = new GameObject("--- ENEMIES ---");
            EnemiesContainer = enemiesObj.transform;
            EnemiesContainer.SetParent(transform);
        }

        if (ItemsContainer == null)
        {
            GameObject itemsObj = new GameObject("--- ITEMS ---");
            ItemsContainer = itemsObj.transform;
            ItemsContainer.SetParent(transform);
        }

        if (MainCamera == null)
        {
            MainCamera = Camera.main;
        }

        // Dentro del método SetupContainers(), después de crear ItemsContainer

        if (ObstaclesContainer == null)
        {
            GameObject obstaclesObj = new GameObject("--- OBSTACLES ---");
            ObstaclesContainer = obstaclesObj.transform;
            ObstaclesContainer.SetParent(transform);
        }
    }

    /// <summary>
    /// Genera el nivel completo
    /// </summary>
    public void GenerateLevel()
    {
        if (CurrentLevelSettings == null)
        {
            UnityEngine.Debug.LogError("[ProceduralLevelGenerator] No hay LevelSettings asignado. Asigna uno en el Inspector.");
            return;
        }

        UnityEngine.Debug.Log($"[ProceduralLevelGenerator] Generando {CurrentLevelSettings.LevelName}...");

        // Limpiar nivel anterior si existe
        ClearLevel();

        // Configurar seed si se especificó
        if (GenerationSeed > 0)
        {
            UnityEngine.Random.InitState(GenerationSeed);
        }

        // Configurar apariencia visual del nivel
        SetupLevelVisuals();

        // Inicializar variables
        currentXPosition = -10f; // Empezar un poco antes del jugador
        currentGroundHeight = CurrentLevelSettings.GroundHeight;

        // Generar chunks del nivel
        for (int i = 0; i < CurrentLevelSettings.LevelLengthInChunks; i++)
        {
            GenerateChunk(i);
        }

        // Generar bandera de finalización al final del nivel
        GenerateLevelEndFlag();

        UnityEngine.Debug.Log($"[ProceduralLevelGenerator] Nivel generado exitosamente. Total objetos: {spawnedObjects.Count}");
    }

    /// <summary>
    /// Configura los aspectos visuales del nivel
    /// </summary>
    private void SetupLevelVisuals()
    {
        // CAMBIAR ESTO: Configurar color de fondo de la cámara
        if (MainCamera != null)
        {
            MainCamera.backgroundColor = CurrentLevelSettings.BackgroundColor;
        }

        // CAMBIAR ESTO: Aquí puedes agregar lógica para cambiar sprites de fondo
        // Ejemplo: backgroundSpriteRenderer.sprite = CurrentLevelSettings.BackgroundSprite;
    }

    /// <summary>
    /// Genera un chunk (segmento) del nivel
    /// </summary>
    private void GenerateChunk(int chunkIndex)
    {
        float chunkStartX = currentXPosition;

        // Decidir si cambiar la altura del terreno
        if (CurrentLevelSettings.ShouldChangeHeight() && chunkIndex > 2)
        {
            float heightChange = CurrentLevelSettings.GetRandomHeightChange();
            currentGroundHeight += heightChange;
            // Limitar la altura para no salirse de los límites razonables
            currentGroundHeight = Mathf.Clamp(currentGroundHeight, -5f, 2f);
        }

        // Decidir si crear un gap (hueco)
        bool hasGap = CurrentLevelSettings.ShouldCreateGap() && chunkIndex > 1;
        float gapSize = 0f;

        if (hasGap)
        {
            gapSize = CurrentLevelSettings.GetRandomGapSize();
            // No generar plataformas en el gap
            currentXPosition += gapSize;
        }
        else
        {
            // Generar plataformas para este chunk
            GeneratePlatforms(chunkStartX, CurrentLevelSettings.ChunkWidth, currentGroundHeight);
            currentXPosition += CurrentLevelSettings.ChunkWidth;
        }

        // Generar enemigos en este chunk (no en gaps)
        if (!hasGap)
        {
            GenerateEnemiesForChunk(chunkStartX, CurrentLevelSettings.ChunkWidth, currentGroundHeight);
        }

        // Generar items en este chunk
        if (!hasGap && CurrentLevelSettings.ShouldSpawnItem())
        {
            GenerateItemForChunk(chunkStartX, CurrentLevelSettings.ChunkWidth, currentGroundHeight);
        }

        // Generar obstáculos en este chunk (no en gaps)
        if (!hasGap && UnityEngine.Random.value <= ObstacleSpawnRate)
        {
            GenerateObstacleForChunk(chunkStartX, CurrentLevelSettings.ChunkWidth, currentGroundHeight);
        }
    }

    /// <summary>
    /// Genera las plataformas de suelo para un chunk
    /// </summary>
    private void GeneratePlatforms(float startX, float width, float height)
    {
        if (CurrentLevelSettings.GroundPrefab == null)
        {
            UnityEngine.Debug.LogWarning("[ProceduralLevelGenerator] No hay GroundPrefab asignado en LevelSettings");
            return;
        }

        // Calcular cuántos bloques necesitamos (asumiendo 1x1 por bloque)
        int blockCount = Mathf.CeilToInt(width);

        for (int i = 0; i < blockCount; i++)
        {
            Vector3 position = new Vector3(startX + i, height, 0f);
            GameObject platform = Instantiate(CurrentLevelSettings.GroundPrefab, position, Quaternion.identity, PlatformsContainer);

            // CAMBIAR ESTO: Aplicar color a las plataformas
            SpriteRenderer sr = platform.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = CurrentLevelSettings.PlatformColor;
            }

            spawnedObjects.Add(platform);
        }
    }

    /// <summary>
    /// Genera enemigos para un chunk
    /// </summary>
    private void GenerateEnemiesForChunk(float startX, float width, float groundHeight)
    {
        int enemyCount = CurrentLevelSettings.GetEnemyCountForChunk();

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemyPrefab = CurrentLevelSettings.GetRandomEnemy();
            if (enemyPrefab == null) continue;

            // Posición aleatoria dentro del chunk, encima del suelo
            float randomX = UnityEngine.Random.Range(startX + 1f, startX + width - 1f);
            float spawnHeight = groundHeight + 1.5f; // Spawn encima del suelo
            Vector3 spawnPosition = new Vector3(randomX, spawnHeight, 0f);

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, EnemiesContainer);
            spawnedObjects.Add(enemy);
        }
    }

    /// <summary>
    /// Genera un item/powerup para un chunk
    /// </summary>
    private void GenerateItemForChunk(float startX, float width, float groundHeight)
    {
        GameObject itemPrefab = CurrentLevelSettings.GetRandomItem();
        if (itemPrefab == null) return;

        // Posición aleatoria dentro del chunk
        float randomX = UnityEngine.Random.Range(startX + 2f, startX + width - 2f);
        float itemHeight = groundHeight + 2.5f; // Spawn flotando sobre el suelo
        Vector3 spawnPosition = new Vector3(randomX, itemHeight, 0f);

        GameObject item = Instantiate(itemPrefab, spawnPosition, Quaternion.identity, ItemsContainer);
        spawnedObjects.Add(item);
    }

    /// <summary>
    /// Genera la bandera de finalización al final del nivel CON plataforma
    /// </summary>
    private void GenerateLevelEndFlag()
    {
        if (LevelEndFlagPrefab == null)
        {
            UnityEngine.Debug.LogError("[ProceduralLevelGenerator] No hay LevelEndFlagPrefab asignado");
            return;
        }

        // Asegurar que hay suelo debajo de la bandera
        // Generar plataforma final de 5 bloques
        float flagPlatformWidth = 5f;
        GeneratePlatforms(currentXPosition, flagPlatformWidth, currentGroundHeight);

        // Posición de la bandera en el centro de la plataforma final
        Vector3 flagPosition = new Vector3(currentXPosition + (flagPlatformWidth / 2f), currentGroundHeight + 1f, 0f);
        GameObject flag = Instantiate(LevelEndFlagPrefab, flagPosition, Quaternion.identity, transform);
        spawnedObjects.Add(flag);

        UnityEngine.Debug.Log($"[ProceduralLevelGenerator] Bandera de finalización colocada en X: {flagPosition.x} con plataforma de {flagPlatformWidth} bloques");
    }

    /// <summary>
    /// Limpia todos los objetos generados del nivel
    /// </summary>
    public void ClearLevel()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();

        UnityEngine.Debug.Log("[ProceduralLevelGenerator] Nivel limpiado");
    }

    /// <summary>
    /// Regenera el nivel (útil para testing)
    /// </summary>
    [ContextMenu("Regenerar Nivel")]
    public void RegenerateLevel()
    {
        GenerateLevel();
    }

    /// <summary>
    /// Dibuja información del nivel en el editor
    /// </summary>
    private void OnDrawGizmos()
    {
        if (CurrentLevelSettings == null) return;

        // Dibujar línea que representa la longitud total del nivel
        Gizmos.color = Color.green;
        float totalLength = CurrentLevelSettings.LevelLengthInChunks * CurrentLevelSettings.ChunkWidth;
        Vector3 start = new Vector3(-10f, CurrentLevelSettings.GroundHeight, 0f);
        Vector3 end = new Vector3(-10f + totalLength, CurrentLevelSettings.GroundHeight, 0f);
        Gizmos.DrawLine(start, end);
    }

    /// <summary>
    /// Genera obstáculos para un chunk
    /// </summary>
    private void GenerateObstacleForChunk(float startX, float width, float groundHeight)
    {
        if (ObstaclePrefab == null) return;

        // Generar 1-2 obstáculos por chunk
        int obstacleCount = UnityEngine.Random.Range(1, 3);

        for (int i = 0; i < obstacleCount; i++)
        {
            // Posición aleatoria dentro del chunk
            float randomX = UnityEngine.Random.Range(startX + 1f, startX + width - 1f);
            float obstacleHeight = groundHeight + 0.5f; // Justo encima del suelo
            Vector3 spawnPosition = new Vector3(randomX, obstacleHeight, 0f);

            GameObject obstacle = Instantiate(ObstaclePrefab, spawnPosition, Quaternion.identity, ObstaclesContainer);
            spawnedObjects.Add(obstacle);
        }
    }
}