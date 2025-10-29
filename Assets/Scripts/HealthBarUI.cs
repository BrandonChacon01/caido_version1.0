using UnityEngine;
using UnityEngine.UI; 

public class HealthBarUI : MonoBehaviour
{
    // Esta será la barra que cambia de tamaño (la imagen verde/roja de relleno)
    [SerializeField] private RectTransform healthBarFill;

    private float fullWidth; // Guardaremos el ancho original de la barra aquí

    void Awake()
    {
        // Al iniciar, guardamos el ancho máximo original de la barra de relleno.
        // Así no necesitas ponerlo a mano en las variables "Ancho" y "Alto".
        fullWidth = healthBarFill.sizeDelta.x;
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        // Calculamos la proporción de vida (un valor entre 0.0 y 1.0)
        float ratio = currentHealth / maxHealth;

        // Actualizamos el tamaño de la barra de relleno basándonos en esa proporción
        healthBarFill.sizeDelta = new Vector2(fullWidth * ratio, healthBarFill.sizeDelta.y);
    }
}