using UnityEngine;
using TopDown; // Mantendo o namespace caso o PlayerMovement esteja nele

public class almofada : MonoBehaviour
{
    [Header("Referências")]
    public Transform alvo;
    private Rigidbody2D rb;

    // --- SEÇÃO DE VIDA ADICIONADA ---
    [Header("Vida")]
    public int vidaMaxima = 3;
    public int vidaAtual;

    [Header("Movimento")]
    public float velocidade = 3f;

    [Header("Ataque")]
    public float distanciaAtaque = 1.5f;
    public int dano = 1; // Ajustei para 1, já que a vida do player é 10
    public float cooldownAtaque = 2f;
    public float proximoAtaque = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // --- INICIALIZAÇÃO DA VIDA ---
        vidaAtual = vidaMaxima;

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
            // Para de seguir quando está na distância de ataque
            rb.linearVelocity = Vector2.zero;

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

        // Opcional: Rotacionar o sprite para olhar para o player
        // Se seu sprite já é feito para um jogo Top-Down e não precisa rotacionar, pode remover as 2 linhas abaixo.
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angulo;
    }

    void Atacar()
    {
        Debug.Log(gameObject.name + " ataca o Player!");

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

    // --- FUNÇÃO PARA RECEBER DANO (CHAMADA PELA ESPADA) ---
    public void TakeDamage(int quantidadeDano)
    {
        vidaAtual -= quantidadeDano;
        Debug.Log(gameObject.name + " levou " + quantidadeDano + " de dano! Vida restante: " + vidaAtual);

        if (vidaAtual <= 0){
            Morrer();
        } 
    }

    // --- FUNÇÃO PARA MORRER ---
    private void Morrer()
    {
        Debug.Log(gameObject.name + " foi derrotado!");

        // Desativa o script e o colisor para que não possa mais atacar ou ser atingido
        this.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // Adicione aqui uma animação de morte, som, ou partículas

        // Destrói o objeto do inimigo após um pequeno atraso (ex: 1 segundo para a animação tocar)
        Destroy(gameObject, 1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}