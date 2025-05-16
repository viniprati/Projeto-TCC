using UnityEngine;
using UnityEngine.Tilemaps;

public class PixelArtMovement2D : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float moveSpeed = 3f;
    public bool snapMovement = true;
    public float pixelSize = 0.0625f;

    [Header("Configurações de Dash")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public KeyCode dashKey = KeyCode.LeftShift;
    public bool invincibleDuringDash = true;

    [Header("Configurações de Colisão")]
    public LayerMask collisionLayers;
    public float collisionCheckRadius = 0.4f;
    public Tilemap solidTilemap;

    [Header("Componentes")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Collider2D playerCollider;
    public Animator animator; // << NOVO: Referência ao Animator

    [Header("Efeitos Visuais")]
    public bool flickerSpriteDuringDash = true;
    public float flickerRate = 0.1f;
    [Range(0f, 1f)] public float spriteVisibleRatio = 0.7f;
    public ParticleSystem dashCollisionEffect;

    private Vector2 movement;
    private Vector2 targetPosition;
    private bool isDashing = false;
    private float dashTimeLeft;
    private float dashCooldownLeft;
    private float flickerTime;
    private bool spriteWasDisabled = false;

    // Nomes dos parâmetros do Animator (para evitar erros de digitação)
    private const string ANIM_MOVE_X = "MoveX";
    private const string ANIM_MOVE_Y = "MoveY";
    private const string ANIM_IS_MOVING = "IsMoving";
    private const string ANIM_LAST_MOVE_X = "LastMoveX";
    private const string ANIM_LAST_MOVE_Y = "LastMoveY";
    // Opcional: Parâmetro para o dash, se você tiver animações de dash
    // private const string ANIM_IS_DASHING = "IsDashing";


    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerCollider == null && invincibleDuringDash)
            playerCollider = GetComponent<Collider2D>();
        if (animator == null) animator = GetComponent<Animator>(); // << NOVO: Pega o componente Animator
    }

    void Update()
    {
        // Cooldown do dash
        if (dashCooldownLeft > 0)
            dashCooldownLeft -= Time.deltaTime;

        // Input de movimento
        if (!isDashing) // Só permite input de movimento se não estiver dando dash
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }
        else
        {
            // Durante o dash, o 'movement' é mantido da direção inicial do dash
            // Não precisamos zerar aqui, pois é usado no FixedUpdate para a direção do dash
        }


        // << MODIFICADO: Atualiza os parâmetros do Animator >>
        if (animator != null)
        {
            animator.SetFloat(ANIM_MOVE_X, movement.x);
            animator.SetFloat(ANIM_MOVE_Y, movement.y);
            animator.SetBool(ANIM_IS_MOVING, movement.sqrMagnitude > 0.01f); // Usa sqrMagnitude para eficiência

            // Guarda a última direção de movimento para a animação de Idle correta
            if (movement.x != 0 || movement.y != 0) // Se houve algum input de movimento
            {
                animator.SetFloat(ANIM_LAST_MOVE_X, movement.x);
                animator.SetFloat(ANIM_LAST_MOVE_Y, movement.y);
            }
        }
        // << REMOVIDO: Flip do sprite >>
        // if (movement.x > 0)
        //     spriteRenderer.flipX = false;
        // else if (movement.x < 0)
        //     spriteRenderer.flipX = true;

        // Ativa o dash
        if (Input.GetKeyDown(dashKey) && dashCooldownLeft <= 0 && movement != Vector2.zero && !isDashing)
            StartDash();

        // Controle do dash
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
                StopDash();

            // Efeito de piscar o sprite
            if (flickerSpriteDuringDash)
            {
                flickerTime += Time.deltaTime;
                float cycleTime = flickerRate / spriteVisibleRatio;

                if (flickerTime >= cycleTime)
                {
                    flickerTime = 0;
                    spriteRenderer.enabled = true;
                }
                else if (flickerTime >= flickerRate)
                {
                    spriteRenderer.enabled = false;
                    spriteWasDisabled = true;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            // A direção do dash (movement.normalized) foi definida quando o dash começou
            Vector2 dashDirection = movement.normalized; // 'movement' aqui é a direção do início do dash
            float dashDistance = dashSpeed * Time.fixedDeltaTime;
            Vector2 newPosition = rb.position + dashDirection * dashDistance;

            if (CanMoveToPosition(newPosition))
            {
                rb.MovePosition(newPosition);
            }
            else
            {
                if (dashCollisionEffect != null)
                {
                    dashCollisionEffect.transform.position = rb.position;
                    dashCollisionEffect.Play();
                }
                StopDash();
            }
        }
        else if (movement != Vector2.zero) // Movimento normal
        {
            if (snapMovement)
            {
                // Para snap movement, é melhor mover uma unidade de pixel por vez na direção desejada
                // A implementação original MoveTowards para o targetPosition já faz algo similar
                targetPosition = rb.position + movement.normalized * pixelSize;
                // Garantir que estamos nos movendo uma quantidade baseada na moveSpeed
                rb.MovePosition(Vector2.MoveTowards(rb.position, rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime, moveSpeed * Time.fixedDeltaTime));

                // Snapping real para a grade de pixels (opcional, pode causar jitter com animações suaves)
                // Vector2 snappedPosition = new Vector2(
                //     Mathf.Round(targetPosition.x / pixelSize) * pixelSize,
                //     Mathf.Round(targetPosition.y / pixelSize) * pixelSize
                // );
                // rb.MovePosition(Vector2.MoveTowards(rb.position, snappedPosition, moveSpeed * Time.fixedDeltaTime));
            }
            else
            {
                Vector2 newPosition = rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime;
                if (CanMoveToPosition(newPosition))
                {
                    rb.MovePosition(newPosition);
                }
            }
        }
    }

    bool CanMoveToPosition(Vector2 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, collisionCheckRadius, collisionLayers))
            return false;

        if (solidTilemap != null)
        {
            Vector3Int cell = solidTilemap.WorldToCell(targetPos);
            if (solidTilemap.HasTile(cell))
                return false;
        }
        return true;
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashCooldownLeft = dashCooldown;
        spriteWasDisabled = false;

        // if (animator != null) animator.SetBool(ANIM_IS_DASHING, true); // << Opcional para animação de dash

        if (invincibleDuringDash && playerCollider != null)
            playerCollider.enabled = false;

        if (flickerSpriteDuringDash)
        {
            flickerTime = 0;
            spriteRenderer.enabled = true;
        }
    }

    void StopDash()
    {
        isDashing = false;
        // if (animator != null) animator.SetBool(ANIM_IS_DASHING, false); // << Opcional

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        if (invincibleDuringDash && playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        flickerTime = 0;
        spriteWasDisabled = false;
    }

    public bool IsInvincible()
    {
        return isDashing && invincibleDuringDash;
    }

    void OnDisable()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, collisionCheckRadius);
    }
}