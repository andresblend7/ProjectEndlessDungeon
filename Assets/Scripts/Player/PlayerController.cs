using System;
using System.Collections;
using System.Collections.Generic;
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
    public GameObject arrow;
    public GameObject arrowSpawner;
   
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
   
    [Header("Colliders")]
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

    // ---------------------------- Estados negativos ----------------------------
    [Header("Negative Effects VFX")]
    public GameObject PoisonVFX;
    private List<TypeOfTickDamage> tickDamageActives = new List<TypeOfTickDamage>();

    public ModelController modelController;


    // --------------------------- EVENTOS ---------------------------
    public Action<int> OnPlayerTakeDamage;


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
        InputManager.OnActionSelectedCommand += HandleActionSelectedCommand;
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
                if( playerUtilities.GetActualToolSelected() == EnumActualToolSelected.Ranged)
                    StartCoroutine(ShootProjectile());
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

    private IEnumerator ShootProjectile()
    {
        Instantiate(arrow, arrowSpawner.transform.position, transform.rotation);
        yield return new WaitForSeconds(0.15f); // Espera antes de disparar para sincronizar con la animación
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



    private void ChangeLookModel(Vector3 targetPos)
    {
        playerModel.transform.LookAt(targetPos);
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

    public int ProcessDamageToPlayer(DamageToPlayer damageToPlayer)
    {
        if (damageToPlayer.typeOfDamage == TypeOfDamage.TickOverTime)
        {
            // solo aplicar si no hay otro del mismo tipo de daño en el tiempo activo
            if(!tickDamageActives.Contains(damageToPlayer.typeOfTickDamage))
            {
                 tickDamageActives.Add(damageToPlayer.typeOfTickDamage);
                 StartCoroutine(ProcessDamagePerTick(damageToPlayer));
            }

        }
        else
        {
            ApplyDamage(damageToPlayer.baseDamageAmount);
        }
        return damageToPlayer.baseDamageAmount;
    }

    private IEnumerator ProcessDamagePerTick(DamageToPlayer damageToPlayer)
    {
        this.EnableDisableVfxNegativeEffect(damageToPlayer.typeOfTickDamage, true);
        float elapsed = 0f;

        while (elapsed < damageToPlayer.duration)
        {

            ApplyDamage(damageToPlayer.baseDamageAmount);

            yield return new WaitForSeconds(damageToPlayer.tickInterval);

            elapsed += damageToPlayer.tickInterval;
        }

        // Al finalizar, quitar el tipo de daño en el tiempo de la lista de activos
        tickDamageActives.Remove(damageToPlayer.typeOfTickDamage);
        this.EnableDisableVfxNegativeEffect(damageToPlayer.typeOfTickDamage, false);
    }

    private void EnableDisableVfxNegativeEffect(TypeOfTickDamage typeOfTickDamage, bool active)
    {
        if(typeOfTickDamage == TypeOfTickDamage.Poison)
        {
            PoisonVFX.SetActive(active);
        }
    }

    private void ApplyDamage(int damage)
    {
        modelController.TakeDamage();
        OnPlayerTakeDamage.Invoke(damage);
        DamageNumberSpawner.Spawn(
            transform.position + Vector3.up * 1.2f,
            damage,
            false,
            false
        );

        PlayerUtilities.Instance.RegisterDamageToPlayer(damage);
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

