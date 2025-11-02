using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class MultiEnemySpawner : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnInterval = 10f;

    private Transform playerTransform;
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
        if (playerTransform != null)
        {
            StartCoroutine(SpawnEnemyRoutine());
        }
    }
    private IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnRandomEnemy();
        }
    }

    // Elige un enemigo al azar del array y le asigna el jugador
    private void SpawnRandomEnemy()
    {
        // 1. Revisa que el array de prefabs no esté vacío
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("No hay 'enemyPrefabs' asignados en el Spawner.");
            return;
        }

        // 2. Elige un prefab al azar del array
        int index = Random.Range(0, enemyPrefabs.Length);
        GameObject prefabToSpawn = enemyPrefabs[index];

        // 3. Crea una nueva instancia del enemigo elegido
        GameObject newEnemy = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);

        // 4. Asigna el 'playerTransform' al script del enemigo
        CholitoAI cholito = newEnemy.GetComponent<CholitoAI>();
        if (cholito != null)
        {

            cholito.Player = playerTransform;
            return; 
        }

        PerroAI perro = newEnemy.GetComponent<PerroAI>();
        if (perro != null)
        {
            perro.player = playerTransform;
            return; 
        }

        VecinoAI vecino = newEnemy.GetComponent<VecinoAI>();
        if (vecino != null)
        {
            vecino.player = playerTransform;
            return; 
        }
    }
}