using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{

    [Header("References")]
    private PlayerUtilities playerUtilities;

    private bool isMoving = false;
    private Vector3 targetPosition;
    [Header("GameObjects")]
    public GameObject playerModel;
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    [Header("Colliders")]
    public ColliderDirectionCheck front;
    public ColliderDirectionCheck back;
    public ColliderDirectionCheck left;
    public ColliderDirectionCheck right;
    public BoxCollider attackCollider;
    [Tooltip("Duración del hitbox activo para acciones y ataques (en segundos)")]
    public float hitboxActiveDuration = 0.05f;
    [Header("RayCast para la colisión del hitbox de accion  (picar/accionar)")]
    public GameObject actionCollider;
    [SerializeField] private Transform origin;
    [SerializeField] public float distanceMaxAction = 5f;
    [SerializeField] public float offsetTomaxDistance = 0.2f;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private Transform hitboxAction; // El objeto que quieres posicionar
    [Tooltip("Radio del SphereCast para detectar colisiones")]
    [SerializeField] private float castRadius = 0.25f;
    private RaycastHit debugHit;
    private bool hasHit;


    public ModelController modelController;

    private void Awake()
    {
        playerUtilities = FindFirstObjectByType<PlayerUtilities>();
        //modelController = playerModel.GetComponentInChildren<ModelController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //REFERERENCES
      

        // Suscribirse al evento de movimiento
        InputManager.OnMoveCommand += HandleMoveCommand;
        InputManager.OnActionSelectedCommand += HandleActionSelectedCommand;

        PlayerMovement.OnMoveCommand += HandleMoveCommand;

      


    }

    /// <summary>
    /// Controlador de las acciones seleccionadas en la UI_Actions
    /// </summary>
    /// <param name="actionType"></param>
    private void HandleActionSelectedCommand(EnumActionType actionType)
    {
        if (playerUtilities == null)
        {
            Debug.LogWarning(actionType + " seleccionado pero playerUtilities es null, ignorando comando");
            return;
        }

        EnumActualToolSelected actualToolSelected = EnumActualToolSelected.None;
        switch (actionType)
        {
            case EnumActionType.Action:
                modelController.ExecuteAnimation(PlayerAnimation.Action);
                if(playerUtilities.GetActualToolSelected() == EnumActualToolSelected.Pickaxe)
                    StartCoroutine(ActiveHitboxAction());
                if (playerUtilities.GetActualToolSelected() == EnumActualToolSelected.Melee)
                    StartCoroutine(ActiveAttackHitBox());
                break;
            case EnumActionType.Tool:
                actualToolSelected = EnumActualToolSelected.Pickaxe;
                break;
            case EnumActionType.Melee:
                actualToolSelected = EnumActualToolSelected.Melee;
                break;
            case EnumActionType.Ranged:
                actualToolSelected = EnumActualToolSelected.Ranged;
                break;
            case EnumActionType.Dodge:
                modelController.ExecuteAnimation(PlayerAnimation.Dodge);
                break;
            default:
                Debug.LogError("Tipo de acción no reconocido");
                return;
        }
        if (actualToolSelected != EnumActualToolSelected.None)
            playerUtilities.SetActualToolSelected(actualToolSelected);

        modelController.ChangeSelectTool(actualToolSelected);
    }

    private IEnumerator ActiveHitboxAction()
    {
        yield return new WaitForSeconds(0.15f);
        actionCollider.SetActive(true);
        yield return new WaitForSeconds(hitboxActiveDuration); // Duración del hitbox activo
        actionCollider.SetActive(false);
    }

    private IEnumerator ActiveAttackHitBox()
    {

       yield return new WaitForSeconds(0.15f);
        attackCollider.enabled = (true);
        yield return new WaitForSeconds(0.06f); // Duración del hitbox activo
        attackCollider.enabled = (false);

        //yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 originPos = origin.position;
        Vector3 direction = origin.forward;

        hasHit = Physics.SphereCast(origin.position, castRadius, direction,
                              out debugHit, distanceMaxAction, hitMask);

        if (hasHit)
        {
            // Si golpea algo, colocamos el objeto justo en el punto de impacto
            //hitboxAction.position = hit.point;
            hitboxAction.position = debugHit.point + direction * offsetTomaxDistance;
        }
        else
        {
            // Si no golpea nada, lo colocamos a la distancia máxima + offset
            hitboxAction.position = origin.position + direction * (distanceMaxAction + offsetTomaxDistance);
        }
    }

    // Este método es llamado cuando se presiona una flecha
    private void HandleMoveCommand(Vector3 targetPos, EnumMoveDirection direction)
    {
        if (isMoving)
        {
            //Debug.Log("Ya se está moviendo, ignoro nuevo comando");
            return; // No aceptar nuevos comandos hasta terminar el actual
        }

        // Verificar si el camino está despejado
        if (IsPathClear(targetPos, direction))
        {
            StartMovement(targetPos);
        }
        else
        {
            Debug.Log("Camino bloqueado!");
            // Opcional: reproducir sonido de error
        }
        ChangeLookModel(targetPos);
    }

    private void ChangeLookModel(Vector3 targetPos)
    {
        playerModel.transform.LookAt(targetPos);
    }

    private bool IsPathClear(Vector3 targetPos, EnumMoveDirection direction)
    {
        //Debug.Log("Verificando colisiones para dirección: " + direction);

        switch (direction)
        {
            case EnumMoveDirection.Up:
                return !front.IsBlocked;
            case EnumMoveDirection.Down:
                return !back.IsBlocked;
            case EnumMoveDirection.Left:
                return !left.IsBlocked;
            case EnumMoveDirection.Right:
                return !right.IsBlocked;
            default:
                return false;
        }
    }

    private void StartMovement(Vector3 newPosition)
    {



        targetPosition = newPosition;
        isMoving = true;

        // Animación       
        //animator.SetBool(moveAnimParameter, true);

        //Debug.Log($"Iniciando movimiento hacia: {targetPosition}");
        StartCoroutine(MovePlayerSmoothly(targetPosition));
    }


    private IEnumerator MovePlayerSmoothly(Vector3 targetPos)
    {

        // Continue looping as long as the object hasn't reached the target
        while (transform.position != targetPos)
        {
            // Move the position one step closer to the target
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            // Wait until the next frame before continuing the loop
            yield return null;
        }

        isMoving = false;
    }

    void OnDestroy()
    {
        enabled = false;
    }

    #region GIZMOS
    void OnDrawGizmos()
    {
        if (origin == null) return;

        Vector3 direction = origin.forward;
        Vector3 start = origin.position;
        Vector3 end = start + direction * distanceMaxAction;

        // Color base
        Gizmos.color = Color.cyan;

        // Esfera inicial
        Gizmos.DrawWireSphere(start, castRadius);

        // Esfera final
        Gizmos.DrawWireSphere(end, castRadius);

        // Línea central
        Gizmos.DrawLine(start, end);

        // Si hay impacto
        if (hasHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(debugHit.point, castRadius);
        }
    }
    #endregion

}

