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
        // Reproducir el sonido al inicio
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
        // Mueve la bala usando la velocidad del Rigidbody2D
        rb2d.linearVelocity = direction * Speed;
    }

    // El m�todo que establece la direcci�n y voltea el sprite
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized; // Normalizamos para asegurar una velocidad constante

        // --- L�GICA PARA VOLTEAR EL SPRITE ---
        // Si la direcci�n en X es negativa (se mueve a la izquierda)
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
        // Intentamos obtener los componentes del objeto con el que colisionamos
        GruntScript grunt = other.GetComponent<GruntScript>();
        if (grunt != null)
        {
            grunt.Hit(Damage);
            DestroyBullet(); 
            return; 
        }

        PlayerMovement Player = other.GetComponent<PlayerMovement>();
        if (Player != null)
        {
            Player.Hit(Damage);
            DestroyBullet();
            return;
        }
        if (!other.isTrigger) { DestroyBullet(); }
    }
}