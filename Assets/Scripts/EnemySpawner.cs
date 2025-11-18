using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // --- Variables Configurables ---
    [Header("Configuraci�n del Spawner")]
    [SerializeField] private GameObject enemyPrefab; 
    [SerializeField] private float spawnInterval = 10f; 

    // --- Referencia Interna ---
    private Transform playerTransform; // Para decirle al nuevo enemigo a qui�n seguir

    // Awake se llama antes que Start. Es ideal para encontrar referencias.
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

    // Start se llama una vez al inicio, despu�s de Awake.
    void Start()
    {
        // Si encontramos al jugador, empezamos la rutina de spawn.
        if (playerTransform != null)
        {
            StartCoroutine(SpawnEnemyRoutine());
        }
    }

    // Una Corrutina es una funci�n que puede pausarse.
    private IEnumerator SpawnEnemyRoutine()
    {
        // Este bucle se ejecutar� para siempre.
        while (true)
        {
            // 1. Espera 5 segundos
            yield return new WaitForSeconds(spawnInterval);

            // 2. Llama a la funci�n para crear un enemigo
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        // 1. Revisa que tengamos un prefab de enemigo asignado
        if (enemyPrefab == null)
        {
            Debug.LogError("No hay 'enemyPrefab' asignado en el Spawner.");
            return;
        }

        // 2. Crea una nueva instancia del enemigo en la posici�n de este spawner (el carro)
        GameObject newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        // 3. �MUY IMPORTANTE! Le decimos al nuevo enemigo d�nde est� el jugador.
        CholitoAI cholo = newEnemy.GetComponent<CholitoAI>();
        if (cholo != null)
        {
            cholo.player = playerTransform;
        }
    }
}