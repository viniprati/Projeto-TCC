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

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Disable all weapons
        foreach (var weapon in weapons)
        {
            weapon.SetActive(false);
        }

        // Firt Weapon
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

        if (Input.GetKeyDown(KeyCode.Q) && Time.time > lastDashTime + dashCooldown)
        {
            StartCoroutine(Dash());
        }

        rb.linearVelocity = new Vector3(moveDirection.x * currentSpeed, rb.linearVelocity.y, moveDirection.z * currentSpeed);
    }

    private System.Collections.IEnumerator Dash()
    {
        lastDashTime = Time.time;
        float startTime = Time.time;
        Vector3 dashVelocity = moveDirection * dashSpeed;

        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = new Vector3(dashVelocity.x, rb.linearVelocity.y, dashVelocity.z);
            yield return null;
        }
    }

    void SwitchWeapon()
    {
        if (Input.mouseScrollDelta.y > 0)  // Mouse UP
        // Mechanics for changing weapons
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
            UpdateWeapon();
        }
        else if (Input.mouseScrollDelta.y < 0)  // Mouse DOWN
        {
            currentWeaponIndex = (currentWeaponIndex - 1 + weapons.Length) % weapons.Length;
            UpdateWeapon();
        }
    }

    void UpdateWeapon()
    {
        // Desativa todas as armas e ativa a arma selecionada
        foreach (var weapon in weapons)
        {
            weapon.SetActive(false);
        }

        if (weapons.Length > 0)
        {
            weapons[currentWeaponIndex].SetActive(true);
        }
    }
}
