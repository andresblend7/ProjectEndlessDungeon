using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFreeMovement : MonoBehaviour
{
    [Header("References")]
    public ModelController modelController;
    private ActionsController actionsController;
    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Dodge")]
    [SerializeField] private float dodgeDistance = 4f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeCooldown = 1f;

    private bool isDodging;
    private float dodgeTimer;
    private float dodgeCooldownTimer;
    private Vector3 dodgeDirection;

    [Header("Attack")]
    private bool isHoldingAttack;

    private Vector2 moveVector;
    private Vector2 lookVector;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        actionsController = FindFirstObjectByType<ActionsController>();

    }

    void FixedUpdate()
    {
        HandleRotation();
        MoveCharacter();
        HandleDodge();

        if (isHoldingAttack)
            HandleAttack();
    }

    #region INPUT

    public void InputPlayer(InputAction.CallbackContext context)
    {
        moveVector = context.ReadValue<Vector2>();

        if (context.canceled)
        {
            moveVector = Vector2.zero;
        }
    }

    public void InputLook(InputAction.CallbackContext context)
    {
        lookVector = context.ReadValue<Vector2>();
    }

    public void InputLookTouch(InputAction.CallbackContext context)
    {
        if (context.started)
            isHoldingAttack = true;

        if (context.canceled)
            isHoldingAttack = false;
    }

    #endregion

    #region ATTACK

    private void HandleAttack()
    {
        actionsController.HandleActionPressed(true);
    }

    #endregion

    #region MOVEMENT

    private void MoveCharacter()
    {
        if (isDodging)
            return;

        //Vector3 movementDir = new Vector3(moveVector.x, 0f, moveVector.y);

        //Soporte para teclado si no hay input del joystick
        Vector2 input = GetFinalMoveInput();
        Vector3 movementDir = new Vector3(input.x, 0f, input.y);

        if (movementDir.sqrMagnitude > 0.01f)
        {
            modelController.SetIsWalking(true);

            movementDir.Normalize();

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            float dot = Vector3.Dot(forward, movementDir);
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
            modelController.SetIsWalking(false);

            rb.linearVelocity = new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );
        }
    }

    #endregion

    #region ROTATION

    private void HandleRotation()
    {
        Vector2 rotationInput = lookVector;

        // 🔥 Si no hay look, usar input combinado (joystick + teclado)
        if (rotationInput.sqrMagnitude < 0.01f)
            rotationInput = GetFinalMoveInput();

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

    #endregion

    #region DODGE

    private void HandleDodge()
    {
        if (dodgeCooldownTimer > 0f)
            dodgeCooldownTimer -= Time.fixedDeltaTime;

        if (!isDodging)
            return;

        float dodgeSpeed = dodgeDistance / dodgeDuration;

        Vector3 velocity = dodgeDirection * dodgeSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        dodgeTimer -= Time.fixedDeltaTime;

        if (dodgeTimer <= 0f)
            isDodging = false;
    }

    public void TryDodge()
    {
        if (isDodging || dodgeCooldownTimer > 0f)
            return;

        Vector3 forwardDirection = transform.forward;
        forwardDirection.y = 0f;

        if (forwardDirection.sqrMagnitude < 0.01f)
            return;

        dodgeDirection = forwardDirection.normalized;

        isDodging = true;
        dodgeTimer = dodgeDuration;
        dodgeCooldownTimer = dodgeCooldown;

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    #endregion

    #region PC controls
    private Vector2 GetKeyboardInput()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A)) h -= 1f;
        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;
        if (Input.GetKey(KeyCode.W)) v += 1f;

        Vector2 dir = new Vector2(h, v);

        return dir.sqrMagnitude > 1f ? dir.normalized : dir;
    }

    private Vector2 GetFinalMoveInput()
    {
        // Si hay input del joystick, usarlo
        if (moveVector.sqrMagnitude > 0.01f)
            return moveVector;

        // Si no, usar teclado
        return GetKeyboardInput();
    }

    #endregion
}