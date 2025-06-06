using UnityEngine;

// Vamos manter o mesmo namespace para consistência
namespace TopDown
{
    public class ArmaSsegueMouse : MonoBehaviour
    {
        [Header("Configuração da Arma")]
        [Tooltip("O objeto que será o pivô da rotação (geralmente o Player ou uma 'mão' vazia filha do Player).")]
        public Transform pivoDeRotacao;

        [Tooltip("Um ajuste fino para corrigir a rotação inicial do sprite da espada. Se o seu sprite aponta para a direita por padrão, deixe 0. Se aponta para cima, use -90.")]
        public float offsetDeAngulo = 0f;

        [Header("Inversão do Sprite")]
        [Tooltip("Marque para inverter a escala do sprite quando o mouse estiver à esquerda do jogador.")]
        public bool inverterSprite = true;

        // Referência privada para a câmera principal para otimização
        private Camera cameraPrincipal;
        private SpriteRenderer spriteRenderer;

        void Start()
        {
            // Guarda a referência da câmera principal para não ter que chamar Camera.main a cada frame (melhora o desempenho)
            cameraPrincipal = Camera.main;
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Se o pivô não for definido no Inspector, avisa o usuário e desativa o script
            if (pivoDeRotacao == null)
            {
                Debug.LogError("O 'Pivô de Rotação' não foi definido no Inspector! A arma não saberá em torno de quem girar.", this.gameObject);
                this.enabled = false;
            }
        }

        void Update()
        {
            // Se o pivô não existe (ex: player morreu), não faz nada
            if (pivoDeRotacao == null) return;

            // 1. Manter a posição da arma sempre na mesma posição do pivô
            transform.position = pivoDeRotacao.position;

            // 2. Obter a posição do mouse no mundo do jogo
            // Input.mousePosition nos dá a posição em pixels na tela (Screen Space)
            // Camera.ScreenToWorldPoint converte essa posição para o mundo do jogo (World Space)
            Vector3 posicaoMouseNoMundo = cameraPrincipal.ScreenToWorldPoint(Input.mousePosition);

            // 3. Calcular a direção do pivô para o mouse
            // Subtraímos a posição do mouse da posição do pivô para obter um vetor que aponta do pivô para o mouse
            Vector2 direcao = new Vector2(
                posicaoMouseNoMundo.x - pivoDeRotacao.position.x,
                posicaoMouseNoMundo.y - pivoDeRotacao.position.y
            );

            // Alternativa mais curta para a linha acima:
            // Vector2 direcao = posicaoMouseNoMundo - pivoDeRotacao.position;

            // 4. Calcular o ângulo dessa direção
            // Mathf.Atan2 nos dá o ângulo em radianos. Multiplicamos por Mathf.Rad2Deg para converter para graus.
            float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;

            // 5. Aplicar a rotação na arma
            // Usamos Quaternion.Euler para criar uma rotação a partir do ângulo calculado
            // Adicionamos o nosso offset para corrigir a rotação inicial do sprite
            transform.rotation = Quaternion.Euler(0f, 0f, angulo + offsetDeAngulo);

            // 6. (Opcional, mas recomendado) Inverter o sprite para que a espada não fique de "cabeça para baixo"
            if (inverterSprite)
            {
                // Verifica se a posição X do mouse está à esquerda ou à direita do pivô
                bool mouseEstaAEsquerda = posicaoMouseNoMundo.x < pivoDeRotacao.position.x;

                if (mouseEstaAEsquerda)
                {
                    // Se estiver à esquerda, inverte a escala no eixo Y
                    transform.localScale = new Vector3(1, -1, 1);
                }
                else
                {
                    // Se estiver à direita, mantém a escala normal
                    transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
    }
}