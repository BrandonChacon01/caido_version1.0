using UnityEngine;

namespace LevelSystem
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Caido/Level Configuration")]
    public class LevelConfiguration : ScriptableObject
    {
        [Header("Información del Nivel")]
        [Tooltip("Nombre del nivel que se mostrará en pantalla")]
        public string levelName = "Nivel 1";

        [Tooltip("Número del nivel en la secuencia (1, 2, 3, 4, 5)")]
        [Range(1, 5)]
        public int levelNumber = 1;

        [Header("Escenas")]
        [Tooltip("Nombre de la escena del nivel (debe coincidir exactamente con el nombre en Build Settings)")]
        public string levelSceneName;

        [Tooltip("Nombre de la escena del interludio siguiente (dejar vacío si es el nivel final)")]
        public string nextInterludioSceneName;

        [Header("UI del Nivel")]
        [Tooltip("Tiempo que se muestra el nombre del nivel al inicio (en segundos)")]
        [Range(1f, 10f)]
        public float levelNameDisplayTime = 3f;

        [Tooltip("Color del texto del nombre del nivel")]
        public Color levelNameColor = Color.white;

        [Header("Configuración Específica")]
        [Tooltip("¿Es el nivel final?")]
        public bool isFinalLevel = false;

        [Tooltip("Escena que se carga al completar el nivel final (menu principal, créditos, etc.)")]
        public string finalSceneName = "MainMenu";
    }
}