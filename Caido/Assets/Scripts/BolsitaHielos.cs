using UnityEngine;

public class BolsitaHielos : MonoBehaviour
{
    // Cantidad de vida que este item va a recuperar.
    public int vidaQueRecupera = 1;

    // Esta función se ejecuta automáticamente cuando otro Collider2D entra en el trigger de este objeto.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Primero, comprobamos si el objeto que ha chocado tiene la etiqueta "Player".
        if (other.CompareTag("Player"))
        {
            Debug.Log("El jugador ha tocado los Hielos.");

            // Obtenemis el script PlayerMovement del objeto con el que hemos chocado.
            PlayerMovement player = other.GetComponent<PlayerMovement>();

            if (player != null)
            {
                // ...llamamos a su método Heal() y le pasamos la cantidad a curar.
                player.Heal(vidaQueRecupera);

                // Destruimos el objeto de la bolsa de hielos para que no se pueda usar más.
                Destroy(gameObject);
            }
        }
    }
}
