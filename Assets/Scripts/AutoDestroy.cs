using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    // Ajusta este tiempo para que sea un poquito más largo que tu animación
    public float delay = 1f;

    void Start()
    {
        Destroy(gameObject, delay);
    }
}