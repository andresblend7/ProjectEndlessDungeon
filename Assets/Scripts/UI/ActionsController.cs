using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class ActionsController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private const string ActionButtonName = "action";
    private const string ToolButtonName = "tool";
    private const string Weapon1ButtonName = "weapon_1";
    private const string Weapon2ButtonName = "weapon_2";
    private const string DodgeButtonName = "dodge";

    [Header("Configuración de Ataque Continuo")]
    [Tooltip("Velocidad de ataque (ataques por segundo)")]
    [SerializeField] private float attackSpeed = 1f; // 2 ataques por segundo por defecto

    [Header("Referencias UI")]
    [SerializeField] private RawImage actionImage;
    [SerializeField] private Texture toolTexture;
    [SerializeField] private Texture meleeAttackTexture;
    [SerializeField] private Texture rangedAttackTexture;

    private Color originalColorActionButton;

    private EnumActionType actionSelected;
    private bool isHoldingAction = false;
    private Coroutine continuousActionCoroutine;
    private string currentClickedObject;

    // Control de cooldown
    private float lastClickTime = -999f;
    [SerializeField]
    private bool isOnCooldown = false;

    private PlayerFreeMovement playerMovementController;

    private void Start()
    {
        originalColorActionButton = actionImage.color;
        playerMovementController = FindFirstObjectByType<PlayerFreeMovement>();
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
        if (clickedObject == null)
        {
            return;
        }

        currentClickedObject = clickedObject.name;
        HandleButtonDown(currentClickedObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Detener el ataque continuo solo para "action"
        if (currentClickedObject == ActionButtonName)
        {
            HandleActionReleased();
        }

        currentClickedObject = null;
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleActionPressed();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            HandleActionReleased();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            HandleToolAction();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            HandleWeapon1Action();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            HandleWeapon2Action();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            HandleDodgeAction();
        }
    }

    private void HandleButtonDown(string buttonName)
    {
        // Manejar diferentes tipos de botones
        switch (buttonName)
        {
            case ActionButtonName:
                HandleActionPressed();
                break;

            case ToolButtonName:
                HandleToolAction();
                break;

            case Weapon1ButtonName:
                HandleWeapon1Action();
                break;

            case Weapon2ButtonName:
                HandleWeapon2Action();
                break;

            case DodgeButtonName:
                HandleDodgeAction();
                break;

            default:
                Debug.LogError("Acción no reconocida: " + buttonName);
                break;
        }
    }

    private void HandleActionPressed()
    {
        // Comprobar el cooldown
        if (IsOnCooldown())
            return;

        // Configurar acción
        actionSelected = EnumActionType.Action;

        // Iniciar acción continua
        isHoldingAction = true;

        // Ejecutar inmediatamente la primera vez
        ExecuteActionWithCooldown();

        // Iniciar la coroutine de ataque continuo
        if (continuousActionCoroutine != null)
        {
            StopCoroutine(continuousActionCoroutine);
        }

        continuousActionCoroutine = StartCoroutine(ContinuousAction());
    }

    private void HandleActionReleased()
    {
        isHoldingAction = false;

        if (continuousActionCoroutine != null)
        {
            StopCoroutine(continuousActionCoroutine);
            continuousActionCoroutine = null;
        }
    }

    private void HandleToolAction()
    {
        actionSelected = EnumActionType.Tool;
        actionImage.texture = toolTexture;
        ExecuteAction();
    }

    private void HandleWeapon1Action()
    {
        actionSelected = EnumActionType.Melee;
        actionImage.texture = meleeAttackTexture;
        ExecuteAction();
    }

    private void HandleWeapon2Action()
    {
        actionSelected = EnumActionType.Ranged;
        actionImage.texture = rangedAttackTexture;
        ExecuteAction();
    }

    private void HandleDodgeAction()
    {
        actionSelected = EnumActionType.Dodge;

        if (playerMovementController != null)
        {
            playerMovementController.TryDodge();
        }

        ExecuteAction();
    }

    private void ExecuteAction()
    {
        //Debug.Log("Ejecutando acción: " + actionSelected);
        InputManager.Instance.SetActionSelected(actionSelected);
    }

    private void ExecuteActionWithCooldown()
    {
        // Ejecutar la acción
        ExecuteAction();

        // Iniciar cooldown solo si no está ya en cooldown
        if (!isOnCooldown)
        {
            StartCoroutine(StartCooldown());
        }
    }

    private IEnumerator StartCooldown()
    {
        actionImage.color = Color.gray; // Cambiar color para indicar cooldown

        isOnCooldown = true;
        float cooldownTime = 1f / attackSpeed;
        yield return new WaitForSeconds(cooldownTime);

        actionImage.color = originalColorActionButton; // Restaurar color original

        isOnCooldown = false;
    }

    private IEnumerator ContinuousAction()
    {
        // Calcular el intervalo entre ataques
        float attackInterval = 1f / attackSpeed;

        // Esperar antes del siguiente ataque (el primero ya se ejecutó)
        yield return new WaitForSeconds(attackInterval);

        while (isHoldingAction)
        {
            // Verificar que no esté en cooldown antes de ejecutar
            if (!IsOnCooldown())
            {
                ExecuteActionWithCooldown();
            }

            // Esperar según la velocidad de ataque
            yield return new WaitForSeconds(attackInterval);
        }
    }

    /// <summary>
    /// Actualiza la velocidad de ataque del jugador
    /// </summary>
    public void SetAttackSpeed(float newAttackSpeed)
    {
        attackSpeed = Mathf.Max(0.1f, newAttackSpeed); // Mínimo 0.1 para evitar divisiones por cero
    }

    /// <summary>
    /// Obtiene la velocidad de ataque actual
    /// </summary>
    public float GetAttackSpeed()
    {
        return attackSpeed;
    }

    /// <summary>
    /// Verifica si actualmente está en cooldown
    /// </summary>
    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }

    private void OnDisable()
    {
        // Limpiar al desactivar
        if (continuousActionCoroutine != null)
        {
            StopCoroutine(continuousActionCoroutine);
            continuousActionCoroutine = null;
        }

        isHoldingAction = false;
        isOnCooldown = false;
    }
}

public enum EnumActionType
{
    None = 1,
    Action,///---- En lo q esté seleccionado es la opción principal
    Tool,
    Melee,
    Ranged,
    Consumable,
    Dodge
}
