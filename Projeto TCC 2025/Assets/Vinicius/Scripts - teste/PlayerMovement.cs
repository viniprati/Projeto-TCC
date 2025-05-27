using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

namespace TopDown
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;
        private Vector2 movementDirection;
        private Vector2 currentInput;

        [Header("Animations")]
        [SerializeField] private Animator anim;
        private string lastDirection = "Down";

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        private void Update()
        {
            HandleAnimations();
        }
        private void FixedUpdate()
        {
            rb.linearVelocity = movementDirection * moveSpeed;
        }


        private void HandleAnimations()
        {
            if (anim == null) return;
            string animationName = "";
            if (movementDirection == Vector2.zero)
                animationName = "Idle";
            else
                animationName = "Walking";
            anim.Play(animationName + lastDirection);
        }

        private Vector3 GetDirection(Vector3 input)
        {
            Vector3 finalDirection = Vector3.zero;
            if (input.y > 0.01f)
            {
                lastDirection = "Up";
                movementDirection = new Vector2(0, 1);
            }
            else if (input.y < -0.01f)
            {
                lastDirection = "Down";
                movementDirection = new Vector2(0, -1);
            }
            else if (input.x > 0.01f)
            {
                lastDirection = "Right";
                movementDirection = new Vector2(1, 0);
            }
            else if (input.x < -0.01f)
            {
                lastDirection = "Left";
                movementDirection = new Vector2(-1, 0);
            }
            return finalDirection;
        }

        private void OnMove(InputValue value)
        {
            currentInput = value.Get<Vector2>().normalized;
            movementDirection = GetDirection(currentInput);
        }
    }
}

