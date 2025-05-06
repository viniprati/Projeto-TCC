using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    private float dashCooldown = 1f;
    private float lastDashTime;

    private Rigidbody rb;
    private Vector3 moveDirection;

    public GameObject[] weapons;  // Player's gun
    private int currentWeaponIndex = 0;  // Index of currently equipped weapon

    // Collision variables
    public LayerMask obstacleLayer;
    public float collisionCheckDistance = 0.5f;
    public float skinWidth = 0.1f;
    private bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Disable all weapons
        foreach (var weapon in weapons)
        {
            weapon.SetActive(false);
        }

        // First Weapon
        if (weapons.Length > 0)
        {
            weapons[currentWeaponIndex].SetActive(true);
        }
    }

    void Update()
    {
        MovePlayer();
        SwitchWeapon();
    }

    void MovePlayer()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        if (Input.GetKeyDown(KeyCode.Q) && Time.time > lastDashTime + dashCooldown && !isDashing)
        {
            StartCoroutine(Dash());
        }

        if (!isDashing)
        {
            // Check for collisions before moving
            Vector3 adjustedMovement = CheckCollisions(moveDirection * currentSpeed * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(adjustedMovement.x / Time.fixedDeltaTime, rb.linearVelocity.y, adjustedMovement.z / Time.fixedDeltaTime);
        }
    }

    private System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        float startTime = Time.time;
        Vector3 dashVelocity = moveDirection * dashSpeed;

        while (Time.time < startTime + dashDuration)
        {
            // Check for collisions during dash
            Vector3 adjustedDash = CheckCollisions(dashVelocity * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(adjustedDash.x / Time.fixedDeltaTime, rb.linearVelocity.y, adjustedDash.z / Time.fixedDeltaTime);
            yield return null;
        }

        isDashing = false;
    }

    private Vector3 CheckCollisions(Vector3 movement)
    {
        // Check for collisions in the movement direction
        RaycastHit hit;

        // Check forward/backward movement
        if (Physics.Raycast(transform.position, movement.normalized, out hit, movement.magnitude + skinWidth, obstacleLayer))
        {
            // Calculate slide direction along the obstacle
            Vector3 slideDirection = Vector3.ProjectOnPlane(movement, hit.normal).normalized;

            // Check if we can slide
            if (!Physics.Raycast(transform.position, slideDirection, out hit, movement.magnitude + skinWidth, obstacleLayer))
            {
                return slideDirection * movement.magnitude;
            }

            // If we can't slide, stop movement in that direction
            return Vector3.zero;
        }

        return movement;
    }

    void SwitchWeapon()
    {
        if (Input.mouseScrollDelta.y > 0)  // Mouse UP
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
            UpdateWeapon();
        }
        else if (Input.mouseScrollDelta.y < 0)  // Mouse Down 
        {
            currentWeaponIndex = (currentWeaponIndex - 1 + weapons.Length) % weapons.Length;
            UpdateWeapon();
        }
    }

    void UpdateWeapon()
    {
        // Disable all weapons and enable the selected one
        foreach (var weapon in weapons)
        {
            weapon.SetActive(false);
        }

        if (weapons.Length > 0)
        {
            weapons[currentWeaponIndex].SetActive(true);
        }
    }

    // Optional: Handle collision events if needed
    private void OnCollisionEnter(Collision collision)
    {
        // You can add specific collision reactions here
        if (collision.gameObject.CompareTag("Obstacle") && isDashing)
        {
            // Example: Stop dashing when hitting a wall
            isDashing = false;
        }
    }
}