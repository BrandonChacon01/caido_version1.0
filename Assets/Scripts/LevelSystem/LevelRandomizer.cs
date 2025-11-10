using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random; // Especificar UnityEngine.Random

namespace LevelSystem
{
    /// <summary>
    /// Sistema de selección aleatoria de niveles
    /// Genera una secuencia aleatoria de niveles y crea una semilla única
    /// </summary>
    public class LevelRandomizer : MonoBehaviour
    {
        // ... resto del código
        private static LevelRandomizer _instance;
        public static LevelRandomizer Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("LevelRandomizer");
                    _instance = go.AddComponent<LevelRandomizer>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Configuración de Niveles")]
        [Tooltip("Lista de todos los niveles disponibles por categoría")]
        public List<LevelCategory> levelCategories = new List<LevelCategory>();

        [Header("Estado Actual")]
        [SerializeField] private List<string> selectedLevelScenes = new List<string>();
        [SerializeField] private string generatedSeed = "";
        [SerializeField] private bool isGenerated = false;

        // Propiedades públicas
        public List<string> SelectedLevelScenes => selectedLevelScenes;
        public string GeneratedSeed => generatedSeed;
        public bool IsGenerated => isGenerated;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Inicializar las categorías si están vacías
            if (levelCategories.Count == 0)
            {
                InitializeDefaultCategories();
            }
        }

        /// <summary>
        /// Inicializa las categorías de niveles por defecto
        /// </summary>
        private void InitializeDefaultCategories()
        {
            levelCategories = new List<LevelCategory>
            {
                new LevelCategory
                {
                    categoryName = "Level1",
                    levelNumber = 1,
                    availableScenes = new List<string> { "N1_A", "N1_B" }
                },
                new LevelCategory
                {
                    categoryName = "Level2",
                    levelNumber = 2,
                    availableScenes = new List<string> { "N2_A", "N2_B" }
                },
                new LevelCategory
                {
                    categoryName = "Level3",
                    levelNumber = 3,
                    availableScenes = new List<string> { "N3_A", "N3_B" }
                },
                new LevelCategory
                {
                    categoryName = "Level4",
                    levelNumber = 4,
                    availableScenes = new List<string> { "N4_A", "N4_B" }
                },
                new LevelCategory
                {
                    categoryName = "Level5",
                    levelNumber = 5,
                    availableScenes = new List<string> { "N5" },
                    isFinalLevel = true
                }
            };

            UnityEngine.Debug.Log("[LevelRandomizer] Categorías de niveles inicializadas por defecto");
        }

        /// <summary>
        /// Genera una selección aleatoria de niveles
        /// </summary>
        public void GenerateRandomLevelSequence()
        {
            selectedLevelScenes.Clear();
            List<string> seedParts = new List<string>();

            UnityEngine.Debug.Log("[LevelRandomizer] Generando secuencia aleatoria de niveles...");

            foreach (LevelCategory category in levelCategories)
            {
                if (category.availableScenes == null || category.availableScenes.Count == 0)
                {
                    UnityEngine.Debug.LogError($"[LevelRandomizer] La categoría {category.categoryName} no tiene escenas disponibles");
                    continue;
                }

                // Seleccionar una escena aleatoria de esta categoría
                string selectedScene = category.availableScenes[Random.Range(0, category.availableScenes.Count)];
                selectedLevelScenes.Add(selectedScene);

                // Extraer la variante (A, B, etc.) para la semilla
                string variant = ExtractVariant(selectedScene);
                seedParts.Add($"{category.levelNumber}{variant}");

                UnityEngine.Debug.Log($"[LevelRandomizer] Nivel {category.levelNumber}: {selectedScene} (Variante: {variant})");
            }

            // Generar la semilla
            generatedSeed = string.Join("", seedParts);
            isGenerated = true;

            UnityEngine.Debug.Log($"[LevelRandomizer] ✅ Secuencia generada. Semilla: {generatedSeed}");

            // Guardar la semilla en el GameStatsManager
            if (GameStatsManager.Instance != null)
            {
                GameStatsManager.Instance.SetLevelSeed(generatedSeed);
            }
        }

        /// <summary>
        /// Extrae la variante del nombre de la escena (A, B, etc.)
        /// </summary>
        private string ExtractVariant(string sceneName)
        {
            // Buscar la última letra mayúscula
            for (int i = sceneName.Length - 1; i >= 0; i--)
            {
                if (char.IsUpper(sceneName[i]) && sceneName[i] != 'N')
                {
                    return sceneName[i].ToString();
                }
            }
            return ""; // Para niveles únicos como N5
        }

        /// <summary>
        /// Obtiene la escena de un nivel específico
        /// </summary>
        public string GetLevelScene(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < selectedLevelScenes.Count)
            {
                return selectedLevelScenes[levelIndex];
            }

            UnityEngine.Debug.LogError($"[LevelRandomizer] Índice de nivel fuera de rango: {levelIndex}");
            return null;
        }

        /// <summary>
        /// Obtiene el nombre genérico del nivel
        /// </summary>
        public string GetLevelName(int levelIndex)
        {
            return $"Nivel {levelIndex + 1}";
        }

        /// <summary>
        /// Obtiene el número total de niveles
        /// </summary>
        public int GetTotalLevels()
        {
            return selectedLevelScenes.Count;
        }

        /// <summary>
        /// Verifica si un nivel es el final
        /// </summary>
        public bool IsLevelFinal(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < levelCategories.Count)
            {
                return levelCategories[levelIndex].isFinalLevel;
            }
            return false;
        }

        /// <summary>
        /// Resetea la selección de niveles
        /// </summary>
        public void ResetSelection()
        {
            selectedLevelScenes.Clear();
            generatedSeed = "";
            isGenerated = false;
            UnityEngine.Debug.Log("[LevelRandomizer] Selección de niveles reseteada");
        }

#if UNITY_EDITOR
        [ContextMenu("Debug - Generar Secuencia Aleatoria")]
        private void DebugGenerateSequence()
        {
            GenerateRandomLevelSequence();
        }

        [ContextMenu("Debug - Mostrar Selección Actual")]
        private void DebugShowSelection()
        {
            if (!isGenerated)
            {
                UnityEngine.Debug.LogWarning("[LevelRandomizer] No se ha generado ninguna secuencia aún");
                return;
            }

            UnityEngine.Debug.Log("=== SELECCIÓN DE NIVELES ===");
            UnityEngine.Debug.Log($"Semilla: {generatedSeed}");
            for (int i = 0; i < selectedLevelScenes.Count; i++)
            {
                UnityEngine.Debug.Log($"  Nivel {i + 1}: {selectedLevelScenes[i]}");
            }
            UnityEngine.Debug.Log("============================");
        }

        [ContextMenu("Debug - Resetear Selección")]
        private void DebugResetSelection()
        {
            ResetSelection();
        }
#endif
    }

    /// <summary>
    /// Categoría de niveles con sus variantes disponibles
    /// </summary>
    [System.Serializable]
    public class LevelCategory
    {
        [Tooltip("Nombre de la categoría (ej: Level1, Level2)")]
        public string categoryName;

        [Tooltip("Número del nivel (1, 2, 3, etc.)")]
        public int levelNumber;

        [Tooltip("Escenas disponibles para este nivel (ej: N1_A, N1_B)")]
        public List<string> availableScenes;

        [Tooltip("¿Es el nivel final?")]
        public bool isFinalLevel = false;
    }
}