using UnityEngine;

public class PositionCameraWall : MonoBehaviour
{
    private Camera mainCamera;
    
    [Tooltip("Un pequeño colchón para que el muro esté fuera de la pantalla, no justo en el borde.")]
    [SerializeField] private float horizontalOffset = 0.5f;

    void Start()
    {
        mainCamera = GetComponentInParent<Camera>();

        if (mainCamera == null)
        {
            Debug.LogError("¡PositionCameraWall no pudo encontrar una Cámara en su padre!");
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // 1. Calcula la mitad del ancho de la vista de la cámara
        float cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;

        // 2. Establece nuestra POSICIÓN LOCAL (relativa a la cámara)
        transform.localPosition = new Vector3(-cameraHalfWidth - horizontalOffset, 0, 0);
    }
}