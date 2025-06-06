using UnityEngine;

// Vamos manter o mesmo namespace para consist�ncia
namespace TopDown
{
    public class ArmaSsegueMouse : MonoBehaviour
    {
        [Header("Configura��o da Arma")]
        [Tooltip("O objeto que ser� o piv� da rota��o (geralmente o Player ou uma 'm�o' vazia filha do Player).")]
        public Transform pivoDeRotacao;

        [Tooltip("Um ajuste fino para corrigir a rota��o inicial do sprite da espada. Se o seu sprite aponta para a direita por padr�o, deixe 0. Se aponta para cima, use -90.")]
        public float offsetDeAngulo = 0f;

        [Header("Invers�o do Sprite")]
        [Tooltip("Marque para inverter a escala do sprite quando o mouse estiver � esquerda do jogador.")]
        public bool inverterSprite = true;

        // Refer�ncia privada para a c�mera principal para otimiza��o
        private Camera cameraPrincipal;
        private SpriteRenderer spriteRenderer;

        void Start()
        {
            // Guarda a refer�ncia da c�mera principal para n�o ter que chamar Camera.main a cada frame (melhora o desempenho)
            cameraPrincipal = Camera.main;
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Se o piv� n�o for definido no Inspector, avisa o usu�rio e desativa o script
            if (pivoDeRotacao == null)
            {
                Debug.LogError("O 'Piv� de Rota��o' n�o foi definido no Inspector! A arma n�o saber� em torno de quem girar.", this.gameObject);
                this.enabled = false;
            }
        }

        void Update()
        {
            // Se o piv� n�o existe (ex: player morreu), n�o faz nada
            if (pivoDeRotacao == null) return;

            // 1. Manter a posi��o da arma sempre na mesma posi��o do piv�
            transform.position = pivoDeRotacao.position;

            // 2. Obter a posi��o do mouse no mundo do jogo
            // Input.mousePosition nos d� a posi��o em pixels na tela (Screen Space)
            // Camera.ScreenToWorldPoint converte essa posi��o para o mundo do jogo (World Space)
            Vector3 posicaoMouseNoMundo = cameraPrincipal.ScreenToWorldPoint(Input.mousePosition);

            // 3. Calcular a dire��o do piv� para o mouse
            // Subtra�mos a posi��o do mouse da posi��o do piv� para obter um vetor que aponta do piv� para o mouse
            Vector2 direcao = new Vector2(
                posicaoMouseNoMundo.x - pivoDeRotacao.position.x,
                posicaoMouseNoMundo.y - pivoDeRotacao.position.y
            );

            // Alternativa mais curta para a linha acima:
            // Vector2 direcao = posicaoMouseNoMundo - pivoDeRotacao.position;

            // 4. Calcular o �ngulo dessa dire��o
            // Mathf.Atan2 nos d� o �ngulo em radianos. Multiplicamos por Mathf.Rad2Deg para converter para graus.
            float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;

            // 5. Aplicar a rota��o na arma
            // Usamos Quaternion.Euler para criar uma rota��o a partir do �ngulo calculado
            // Adicionamos o nosso offset para corrigir a rota��o inicial do sprite
            transform.rotation = Quaternion.Euler(0f, 0f, angulo + offsetDeAngulo);

            // 6. (Opcional, mas recomendado) Inverter o sprite para que a espada n�o fique de "cabe�a para baixo"
            if (inverterSprite)
            {
                // Verifica se a posi��o X do mouse est� � esquerda ou � direita do piv�
                bool mouseEstaAEsquerda = posicaoMouseNoMundo.x < pivoDeRotacao.position.x;

                if (mouseEstaAEsquerda)
                {
                    // Se estiver � esquerda, inverte a escala no eixo Y
                    transform.localScale = new Vector3(1, -1, 1);
                }
                else
                {
                    // Se estiver � direita, mant�m a escala normal
                    transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
    }
}