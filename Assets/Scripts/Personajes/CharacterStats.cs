using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Stats Universales")]
    public float maxHealth = 10f;
    public float moveSpeed = 1f; // 'Speed' del jugador, 'moveSpeed' de enemigos

    // 'protected' significa que esta clase y sus 'hijos' (PlayerController) pueden verlas
    protected float currentHealth;
    protected Rigidbody2D rb;
    protected Animator anim;

    // Usamos 'virtual' para que las clases 'hijo' puedan sobreescribir esta función
    protected virtual void Awake()
    {
        // Obtenemos los componentes universales
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        // Todos los personajes empiezan con la vida al máximo
        currentHealth = maxHealth;
    }

    // Un método universal para recibir daño
    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    // Un método universal para curar
    public virtual void Heal(float amount)
    {
        currentHealth += amount;
        Debug.Log(gameObject.name + " recibió " + amount + " de daño. Vida restante: " + currentHealth);
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    // Un método universal para morir
    protected virtual void Die()
    {
        // La acción de morir base es solo destruirse
        Debug.Log(gameObject.name + " ha muerto.");
        Destroy(gameObject);
    }
}