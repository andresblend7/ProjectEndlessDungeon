using UnityEngine;

public class ModelController : MonoBehaviour
{
    [Header("Modelos / parents de los objetos a utilizar")]
    public GameObject picaxe;
    public GameObject sword;
    public GameObject ranged;

    public ParticleSystem slashEffect;

    [Header("References")]
    private PlayerUtilities playerUtilities;
    private Animator animator;

    [SerializeField] public bool isExecutingAction = false;

    private void Awake()
    {
        playerUtilities = FindFirstObjectByType<PlayerUtilities>();
        animator = GetComponent<Animator>();
    }

    public void ChangeSelectTool(EnumActualToolSelected actualToolSelected)
    {
        switch (actualToolSelected)
        {
            case EnumActualToolSelected.None:
                break;

            case EnumActualToolSelected.Pickaxe:
                sword.SetActive(false);
                ranged.SetActive(false);
                picaxe.SetActive(true);
                break;

            case EnumActualToolSelected.Melee:
                picaxe.SetActive(false);
                ranged.SetActive(false);
                sword.SetActive(true);
                break;

            case EnumActualToolSelected.Ranged:
                picaxe.SetActive(false);
                sword.SetActive(false);
                ranged.SetActive(true);
                break;

            default:
                Debug.LogError("Herramienta no reconocida");
                return;
        }

        isExecutingAction = false;
    }

    public void ExecuteAnimation(PlayerAnimation animation)
    {

        //Debug.Log("ExecuteAnimation");

        if (animation == PlayerAnimation.Dodge)
        {
            animator.SetTrigger("Dodge");
            return;
        }

        if (playerUtilities.GetActualToolSelected() == EnumActualToolSelected.Pickaxe)
        {
            animator.SetTrigger("Pick");
        }

        if (playerUtilities.GetActualToolSelected() == EnumActualToolSelected.Melee)
        {
            animator.SetTrigger("Attack");

            slashEffect.gameObject.SetActive(true);
            slashEffect.Play();
        }
    }

    public void StartExecutingAction()
    {
        isExecutingAction = true;

        //Debug.Log("Empezó la animación");

        // Forzar que deje de caminar
        animator.SetBool("Walk", false);
    }

    public void StopExecutingAction()
    {
        //Debug.Log("Terminó la animación");

        isExecutingAction = false;
    }

    public void SetIsWalking(bool isWalking)
    {
        // Si está ejecutando acción, nunca permitir caminar
        if (isExecutingAction)
        {
            animator.SetBool("Walk", false);
            return;
        }

        animator.SetBool("Walk", isWalking);
    }
}

public enum PlayerAnimation
{
    Action,
    TakeDamage,
    Die,
    Idle,
    Dodge
}