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