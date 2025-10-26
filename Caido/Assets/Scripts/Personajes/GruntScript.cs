using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GruntScript : MonoBehaviour
{
    // --- Variables Públicas ---
    public Transform Player;
    public GameObject BulletPrefab;
    public float Health = 3f;
    public float Rate = 0.25f;
    public float Recoil = 2f;
    private float LastShoot;

    private float maxHealth; // Para guardar la vida inicial

    [Header("UI")]
    [SerializeField] private Slider healthSlider;

    // --- VARIABLES DE IA (SIMPLIFICADAS) ---
    [Header("IA de Movimiento")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 1.5f;

    // --- NUEVAS VARIABLES DE DETECCIÓN ---
    [Header("Detección de Entorno")]
    [SerializeField] private Transform ledgeCheck;     // Un objeto vacío al frente de los pies del enemigo
    [SerializeField] private float checkDistance = 0.2f; // Qué tan lejos disparar el rayo hacia abajo
    [SerializeField] private LayerMask groundLayer;    // La capa del suelo

    // --- Componentes y Control ---
    private Rigidbody2D rb;
    private float currentMoveDirection = 0f; // 0 = quieto, 1 = derecha, -1 = izquierda


    // Se llama una vez al inicio
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // --- Configuración de la Barra de Vida ---
        maxHealth = Health; // Guarda la vida máxima
        healthSlider.maxValue = maxHealth; // Configura el valor máximo del slider
        healthSlider.value = Health;       // Actualiza el slider al valor actual (lleno)
    }


    // Se llama en cada frame. Ideal para tomar decisiones.
    void Update()
    {
        if (Player == null) return;

        // --- 1. Decidir Dirección (Mirar al jugador) ---
        Vector3 directionToPlayer = Player.position - transform.position;

        if (directionToPlayer.x >= 0.0f)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
        else
        {
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }

        // --- 2. Decidir Movimiento (Basado en distancia) ---
        float distance = Mathf.Abs(Player.position.x - transform.position.x);

        // Si estamos MÁS LEJOS que la distancia de paro...
        if (distance > stoppingDistance)
        {
            // ...nos movemos hacia el jugador.
            // Mathf.Sign() nos da 1 si el jugador está a la derecha, -1 si está a la izquierda.
            currentMoveDirection = Mathf.Sign(directionToPlayer.x);
        }
        else
        {
            // Estamos DENTRO del rango: nos detenemos y disparamos
            currentMoveDirection = 0f;

            if (Time.time > LastShoot + Rate)
            {
                Shoot();
                LastShoot = Time.time;
            }
        }
    }
    private void Shoot()
    {
        Vector3 direction = new Vector3(transform.localScale.x, 0.0f, 0.0f);

        GameObject bullet = Instantiate(BulletPrefab, transform.position + direction * 0.1f, Quaternion.identity);
        bullet.GetComponent<BulletScript>().SetDirection(direction);

        // Aplicamos el retroceso (recoil) al enemigo
        rb.AddForce(-direction * Recoil, ForceMode2D.Impulse);
    }
    public void Hit(float damage)
    {
        Health -= damage;
        healthSlider.value = Health;
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.red; // Dibuja una línea roja
                                      // Dibuja la línea desde el 'ledgeCheck' hacia abajo
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + (Vector3.down * checkDistance));
        }
    }

    private void FixedUpdate()
    {
        // Por defecto, asumimos que nos moveremos si lo decidimos en Update()
        float actualMoveDirection = currentMoveDirection;

        // --- 3. Chequeo de Seguridad (¡No caerse!) ---

        // Solo revisamos el suelo si el enemigo tiene intención de moverse
        if (actualMoveDirection != 0f)
        {
            // Disparamos un rayo hacia ABAJO desde el 'ledgeCheck'
            bool isGroundedAhead = Physics2D.Raycast(ledgeCheck.position, Vector2.down, checkDistance, groundLayer);

            // Si NO hay suelo al frente... ¡DETENTE!
            if (!isGroundedAhead)
            {
                actualMoveDirection = 0f; // Cancelamos el movimiento
            }
        }

        // --- 4. Aplicar Movimiento ---
        // Aplicamos la velocidad (que será 0 si no hay suelo o si está en rango de disparo)
        rb.linearVelocity = new Vector2(actualMoveDirection * moveSpeed, rb.linearVelocity.y);
    }

}