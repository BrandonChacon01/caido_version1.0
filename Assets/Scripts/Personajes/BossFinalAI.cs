using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boss Final - Jefe que spawnea enemigos y bloquea el paso
/// </summary>
public class BossFinalAI : CharacterStats
{
    [Header("🎯 Configuración del Jefe")]
    [Tooltip("Velocidad de movimiento del jefe")]
    [SerializeField] private float bossSpeed = 0.5f;

    [Tooltip("¿El jefe patrulla o se queda quieto?")]
    [SerializeField] private bool canMove = true;

    [Tooltip("Distancia mínima al jugador (se detiene si está más cerca)")]
    [SerializeField] private float stopDistance = 1.5f;

    [Tooltip("Distancia para empezar a perseguir (si está más lejos)")]
    [SerializeField] private float chaseDistance = 8f;

    [Tooltip("Distancia para huir del jugador (si está MUY cerca, ir al lado opuesto)")]
    [SerializeField] private float fleeDistance = 0.8f;

    [Tooltip("Detectar bordes para evitar caídas")]
    [SerializeField] private bool detectEdges = true;

    [Tooltip("Distancia del raycast para detectar suelo")]
    [SerializeField] private float edgeDetectionDistance = 2f;

    [Header("💀 Spawn de Enemigos")]
    [Tooltip("Prefabs de enemigos que puede spawnear (arrastra aquí los prefabs)")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Tooltip("Tiempo entre spawns de enemigos (segundos)")]
    [SerializeField] private float spawnInterval = 15f;

    [Tooltip("Máximo de enemigos vivos a la vez")]
    [SerializeField] private int maxEnemiesAlive = 5;

    [Tooltip("Offset de spawn en las piernas (izquierda y derecha)")]
    [SerializeField] private Vector2 leftLegOffset = new Vector2(-0.5f, -0.5f);
    [SerializeField] private Vector2 rightLegOffset = new Vector2(0.5f, -0.5f);

    [Header("💚 Barra de Vida")]
    [Tooltip("Slider de la barra de vida del jefe (en el UI)")]
    public Slider bossHealthBar;

    [Tooltip("Imagen de relleno de la barra de vida")]
    public Image bossHealthFill;

    [Tooltip("Color de la barra cuando tiene vida")]
    public Color healthyColor = Color.green;

    [Tooltip("Color de la barra cuando está bajo de vida")]
    public Color lowHealthColor = Color.red;

    [Header("🎨 Efectos Visuales")]
    [Tooltip("Duración del efecto de daño (flash rojo)")]
    [SerializeField] private float damageFlashDuration = 0.1f;

    [Tooltip("Duración de la animación de muerte")]
    [SerializeField] private float deathFadeDuration = 1f;

    [Header("🔊 Audio (Opcional)")]
    [Tooltip("Sonido al recibir daño")]
    [SerializeField] private AudioClip damageSound;

    [Tooltip("Sonido al morir")]
    [SerializeField] private AudioClip deathSound;

    [Tooltip("Sonido al spawnear enemigos")]
    [SerializeField] private AudioClip spawnSound;

    // Variables privadas
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Transform player;
    private float currentMoveDirection = 1f;
    private float nextSpawnTime = 0f;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool isDead = false;

    protected override void Awake()
    {
        // 🔥 CONFIGURAR ANTES
        maxHealth = 10f;
        currentHealth = 10f;

        base.Awake();

        moveSpeed = 0.5f;
        spriteRenderer = GetComponent<SpriteRenderer>();

        UnityEngine.Debug.Log($"[BossFinal] Awake - Vida: {currentHealth}/{maxHealth}");
    }

    protected override void Start()
    {
        // 🔥 CONFIGURAR ANTES
        maxHealth = 10f;
        currentHealth = 10f;

        base.Start();

        // 🔥 FORZAR DESPUÉS también
        maxHealth = 10f;
        currentHealth = 10f;

        UnityEngine.Debug.Log($"[BossFinal] Start FINAL - Vida: {currentHealth}/{maxHealth}");

        // Guardar color original
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Buscar al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Configurar la barra de vida
        InitializeHealthBar();

        // Iniciar el sistema de spawn
        nextSpawnTime = Time.time + spawnInterval;

        UnityEngine.Debug.Log($"[BossFinal] Inicialización completa");
    }

