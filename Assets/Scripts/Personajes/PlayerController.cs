using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : CharacterStats
{
    // --- Variables de Movimiento y Disparo (Específicas del Jugador) ---
    public float JumpForce;
    public float Rate;

    // --- Variables de Daño por Contacto ---
    [SerializeField] private float damageOnContact = 1f;

    // --- NUEVO: Sistema de Munición ---
    [Header("Sistema de Munición")]
    public Slider ammoBar;          // --- NUEVO --- (Arrastra tu "heatBar" aquí)
    public int maxAmmo = 3;         // --- NUEVO ---
    public float reloadTime = 1.5f;   // --- NUEVO --- (Tiempo que tarda en recargar)

    private int currentAmmo;          // --- NUEVO ---
    private bool isReloading = false; // --- NUEVO ---

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

        // --- LÓGICA DE MUNICIÓN (MODIFICADA) ---
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

        // Si el jugador INTENTA disparar
        if (Input.GetKey(KeyCode.Space)) 
        {
            // Y no estamos recargando, y tenemos munición, y ha pasado el tiempo de cadencia
            if (!isReloading && currentAmmo > 0 && Time.time > LastShoot + Rate) // --- MODIFICADO ---
            {
                Shoot();
                LastShoot = Time.time;
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
}