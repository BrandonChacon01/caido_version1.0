using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : CharacterStats
{
    // --- Variables de Movimiento y Disparo (Espec�ficas del Jugador) ---
    public float JumpForce;
    public float Rate;

    // --- Variables de Da�o y Empuje por Contacto ---
    [SerializeField] private float damageOnContact = 1f;

    [Header("Sistema de Munici�n")]
    public Slider ammoBar;
    public int maxAmmo = 3;
    public float reloadTime = 1.5f;

    private int currentAmmo;
    private bool isReloading = false;

    [Header("Ataque Melee")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private float meleeAttackDamage = 2f;
    [SerializeField] private float meleeAttackCooldown = 1f;
    [SerializeField] private float kickAnimationDuration = 0.3f;
    private bool isKicking = false;

    // --- Referencias a Componentes (Espec�ficas del Jugador) ---
    public HealthBarUI healthBar;
    public GameObject BulletPrefab;

    // --- Variables Privadas de Control ---
    private float Horizontal;
    private bool Grounded;
    private float LastShoot;

    private bool isTrapped = false; // Bandera para saber si está atrapado

    // --- Variables de Power-Up ---
    private float velocidadOriginal;
    private bool conPowerUpVelocidad = false;

    [Header("Sistema de Derretimiento")]
    [SerializeField] private float meltRate = 0.5f;
    [SerializeField] private float minMeltScale = 0.3f;

    private int facingDirection = 1;


    protected override void Awake()
    {
        base.Awake();
    }


    protected override void Start()
    {
        base.Start();

        velocidadOriginal = moveSpeed;

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        currentAmmo = maxAmmo;
        if (ammoBar != null)
        {
            ammoBar.maxValue = maxAmmo;
            ammoBar.value = currentAmmo;
        }
    }

    private void Update()
    {
        // --- MODIFICADO ---
        // Si está atrapado O pateando, no procesa inputs de movimiento/disparo
        if (isTrapped || isKicking)
        {
            // Si está atrapado, forza la detención horizontal
            if (isTrapped)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return; // No procesa inputs de movimiento/disparo
        }

        // --- LÓGICA DE INPUTS (Movimiento) ---
        Horizontal = Input.GetAxisRaw("Horizontal");

        if (Horizontal < 0.0f) facingDirection = -1;
        else if (Horizontal > 0.0f) facingDirection = 1;

        // No actualiza la anim de correr si está pateando (la corrutina lo maneja)
        if (!isKicking)
        {
            anim.SetBool("running", Horizontal != 0.0f);
        }

        // --- LÓGICA DE INPUTS (Acciones) ---
        if (Physics2D.Raycast(transform.position, Vector3.down, 0.1f))
        {
            Grounded = true;
        }
        else Grounded = false;

        if (Input.GetKeyDown(KeyCode.W) && Grounded)
        {
            Jump();
        }

        // Añadido chequeo de !isKicking
        if (Input.GetKey(KeyCode.Space))
        {
            if (!isReloading && !isKicking && currentAmmo > 0 && Time.time > LastShoot + Rate)
            {
                Shoot();
                LastShoot = Time.time;
            }
        }

        // --- MODIFICADO ---
        // Ahora llama a la Corrutina y solo revisa si no está recargando
        if (Input.GetKeyDown(KeyCode.F) && !isReloading)
        {
            StartCoroutine(MeleeAttackRoutine());
        }

        HandleMelting();
    }

    private void LateUpdate()
    {
        if (isKicking) return;
        UpdateVisualMelt();
    }

    private void FixedUpdate()
    {
        // Si está atrapado o pateando, detenemos la velocidad X
        if (isTrapped || isKicking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(Horizontal * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.AddForce(Vector2.up * JumpForce);
    }

    private void Shoot()
    {
        Vector3 direction = new Vector3(facingDirection, 0.0f, 0.0f);
        GameObject bullet = Instantiate(BulletPrefab, transform.position + direction * 0.1f, Quaternion.identity);
        bullet.GetComponent<BulletScript>().SetDirection(direction);

        currentAmmo--;
        UpdateAmmoBar();
    }

    private void MeleeAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.CompareTag("Player"))
            {
                continue;
            }
            // Busca la clase base 'CharacterStats' para dañar a CUALQUIER enemigo
            CharacterStats enemy = enemyCollider.GetComponent<CharacterStats>();
            if (enemy != null)
            {
                // Llama al método TakeDamage de la base (que ya maneja la vida y la muerte)
                enemy.TakeDamage(meleeAttackDamage);
            }
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Recargando...");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        UpdateAmmoBar();
        isReloading = false;
        Debug.Log("�Recarga completa!");
    }

    private void UpdateAmmoBar()
    {
        if (ammoBar != null)
        {
            ammoBar.value = currentAmmo;
        }
    }

    public void Hit(float damage)
    {
        base.TakeDamage(damage);

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    protected override void Die()
    {
        Debug.Log("El jugador ha muerto.");
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.MostrarPanelGameOver();
        }
        Destroy(gameObject);
    }

    public void Heal(int amount)
    {
        base.Heal(amount);

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    public void ActivarPowerUpVelocidad(float multiplicadorVelocidad, float duracion)
    {
        if (!conPowerUpVelocidad)
        {
            StartCoroutine(PowerUpVelocidadCoroutine(multiplicadorVelocidad, duracion));
        }
    }

    private IEnumerator PowerUpVelocidadCoroutine(float multiplicador, float tiempo)
    {
        conPowerUpVelocidad = true;
        moveSpeed *= multiplicador;
        Debug.Log("�Power-up de velocidad activado! Velocidad actual: " + moveSpeed);
        yield return new WaitForSeconds(tiempo);
        moveSpeed = velocidadOriginal;
        conPowerUpVelocidad = false;
        Debug.Log("Power-up de velocidad terminado. Velocidad restaurada a: " + moveSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            float collisionAngle = Vector2.Dot(contact.normal, Vector2.up);

            if (collisionAngle > 0.7f)
            {
                Debug.Log("Aterrizaste sobre el enemigo.");
            }
            else
            {
                Debug.Log("Choque lateral con enemigo.");
                Hit(damageOnContact);
            }
        }
    }

    private void HandleMelting()
    {
        if (currentHealth > 0)
        {
            float meltDamage = meltRate * Time.deltaTime;
            TakeDamage(meltDamage);

            if (healthBar != null)
            {
                healthBar.UpdateHealthBar(currentHealth, maxHealth);
            }
        }
    }

    private void UpdateVisualMelt()
    {
        float healthPercent = currentHealth / maxHealth;
        float targetScale = Mathf.Lerp(minMeltScale, 1.0f, healthPercent);
        transform.localScale = new Vector3(targetScale * facingDirection, 1.0f, 1.0f);
    }

    // --- NUEVO M�TODO ---
    // Dibuja el radio de ataque melee en el editor
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

    // --- NUEVO MÉTODO: Recarga manual ---
    // Este método será llamado por el objeto de munición
    public void RechargeAmmo(int amount)
    {
        // Sumamos la cantidad
        currentAmmo += amount;

        // Nos aseguramos de no pasarnos del máximo
        if (currentAmmo > maxAmmo)
        {
            currentAmmo = maxAmmo;
        }

        // Actualizamos la barra de UI
        UpdateAmmoBar();

        Debug.Log("¡Munición recargada! Actual: " + currentAmmo);
    }

    // Función pública para que las trampas la llamen
    public void Immobilize(float duration)
    {
        // Solo activamos si no está ya atrapado (para evitar conflictos)
        if (!isTrapped)
        {
            StartCoroutine(ImmobilizeRoutine(duration));
        }
    }

    private IEnumerator ImmobilizeRoutine(float duration)
    {
        isTrapped = true;

        // --- NUEVA LÍNEA CLAVE ---
        // Esto mata cualquier movimiento que llevara al instante.
        rb.linearVelocity = Vector2.zero;
        // (Nota: Si usas una versión antigua de Unity, usa 'rb.velocity' en lugar de 'rb.linearVelocity')

        // Opcional: También reseteamos la animación a Idle
        anim.SetBool("running", false);

        Debug.Log("¡Jugador atrapado y detenido!");

        yield return new WaitForSeconds(duration);

        isTrapped = false;
        Debug.Log("¡Jugador liberado!");
    }

    private IEnumerator MeleeAttackRoutine()
    {
        // 1. Inicia el estado de patada
        isKicking = true;
        anim.SetBool("isKicking", true); // <-- Envía el Booleano al Animator
        anim.SetBool("running", false); // Detiene la animación de correr

        // 2. Ejecuta la lógica de daño (OverlapCircle)
        MeleeAttack(); 
        
        // 3. Espera a que termine la animación
        yield return new WaitForSeconds(kickAnimationDuration); 

        // 4. Termina la animación
        anim.SetBool("isKicking", false);

        // 5. Espera el resto del cooldown
        // (Ej: 1s Cooldown - 0.3s Anim = 0.7s de espera)
        yield return new WaitForSeconds(meleeAttackCooldown - kickAnimationDuration); 

        // 6. Permite patear de nuevo
        isKicking = false; 
    }
}

