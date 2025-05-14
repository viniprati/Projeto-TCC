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
    public LayerMask collisionLayers; // Camadas que bloqueiam o movimento
    public float collisionCheckRadius = 0.4f; // Raio do colisor do personagem
    public Tilemap solidTilemap; // Referência ao Tilemap sólido (opcional)

    [Header("Componentes")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Collider2D playerCollider;

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

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerCollider == null && invincibleDuringDash)
            playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Cooldown do dash
        if (dashCooldownLeft > 0)
            dashCooldownLeft -= Time.deltaTime;

        // Input de movimento
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Flip do sprite
        if (movement.x > 0)
            spriteRenderer.flipX = false;
        else if (movement.x < 0)
            spriteRenderer.flipX = true;

        // Ativa o dash
        if (Input.GetKeyDown(dashKey) && dashCooldownLeft <= 0 && movement != Vector2.zero)
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
            Vector2 dashDirection = movement.normalized;
            float dashDistance = dashSpeed * Time.fixedDeltaTime;
            Vector2 newPosition = rb.position + dashDirection * dashDistance;

            // Verifica colisão
            if (CanMoveToPosition(newPosition))
            {
                rb.MovePosition(newPosition);
            }
            else
            {
                // Colisão detectada - interrompe o dash
                if (dashCollisionEffect != null)
                {
                    dashCollisionEffect.transform.position = rb.position;
                    dashCollisionEffect.Play();
                }
                StopDash();
            }
        }
        else if (movement != Vector2.zero)
        {
            // Movimento normal
            if (snapMovement)
            {
                targetPosition = rb.position + movement.normalized * pixelSize;
                rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime));
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
        // Verifica colisão com objetos físicos
        if (Physics2D.OverlapCircle(targetPos, collisionCheckRadius, collisionLayers))
            return false;

        // Verifica tilemap sólido (se configurado)
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

        if (invincibleDuringDash && playerCollider != null)
            playerCollider.enabled = false;

        // Prepara efeito de piscar
        if (flickerSpriteDuringDash)
        {
            flickerTime = 0;
            spriteRenderer.enabled = true;
        }
    }

    void StopDash()
    {
        isDashing = false;

        // Garante que o sprite esteja visível
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // Restaura colisão
        if (invincibleDuringDash && playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        // Reseta variáveis de piscar
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
        // Desenha o raio de colisão
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, collisionCheckRadius);
    }
}