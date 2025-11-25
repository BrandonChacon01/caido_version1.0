using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : CharacterStats
{
    public float JumpForce;
    public float Rate;

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
    [SerializeField] private float meleeAttackCooldown = 1f;
    [SerializeField] private float kickAnimationDuration = 0.3f;
    private bool isKicking = false;

    public HealthBarUI healthBar;
    public GameObject BulletPrefab;

    private float Horizontal;
    private bool Grounded;
    private float LastShoot;
    private bool isTrapped = false;

    private float velocidadOriginal;
    private Coroutine activeSpeedPowerUp = null;

    private bool isInvincible = false;
    private Coroutine activeSunscreenPowerUp = null;
    private Color colorOriginal;

    [Header("Sistema de Derretimiento")]
    [SerializeField] private float meltRate = 0.5f;
    [SerializeField] private float minMeltScale = 0.3f;

    private int facingDirection = 1;
    private SpriteRenderer spriteRenderer;

    private float currentLeftBoundary = float.NegativeInfinity;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        velocidadOriginal = moveSpeed;
        if (spriteRenderer != null) colorOriginal = spriteRenderer.color;

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
        if (isTrapped || isKicking)
        {
            if (isTrapped)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        Horizontal = Input.GetAxisRaw("Horizontal");

        if (Horizontal < 0.0f) facingDirection = -1;
        else if (Horizontal > 0.0f) facingDirection = 1;

        if (!isKicking)
        {
            anim.SetBool("running", Horizontal != 0.0f);
        }

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
            if (!isReloading && !isKicking && currentAmmo > 0 && Time.time > LastShoot + Rate)
            {
                Shoot();
                LastShoot = Time.time;
            }
        }

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
        if (isTrapped || isKicking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(Horizontal * moveSpeed, rb.linearVelocity.y);

        if (currentLeftBoundary != float.NegativeInfinity)
        {
            Vector3 pos = transform.position;

            if (pos.x < currentLeftBoundary)
            {
                pos.x = currentLeftBoundary;
                transform.position = pos;

                if (rb.linearVelocity.x < 0)
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }
            }
        }
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
            if (enemyCollider.CompareTag("Player")) continue;

            CharacterStats enemy = enemyCollider.GetComponent<CharacterStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(meleeAttackDamage);
            }
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        UpdateAmmoBar();
        isReloading = false;
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
        if (isInvincible) return;

        base.TakeDamage(damage);

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    public void MuerteInstantanea()
    {
        UnityEngine.Debug.Log("¡Muerte instantánea! El jugador cayó en la DeathZone.");

        if (activeSunscreenPowerUp != null)
        {
            StopCoroutine(activeSunscreenPowerUp);
            activeSunscreenPowerUp = null;
        }

        if (activeSpeedPowerUp != null)
        {
            StopCoroutine(activeSpeedPowerUp);
            activeSpeedPowerUp = null;
        }

        isInvincible = false;
        if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
        moveSpeed = velocidadOriginal;

        Die();
    }

    protected override void Die()
    {
        UnityEngine.Debug.Log("El jugador ha muerto.");
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

    public void SetLeftBoundary(float boundary)
    {
        currentLeftBoundary = boundary;
    }

    public void ActivarPowerUpVelocidad(float multiplicadorVelocidad, float duracion)
    {
        if (activeSpeedPowerUp != null) StopCoroutine(activeSpeedPowerUp);
        activeSpeedPowerUp = StartCoroutine(PowerUpVelocidadCoroutine(multiplicadorVelocidad, duracion));
    }

    private IEnumerator PowerUpVelocidadCoroutine(float multiplicador, float tiempo)
    {
        moveSpeed *= multiplicador;
        yield return new WaitForSeconds(tiempo);
        moveSpeed = velocidadOriginal;
        activeSpeedPowerUp = null;
    }

    public void CancelSpeedPowerUp()
    {
        if (activeSpeedPowerUp != null) StopCoroutine(activeSpeedPowerUp);
        moveSpeed = velocidadOriginal;
        activeSpeedPowerUp = null;
    }

    public void ActivarProtectorSolar(float duracion, float auraDamage, float auraRadius)
    {
        if (activeSunscreenPowerUp != null) StopCoroutine(activeSunscreenPowerUp);
        activeSunscreenPowerUp = StartCoroutine(ProtectorSolarRoutine(duracion, auraDamage, auraRadius));
    }

    private IEnumerator ProtectorSolarRoutine(float duration, float auraDamage, float auraRadius)
    {
        isInvincible = true;

        if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.9f, 0.4f, 1f);

        UnityEngine.Debug.Log("¡Protector Solar activado! Invencible y quemando enemigos.");

        float timer = 0;
        float damageTickRate = 0.5f;
        float nextDamageTime = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            if (Time.time >= nextDamageTime)
            {
                Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, auraRadius);
                foreach (Collider2D col in enemies)
                {
                    if (col.CompareTag("Player")) continue;

                    CharacterStats enemyStats = col.GetComponent<CharacterStats>();
                    if (enemyStats != null)
                    {
                        enemyStats.TakeDamage(auraDamage);
                        UnityEngine.Debug.Log("Aura quemó a " + col.name);
                    }
                }
                nextDamageTime = Time.time + damageTickRate;
            }

            yield return null;
        }

        isInvincible = false;
        if (spriteRenderer != null) spriteRenderer.color = colorOriginal;
        activeSunscreenPowerUp = null;
        UnityEngine.Debug.Log("Protector Solar terminado.");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isInvincible && collision.gameObject.CompareTag("Enemy"))
        {
            UnityEngine.Debug.Log("Choque con enemigo anulado por Protector Solar.");
            return;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            float collisionAngle = Vector2.Dot(contact.normal, Vector2.up);

            if (collisionAngle > 0.7f)
            {
                UnityEngine.Debug.Log("Aterrizaste sobre el enemigo.");
            }
            else
            {
                UnityEngine.Debug.Log("Choque lateral con enemigo.");
                Hit(damageOnContact);
            }
        }
    }

    private void HandleMelting()
    {
        if (isInvincible) return;

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

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3.0f);
    }

    public void RechargeAmmo(int amount)
    {
        currentAmmo += amount;
        if (currentAmmo > maxAmmo) currentAmmo = maxAmmo;
        UpdateAmmoBar();
    }

    public void Immobilize(float duration)
    {
        if (!isTrapped) StartCoroutine(ImmobilizeRoutine(duration));
    }

    private IEnumerator ImmobilizeRoutine(float duration)
    {
        isTrapped = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("running", false);
        yield return new WaitForSeconds(duration);
        isTrapped = false;
    }

    private IEnumerator MeleeAttackRoutine()
    {
        isKicking = true;
        anim.SetBool("isKicking", true);
        anim.SetBool("running", false);

        MeleeAttack();

        yield return new WaitForSeconds(kickAnimationDuration);
        anim.SetBool("isKicking", false);
        yield return new WaitForSeconds(meleeAttackCooldown - kickAnimationDuration);
        isKicking = false;
    }
}