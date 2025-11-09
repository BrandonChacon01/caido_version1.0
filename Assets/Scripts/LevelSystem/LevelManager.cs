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
                    GameObject go = new GameObject("LevelManager");
                    _instance = go.AddComponent<LevelManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Configuración de Niveles")]
        [Tooltip("Array con todas las configuraciones de niveles en orden")]
        public LevelConfiguration[] levelConfigurations;

        [Header("Estado Actual")]
        [SerializeField] private int currentLevelIndex = 0;
        [SerializeField] private bool isLoadingScene = false;

        public int CurrentLevelIndex => currentLevelIndex;
        public LevelConfiguration CurrentLevelConfig =>
            (currentLevelIndex >= 0 && currentLevelIndex < levelConfigurations.Length)
            ? levelConfigurations[currentLevelIndex]
            : null;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartGame()
        {
            currentLevelIndex = 0;
            LoadLevel(0);
        }

        public void LoadLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= levelConfigurations.Length)
            {
                UnityEngine.Debug.LogError($"[LevelManager] Índice de nivel inválido: {levelIndex}");
                return;
            }

            if (isLoadingScene)
            {
                UnityEngine.Debug.LogWarning("[LevelManager] Ya se está cargando una escena");
                return;
            }

            currentLevelIndex = levelIndex;
            LevelConfiguration config = levelConfigurations[levelIndex];

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

            if (config.isFinalLevel)
            {
                UnityEngine.Debug.Log("[LevelManager] ¡Nivel final completado! Cargando escena final");
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
            int nextIndex = currentLevelIndex + 1;

            if (nextIndex < levelConfigurations.Length)
            {
                LoadLevel(nextIndex);
            }
            else
            {
                UnityEngine.Debug.LogWarning("[LevelManager] No hay más niveles disponibles");
            }
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoadingScene = true;

            UnityEngine.AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
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