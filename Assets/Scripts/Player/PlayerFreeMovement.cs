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

    private void FixedUpdate()
    {
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
    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Intentando esquivar...");
            TryDodge();
        }
#endif
    }

    private void MoveCharacter()
    {
        // --- Rotaci�n (Esto d�jalo igual, est� perfecto) ---
        if (moveVector != Vector2.zero)
        {
            Vector3 direction = new Vector3(moveVector.x, 0f, moveVector.y).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        // --- Movimiento CORREGIDO ---
        // En lugar de calcular posici�n, calculamos velocidad.

        // 1. Calculamos la velocidad deseada en X y Z
        Vector3 movement = new Vector3(moveVector.x, 0f, moveVector.y).normalized * moveSpeed;

        // 2. IMPORTANTE: Mantenemos la velocidad Y actual del Rigidbody (para la gravedad)
        // Si no haces esto, el personaje flotar� o caer� lento.
        Vector3 finalVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        // 3. Aplicamos la velocidad
        rb.linearVelocity = finalVelocity;
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
