using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform Player;

    [Header("Configuración de Cámara")]
    [SerializeField] private float leftBoundaryOffset = 8f;
    [SerializeField] private bool debugMode = true;

    private float maxReachedX;
    private PlayerController playerController;

    void Start()
    {
        maxReachedX = transform.position.x;

        if (Player != null)
        {
            playerController = Player.GetComponent<PlayerController>();

            if (playerController != null && debugMode)
            {
                UnityEngine.Debug.Log("✓ CameraScript: PlayerController encontrado correctamente");
            }
            else if (debugMode)
            {
                UnityEngine.Debug.LogWarning("✗ CameraScript: No se encontró PlayerController en " + Player.name);
            }
        }
        else if (debugMode)
        {
            UnityEngine.Debug.LogError("✗ CameraScript: No hay Player asignado en el Inspector!");
        }
    }

    void LateUpdate()
    {
        if (Player == null) return;

        maxReachedX = Mathf.Max(maxReachedX, Player.position.x);

        Vector3 position = transform.position;
        position.x = maxReachedX;
        transform.position = position;

        float leftBoundary = transform.position.x - leftBoundaryOffset;

        // +++ MODIFICADO: Solo comunicar el límite al jugador +++
        if (playerController != null)
        {
            playerController.SetLeftBoundary(leftBoundary);
        }

        // +++ MODIFICADO: SOLO forzar posición si es el JUGADOR +++
        if (Player.CompareTag("Player"))
        {
            Vector3 playerPos = Player.position;
            if (playerPos.x < leftBoundary)
            {
                playerPos.x = leftBoundary;
                Player.position = playerPos;

                Rigidbody2D rb = Player.GetComponent<Rigidbody2D>();
                if (rb != null && rb.linearVelocity.x < 0)
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }

                if (debugMode)
                {
                    UnityEngine.Debug.Log("⚠ Jugador forzado al límite izquierdo: " + leftBoundary);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Player == null) return;

        Gizmos.color = Color.red;
        float leftLimit = transform.position.x - leftBoundaryOffset;

        Gizmos.DrawLine(
            new Vector3(leftLimit, transform.position.y - 10f, 0),
            new Vector3(leftLimit, transform.position.y + 10f, 0)
        );

        Gizmos.DrawWireCube(
            new Vector3(leftLimit, transform.position.y, 0),
            new Vector3(0.5f, 20f, 1f)
        );
    }
}