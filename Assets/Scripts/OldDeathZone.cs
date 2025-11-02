using UnityEngine;

public class OldDeathZone : MonoBehaviour
{
    // Esta función se activa automáticamente cuando CUALQUIER
    // otro collider 2D (que tenga un Rigidbody2D) entra en este trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Revisamos si el objeto que entró es el JUGADOR
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            // ¡Es el jugador!
            // En lugar de destruirlo, llamamos a su método Hit()
            // con un número de daño muy alto para matarlo al instante.
            // Esto asegura que se muestre la pantalla de Game Over.
            player.Hit(9999f);
            return; // Salimos de la función
        }

        // 2. Revisamos si el objeto que entró es un ENEMIGO
        GruntScript enemy = other.GetComponent<GruntScript>();
        if (enemy != null)
        {
            // Es un enemigo. No necesitamos mostrar Game Over,
            // así que simplemente lo destruimos.
            Destroy(other.gameObject);
            return; // Salimos de la función
        }

        // 3. (Opcional) Revisamos si es una BALA
        BulletScript bullet = other.GetComponent<BulletScript>();
        if (bullet != null)
        {
            // Destruimos la bala para que no se acumulen
            Destroy(other.gameObject);
        }
    }
}