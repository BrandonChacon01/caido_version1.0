using UnityEngine;

public class SlashDamage : MonoBehaviour
{
    public float damage;
    public float lifeTime = 0.3f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Hit(damage);
            }
            GetComponent<Collider2D>().enabled = false;
        }
    }
}