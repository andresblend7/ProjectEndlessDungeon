using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFreeMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Dodge")]
    [SerializeField] private float dodgeDistance = 4f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeCooldown = 1f;
    private bool isDodging = false;
    private float dodgeTimer;
    private float dodgeCooldownTimer;
    private Vector3 dodgeDirection;


    private Vector2 moveVector;
    private bool isFirstMove = true;
    private Rigidbody rb;

    private Vector2 lookVector;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void InputPlayer(InputAction.CallbackContext _context)
    {
        moveVector = _context.ReadValue<Vector2>();

        if (_context.canceled)
        {
            isFirstMove = true;
            moveVector = Vector2.zero;
        }
    }

    public void InputLook(InputAction.CallbackContext _context)
    {
        lookVector = _context.ReadValue<Vector2>();

        //Debug.Log("Look: " + lookVector);

        if (_context.canceled)
            lookVector = Vector2.zero;
    }

    private void FixedUpdate()
    {
        HandleRotation();
        MoveCharacter();

        if (isDodging)
        {
            float dodgeSpeed = dodgeDistance / dodgeDuration;

            Vector3 velocity = dodgeDirection * dodgeSpeed;
            velocity.y = rb.linearVelocity.y; // mantener gravedad

            rb.linearVelocity = velocity;

            dodgeTimer -= Time.fixedDeltaTime;

            if (dodgeTimer <= 0f)
            {
                isDodging = false;
            }
        }

        if (dodgeCooldownTimer > 0f)
            dodgeCooldownTimer -= Time.fixedDeltaTime;



    }

    private void MoveCharacter()
    {
        if (isDodging) return;

        Vector3 movementDir = new Vector3(moveVector.x, 0f, moveVector.y);

        if (movementDir.sqrMagnitude > 0.01f)
        {
            movementDir.Normalize();

            // Dirección hacia donde mira el personaje
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            // Dot product (-1 a 1)
            float dot = Vector3.Dot(forward, movementDir);

            // Convertimos de (-1,1) a (0.4,1)
            // 1   → 1 (velocidad completa)
            // 0   → 0.7
            // -1  → 0.4
            float speedMultiplier = Mathf.Lerp(0.4f, 1f, (dot + 1f) / 2f);

            float finalSpeed = moveSpeed * speedMultiplier;

            Vector3 movement = movementDir * finalSpeed;

            rb.linearVelocity = new Vector3(
                movement.x,
                rb.linearVelocity.y,
                movement.z
            );
        }
        else
        {
            rb.linearVelocity = new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );
        }
    }

    private void HandleRotation()
    {
        Vector2 rotationInput = lookVector;

        // Fallback opcional: si no usa el stick derecho, usar movimiento
        if (rotationInput.sqrMagnitude < 0.01f)
            rotationInput = moveVector;

        if (rotationInput.sqrMagnitude > 0.01f)
        {
            Vector3 direction = new Vector3(rotationInput.x, 0f, rotationInput.y);

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            rb.rotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    public void TryDodge()
    {
        if (isDodging) return;
        if (dodgeCooldownTimer > 0f) return;

        // Dirección basada en hacia donde está mirando
        Vector3 forwardDirection = transform.forward;
        forwardDirection.y = 0f; // evitar inclinaciones raras

        if (forwardDirection.sqrMagnitude < 0.01f)
            return;

        dodgeDirection = forwardDirection.normalized;

        isDodging = true;
        dodgeTimer = dodgeDuration;
        dodgeCooldownTimer = dodgeCooldown;

        // Cancelar velocidad horizontal actual
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }


}
