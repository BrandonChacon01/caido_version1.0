using UnityEngine;

/// <summary>
/// CameraFollow - Hace que la cámara siga suavemente al jugador
/// Optimizado para pixel art con movimiento suave
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform del jugador a seguir")]
    public Transform Target;

    [Header("Configuración de Seguimiento")]
    [Tooltip("Suavidad del movimiento (menor = más suave)")]
    [Range(0.01f, 1f)]
    public float SmoothSpeed = 0.125f;

    [Header("Offset y Límites")]
    [Tooltip("Desplazamiento de la cámara respecto al jugador")]
    public Vector3 Offset = new Vector3(0f, 2f, -10f);

    [Tooltip("¿Limitar movimiento vertical?")]
    public bool LimitVerticalMovement = true;

    [Tooltip("Altura mínima de la cámara")]
    public float MinY = -5f;

    [Tooltip("Altura máxima de la cámara")]
    public float MaxY = 10f;

    [Tooltip("¿Limitar movimiento horizontal?")]
    public bool LimitHorizontalMovement = false;

    [Tooltip("Posición X mínima")]
    public float MinX = -50f;

    [Tooltip("Posición X máxima")]
    public float MaxX = 50f;

    private void LateUpdate()
    {
        if (Target == null)
        {
            // Intentar encontrar al jugador automáticamente
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Target = player.transform;
                UnityEngine.Debug.Log("[CameraFollow] Jugador encontrado automáticamente");
            }
            else
            {
                return; // No hay target, no hacer nada
            }
        }

        // Calcular posición deseada
        Vector3 desiredPosition = Target.position + Offset;

        // Aplicar límites si están activados
        if (LimitVerticalMovement)
        {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, MinY, MaxY);
        }

        if (LimitHorizontalMovement)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, MinX, MaxX);
        }

        // Mantener la Z de la cámara (profundidad)
        desiredPosition.z = transform.position.z;

        // Interpolar suavemente hacia la posición deseada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed);

        // Aplicar la nueva posición
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// Dibuja los límites de la cámara en el editor
    /// </summary>
    private void OnDrawGizmos()
    {
        if (LimitVerticalMovement || LimitHorizontalMovement)
        {
            Gizmos.color = Color.yellow;

            float left = LimitHorizontalMovement ? MinX : -1000f;
            float right = LimitHorizontalMovement ? MaxX : 1000f;
            float bottom = LimitVerticalMovement ? MinY : -1000f;
            float top = LimitVerticalMovement ? MaxY : 1000f;

            // Dibujar rectángulo de límites
            Vector3 bottomLeft = new Vector3(left, bottom, 0);
            Vector3 bottomRight = new Vector3(right, bottom, 0);
            Vector3 topLeft = new Vector3(left, top, 0);
            Vector3 topRight = new Vector3(right, top, 0);

            if (LimitVerticalMovement)
            {
                Gizmos.DrawLine(bottomLeft, bottomRight); // Línea inferior
                Gizmos.DrawLine(topLeft, topRight); // Línea superior
            }

            if (LimitHorizontalMovement)
            {
                Gizmos.DrawLine(bottomLeft, topLeft); // Línea izquierda
                Gizmos.DrawLine(bottomRight, topRight); // Línea derecha
            }
        }
    }
}