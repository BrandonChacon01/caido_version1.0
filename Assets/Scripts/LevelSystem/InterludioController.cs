using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

namespace LevelSystem
{
    public class InterludioController : MonoBehaviour
    {
        [Header("Referencias UI")]
        [Tooltip("Imagen que se mostrará en el interludio")]
        public UnityEngine.UI.Image interludioImage;

        [Tooltip("Texto de instrucciones (ej: 'Presiona ESPACIO para continuar')")]
        public TextMeshProUGUI instructionText;

        [Tooltip("Panel opcional de fondo/overlay")]
        public GameObject backgroundPanel;

        [Header("Audio")]
        [Tooltip("AudioSource para la música de fondo del interludio")]
        public AudioSource backgroundMusicSource;

        [Tooltip("Clip de audio que se reproducirá como música de fondo")]
        public AudioClip backgroundMusicClip;

        [Tooltip("Volumen de la música de fondo (0-1)")]
        [Range(0f, 1f)]
        public float musicVolume = 0.5f;

        [Tooltip("¿Reproducir la música en loop?")]
        public bool loopMusic = true;

        [Tooltip("Duración del fade in de la música (segundos)")]
        public float musicFadeInDuration = 1f;

        [Tooltip("Duración del fade out de la música al salir (segundos)")]
        public float musicFadeOutDuration = 0.5f;

        [Header("Configuración")]
        [Tooltip("Sprite/imagen específica para este interludio")]
        public Sprite interludioSprite;

        [Tooltip("Tecla que el jugador debe presionar para continuar")]
        public KeyCode continueKey = KeyCode.Space;

        [Tooltip("Tiempo mínimo antes de poder continuar (en segundos)")]
        [Range(0f, 5f)]
        public float minimumDisplayTime = 1f;

        [Tooltip("Texto de las instrucciones")]
        public string instructionsMessage = "Presiona ESPACIO para continuar";

        [Header("Efectos Visuales")]
        [Tooltip("Duración del fade in al inicio (segundos)")]
        public float fadeInDuration = 0.5f;

        [Tooltip("¿Hacer parpadear el texto de instrucciones?")]
        public bool blinkInstructionText = true;

        [Tooltip("Velocidad del parpadeo")]
        public float blinkSpeed = 1f;

        [Header("🔹 NUEVO: Configuración Final")]
        [Tooltip("¿Es este el interludio final del juego?")]
        public bool isFinalInterludio = false;

        [Tooltip("Escena a la que ir si es el interludio final (ej: MainMenu)")]
        public string finalSceneName = "MainMenu";

        private bool canContinue = false;
        private bool isTransitioning = false;

        private void Start()
        {
            InitializeInterludio();
        }

        private void Update()
        {
            // Verificar si el jugador presiona la tecla para continuar
            if (canContinue && !isTransitioning && Input.GetKeyDown(continueKey))
            {
                ContinueToNext();
            }
        }

        private void InitializeInterludio()
        {
            UnityEngine.Debug.Log($"[InterludioController] Inicializando interludio {(isFinalInterludio ? "(FINAL)" : "")}");

            // Configurar la imagen del interludio
            if (interludioImage != null && interludioSprite != null)
            {
                interludioImage.sprite = interludioSprite;
            }

            // Configurar el texto de instrucciones
            if (instructionText != null)
            {
                instructionText.text = instructionsMessage;

                if (blinkInstructionText)
                {
                    StartCoroutine(BlinkTextCoroutine());
                }
            }

            // Iniciar música de fondo
            InitializeBackgroundMusic();

            // Iniciar la secuencia de interludio
            StartCoroutine(InterludioSequence());
        }

        private void InitializeBackgroundMusic()
        {
            if (backgroundMusicSource == null)
            {
                UnityEngine.Debug.LogWarning("[InterludioController] No hay AudioSource asignado para la música de fondo");
                return;
            }

            if (backgroundMusicClip == null)
            {
                UnityEngine.Debug.LogWarning("[InterludioController] No hay AudioClip asignado para la música de fondo");
                return;
            }

            // Configurar el AudioSource
            backgroundMusicSource.clip = backgroundMusicClip;
            backgroundMusicSource.loop = loopMusic;
            backgroundMusicSource.volume = 0f; // Empezar en volumen 0 para el fade in

            // Reproducir música
            backgroundMusicSource.Play();

            // Iniciar fade in
            StartCoroutine(FadeInMusic(backgroundMusicSource, musicVolume, musicFadeInDuration));

            UnityEngine.Debug.Log($"[InterludioController] Música de fondo iniciada: {backgroundMusicClip.name}");
        }

        private IEnumerator FadeInMusic(AudioSource audioSource, float targetVolume, float duration)
        {
            float elapsedTime = 0f;
            float startVolume = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
                yield return null;
            }

            audioSource.volume = targetVolume;
        }

