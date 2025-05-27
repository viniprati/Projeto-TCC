using UnityEngine;

namespace TopDown
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        private Vector2 _movementInput;
        // _lastMovementDirection guardará a direção normalizada para lógica de dash/tiro
        private Vector2 _lastRawInputDirection = Vector2.down; // Guarda o último input bruto para LastMoveX/Y

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;
        [SerializeField] private KeyCode dashKey = KeyCode.Space;
        private bool _isDashing = false;
        private float _dashTimer;
        private float _dashCooldownTimer;

        [Header("Shooting")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private float shootCooldown = 0.5f;
        [SerializeField] private KeyCode shootKey = KeyCode.Mouse0; // Botão esquerdo do mouse
        private float _shootCooldownTimer;

        [Header("Animations")]
        private Animator _anim;
        // Nomes dos parâmetros do Animator (para fácil alteração se necessário)
        private const string AnimParamMoveX = "MoveX";
        private const string AnimParamMoveY = "MoveY";
        private const string AnimParamLastMoveX = "LastMoveX";
        private const string AnimParamLastMoveY = "LastMoveY";
        private const string AnimParamIsMoving = "IsMoving"; // Adicione este parâmetro Bool ao seu Animator
        private const string AnimParamDash = "Dash";         // Adicione este Trigger se tiver animação de dash
        private const string AnimParamShoot = "Shoot";       // Adicione este Trigger se tiver animação de tiro


        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();

            if (firePoint == null)
            {
                Transform foundFirePoint = transform.Find("FirePoint");
                if (foundFirePoint != null) firePoint = foundFirePoint;
                else
                {
                    GameObject fpGo = new GameObject("FirePoint");
                    fpGo.transform.SetParent(transform);
                    fpGo.transform.localPosition = Vector3.zero;
                    firePoint = fpGo.transform;
                    Debug.LogWarning("FirePoint não foi atribuído e foi criado automaticamente. Ajuste sua posição se necessário.");
                }
            }

            // Garante que o Animator tenha os parâmetros necessários (checa apenas na inicialização)
            // Isso é mais para debug, a extensão HasParameter é usada em tempo real.
            if (!_anim.HasParameter(AnimParamMoveX)) Debug.LogError($"Animator não tem o parâmetro: {AnimParamMoveX}");
            if (!_anim.HasParameter(AnimParamMoveY)) Debug.LogError($"Animator não tem o parâmetro: {AnimParamMoveY}");
            if (!_anim.HasParameter(AnimParamLastMoveX)) Debug.LogError($"Animator não tem o parâmetro: {AnimParamLastMoveX}");
            if (!_anim.HasParameter(AnimParamLastMoveY)) Debug.LogError($"Animator não tem o parâmetro: {AnimParamLastMoveY}");
            if (!_anim.HasParameter(AnimParamIsMoving)) Debug.LogError($"Animator não tem o parâmetro: {AnimParamIsMoving}. Crie um parâmetro Bool com este nome.");
        }

        private void Update()
        {
            HandleInput();
            HandleDashState();
            HandleShooting();
            HandleAnimations();
        }

        private void FixedUpdate()
        {
            if (_isDashing)
            {
                return;
            }
            MoveCharacter();
        }

        private void HandleInput()
        {
            if (_isDashing)
            {
                _movementInput = Vector2.zero;
                return;
            }

            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            _movementInput = new Vector2(moveX, moveY);

            // Não normaliza aqui, pois o Blend Tree espera valores entre -1 e 1 para cada eixo.
            // A normalização para velocidade de movimento é feita implicitamente ao aplicar moveSpeed
            // ou pode ser feita antes de aplicar ao Rigidbody se você permitir movimento diagonal total.
            // Para o blend tree, os valores brutos são geralmente melhores.

            // Guarda a última direção de input significativo para idle e mirar
            // Usamos _movementInput diretamente, pois o Blend Tree também usa valores não normalizados.
            if (_movementInput.sqrMagnitude > 0.01f)
            {
                _lastRawInputDirection = _movementInput; // Não normaliza para manter a fidelidade ao input para o blend tree de idle
            }
        }

        private void MoveCharacter()
        {
            // Para movimento, é bom normalizar se você quer velocidade consistente em diagonais.
            Vector2 effectiveMovement = _movementInput;
            if (effectiveMovement.sqrMagnitude > 1)
            {
                effectiveMovement.Normalize();
            }
            _rb.linearVelocity = effectiveMovement * moveSpeed;
        }

        private void HandleDashState()
        {
            if (_dashCooldownTimer > 0)
            {
                _dashCooldownTimer -= Time.deltaTime;
            }

            if (Input.GetKeyDown(dashKey) && !_isDashing && _dashCooldownTimer <= 0)
            {
                StartDash();
            }

            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0)
                {
                    StopDash();
                }
            }
        }

        private void StartDash()
        {
            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;

            Vector2 dashDirection;
            if (_movementInput.sqrMagnitude > 0.01f) // Se estiver se movendo
            {
                dashDirection = _movementInput.normalized;
            }
            else // Se parado, usa a última direção que estava olhando
            {
                // Normaliza _lastRawInputDirection para o dash
                dashDirection = _lastRawInputDirection.normalized;
                // Se _lastRawInputDirection for zero (começo do jogo sem mover), usa um padrão
                if (dashDirection == Vector2.zero) dashDirection = Vector2.down;
            }

            _rb.linearVelocity = dashDirection * dashSpeed;

            if (_anim.HasParameter(AnimParamDash))
                _anim.SetTrigger(AnimParamDash);
        }

        private void StopDash()
        {
            _isDashing = false;
        }


        private void HandleShooting()
        {
            if (_shootCooldownTimer > 0)
            {
                _shootCooldownTimer -= Time.deltaTime;
            }

            if (Input.GetKeyDown(shootKey) && _shootCooldownTimer <= 0 && !_isDashing) // Não atira durante o dash
            {
                if (projectilePrefab == null || firePoint == null)
                {
                    Debug.LogError("Projectile Prefab ou Fire Point não atribuído!");
                    return;
                }
                Shoot();
                _shootCooldownTimer = shootCooldown;
            }
        }

        private void Shoot()
        {
            // Direção do Tiro: baseada na última direção de input não nulo, normalizada.
            Vector2 shootDirection = _lastRawInputDirection.normalized;
            if (shootDirection == Vector2.zero) // Caso comece atirando sem nunca ter movido
            {
                shootDirection = Vector2.down; // Ou qualquer direção padrão que você preferir
            }


            GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D projectileRb = projectileGO.GetComponent<Rigidbody2D>();

            if (projectileRb != null)
            {
                projectileRb.linearVelocity = shootDirection * projectileSpeed;
            }
            else
            {
                Debug.LogWarning("Projétil não tem Rigidbody2D.");
            }

            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            projectileGO.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            if (_anim.HasParameter(AnimParamShoot))
                _anim.SetTrigger(AnimParamShoot);
        }


        private void HandleAnimations()
        {
            bool isCurrentlyMoving = _movementInput.sqrMagnitude > 0.01f;

            // Define o parâmetro IsMoving para transição entre Idle e Walk
            _anim.SetBool(AnimParamIsMoving, isCurrentlyMoving && !_isDashing);

            if (isCurrentlyMoving && !_isDashing)
            {
                // Se movendo, atualiza MoveX e MoveY para o Blend Tree de caminhada
                _anim.SetFloat(AnimParamMoveX, _movementInput.x);
                _anim.SetFloat(AnimParamMoveY, _movementInput.y);

                // Também atualiza LastMoveX e LastMoveY para que, ao parar,
                // o Blend Tree de Idle use a direção correta.
                _anim.SetFloat(AnimParamLastMoveX, _movementInput.x);
                _anim.SetFloat(AnimParamLastMoveY, _movementInput.y);
            }
            else if (!_isDashing) // Parado e não dando dash
            {
                // Se parado, o Blend Tree de Idle usará LastMoveX e LastMoveY.
                // Esses valores já foram definidos quando o personagem estava se movendo.
                // Podemos zerar MoveX e MoveY para garantir que o Blend Tree de caminhada
                // não esteja tentando usar valores antigos se a lógica de transição falhar,
                // mas o IsMoving=false deve cuidar disso.
                _anim.SetFloat(AnimParamMoveX, 0);
                _anim.SetFloat(AnimParamMoveY, 0);
            }
            // Se estiver dando dash, o IsMoving será false, e os triggers de dash (se houver) podem ser usados.
            // A animação de Idle/Walk não deve tocar durante o dash se IsMoving estiver corretamente configurado.
        }
    }

    // Extensão para Animator.HasParameter (coloque fora da classe PlayerMovement, mas dentro do namespace ou globalmente)
    public static class AnimatorExtensions
    {
        public static bool HasParameter(this Animator animator, string paramName)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName) return true;
            }
            return false;
        }
    }
}