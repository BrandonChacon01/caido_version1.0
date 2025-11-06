using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : CharacterStats
{
    // --- Variables de Movimiento y Disparo (Específicas del Jugador) ---
    public float JumpForce;
    public float Rate;

    // --- Variables de Daño y Empuje por Contacto ---
    [SerializeField] private float damageOnContact = 1f;

    [Header("Sistema de Munición")]
    public Slider ammoBar;
    public int maxAmmo = 3;
    public float reloadTime = 1.5f;

    private int currentAmmo;
    private bool isReloading = false;

    [Header("Ataque Melee")]
    [SerializeField] private Transform attackPoint; 
    [SerializeField] private float attackRadius = 0.5f; 
    [SerializeField] private float meleeAttackDamage = 2f; 
    [SerializeField] private float meleeAttackRate = 1f; 
    private float lastMeleeAttack;

    // --- Referencias a Componentes (Específicas del Jugador) ---
    public HealthBarUI healthBar;
    public GameObject BulletPrefab;

    // --- Variables Privadas de Control ---
    private float Horizontal;
    private bool Grounded;
    private float LastShoot;

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
        Horizontal = Input.GetAxisRaw("Horizontal");

        if (Horizontal < 0.0f) facingDirection = -1;
        else if (Horizontal > 0.0f) facingDirection = 1;

        anim.SetBool("running", Horizontal != 0.0f);

        if (Physics2D.Raycast(transform.position, Vector3.down, 0.1f))
        {
            Grounded = true;
        }
        else Grounded = false;

        if (Input.GetKeyDown(KeyCode.W) && Grounded)
        {
            Jump();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (!isReloading && currentAmmo > 0 && Time.time > LastShoot + Rate)
            {
                Shoot();
                LastShoot = Time.time;
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && Time.time > lastMeleeAttack + meleeAttackRate)
        {
            lastMeleeAttack = Time.time;
            MeleeAttack();
            // (Opcional) Activar animación de patada
            // anim.SetTrigger("Kick"); 
        }

        HandleMelting();
    }

    private void LateUpdate()
    {
        UpdateVisualMelt();
    }

    private void FixedUpdate()
    {
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

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    private void MeleeAttack()
    {
        // 1. Detecta todos los colliders en un círculo
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);

        // 2. Recorre todos los colliders golpeados
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            // 3. Comprueba si es un Cholito
            CholitoAI cholito = enemyCollider.GetComponent<CholitoAI>();
            if (cholito != null)
            {
                cholito.Hit(meleeAttackDamage);
                continue; // Pasa al siguiente objeto
            }

            // 4. Comprueba si es un Perro
            PerroAI perro = enemyCollider.GetComponent<PerroAI>();
            if (perro != null)
            {
                perro.Hit(meleeAttackDamage);
                continue;
            }

            // 5. Comprueba si es un Albañil
            AlbanilAI albanil = enemyCollider.GetComponent<AlbanilAI>();
            if (albanil != null)
            {
                albanil.Hit(meleeAttackDamage);
                continue;
            }

            // 6. Comprueba si es un Vecino
            VecinoAI vecino = enemyCollider.GetComponent<VecinoAI>();
            if (vecino != null)
            {
                vecino.Hit(meleeAttackDamage);
                continue;
            }

            // 7. Comprueba si es un Taquero
            TaqueroAI taquero = enemyCollider.GetComponent<TaqueroAI>();
            if (taquero != null)
            {
                taquero.Hit(meleeAttackDamage);
                continue;
            }

            // 8. Comprueba si es un Elotero
            EloteroAI elotero = enemyCollider.GetComponent<EloteroAI>();
            if (elotero != null)
            {
                elotero.Hit(meleeAttackDamage);
                continue;
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
        Debug.Log("¡Recarga completa!");
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
        Debug.Log("¡Power-up de velocidad activado! Velocidad actual: " + moveSpeed);
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

    // --- NUEVO MÉTODO ---
    // Dibuja el radio de ataque melee en el editor
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}