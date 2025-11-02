using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// DeathZone - Zona que mata al jugador si cae fuera del nivel
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class DeathZone : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Panel de Game Over (arrastra desde la Hierarchy)")]
    public GameObject GameOverPanel;

    [Tooltip("¿Destruir al jugador al morir?")]
    public bool DestroyPlayerOnDeath = false;

    private BoxCollider2D triggerCollider;

    private void Awake()
    {
        // Configurar el collider
        triggerCollider = GetComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;

        // Ocultar el panel al inicio
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si es el jugador
        if (other.CompareTag("Player"))
        {
            PlayerDied(other.gameObject);
        }
    }

    /// <summary>
    /// Maneja la muerte del jugador
    /// </summary>
    private void PlayerDied(GameObject player)
    {
        UnityEngine.Debug.Log("[DeathZone] ¡Jugador cayó al vacío!");

        // Mostrar panel de Game Over
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(true);
            Time.timeScale = 0f; // Pausar el juego
        }
        else
        {
            // Si no hay panel, volver al menú directamente
            UnityEngine.Debug.LogWarning("[DeathZone] No hay GameOverPanel asignado. Volviendo al menú...");
            SceneManager.LoadScene("MainMenu");
        }

        // Opcional: Destruir al jugador
        if (DestroyPlayerOnDeath)
        {
            Destroy(player);
        }
    }

    /// <summary>
    /// Dibuja la zona de muerte en el editor
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Rojo semi-transparente
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.DrawCube(transform.position, new Vector3(col.size.x, col.size.y, 1f));
        }
    }
}