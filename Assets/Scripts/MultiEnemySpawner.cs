using System.Collections;
using System.Collections.Generic; 
using System.Linq; 
using UnityEngine;

public class MultiEnemySpawner : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxEnemies = 5; 
    [SerializeField] private float activationDistance = 20f; 
    [Tooltip("Punto específico donde aparecerán los enemigos.")]
    [SerializeField] private Transform spawnPoint; // --- NUEVO ---

    private Transform playerTransform;
    
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool isSpawning = false; 

    void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("Spawner no pudo encontrar al Jugador.");
        }
    }
    
    void Start()
    {
        // La lógica de inicio ahora está en Update()
    }
    
    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= activationDistance && !isSpawning)
        {
            isSpawning = true;
            StartCoroutine("SpawnEnemyRoutine"); 
        }
        else if (distance > activationDistance && isSpawning)
        {
            isSpawning = false;
            StopCoroutine("SpawnEnemyRoutine"); 
        }
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            spawnedEnemies.RemoveAll(item => item == null);

            if (spawnedEnemies.Count < maxEnemies)
            {
                SpawnRandomEnemy();
            }
        }
    }

    // Elige un enemigo al azar del array y le asigna el jugador
    private void SpawnRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("No hay 'enemyPrefabs' asignados en el Spawner.");
            return;
        }

        // --- LÓGICA MODIFICADA ---
        // Determina la posición de spawn: si spawnPoint está asignado, úsalo. Si no, usa el propio spawner.
        Vector3 spawnPosition = (spawnPoint != null) ? spawnPoint.position : transform.position;

        int index = Random.Range(0, enemyPrefabs.Length);
        GameObject prefabToSpawn = enemyPrefabs[index];

        // Usa la posición determinada para instanciar
        GameObject newEnemy = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        spawnedEnemies.Add(newEnemy);

        // Asignación de la referencia del jugador
        CharacterStats enemyStats = newEnemy.GetComponent<CharacterStats>();
        if (enemyStats != null)
        {
            if (enemyStats is PerroAI perro) perro.player = playerTransform;
            else if (enemyStats is CholitoAI cholito) cholito.player = playerTransform;
            else if (enemyStats is VecinoAI vecino) vecino.player = playerTransform;
            else if (enemyStats is TaqueroAI taquero) taquero.player = playerTransform;
            else if (enemyStats is AlbanilAI albanil) albanil.player = playerTransform;
            else if (enemyStats is EloteroAI elotero) elotero.player = playerTransform;
        }
    }

    // Dibuja el radio de activación y el punto de spawn
    private void OnDrawGizmosSelected()
    {
        // Dibuja el radio de activación (verde)
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, activationDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationDistance);

        // --- NUEVO ---
        // Dibuja el punto de spawn elegido (en azul)
        if (spawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(spawnPoint.position, 0.1f); // Dibuja una esfera en el punto
            Gizmos.color = new Color(0f, 0f, 1f, 0.7f);
            Gizmos.DrawWireSphere(spawnPoint.position, 0.1f);
        }
    }
}