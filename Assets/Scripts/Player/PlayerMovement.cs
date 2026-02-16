using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float deadzone = 0.5f; // Zona muerta para evitar inputs accidentales
    [SerializeField] private float initialDelay = 0.3f; // Delay antes del primer movimiento repetido (opcional)
    [SerializeField] private float moveRepeatRate = 0.2f; // Tiempo entre movimientos repetidos


    private Vector2 moveVector;
    private float lastInputTime;
    private EnumMoveDirection lastDirection;




    [Header("References")]
    public PlayerUtilities playerUtilities;
    [SerializeField] public float moveDistance = 1f;

    // Evento para notificar movimiento
    public delegate void MovementCommand(Vector3 targetPos, EnumMoveDirection direction);
    public static event MovementCommand OnMoveCommand;

    //evento para notificar acción seleccionada
    public delegate void ActionSelectedCommand(EnumActionType actionType);
    public static event ActionSelectedCommand OnActionSelectedCommand;

    private float nextMoveTime;
    private bool isFirstMove = true;

    public void InputPlayer(InputAction.CallbackContext _context)
    {
        moveVector = _context.ReadValue<Vector2>();

        // Reset cuando se suelta el stick
        if (_context.canceled)
        {
            isFirstMove = true;
        }
    }

    void Update()
    {
        // Verificar que el input supere la zona muerta
        if (moveVector.magnitude < deadzone)
        {
            isFirstMove = true;
            return;
        }

        // Verificar si es momento de ejecutar el movimiento
        if (Time.time < nextMoveTime)
            return;

        // Detectar dirección predominante
        EnumMoveDirection direction = GetDirectionFromVector(moveVector);

        // Ejecutar movimiento
        ExecuteMove(direction);

        // Configurar el siguiente tiempo de movimiento
        if (isFirstMove)
        {
            // Primer movimiento tiene un delay opcional
            nextMoveTime = Time.time + initialDelay;
            isFirstMove = false;
        }
        else
        {
            // Movimientos subsecuentes usan el rate normal
            nextMoveTime = Time.time + moveRepeatRate;
        }
    }

    private EnumMoveDirection GetDirectionFromVector(Vector2 input)
    {
        // Determinar si es más horizontal o vertical
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            // Movimiento horizontal
            return input.x > 0 ? EnumMoveDirection.Right : EnumMoveDirection.Left;
        }
        else
        {
            // Movimiento vertical
            return input.y > 0 ? EnumMoveDirection.Up : EnumMoveDirection.Down;
        }
    }

    private void ExecuteMove(EnumMoveDirection direction)
    {
        var actualPlayerPos = playerUtilities.GetActualPosition();
        Vector3 moveVector = direction switch
        {
            EnumMoveDirection.Up => new Vector3(0, 0, moveDistance),
            EnumMoveDirection.Down => new Vector3(0, 0, -moveDistance),
            EnumMoveDirection.Left => new Vector3(-moveDistance, 0, 0),
            EnumMoveDirection.Right => new Vector3(moveDistance, 0, 0),
            _ => Vector3.zero
        };

        var targetPosition = actualPlayerPos + moveVector;
        OnMoveCommand?.Invoke(targetPosition, direction);
    }
}
