using UnityEngine;

namespace TopDown
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 10;
        private int currentHealth;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        private Vector2 _movementInput;
        private Vector2 _lastRawInputDirection = Vector2.down;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;
        [SerializeField] private KeyCode dashKey = KeyCode.Space;
        private bool _isDashing = false;
        private float _dashTimer;
        private float _dashCooldownTimer;

        [Header("Animations")]
        private Animator _anim;
        private const string AnimParamMoveX = "MoveX";
        private const string AnimParamMoveY = "MoveY";
        private const string AnimParamLastMoveX = "LastMoveX";
        private const string AnimParamLastMoveY = "LastMoveY";
        private const string AnimParamIsMoving = "IsMoving";
        private const string AnimParamDash = "Dash";

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();
            currentHealth = maxHealth;

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
            HandleAnimations();
        }

        private void FixedUpdate()
        {
            if (_isDashing) return;
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

            if (_movementInput.sqrMagnitude > 0.01f)
                _lastRawInputDirection = _movementInput;
        }

        private void MoveCharacter()
        {
            Vector2 effectiveMovement = _movementInput;
            if (effectiveMovement.sqrMagnitude > 1)
                effectiveMovement.Normalize();

            _rb.linearVelocity = effectiveMovement * moveSpeed;
        }

        private void HandleDashState()
        {
            if (_dashCooldownTimer > 0)
                _dashCooldownTimer -= Time.deltaTime;

            if (Input.GetKeyDown(dashKey) && !_isDashing && _dashCooldownTimer <= 0)
                StartDash();

            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0)
                    StopDash();
            }
        }

        private void StartDash()
        {
            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;

            Vector2 dashDirection = _movementInput.sqrMagnitude > 0.01f
                ? _movementInput.normalized
                : (_lastRawInputDirection == Vector2.zero ? Vector2.down : _lastRawInputDirection.normalized);

            _rb.linearVelocity = dashDirection * dashSpeed;

            if (_anim.HasParameter(AnimParamDash))
                _anim.SetTrigger(AnimParamDash);
        }

        private void StopDash()
        {
            _isDashing = false;
        }

        private void HandleAnimations()
        {
            bool isCurrentlyMoving = _movementInput.sqrMagnitude > 0.01f;

            _anim.SetBool(AnimParamIsMoving, isCurrentlyMoving && !_isDashing);

            if (isCurrentlyMoving && !_isDashing)
            {
                _anim.SetFloat(AnimParamMoveX, _movementInput.x);
                _anim.SetFloat(AnimParamMoveY, _movementInput.y);

                _anim.SetFloat(AnimParamLastMoveX, _movementInput.x);
                _anim.SetFloat(AnimParamLastMoveY, _movementInput.y);
            }
            else if (!_isDashing)
            {
                _anim.SetFloat(AnimParamMoveX, 0);
                _anim.SetFloat(AnimParamMoveY, 0);
            }
        }

        public void TakeDamage(int amount)
        {
            currentHealth -= amount;
            Debug.Log($"Player levou dano. Vida atual: {currentHealth}");

            if (currentHealth <= 0)
                Die();
        }

        private void Die()
        {
            Debug.Log("Player morreu.");
            gameObject.SetActive(false);
        }
    }

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