    private void Update()
    {
        if (isDead) return;

        // Sistema de spawn de enemigos
        HandleEnemySpawning();
    }

    private void FixedUpdate()
    {
        if (isDead || !canMove) return;

        // Movimiento inteligente
        HandleMovement();
    }

    /// <summary>
    /// Inicializar la barra de vida del jefe
    /// </summary>
    private void InitializeHealthBar()
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.maxValue = maxHealth;
            bossHealthBar.value = currentHealth;
            bossHealthBar.gameObject.SetActive(true);
        }

        if (bossHealthFill != null)
        {
            bossHealthFill.color = healthyColor;
        }
    }

    /// <summary>
    /// Actualizar la barra de vida
    /// </summary>
    private void UpdateHealthBar()
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.value = currentHealth;

            // Cambiar color según la vida restante
            if (bossHealthFill != null)
            {
                float healthPercent = currentHealth / maxHealth;
                bossHealthFill.color = Color.Lerp(lowHealthColor, healthyColor, healthPercent);
            }
        }
    }

    /// <summary>
    /// Detectar si hay suelo adelante (para no caerse)
    /// </summary>
    private bool IsGroundAhead()
    {
        if (!detectEdges) return true; // Si no detectamos bordes, siempre hay suelo

        // Raycast hacia abajo desde adelante del boss
        Vector2 frontPosition = new Vector2(
            transform.position.x + (currentMoveDirection * 0.5f),
            transform.position.y
        );

        RaycastHit2D hit = Physics2D.Raycast(frontPosition, Vector2.down, edgeDetectionDistance);

        // Debug visual
        Debug.DrawRay(frontPosition, Vector2.down * edgeDetectionDistance, hit ? Color.green : Color.red);

        return hit.collider != null;
    }

    /// <summary>
    /// Movimiento: Comportamiento inteligente según distancia al jugador
    /// </summary>
    private void HandleMovement()
    {
        if (player == null) return;

        // Calcular distancia y dirección al jugador
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float directionToPlayer = player.position.x - transform.position.x;

        // 🏃 COMPORTAMIENTO 1: MUY CERCA - HUIR
        if (distanceToPlayer < fleeDistance)
        {
            currentMoveDirection = directionToPlayer > 0 ? -1f : 1f; // Ir al lado opuesto

            if (IsGroundAhead())
            {
                rb.linearVelocity = new Vector2(currentMoveDirection * bossSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        // ⏸️ COMPORTAMIENTO 2: CERCA - DETENERSE
        else if (distanceToPlayer < stopDistance)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        // 🎯 COMPORTAMIENTO 3: DISTANCIA MEDIA - PERSEGUIR
        else if (distanceToPlayer < chaseDistance)
        {
            currentMoveDirection = directionToPlayer > 0 ? 1f : -1f;

            if (IsGroundAhead())
            {
                rb.linearVelocity = new Vector2(currentMoveDirection * bossSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        // 🚶 COMPORTAMIENTO 4: MUY LEJOS - CAMINAR LENTO
        else
        {
            currentMoveDirection = directionToPlayer > 0 ? 1f : -1f;

            if (IsGroundAhead())
            {
                rb.linearVelocity = new Vector2(currentMoveDirection * (bossSpeed * 0.5f), rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }

        // Flip del sprite según la dirección
        if (spriteRenderer != null && Mathf.Abs(directionToPlayer) > 0.1f)
        {
            spriteRenderer.flipX = directionToPlayer < 0;
        }
    }

    /// <summary>
    /// Sistema de spawn de enemigos
    /// </summary>
    private void HandleEnemySpawning()
    {
        // Limpiar enemigos muertos de la lista
        spawnedEnemies.RemoveAll(enemy => enemy == null);

        // Verificar si es tiempo de spawnear y no hemos llegado al límite
        if (Time.time >= nextSpawnTime && spawnedEnemies.Count < maxEnemiesAlive)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    /// <summary>
    /// Spawnear un enemigo aleatorio
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            UnityEngine.Debug.LogWarning("[BossFinal] No hay prefabs de enemigos asignados");
            return;
        }

        // Seleccionar prefab aleatorio
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        if (enemyPrefab == null)
        {
            UnityEngine.Debug.LogWarning("[BossFinal] Prefab de enemigo nulo en el array");
            return;
        }

        // Elegir posición aleatoria (pierna izquierda o derecha)
        Vector2 spawnOffset = Random.value > 0.5f ? leftLegOffset : rightLegOffset;
        Vector3 spawnPosition = transform.position + new Vector3(spawnOffset.x, spawnOffset.y, 0f);

        // Instanciar enemigo
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        spawnedEnemies.Add(spawnedEnemy);

        // Reproducir sonido de spawn
        PlaySound(spawnSound);

        UnityEngine.Debug.Log($"[BossFinal] Enemigo spawneado: {enemyPrefab.name} | Total activos: {spawnedEnemies.Count}");
    }

    /// <summary>
    /// Recibir daño (llamado por balas o ataques melee)
    /// </summary>
    public void Hit(float damage)
    {
        if (isDead) return;

        UnityEngine.Debug.Log($"[BossFinal] ========== HIT RECIBIDO ==========");
        UnityEngine.Debug.Log($"[BossFinal] Daño recibido: {damage}");
        UnityEngine.Debug.Log($"[BossFinal] maxHealth = {maxHealth}");
        UnityEngine.Debug.Log($"[BossFinal] currentHealth ANTES = {currentHealth}");

        // Restar vida directamente
        currentHealth -= damage;

        UnityEngine.Debug.Log($"[BossFinal] currentHealth DESPUÉS = {currentHealth}");
        UnityEngine.Debug.Log($"[BossFinal] ¿Está muerto? {currentHealth <= 0}");

        // Verificar muerte
        if (currentHealth <= 0)
        {
            UnityEngine.Debug.LogError($"[BossFinal] 💀 MURIENDO porque currentHealth = {currentHealth}");
            currentHealth = 0;
            Die();
            return;
        }

        UnityEngine.Debug.Log($"[BossFinal] ========== FIN HIT ==========");

        // Actualizar barra
        UpdateHealthBar();

        // Efecto visual de daño
        StartCoroutine(DamageFlashEffect());

        // Sonido de daño
        PlaySound(damageSound);
    }

    /// <summary>
    /// Muerte del jefe
    /// </summary>
    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        UnityEngine.Debug.Log("[BossFinal] 💀 Jefe derrotado!");

        // Detener movimiento
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        // Desactivar animador si existe
        if (anim != null)
        {
            anim.enabled = false;
        }

        // Ocultar barra de vida
        if (bossHealthBar != null)
        {
            bossHealthBar.gameObject.SetActive(false);
        }

        // Sonido de muerte
        PlaySound(deathSound);

        // Destruir enemigos spawneados (opcional)
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();

        // Animación de desvanecimiento
        StartCoroutine(DeathFadeOut());
    }

    /// <summary>
    /// Efecto de flash rojo al recibir daño
    /// </summary>
    private IEnumerator DamageFlashEffect()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// Animación de desvanecimiento al morir
    /// </summary>
    private IEnumerator DeathFadeOut()
    {
        if (spriteRenderer == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsedTime < deathFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / deathFadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Reproducir sonido
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (Camera.main != null)
        {
            AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    /// <summary>
    /// Detectar colisiones con paredes para cambiar dirección
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Cambiar dirección al chocar con paredes/suelo
        // Verificar si es colisión lateral (no desde arriba/abajo)
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Si la normal es más horizontal que vertical = pared
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                currentMoveDirection *= -1f;
                UnityEngine.Debug.Log("[BossFinal] Chocó con pared, cambiando dirección");
                break;
            }
        }
    }

    /// <summary>
    /// Gizmos para visualizar las posiciones de spawn y zonas de comportamiento
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Dibujar posiciones de spawn de enemigos
        Gizmos.color = Color.yellow;
        Vector3 leftPos = transform.position + new Vector3(leftLegOffset.x, leftLegOffset.y, 0f);
        Vector3 rightPos = transform.position + new Vector3(rightLegOffset.x, rightLegOffset.y, 0f);

        Gizmos.DrawWireSphere(leftPos, 0.2f);
        Gizmos.DrawWireSphere(rightPos, 0.2f);

        // Dibujar línea del collider del jefe
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>()?.bounds.size ?? Vector3.one);

        // Dibujar zonas de comportamiento
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeDistance); // Zona de huida

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance); // Zona de parada

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseDistance); // Zona de persecución
    }
}