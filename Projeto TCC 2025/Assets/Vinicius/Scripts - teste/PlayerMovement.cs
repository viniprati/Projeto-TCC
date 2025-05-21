using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 input;
    Animator anim;
    private Vector2 lastMoveDirection;
    private bool facingLeft = true; // nosso sprite está virado pra esquerda

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ProcessInputs();
        Animate();

        // Flip - checa se está olhando pro lado errado
        if (input.x < 0 && !facingLeft || input.x > 0 && facingLeft)
        {
            Flip();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }

    void ProcessInputs()
    {
        input = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) input.y = 1;
        if (Input.GetKey(KeyCode.S)) input.y = -1;
        if (Input.GetKey(KeyCode.A)) input.x = -1;
        if (Input.GetKey(KeyCode.D)) input.x = 1;

        input.Normalize();

        if (input != Vector2.zero)
        {
            lastMoveDirection = input;
        }
    }

    void Animate()
    {
        if (anim == null) return;

        anim.SetFloat("MoveX", input.x);
        anim.SetFloat("MoveY", input.y);
        anim.SetFloat("Speed", input.sqrMagnitude);
        anim.SetFloat("LastMoveX", lastMoveDirection.x);
        anim.SetFloat("LastMoveY", lastMoveDirection.y);
    }

    void Flip()
    {
        facingLeft = !facingLeft;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
}
