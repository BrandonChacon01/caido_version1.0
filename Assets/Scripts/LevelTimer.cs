using UnityEngine;
using TMPro;
using System;

namespace LevelSystem
{
    /// <summary>
    /// Controlador del timer de nivel con cuenta regresiva
    /// </summary>
    public class LevelTimer : MonoBehaviour
    {
        [Header("Referencias UI")]
        [Tooltip("Texto donde se muestra el tiempo")]
        public TextMeshProUGUI timerText;

        [Header("Configuración")]
        [Tooltip("Tiempo máximo del nivel en segundos")]
        public float maxTime = 180f; // 3 minutos

        [Header("Colores de Advertencia")]
        [Tooltip("Color normal del timer")]
        public Color normalColor = Color.white;

        [Tooltip("Color cuando queda poco tiempo (< 30 seg)")]
        public Color warningColor = Color.yellow;

        [Tooltip("Color cuando queda muy poco tiempo (< 10 seg)")]
        public Color criticalColor = Color.red;

        [Header("Estado")]
        [SerializeField] private float currentTime;
        [SerializeField] private bool isRunning = false;
        [SerializeField] private bool isCompleted = false;

        // Evento que se dispara cuando se acaba el tiempo
        public event Action OnTimeUp;

        // Propiedades públicas
        public float CurrentTime => currentTime;
        public float TimeElapsed => maxTime - currentTime;
        public bool IsRunning => isRunning;
        public bool IsCompleted => isCompleted;

        private void Start()
        {
            // Inicializar el timer
            currentTime = maxTime;
            UpdateTimerDisplay();

            // Iniciar automáticamente
            StartTimer();
        }

        private void Update()
        {
            if (isRunning && !isCompleted)
            {
                // Decrementar el tiempo
                currentTime -= Time.deltaTime;

                // Verificar si se acabó el tiempo
                if (currentTime <= 0f)
                {
                    currentTime = 0f;
                    TimeUp();
                }

                // Actualizar el display
                UpdateTimerDisplay();
                UpdateTimerColor();
            }

            // 🔹 NUEVO: Actualizar display incluso cuando está pausado (para que se vea el tiempo congelado)
            else if (!isRunning && !isCompleted)
            {
                UpdateTimerDisplay();
                UpdateTimerColor();
            }
        }

        /// <summary>
        /// Inicia el timer
        /// </summary>
        public void StartTimer()
        {
            isRunning = true;
            UnityEngine.Debug.Log($"[LevelTimer] Timer iniciado: {maxTime} segundos");
        }

        /// <summary>
        /// Pausa el timer
        /// </summary>
        public void PauseTimer()
        {
            isRunning = false;
            UnityEngine.Debug.Log("[LevelTimer] Timer pausado");
        }

        /// <summary>
        /// Reanuda el timer
        /// </summary>
        public void ResumeTimer()
        {
            if (!isCompleted)
            {
                isRunning = true;
                UnityEngine.Debug.Log("[LevelTimer] Timer reanudado");
            }
        }

        /// <summary>
        /// Detiene el timer completamente (al completar el nivel)
        /// </summary>
        public void StopTimer()
        {
            isRunning = false;
            isCompleted = true;

            float timeUsed = maxTime - currentTime;
            UnityEngine.Debug.Log($"[LevelTimer] Timer detenido. Tiempo usado: {FormatTime(timeUsed)}");

            // Registrar el tiempo en el GameStatsManager
            if (GameStatsManager.Instance != null)
            {
                GameStatsManager.Instance.RegisterLevelTime(timeUsed); // ⬅️ AQUÍ se registra
            }
        }

        /// <summary>
        /// Resetea el timer a su valor máximo
        /// </summary>
        public void ResetTimer()
        {
            currentTime = maxTime;
            isRunning = false;
            isCompleted = false;
            UpdateTimerDisplay();
            UpdateTimerColor();
            UnityEngine.Debug.Log("[LevelTimer] Timer reseteado");
        }

        /// <summary>
        /// Se llama cuando se acaba el tiempo
        /// </summary>
        private void TimeUp()
        {
            isRunning = false;
            isCompleted = true;
            UnityEngine.Debug.Log("[LevelTimer] ¡Tiempo agotado!");

            // Disparar evento
            OnTimeUp?.Invoke();

            // TODO: Aquí puedes agregar lógica de Game Over o reinicio de nivel
            // Por ejemplo: GameManager.Instance.GameOver();
        }

        /// <summary>
        /// Actualiza el texto del timer
        /// </summary>
        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                timerText.text = FormatTime(currentTime);
            }
        }

        /// <summary>
        /// Actualiza el color del timer según el tiempo restante
        /// </summary>
        private void UpdateTimerColor()
        {
            if (timerText == null) return;

            if (currentTime <= 10f)
            {
                timerText.color = criticalColor;

                // Efecto de parpadeo cuando es crítico
                if (currentTime % 1f < 0.5f)
                {
                    timerText.color = new Color(criticalColor.r, criticalColor.g, criticalColor.b, 0.5f);
                }
            }
            else if (currentTime <= 30f)
            {
                timerText.color = warningColor;
            }
            else
            {
                timerText.color = normalColor;
            }
        }

        /// <summary>
        /// Formatea el tiempo en MM:SS
        /// </summary>
        private string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        /// <summary>
        /// Añade tiempo extra al timer (power-up o bonus)
        /// </summary>
        public void AddTime(float extraTime)
        {
            currentTime += extraTime;
            if (currentTime > maxTime) currentTime = maxTime;
            UnityEngine.Debug.Log($"[LevelTimer] +{extraTime} segundos añadidos");
        }

#if UNITY_EDITOR
        [ContextMenu("Debug - Añadir 30 segundos")]
        private void DebugAddTime()
        {
            AddTime(30f);
        }

        [ContextMenu("Debug - Reducir a 15 segundos")]
        private void DebugSetLowTime()
        {
            currentTime = 15f;
        }

        [ContextMenu("Debug - Reducir a 5 segundos")]
        private void DebugSetCriticalTime()
        {
            currentTime = 5f;
        }
#endif
    }
}