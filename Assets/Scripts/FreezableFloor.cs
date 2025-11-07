using UnityEngine;

public class FreezableFloor : MonoBehaviour
{
    [Header("Configuración de Hielo")]
    [Tooltip("El material físico que se aplicará cuando se congele.")]
    [SerializeField] private PhysicsMaterial2D slipperyMaterial;
    
    [Tooltip("El Tag del proyectil que activa el congelamiento.")]
    [SerializeField] private string projectileTag = "EnemyProjectile";

    [Tooltip("Arrastra aquí el Collider SÓLIDO que el jugador pisa (no el trigger).")]
    [SerializeField] private Collider2D solidCollider;

    [Header("Feedback Visual (Opcional)")]
    [Tooltip("Si tienes un SpriteRenderer, cambiará a este color al congelarse.")]
    [SerializeField] private Color frozenColor = new Color(0.5f, 1f, 1f, 1f); // Cian

    private bool isFrozen = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si ya está congelado, ignoramos
        if (isFrozen) return;

        // Si lo que entró tiene el tag correcto (ej. es un Taco)
        if (other.CompareTag(projectileTag))
        {
            Freeze();
        }
    }

    // Función pública por si quieres congelarlo desde otro script o evento
    public void Freeze()
    {
        isFrozen = true;

        // 1. Aplicar el material resbaladizo al collider sólido
        if (solidCollider != null && slipperyMaterial != null)
        {
            solidCollider.sharedMaterial = slipperyMaterial;

            // TRUCO CLAVE: Desactivar y reactivar el collider fuerza a Unity 
            // a actualizar el sistema de físicas inmediatamente.
            solidCollider.enabled = false;
            solidCollider.enabled = true;
        }
        else
        {
            Debug.LogWarning($"'{gameObject.name}' intentó congelarse pero le falta el SolidCollider o el SlipperyMaterial.");
        }

        // 2. Feedback visual (cambiar color del sprite si existe)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = frozenColor;
        }

        Debug.Log($"{gameObject.name} se ha congelado!");
    }
}