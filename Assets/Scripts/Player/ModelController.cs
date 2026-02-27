using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelController : MonoBehaviour
{
    //Modelos / parents de los objetos a utilizar
    [Header("Modelos / parents de los objetos a utilizar")]
    public GameObject picaxe;
    public GameObject sword;
    public GameObject ranged;

    [Header("References")]
    private PlayerUtilities playerUtilities;

    private void Awake()
    {
        playerUtilities = FindFirstObjectByType<PlayerUtilities>();

    }

    // Start is called before the first frame update
    void Start()
    {



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

    }

    public void ExecuteAnimation(PlayerAnimation animation)
    {
        if (animation == PlayerAnimation.Dodge)
            this.GetComponent<Animator>().SetTrigger("Dodge");
        else
        {


            if (playerUtilities.GetActualToolSelected() == EnumActualToolSelected.Pickaxe)
                this.GetComponent<Animator>().SetTrigger("Pick");

            if (playerUtilities.GetActualToolSelected() == EnumActualToolSelected.Melee)
                this.GetComponent<Animator>().SetTrigger("Attack");
        }


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

