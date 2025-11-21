using UnityEngine;

public class AutoScrollCamera : MonoBehaviour
{
    [Header("Configuración de Scroll")]
    [Tooltip("Velocidad a la que la cámara avanza automáticamente.")]
    [SerializeField] private float scrollSpeed = 2f;

    [Tooltip("Si es true, el jugador morirá si toca el borde izquierdo (fuera de cámara).")]
    [SerializeField] private bool killPlayerOnLeftEdge = true;
    
    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        // 1. Mover la cámara hacia la derecha constantemente
        transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);

        // 2. (Opcional) Matar al jugador si se queda atrás
        if (killPlayerOnLeftEdge && player != null)
        {
            // Calcular el borde izquierdo de la cámara
            float leftEdge = transform.position.x - (Camera.main.orthographicSize * Camera.main.aspect);
            
            // Si el jugador está a la izquierda de ese borde...
            if (player.position.x < leftEdge - 1f) // -1 de margen
            {
                // ...muere.
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null) pc.Hit(9999);
            }
        }
    }
}