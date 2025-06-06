using UnityEngine;
using TopDown;

public class InimigoSeguidor : MonoBehaviour
{
    [Header("Referências")]
    public Transform alvo;
    private Rigidbody2D rb;

    [Header("Movimento")]
    public float velocidade = 3f;

    [Header("Ataque")]
    public float distanciaAtaque = 1.5f;
    public int dano = 10;
    public float cooldownAtaque = 2f;
    private float proximoAtaque = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (alvo == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                alvo = playerObj.transform;
            }
            else
            {
                Debug.LogError("Inimigo não encontrou o Player. Verifique a tag 'Player'.");
                this.enabled = false;
            }
        }
    }

    void Update()
    {
        if (alvo == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distancia = Vector2.Distance(transform.position, alvo.position);

        if (distancia <= distanciaAtaque)
        {
            if (Time.time >= proximoAtaque)
            {
                Atacar();
                proximoAtaque = Time.time + cooldownAtaque;
            }
        }
        else
        {
            Seguir();
        }
    }

    void Seguir()
    {
        Vector2 direcao = (alvo.position - transform.position).normalized;
        rb.linearVelocity = direcao * velocidade;

        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angulo;
    }

    void Atacar()
    {
        rb.linearVelocity = Vector2.zero;
        Debug.Log("Almofada Malvada ataca o Player!");

        PlayerMovement player = alvo.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.TakeDamage(dano);
        }
        else
        {
            Debug.LogWarning("O alvo não tem um script 'PlayerMovement' com TakeDamage.");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}
