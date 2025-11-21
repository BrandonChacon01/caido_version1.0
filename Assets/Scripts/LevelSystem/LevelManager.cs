using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace LevelSystem
{
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager _instance;
        public static LevelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 1. PRIMERO intentamos buscar uno existente en la escena
                    // Esto evita crear uno nuevo vacío si ya pusiste uno en el editor
                    _instance = Object.FindFirstObjectByType<LevelManager>();

                    if (_instance == null)
                    {
                        // Solo si NO existe en absoluto, creamos uno nuevo
                        GameObject go = new GameObject("LevelManager");
                        _instance = go.AddComponent<LevelManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Configuración de Niveles")]
        [Tooltip("Array con todas las configuraciones de niveles en orden")]
        // 2. INICIALIZACIÓN: Asignamos un array vacío por defecto para evitar NullReferenceException si no se configura en el inspector
        public LevelConfiguration[] levelConfigurations = new LevelConfiguration[0];

        [Header("Estado Actual")]
        [SerializeField] private int currentLevelIndex = 0;
        [SerializeField] private bool isLoadingScene = false;

        public int CurrentLevelIndex => currentLevelIndex;

        /// <summary>
        /// Obtiene la configuración del nivel actual
        /// </summary>
        public LevelConfiguration CurrentLevelConfig
        {
            get
            {
                // Agregamos chequeo de nulidad extra por seguridad
                if (LevelRandomizer.Instance != null && LevelRandomizer.Instance.IsGenerated)
                {
                    return GenerateDynamicLevelConfig(currentLevelIndex);
                }

                // 3. PROTECCIÓN: Verificamos explícitamente si el array es nulo o está vacío
                if (levelConfigurations == null || levelConfigurations.Length == 0)
                {
                    // Retornamos null para que el LevelController lo maneje en lugar de crashear aquí
                    return null;
                }

                if (currentLevelIndex >= 0 && currentLevelIndex < levelConfigurations.Length)
                {
                    return levelConfigurations[currentLevelIndex];
                }

                return null;
            }
        }

        // ... El resto de tu código (GenerateDynamicLevelConfig, Awake, etc) se mantiene igual ...
        
        /// <summary>
        /// Genera una configuración de nivel dinámica basada en el nivel aleatorio seleccionado
        /// </summary>
        private LevelConfiguration GenerateDynamicLevelConfig(int levelIndex)
        {
            if (LevelRandomizer.Instance == null || !LevelRandomizer.Instance.IsGenerated)
            {
                return null;
            }

            // Crear una configuración temporal
            LevelConfiguration config = ScriptableObject.CreateInstance<LevelConfiguration>();

            config.levelNumber = levelIndex + 1;
            config.levelName = LevelRandomizer.Instance.GetLevelName(levelIndex);
            config.levelSceneName = LevelRandomizer.Instance.GetLevelScene(levelIndex);
            config.isFinalLevel = LevelRandomizer.Instance.IsLevelFinal(levelIndex);

            // Configurar el interludio siguiente (si no es el nivel final)
            if (!config.isFinalLevel && levelIndex < 4) // Hay 4 interludios (después de niveles 1-4)
            {
                config.nextInterludioSceneName = $"Interludio{levelIndex + 1}";
            }
            else if (config.isFinalLevel)
            {
                config.finalSceneName = "VictoryScreen"; // Asegúrate que coincida con tu Build Settings
            }

            // Configuración visual por defecto
            config.levelNameDisplayTime = 3f;
            config.levelNameColor = Color.white;

            return config;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Debug extra para verificar al inicio
            if (levelConfigurations == null) 
                levelConfigurations = new LevelConfiguration[0];
        }

        private void LoadRandomizedLevel(int levelIndex)
        {
            if (LevelRandomizer.Instance == null || !LevelRandomizer.Instance.IsGenerated)
            {
                UnityEngine.Debug.LogError("[LevelManager] No hay niveles aleatorios generados. Usando configuración tradicional.");
                LoadLevel(levelIndex);
                return;
            }

            string sceneName = LevelRandomizer.Instance.GetLevelScene(levelIndex);
            if (string.IsNullOrEmpty(sceneName))
            {
                UnityEngine.Debug.LogError($"[LevelManager] No se pudo obtener la escena para el nivel {levelIndex}");
                return;
            }

            UnityEngine.Debug.Log($"[LevelManager] Cargando nivel aleatorio {levelIndex + 1}: {sceneName}");
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        public void StartGame()
        {
            currentLevelIndex = 0;

            if (GameStatsManager.Instance != null)
            {
                GameStatsManager.Instance.StartTracking();
            }

            if (LevelRandomizer.Instance != null && LevelRandomizer.Instance.IsGenerated)
            {
                UnityEngine.Debug.Log("[LevelManager] Usando niveles generados aleatoriamente");
                LoadRandomizedLevel(0);
            }
            else
            {
                UnityEngine.Debug.Log("[LevelManager] Usando configuración de niveles tradicional");
                LoadLevel(0);
            }
        }

        public void LoadLevel(int levelIndex)
        {
            // Protección contra array nulo
            if (levelConfigurations == null || levelIndex < 0 || levelIndex >= levelConfigurations.Length)
            {
                UnityEngine.Debug.LogError($"[LevelManager] Índice de nivel inválido o configuración vacía: {levelIndex}");
                return;
            }

            if (isLoadingScene)
            {
                UnityEngine.Debug.LogWarning("[LevelManager] Ya se está cargando una escena");
                return;
            }

            currentLevelIndex = levelIndex;
            LevelConfiguration config = levelConfigurations[levelIndex];

            if (config == null)
            {
                UnityEngine.Debug.LogError($"[LevelManager] La configuración en el índice {levelIndex} es NULL");
                return;
            }

            if (string.IsNullOrEmpty(config.levelSceneName))
            {
                UnityEngine.Debug.LogError($"[LevelManager] Nombre de escena vacío en configuración {config.name}");
                return;
            }

            UnityEngine.Debug.Log($"[LevelManager] Cargando nivel {config.levelNumber}: {config.levelName} - Escena: {config.levelSceneName}");
            StartCoroutine(LoadSceneAsync(config.levelSceneName));
        }

        public void CompleteCurrentLevel()
        {
            if (isLoadingScene) return;

            LevelConfiguration config = CurrentLevelConfig;
            if (config == null)
            {
                UnityEngine.Debug.LogError("[LevelManager] No hay configuración de nivel actual");
                return;
            }

            UnityEngine.Debug.Log($"[LevelManager] Nivel completado: {config.levelName}");

            if (GameStatsManager.Instance != null)
            {
                GameStatsManager.Instance.RegisterLevelCompleted(config.levelNumber);
            }

            if (config.isFinalLevel)
            {
                UnityEngine.Debug.Log($"[LevelManager] ¡Nivel final completado! Cargando escena final: {config.finalSceneName}");

                if (string.IsNullOrEmpty(config.finalSceneName))
                {
                    UnityEngine.Debug.LogError("[LevelManager] ERROR: finalSceneName está vacío!");
                    return;
                }

                StartCoroutine(LoadSceneAsync(config.finalSceneName));
            }
            else
            {
                if (!string.IsNullOrEmpty(config.nextInterludioSceneName))
                {
                    UnityEngine.Debug.Log($"[LevelManager] Cargando interludio: {config.nextInterludioSceneName}");
                    StartCoroutine(LoadSceneAsync(config.nextInterludioSceneName));
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[LevelManager] No hay interludio configurado, pasando directamente al siguiente nivel");
                    LoadNextLevel();
                }
            }
        }

        public void LoadNextLevel()
        {
            currentLevelIndex++;

            if (LevelRandomizer.Instance != null && LevelRandomizer.Instance.IsGenerated)
            {
                if (currentLevelIndex < LevelRandomizer.Instance.GetTotalLevels())
                {
                    UnityEngine.Debug.Log($"[LevelManager] Avanzando al siguiente nivel aleatorio: {currentLevelIndex + 1}");
                    LoadRandomizedLevel(currentLevelIndex);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[LevelManager] No hay más niveles aleatorios disponibles");
                }
            }
            else
            {
                // Check de seguridad para el array
                if (levelConfigurations != null && currentLevelIndex < levelConfigurations.Length)
                {
                    UnityEngine.Debug.Log($"[LevelManager] Avanzando al siguiente nivel: {CurrentLevelConfig.levelName}");
                    LoadLevel(currentLevelIndex);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[LevelManager] No hay más niveles configurados o el array es nulo");
                }
            }
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoadingScene = true;

            UnityEngine.AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            if (asyncLoad == null)
            {
                 UnityEngine.Debug.LogError($"[LevelManager] Error fatal: No se pudo cargar la escena '{sceneName}'. ¿Está en el Build Settings?");
                 isLoadingScene = false;
                 yield break;
            }

            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            asyncLoad.allowSceneActivation = true;

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            isLoadingScene = false;
            UnityEngine.Debug.Log($"[LevelManager] Escena cargada: {sceneName}");
        }

        public void RestartCurrentLevel()
        {
            LoadLevel(currentLevelIndex);
        }

        public void ResetProgress()
        {
            currentLevelIndex = 0;
        }
    }
}