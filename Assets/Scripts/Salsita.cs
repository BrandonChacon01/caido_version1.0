using UnityEngine;

public class Salsita : MonoBehaviour
{
    [Header("Configuración del Power-Up")]
    public float multiplicadorDeVelocidad = 1.5f;
    public float duracionDelPowerUp = 5.0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                player.ActivarPowerUpVelocidad(multiplicadorDeVelocidad, duracionDelPowerUp);
                Destroy(gameObject);
            }
        }
    }
}