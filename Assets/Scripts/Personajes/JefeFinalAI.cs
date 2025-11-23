using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// No olvides poner el namespace si usas uno

public class JefeFinalAI : BaseEnemyAI
{
    [Header("Configuración de Jefe")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float xOffsetFromCamera = -8f; 
    [SerializeField] private float yPosition = 0f; 
    
    [Header("Ataque (Lanzamiento de Enemigos)")]
    [SerializeField] private GameObject[] enemyProjectiles; 
    [SerializeField] private Transform throwPoint; 
    [SerializeField] private float throwForceX = 10f;
    [SerializeField] private float throwForceY = 5f;
    [SerializeField] private float attackRate = 3f;

    [Header("Debug Trayectoria")]
    [SerializeField] private LineRenderer lineRenderer; // Arrastra el componente aquí
    [SerializeField] private int trajectorySteps = 30; // Cuántos puntos dibujar
    [SerializeField] private float timeStep = 0.1f; // Espacio de tiempo entre puntos

    // Variables internas
    private float lastAttackTime;

    protected override void Start()
    {
        base.Start();
        if (mainCamera == null) mainCamera = Camera.main;
        
        // Si no asignaste el LineRenderer manual, intentamos buscarlo
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    protected override void Update()
    {
        if (player == null) return;

        // 1. Movimiento
        if (mainCamera != null)
        {
            Vector3 newPos = transform.position;
            newPos.x = mainCamera.transform.position.x + xOffsetFromCamera;
            newPos.y = yPosition; 
            transform.position = newPos;
        }

        // 2. Ataque
        if (Time.time > lastAttackTime + attackRate)
        {
            LanzarEnemigo();
            lastAttackTime = Time.time;
        }

        // 3. DIBUJAR TRAYECTORIA (Nuevo)
        DibujarTrayectoria();
    }

    protected override void FixedUpdate()
    {
        // Vacío intencionalmente
    }

    private void LanzarEnemigo()
    {
        if (enemyProjectiles.Length == 0) return;

        int index = Random.Range(0, enemyProjectiles.Length);
        GameObject prefab = enemyProjectiles[index];

        Vector3 spawnPos = (throwPoint != null) ? throwPoint.position : transform.position;
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        BaseEnemyAI enemyAI = enemy.GetComponent<BaseEnemyAI>();

        if (enemyAI != null) enemyAI.player = this.player;

        if (enemyRb != null)
        {
            enemyRb.bodyType = RigidbodyType2D.Dynamic;
            Vector2 force = new Vector2(throwForceX, throwForceY);
            enemyRb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    // --- LÓGICA DE LA TRAYECTORIA ---
    private void DibujarTrayectoria()
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = trajectorySteps;
        
        // Punto de inicio
        Vector3 startPos = (throwPoint != null) ? throwPoint.position : transform.position;
        
        // Simulamos la velocidad inicial. 
        // Nota: AddForce(Impulse) -> Velocidad = Fuerza / Masa.
        // Asumimos Masa = 1 para el dibujo (si tus enemigos pesan más, ajusta aquí: force / masa).
        Vector2 velocity = new Vector2(throwForceX, throwForceY); 

        for (int i = 0; i < trajectorySteps; i++)
        {
            // Calcular el tiempo en este punto
            float time = i * timeStep;

            // Fórmula de física: Posición = PosInicial + (Velocidad * tiempo) + (0.5 * Gravedad * tiempo^2)
            Vector2 posAtTime = (Vector2)startPos + (velocity * time) + (0.5f * Physics2D.gravity * time * time);

            lineRenderer.SetPosition(i, posAtTime);
        }
    }

    protected override void HandleChase() { }
    protected override void HandleAttack() { }
}