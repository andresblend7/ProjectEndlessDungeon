using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DirectionalControls : MonoBehaviour,  IPointerClickHandler, IPointerUpHandler
{

    private string lastArrowPressed = "";
    private bool isPressed = false;
    private RawImage imgArrowPressed;
    private Color normalColor = Color.white;
    private Color pressedColor = Color.blue;

    // This function is called when a pointer click (press and release) is completed on the RawImage
    public void OnPointerClick(PointerEventData eventData)
    {
        return;
        //Debug.Log("RawImage was clicked/tapped!" + eventData.pointerCurrentRaycast.gameObject.name);

        lastArrowPressed = eventData.pointerCurrentRaycast.gameObject.name;
        imgArrowPressed = eventData.pointerCurrentRaycast.gameObject.GetComponent<RawImage>();
        imgArrowPressed.color = pressedColor;

        EnumMoveDirection direction = EnumMoveDirection.None;
        switch (lastArrowPressed)
        {
            case "Up":
                direction = EnumMoveDirection.Up;
                break;
            case "Down":
                direction = EnumMoveDirection.Down;
                break;
            case "Left":
                direction = EnumMoveDirection.Left;
                break;
            case "Right":
                direction = EnumMoveDirection.Right;
                break;
            default:
                Debug.LogError("Flecha no reconocida");
                return;
        }

        InputManager.Instance.SetDirectionPressed(direction, true);
        // Add your custom logic here
    }

    // Cuando se suelta la flecha
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        if(imgArrowPressed!= null)
            imgArrowPressed.color = normalColor;

        // Informar al InputManager que esta dirección ya no está presionada
        //InputManager.Instance.SetDirectionPressed(moveDirection, false);
    }
}