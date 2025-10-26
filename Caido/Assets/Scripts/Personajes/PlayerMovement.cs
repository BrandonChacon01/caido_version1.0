using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // --- Variables de Movimiento y Disparo ---
    public float Speed;
    public float JumpForce;
    public float Rate;
    public float Recoil;

    // --- Variables de Daño y Empuje por Contacto ---
    [SerializeField] private float damageOnContact = 1f;  
    [SerializeField] private float knockbackForce = 10f; 

    // --- Variables de Vida ---
    public float maxHealth = 5f; // Vida máxima, ahora pública para ajustarla si quieres
    private float currentHealth; // Vida actual del jugador

    [Header("Sistema de Sobrecalentamiento")]
    public Slider heatBar; 
    public float maxHeat = 100f;   
    public float heatPerShot = 20f;  
    public float cooldownRate = 30f;
    public int enfriamientoMax = 1;

    [SerializeField] private Image heatFillImage;
    private float currentHeat = 0f;
    private bool isOverheated = false;

    // --- Referencias a Componentes ---
    public HealthBarUI healthBar; 
    private Rigidbody2D Rigidbody2D;
    private Animator Animator;
    public GameObject BulletPrefab;

    // --- Variables Privadas de Control ---
    private float Horizontal;
    private bool Grounded;
    private float LastShoot;

    // --- Variables de Power-Up ---
    private float velocidadOriginal; // Para guardar la velocidad normal del jugador
    private bool conPowerUpVelocidad = false; // Para saber si ya tenemos el power-up activo

    [Header("Sistema de Derretimiento")]
    [SerializeField] private float meltRate = 0.5f; // Puntos de vida perdidos por segundo
    [SerializeField] private float minMeltScale = 0.3f; // La escala X/Y mínima antes de morir

    private int facingDirection = 1; // 1 = derecha, -1 = izquierda

    private void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();

        velocidadOriginal = Speed;

        // Inicializamos la vida del jugador al empezar
        currentHealth = maxHealth;

        // Nos aseguramos de que la barra de vida sepa cuál es la vida inicial
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
        // Inicializamos la barra de sobrecalentamiento
        if (heatBar != null)
        {
            heatBar.maxValue = maxHeat;
            heatBar.value = 0;
        }
    }

    private void Update()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");

        // --- LÓGICA DE MOVIMIENTO Y DIRECCIÓN ---
        if (Horizontal < 0.0f) facingDirection = -1;
        else if (Horizontal > 0.0f) facingDirection = 1;
        Animator.SetBool("running", Horizontal != 0.0f);

        // Detectar Suelo
        if (Physics2D.Raycast(transform.position, Vector3.down, 0.1f))
        {
            Grounded = true;
        }
        else Grounded = false;

        // Salto
        if (Input.GetKeyDown(KeyCode.W) && Grounded)
        {
            Jump();
        }

        bool isTryingToShoot = Input.GetKey(KeyCode.Space);

        // 1. Si el jugador INTENTA disparar
        if (isTryingToShoot && !isOverheated && Time.time > LastShoot + Rate)
        {
            Shoot();
            LastShoot = Time.time;
            currentHeat += heatPerShot; // Añadir calor
            if (currentHeat >= maxHeat)
            {
                currentHeat = maxHeat;
                isOverheated = true;
                Debug.Log("¡Pistola Sobrecalentada!");
            }
        }
        if ((!isTryingToShoot && currentHeat > 0) || (isOverheated && currentHeat > 0))
        {
            currentHeat -= cooldownRate * Time.deltaTime; 
            if (isOverheated && currentHeat <= enfriamientoMax)
            {
                currentHeat = enfriamientoMax;
                isOverheated = false;
                Debug.Log("¡Pistola lista!");
            }
            // Asegurarse de no pasar de 0 en el enfriamiento normal
            else if (currentHeat < 0)
            {
                currentHeat = 0;
            }
        }
        if (heatBar != null)
        {
            heatBar.value = currentHeat;
            if (heatFillImage != null)
            {
                if (currentHeat > 70)
                {
                    heatFillImage.color = Color.red; 
                }
                else
                {
                    heatFillImage.color = new Color(1.0f, 0.647f, 0.0f); ; 
                }
            }
        }

        HandleMelting();
    }

    private void LateUpdate()
    {
        UpdateVisualMelt();
    }
    private void FixedUpdate()
    {
        Rigidbody2D.linearVelocity = new Vector2(Horizontal * Speed, Rigidbody2D.linearVelocity.y);
    }

    private void Jump()
    {
        Rigidbody2D.AddForce(Vector2.up * JumpForce);
    }

    private void Shoot()
    {
        Vector3 direction = new Vector3(facingDirection, 0.0f, 0.0f);
        GameObject bullet = Instantiate(BulletPrefab, transform.position + direction * 0.1f, Quaternion.identity);
        bullet.GetComponent<BulletScript>().SetDirection(direction);
        Rigidbody2D.AddForce(-direction * Recoil, ForceMode2D.Impulse);
    }

    public void Hit(float damage)
    {
        currentHealth -= damage;
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        // Si la vida llega a cero o menos...
        if (currentHealth <= 0)
        {
            currentHealth = 0; // Asegurarse de que no sea negativo
            Die();
        }
    }

    // Método público para recuperar vida
    public void Heal(int amount)
    {
        // Añadimos la cantidad de vida recuperada
        currentHealth += amount;
        Debug.Log("Vida recuperada. Vida actual: " + currentHealth);

        // Nos aseguramos de que la vida no supere el máximo
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // Actualizamos la barra de vida en la UI
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    // Método público que el item "salsita" llamará
    public void ActivarPowerUpVelocidad(float multiplicadorVelocidad, float duracion)
    {
        // Si no tenemos ya un power-up de velocidad activo, iniciamos la corrutina
        if (!conPowerUpVelocidad)
        {
            StartCoroutine(PowerUpVelocidadCoroutine(multiplicadorVelocidad, duracion));
        }
    }

    // La Corrutina que actúa como el temporizador del power-up
    private IEnumerator PowerUpVelocidadCoroutine(float multiplicador, float tiempo)
    {
        // 1. Marcamos que el power-up está activo
        conPowerUpVelocidad = true;

        // 2. Aumentamos la velocidad
        Speed *= multiplicador; // Multiplica la velocidad actual. Ej: si es 5 y el multiplicador es 2, ahora será 10.
        Debug.Log("¡Power-up de velocidad activado! Velocidad actual: " + Speed);

        // 3. Pausamos la ejecución de esta función durante el tiempo especificado
        yield return new WaitForSeconds(tiempo);

        // 4. Pasado el tiempo, restauramos la velocidad original
        Speed = velocidadOriginal;
        conPowerUpVelocidad = false; // Marcamos que el power-up ya no está activo
        Debug.Log("Power-up de velocidad terminado. Velocidad restaurada a: " + Speed);
    }

    private void Die()
    {
        // Solo ejecuta esto una vez
        if (currentHealth <= 0)
        {
            Debug.Log("El jugador ha muerto.");

            UIManager uiManager = FindFirstObjectByType<UIManager>();

            if (uiManager != null)
            {
                uiManager.MostrarPanelGameOver();
            }

            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Comprobamos si el objeto con el que chocamos tiene la etiqueta "Enemy"
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // --- NUEVA LÓGICA DE DIRECCIÓN ---

            // 1. Obtenemos el primer punto de contacto de la colisión
            ContactPoint2D contact = collision.GetContact(0);

            
            float collisionAngle = Vector2.Dot(contact.normal, Vector2.up);

            // 3. Comprobamos el ángulo.
            // Usamos 0.7f como un umbral (cercano a 1).
            // Si es mayor, significa que aterrizamos "bastante" encima de él.
            if (collisionAngle > 0.7f)
            {
                // ----- CASO 1: ATERRIZAMOS ENCIMA -----
                Debug.Log("Aterrizaste sobre el enemigo.");

                // (Opcional) ¡Podemos matar al enemigo! (Estilo Mario)
                // collision.gameObject.GetComponent<GruntScript>().Hit(100f); 

                // (Opcional) ¡Podemos hacer un pequeño rebote!
                // Rigidbody2D.AddForce(Vector2.up * 5f, ForceMode2D.Impulse); 
            }
            else
            {
                // ----- CASO 2: CHOCAMOS POR EL LADO O DESDE ABAJO -----
                Debug.Log("Choque lateral con enemigo.");

                // Aplicar Daño
                Hit(damageOnContact);

                // Aplicar Empuje (Knockback)
                Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
                Rigidbody2D.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    private void HandleMelting()
    {
        // Solo nos derretimos si estamos vivos
        if (currentHealth > 0)
        {
            // Aplicar daño por derretimiento basado en el tiempo
            currentHealth -= meltRate * Time.deltaTime;

            // Actualizar la barra de vida
            if (healthBar != null)
            {
                healthBar.UpdateHealthBar(currentHealth, maxHealth);
            }

            // Comprobar si acabamos de morir por derretimiento
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }
    }

    private void UpdateVisualMelt()
    {
        // 1. Calcular el porcentaje de vida (un valor entre 0.0 y 1.0)
        float healthPercent = currentHealth / maxHealth;

        // 2. Mapear ese porcentaje a nuestra escala deseada
        // Mathf.Lerp(valor_min, valor_max, porcentaje)
        float targetScale = Mathf.Lerp(minMeltScale, 1.0f, healthPercent);

        // 3. Aplicar la escala
        transform.localScale = new Vector3(targetScale * facingDirection, 1.0f, 1.0f);
    }
}
