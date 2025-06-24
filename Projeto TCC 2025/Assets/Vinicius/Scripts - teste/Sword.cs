using UnityEngine;
using TopDown;

public class Sword : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRange = 0.7f;
    public int damage = 1;
    public float attackCooldown = 0.5f;

    private float lastAttackTime;
    private Animator anim;
    private PlayerMovement playerMovement;

    [Header("Movimento")]
    public float velocidade = 3f;

    [Header("Ataque")]
    public float distanciaAtaque = 1.5f;
    public int dano = 1;
    public float cooldownAtaque = 2f;
    public float proximoAtaque = 0f;

     void Start()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponentInParent<PlayerMovement>();

        // Tenta encontrar o AttackPoint se não estiver atribuído
        if (attackPoint == null)
        {
            attackPoint = transform.Find("AttackPoint");
            if (attackPoint == null)
                Debug.LogError("AttackPoint não encontrado como filho da Sword.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        anim?.SetTrigger("Attack");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

        foreach (Collider2D hit in hits)
        {
            almofada enemy = hit.GetComponent<almofada>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
