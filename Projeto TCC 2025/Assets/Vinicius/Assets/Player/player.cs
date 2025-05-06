using UnityEngine;

public class PixelArtMovement2D : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float moveSpeed = 3f;
    public bool snapMovement = true;
    public float pixelSize = 0.0625f;

    [Header("Componentes")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private Vector2 targetPosition;

    void Start()
    {
        // Verificação automática de componentes
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        Debug.Log("Script inicializado. Componentes encontrados: " +
                 (rb != null ? "Rigidbody2D " : "") +
                 (spriteRenderer != null ? "SpriteRenderer" : ""));
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Debug para verificar input
        if (movement != Vector2.zero)
            Debug.Log("Input detectado: " + movement);

        if (movement.x > 0)
            spriteRenderer.flipX = false;
        else if (movement.x < 0)
            spriteRenderer.flipX = true;
    }

    void FixedUpdate()
    {
        if (movement == Vector2.zero) return;

        if (snapMovement)
        {
            targetPosition = rb.position + movement.normalized * pixelSize;
            Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
            Debug.Log("Movendo para: " + newPosition);
        }
        else
        {
            Vector2 newPosition = rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
            Debug.Log("Movendo para: " + newPosition);
        }
    }
}