        private IEnumerator FadeOutMusic(AudioSource audioSource, float duration)
        {
            float elapsedTime = 0f;
            float startVolume = audioSource.volume;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
                yield return null;
            }

            audioSource.volume = 0f;
            audioSource.Stop();
        }

        private IEnumerator InterludioSequence()
        {
            // Fade in inicial
            if (interludioImage != null)
            {
                yield return StartCoroutine(FadeImage(interludioImage, 0f, 1f, fadeInDuration));
            }

            // Esperar el tiempo mínimo de visualización
            yield return new WaitForSeconds(minimumDisplayTime);

            // Permitir que el jugador continúe
            canContinue = true;
            UnityEngine.Debug.Log("[InterludioController] Listo para continuar - Presiona " + continueKey);
        }

        /// <summary>
        /// 🔹 MODIFICADO: Decide si cargar el siguiente nivel o ir al MainMenu
        /// </summary>
        private void ContinueToNext()
        {
            if (isTransitioning) return;

            isTransitioning = true;
            canContinue = false;

            if (isFinalInterludio)
            {
                UnityEngine.Debug.Log("[InterludioController] 🎉 Interludio FINAL - Volviendo al MainMenu");

                // Detener música con fade out antes de ir al MainMenu
                if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
                {
                    StartCoroutine(FadeOutAndLoadMainMenu());
                }
                else
                {
                    LoadMainMenu();
                }
            }
            else
            {
                UnityEngine.Debug.Log("[InterludioController] Continuando al siguiente nivel...");

                // Detener música con fade out antes de cargar el siguiente nivel
                if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
                {
                    StartCoroutine(FadeOutAndLoadNextLevel());
                }
                else
                {
                    LoadNextLevel();
                }
            }
        }

        /// <summary>
        /// 🔹 NUEVO: Fade out de música y cargar MainMenu
        /// </summary>
        private IEnumerator FadeOutAndLoadMainMenu()
        {
            // Hacer fade out de la música
            yield return StartCoroutine(FadeOutMusic(backgroundMusicSource, musicFadeOutDuration));

            // Cargar MainMenu
            LoadMainMenu();
        }

        /// <summary>
        /// 🔹 NUEVO: Cargar la escena del MainMenu
        /// </summary>
        private void LoadMainMenu()
        {
            // Limpiar datos de la run si es necesario
            if (GameStatsManager.Instance != null)
            {
                UnityEngine.Debug.Log("[InterludioController] Limpiando estadísticas de la run completada");
                // Opcional: Puedes mantener las stats o limpiarlas
                // GameStatsManager.Instance.ResetStats();
            }

            // 🔹 MODIFICADO: Limpiar payload manualmente
            LoadingPayload.NextScene = "";
            LoadingPayload.UseLevelSystem = false;

            UnityEngine.Debug.Log($"[InterludioController] 🏠 Cargando escena: {finalSceneName}");
            SceneManager.LoadScene(finalSceneName);
        }

        /// <summary>
        /// Fade out de música y cargar el siguiente nivel
        /// </summary>
        private IEnumerator FadeOutAndLoadNextLevel()
        {
            // Hacer fade out de la música
            yield return StartCoroutine(FadeOutMusic(backgroundMusicSource, musicFadeOutDuration));

            // Cargar el siguiente nivel
            LoadNextLevel();
        }

        /// <summary>
        /// Cargar el siguiente nivel usando el LevelManager
        /// </summary>
        private void LoadNextLevel()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadNextLevel();
            }
            else
            {
                UnityEngine.Debug.LogError("[InterludioController] LevelManager no encontrado. No se puede cargar el siguiente nivel.");
            }
        }

        private IEnumerator FadeImage(UnityEngine.UI.Image image, float startAlpha, float endAlpha, float duration)
        {
            float elapsedTime = 0f;
            Color color = image.color;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                color.a = alpha;
                image.color = color;
                yield return null;
            }

            color.a = endAlpha;
            image.color = color;
        }

        private IEnumerator BlinkTextCoroutine()
        {
            if (instructionText == null) yield break;

            while (true)
            {
                // Fade out
                yield return StartCoroutine(FadeText(instructionText, 1f, 0.3f, blinkSpeed * 0.5f));

                // Fade in
                yield return StartCoroutine(FadeText(instructionText, 0.3f, 1f, blinkSpeed * 0.5f));
            }
        }

        private IEnumerator FadeText(TextMeshProUGUI text, float startAlpha, float endAlpha, float duration)
        {
            float elapsedTime = 0f;
            Color color = text.color;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                color.a = alpha;
                text.color = color;
                yield return null;
            }

            color.a = endAlpha;
            text.color = color;
        }

#if UNITY_EDITOR
        [ContextMenu("Test - Simular Continuar")]
        private void TestContinue()
        {
            if (Application.isPlaying)
            {
                canContinue = true;
                ContinueToNext();
            }
            else
            {
                UnityEngine.Debug.LogWarning("Solo funciona en modo Play");
            }
        }
#endif
    }
}