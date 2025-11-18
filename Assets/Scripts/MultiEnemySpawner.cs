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

        int index = Random.Range(0, enemyPrefabs.Length);
        GameObject prefabToSpawn = enemyPrefabs[index];

        GameObject newEnemy = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);

        spawnedEnemies.Add(newEnemy);

        CharacterStats enemyStats = newEnemy.GetComponent<CharacterStats>();
        if (enemyStats != null)
        {
            if (enemyStats is PerroAI perro) perro.player = playerTransform;
            else if (enemyStats is CholitoAI cholito) cholito.player = playerTransform;
            else if (enemyStats is VecinoAI vecino) vecino.player = playerTransform;
            else if (enemyStats is TaqueroAI taquero) taquero.player = playerTransform;
            else if (enemyStats is AlbanilAI albanil) albanil.player = playerTransform;
        }
    }

    // Dibuja el radio de activación en el editor de Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        // Dibuja una esfera rellena
        Gizmos.DrawSphere(transform.position, activationDistance);
        
        // Dibuja el borde (verde sólido)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}