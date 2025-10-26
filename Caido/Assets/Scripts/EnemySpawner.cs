using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // --- Variables Configurables ---
    [Header("Configuración del Spawner")]
    [SerializeField] private GameObject enemyPrefab; // Arrastra tu Prefab del "Grunt" aquí
    [SerializeField] private float spawnInterval = 10f;  // El tiempo (5 seg) entre cada spawn

    // --- Referencia Interna ---
    private Transform playerTransform; // Para decirle al nuevo enemigo a quién seguir

    // Awake se llama antes que Start. Es ideal para encontrar referencias.
    void Awake()
    {
        // Buscamos al jugador por su Etiqueta (Tag).
        // ¡Asegúrate de que tu Player tenga el Tag "Player"!
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

    // Start se llama una vez al inicio, después de Awake.
    void Start()
    {
        // Si encontramos al jugador, empezamos la rutina de spawn.
        if (playerTransform != null)
        {
            StartCoroutine(SpawnEnemyRoutine());
        }
    }

    // Una Corrutina es una función que puede pausarse.
    private IEnumerator SpawnEnemyRoutine()
    {
        // Este bucle se ejecutará para siempre.
        while (true)
        {
            // 1. Espera 5 segundos
            yield return new WaitForSeconds(spawnInterval);

            // 2. Llama a la función para crear un enemigo
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

        // 2. Crea una nueva instancia del enemigo en la posición de este spawner (el carro)
        GameObject newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        // 3. ¡MUY IMPORTANTE! Le decimos al nuevo enemigo dónde está el jugador.
        GruntScript grunt = newEnemy.GetComponent<GruntScript>();
        if (grunt != null)
        {
            grunt.Player = playerTransform;
        }
    }
}