using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float Speed = 0.6f;
    public float Damage = 1f;
    public AudioClip Sound;

    private Rigidbody2D rb2d;
    private SpriteRenderer spriteRenderer;
    private Vector2 direction;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (Sound != null && Camera.main != null)
        {
            AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(Sound);
            }
        }
    }

    private void FixedUpdate()
    {
        rb2d.linearVelocity = direction * Speed;
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;

        if (direction.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (direction.x > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    public void DestroyBullet()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CholitoAI cholito = other.GetComponent<CholitoAI>();
        if (cholito != null)
        {
            cholito.Hit(Damage);
            DestroyBullet();
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Hit(Damage); 
            DestroyBullet();
            return;
        }

        PerroAI perro = other.GetComponent<PerroAI>();
        if (perro != null)
        {
            perro.Hit(Damage);
            DestroyBullet();
            return;
        }

        VecinoAI vecino = other.GetComponent<VecinoAI>();
        if (vecino != null)
        {
            vecino.Hit(Damage);
            DestroyBullet();
            return;
        }

        if (!other.isTrigger) { DestroyBullet(); }
    }
}