using UnityEngine;

public class PerroAI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player; 
    [SerializeField] private LayerMask groundLayer; 
    [SerializeField] private Transform groundCheck;
    private Rigidbody2D rb;
    private Animator anim;

    [Header("IA - Movimiento")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.5f; 

    [Header("IA - Salto")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float jumpInterval = 3f; 
    [SerializeField] private float checkRadius = 0.2f; 
    private float jumpTimer;
    private bool isGrounded;

    [Header("IA - Combate")]
    [SerializeField] private float contactDamage = 2f; 
    [SerializeField] private float knockbackForce = 8f; 

    private float currentMoveDirection = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Intenta encontrar al jugador autom�ticamente si no est� asignado
        if (player == null)
        {
            try
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }
            catch
            {
                Debug.LogError("�El Perro AI no pudo encontrar al Jugador! Aseg�rate de que el jugador tenga el Tag 'Player'.");
            }
        }
        jumpTimer = jumpInterval; // Inicia el temporizador de salto
    }

    void Update()
    {
        if (player == null) return; // Si no hay jugador, no hace nada

        // --- 1. Detectar Suelo ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        anim.SetBool("isGrounded", isGrounded); // Env�a info al Animator

        // --- 2. Decidir Direcci�n y Volteo ---
        Vector3 directionToPlayer = player.position - transform.position;

        if (directionToPlayer.x > 0.0f) // Jugador a la derecha
        {
            transform.localScale = new Vector3(1, 1, 1);
            currentMoveDirection = 1;
        }
        else // Jugador a la izquierda
        {
            transform.localScale = new Vector3(-1, 1, 1);
            currentMoveDirection = -1;
        }

        // --- 3. Decidir Movimiento ---
        float distance = Mathf.Abs(directionToPlayer.x);

        if (distance < stoppingDistance)
        {
            // Si est� muy cerca, se detiene
            currentMoveDirection = 0;
        }

        anim.SetBool("isMoving", currentMoveDirection != 0); // Env�a info al Animator

        // --- 4. L�gica de Salto ---
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0 && isGrounded)
        {
            Jump();
            jumpTimer = jumpInterval; // Reinicia el temporizador
        }
    }

    void FixedUpdate()
    {
        // Aplica el movimiento en FixedUpdate
        rb.linearVelocity = new Vector2(currentMoveDirection * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        // Aplica la fuerza de salto
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // --- 5. L�gica de Da�o por Contacto ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si choca con el jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement playerScript = collision.gameObject.GetComponent<PlayerMovement>();
            if (playerScript != null)
            {
                // 1. Aplica el da�o
                playerScript.Hit(contactDamage);

                // 2. Aplica empuje (knockback) AL JUGADOR
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    // Calcula la direcci�n lejos del perro
                    Vector2 knockbackDirection = (player.position - transform.position).normalized;
                    // Damos un peque�o empuj�n hacia arriba tambi�n
                    knockbackDirection.y = 0.5f;
                    playerRb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    // (Opcional) Dibuja el detector de suelo en el editor
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}