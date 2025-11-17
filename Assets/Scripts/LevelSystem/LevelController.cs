using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace LevelSystem
{
    public class LevelController : MonoBehaviour
    {
        [Header("Referencias UI")]
        [Tooltip("Referencia al objeto Canvas que contiene el UI del nombre del nivel")]
        public GameObject levelNameUICanvas;

        [Tooltip("Texto donde se mostrará el nombre del nivel (TextMeshPro)")]
        public TextMeshProUGUI levelNameText;

        [Header("Audio")]
        [Tooltip("AudioSource para la música de fondo del nivel")]
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

        [Header("Configuración Manual (Opcional)")]
        [Tooltip("Si está activado, usa la configuración manual en lugar del LevelManager")]
        public bool useManualConfig = false;

        [Tooltip("Configuración manual del nivel (solo si useManualConfig está activado)")]
        public LevelConfiguration manualLevelConfig;

        private LevelConfiguration currentConfig;

        private void Start()
        {
            InitializeLevel();
        }

        private void InitializeLevel()
        {
            // Obtener la configuración del nivel
            if (useManualConfig && manualLevelConfig != null)
            {
                currentConfig = manualLevelConfig;
                UnityEngine.Debug.Log($"[LevelController] Usando configuración manual: {currentConfig.levelName}");
            }
            else
            {
                currentConfig = LevelManager.Instance.CurrentLevelConfig;

                if (currentConfig == null)
                {
                    UnityEngine.Debug.LogError("[LevelController] No se pudo obtener la configuración del nivel actual desde LevelManager");
                    return;
                }
            }

            UnityEngine.Debug.Log($"[LevelController] Inicializando nivel: {currentConfig.levelName} (Número: {currentConfig.levelNumber})");

            // Iniciar música de fondo
            InitializeBackgroundMusic();

            // Mostrar el nombre del nivel
            if (levelNameUICanvas != null && levelNameText != null)
            {
                StartCoroutine(ShowLevelNameCoroutine());
            }
            else
            {
                UnityEngine.Debug.LogWarning("[LevelController] No hay referencias UI configuradas para mostrar el nombre del nivel");
            }
        }

        private void InitializeBackgroundMusic()
        {
            if (backgroundMusicSource == null)
            {
                UnityEngine.Debug.LogWarning("[LevelController] No hay AudioSource asignado para la música de fondo");
                return;
            }

            if (backgroundMusicClip == null)
            {
                UnityEngine.Debug.LogWarning("[LevelController] No hay AudioClip asignado para la música de fondo");
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

            UnityEngine.Debug.Log($"[LevelController] Música de fondo iniciada: {backgroundMusicClip.name}");
        }

        private IEnumerator FadeInMusic(AudioSource audioSource, float targetVolume, float duration)
        {
            float elapsedTime = 0f;
            float startVolume = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
                yield return null;
            }

            audioSource.volume = targetVolume;
        }

        private IEnumerator ShowLevelNameCoroutine()
        {
            // Activar el canvas
            levelNameUICanvas.SetActive(true);

            // Establecer el nombre del nivel
            levelNameText.text = currentConfig.levelName;
            levelNameText.color = currentConfig.levelNameColor;

            // Animación de aparición (fade in) - usando tiempo real
            yield return StartCoroutine(FadeTextRealtime(levelNameText, 0f, 1f, 0.5f));

            // Usar WaitForSecondsRealtime en lugar de WaitForSeconds
            yield return new WaitForSecondsRealtime(currentConfig.levelNameDisplayTime);

            // Animación de desaparición (fade out) - usando tiempo real
            yield return StartCoroutine(FadeTextRealtime(levelNameText, 1f, 0f, 0.5f));

            // Desactivar el canvas completamente
            levelNameUICanvas.SetActive(false);
        }

        /// <summary>
        /// Fade de texto usando tiempo real (no afectado por Time.timeScale)
        /// </summary>
        private IEnumerator FadeTextRealtime(TextMeshProUGUI text, float startAlpha, float endAlpha, float duration)
        {
            float elapsedTime = 0f;
            Color color = text.color;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime; // Usar tiempo real
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                color.a = alpha;
                text.color = color;
                yield return null;
            }

            // Asegurar que termina en el alpha final
            color.a = endAlpha;
            text.color = color;
        }

        // MANTÉN el método FadeText original por si acaso (opcional, puedes eliminarlo)
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

        /// <summary>
        /// Método público para obtener la configuración actual del nivel
        /// </summary>
        public LevelConfiguration GetCurrentConfig()
        {
            return currentConfig;
        }

        /// <summary>
        /// Método público para detener la música de fondo con fade out
        /// </summary>
        public void StopBackgroundMusic(float fadeOutDuration = 1f)
        {
            if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
            {
                StartCoroutine(FadeOutMusic(backgroundMusicSource, fadeOutDuration));
            }
        }

        private IEnumerator FadeOutMusic(AudioSource audioSource, float duration)
        {
            float elapsedTime = 0f;
            float startVolume = audioSource.volume;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
                yield return null;
            }

            audioSource.volume = 0f;
            audioSource.Stop();
        }

#if UNITY_EDITOR
        [ContextMenu("Debug - Mostrar Info del Nivel")]
        private void DebugShowLevelInfo()
        {
            if (currentConfig != null)
            {
                UnityEngine.Debug.Log($"=== LEVEL INFO ===");
                UnityEngine.Debug.Log($"Nombre: {currentConfig.levelName}");
                UnityEngine.Debug.Log($"Número: {currentConfig.levelNumber}");
                UnityEngine.Debug.Log($"Escena: {currentConfig.levelSceneName}");
                UnityEngine.Debug.Log($"Es Final: {currentConfig.isFinalLevel}");
                UnityEngine.Debug.Log($"=================");
            }
            else
            {
                UnityEngine.Debug.LogWarning("No hay configuración cargada");
            }
        }
#endif
    }
}