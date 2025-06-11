using UnityEngine;

public class Sword : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    public int damage = 1;

    public GameObject projectilePrefab;
    public Transform firePoint;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("SpriteRenderer não encontrado na espada!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (projectilePrefab == null || firePoint == null)
            {
                return;
            }

            Attack();
        }

        // Flip ao apertar A
        if (Input.GetKeyDown(KeyCode.A))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        // Diminui Order in Layer ao apertar W
        if (Input.GetKeyDown(KeyCode.W) && sr != null)
        {
            sr.sortingOrder -= 2;
        }
    }

    void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Acertou " + enemy.name);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
