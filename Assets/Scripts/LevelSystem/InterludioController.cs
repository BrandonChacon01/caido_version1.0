using UnityEngine;
using UnityEngine.UI;
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
                ContinueToNextLevel();
            }
        }

        private void InitializeInterludio()
        {
            UnityEngine.Debug.Log("[InterludioController] Inicializando interludio");

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

            // Iniciar la secuencia de interludio
            StartCoroutine(InterludioSequence());
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

        private void ContinueToNextLevel()
        {
            if (isTransitioning) return;

            isTransitioning = true;
            canContinue = false;

            UnityEngine.Debug.Log("[InterludioController] Continuando al siguiente nivel...");

            // Cargar el siguiente nivel a través del LevelManager
            LevelManager.Instance.LoadNextLevel();
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
                ContinueToNextLevel();
            }
            else
            {
                UnityEngine.Debug.LogWarning("Solo funciona en modo Play");
            }
        }
#endif
    }
}