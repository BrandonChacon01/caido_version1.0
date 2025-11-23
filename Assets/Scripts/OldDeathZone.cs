using UnityEngine;

public class OlDDeathZone : MonoBehaviour
{
    // Esta función se activa automáticamente cuando CUALQUIER
    // otro collider 2D (que tenga un Rigidbody2D) entra en este trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Revisamos si el objeto que entró es el JUGADOR
        // (Buscamos el nuevo script 'PlayerController')
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Hit(9999f);
            return;
        }

        CharacterStats enemy = other.GetComponent<CharacterStats>();
        if (enemy != null)
        {
            Destroy(other.gameObject);
            return; // Salimos de la función
        }

    }
}