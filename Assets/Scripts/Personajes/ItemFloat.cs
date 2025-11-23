using UnityEngine;

public class ItemFloat : MonoBehaviour
{
    [Header("Configuración de Flotación")]
    [Tooltip("Qué tan alto y bajo se mueve el objeto")]
    [SerializeField] private float amplitud = 0.03f; 

    [Tooltip("Qué tan rápido se mueve")]
    [SerializeField] private float velocidad = 2f;  


    private Vector3 posInicial;

    private void Start()
    {
        // Guardamos la posición original donde colocaste el objeto en la escena
        posInicial = transform.position;
    }

    private void Update()
    {
        // --- Fórmula matemática para el movimiento suave ---
        // Mathf.Sin(Time.time) crea una onda que va de -1 a 1 suavemente.
        float nuevaY = posInicial.y + (Mathf.Sin(Time.time * velocidad) * amplitud);
        
        // Aplicamos la nueva posición manteniendo la X y Z originales
        transform.position = new Vector3(transform.position.x, nuevaY, transform.position.z);

    }
}