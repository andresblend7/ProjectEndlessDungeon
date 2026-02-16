using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFreeMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

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
    }

    private void MoveCharacter()
    {
        // --- Rotación (Esto déjalo igual, está perfecto) ---
        if (moveVector != Vector2.zero)
        {
            Vector3 direction = new Vector3(moveVector.x, 0f, moveVector.y).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        // --- Movimiento CORREGIDO ---
        // En lugar de calcular posición, calculamos velocidad.

        // 1. Calculamos la velocidad deseada en X y Z
        Vector3 movement = new Vector3(moveVector.x, 0f, moveVector.y).normalized * moveSpeed;

        // 2. IMPORTANTE: Mantenemos la velocidad Y actual del Rigidbody (para la gravedad)
        // Si no haces esto, el personaje flotará o caerá lento.
        Vector3 finalVelocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        // 3. Aplicamos la velocidad
        rb.velocity = finalVelocity;
    }
}
