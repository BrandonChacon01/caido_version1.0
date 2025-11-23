using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Stats Universales")]
    public float maxHealth = 10f;
    public float moveSpeed = 1f;

    protected float currentHealth;
    protected Rigidbody2D rb;
    protected Animator anim;

    // 🔹 AGREGADO: virtual para que las clases hijas puedan hacer override
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

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public virtual void Heal(float amount)
    {
        currentHealth += amount;
        Debug.Log(gameObject.name + " recibió " + amount + " de daño. Vida restante: " + currentHealth);
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " ha muerto.");
        Destroy(gameObject);
    }
}