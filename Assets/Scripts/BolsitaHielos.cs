using UnityEngine;

public class BolsitaHielos : MonoBehaviour
{
    public int vidaQueRecupera = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("El jugador ha tocado los Hielos.");
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                player.Heal(vidaQueRecupera);
                Destroy(gameObject);
            }
        }
    }
}
