using UnityEngine;
using UnityEngine.Tilemaps;

public class PixelArtMovement2D : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float moveSpeed = 3f;
    public bool snapMovement = true;
    public float pixelSize = 0.0625f;

    [Header("Configurações de Colisão")]
    public LayerMask collisionLayers;
    public float collisionCheckRadius = 0.4f;
    public Tilemap solidTilemap;

    [Header("Componentes")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Collider2D playerCollider;
    public Animator animator;

    private Vector2 movement;
    private Vector2 targetPosition;

    private const string ANIM_IS_MOVING = "IsMoving";
    private const string ANIM_DIRECTION = "Direction";

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        movement = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) movement.y = 1;
        else if (Input.GetKey(KeyCode.S)) movement.y = -1;

        if (Input.GetKey(KeyCode.A)) movement.x = -1;
        else if (Input.GetKey(KeyCode.D)) movement.x = 1;

        if (animator != null)
        {
            animator.SetBool(ANIM_IS_MOVING, movement != Vector2.zero);

            if (movement != Vector2.zero)
            {
                string direction = GetDirectionFromVector(movement);
                animator.SetTrigger(direction);
                animator.ResetTrigger("up");
                animator.ResetTrigger("down");
                animator.ResetTrigger("left");
                animator.ResetTrigger("right");
            }
        }
    }

    void FixedUpdate()
    {
        if (movement != Vector2.zero)
        {
            if (snapMovement)
            {
                targetPosition = rb.position + movement.normalized * pixelSize;
                rb.MovePosition(Vector2.MoveTowards(rb.position, rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime, moveSpeed * Time.fixedDeltaTime));
            }
            else
            {
                Vector2 newPosition = rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime;
                if (CanMoveToPosition(newPosition))
                    rb.MovePosition(newPosition);
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

    string GetDirectionFromVector(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x > 0 ? "right" : "left";
        else
            return dir.y > 0 ? "up" : "down";
    }

    void OnDisable()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, collisionCheckRadius);
    }
}
