using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] public float moveDistance = 1f;
    [SerializeField] private float moveSpeed = 5f;

    [Header("References")]
    public PlayerUtilities playerUtilities;

    // Para almacenar todas las direcciones presionadas
    private Dictionary<EnumMoveDirection, bool> activeDirections = new Dictionary<EnumMoveDirection, bool>();

    // Evento para notificar movimiento
    public delegate void MovementCommand(Vector3 targetPos, EnumMoveDirection direction);
    public static event MovementCommand OnMoveCommand;

    //evento para notificar acción seleccionada
    public delegate void ActionSelectedCommand(EnumActionType actionType);
    public static event ActionSelectedCommand OnActionSelectedCommand;




    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }


    // Llamado por los botones de flecha
    public void SetDirectionPressed(EnumMoveDirection direction, bool isPressed)
    {
       
            activeDirections[direction] = isPressed;

            // Si se acaba de presionar (no soltar), ejecutar movimiento
            if (isPressed)
            {
            //Debug.Log("InputManager: Dirección presionada " + direction);
            ExecuteMove(direction);
            }
     
    }

    // Ejecutar movimiento de 1 metro
    private void ExecuteMove(EnumMoveDirection direction)
    {

        var actualPlayerPos = playerUtilities.GetActualPosition();
         Vector3 moveVector = direction switch
        {
            EnumMoveDirection.Up => new Vector3(0, 0, moveDistance),
            EnumMoveDirection.Down => new Vector3(0, 0, -moveDistance),
            EnumMoveDirection.Left => new Vector3(-moveDistance, 0, 0),
            EnumMoveDirection.Right => new Vector3(moveDistance, 0, 0),
            //EnumMoveDirection.UpLeft => new Vector3(-moveDistance, 0, moveDistance).normalized * moveDistance,
            //EnumMoveDirection.UpRight => new Vector3(moveDistance, 0, moveDistance).normalized * moveDistance,
            //EnumMoveDirection.DownLeft => new Vector3(-moveDistance, 0, -moveDistance).normalized * moveDistance,
            //EnumMoveDirection.DownRight => new Vector3(moveDistance, 0, -moveDistance).normalized * moveDistance,
            _ => Vector3.zero
        };
         var targetPosition = actualPlayerPos + moveVector;

        // Disparar evento para que el PlayerController lo maneje
        OnMoveCommand?.Invoke(targetPosition, direction);

        // Opcional: feedback visual/sonoro
        PlayMovementFeedback();
    }

    private void PlayMovementFeedback()
    {
        // Aquí puedes añadir sonido o efectos de partículas
        // Ejemplo: AudioManager.Instance.PlaySound("Move");
    }

    public void SetActionSelected(EnumActionType actionType)
    {
        OnActionSelectedCommand.Invoke(actionType);
    }

    // Para debugging en el Editor
    void OnGUI()
    {
        if (!Application.isEditor) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== INPUT MANAGER DEBUG ===");

        foreach (var kvp in activeDirections)
        {
            GUILayout.Label($"{kvp.Key}: {(kvp.Value ? "PRESIONADO" : "libre")}");
        }
        GUILayout.EndArea();
    }
}
public enum EnumMoveDirection
{
    None = 0,
    Up,
    Down,
    Left,
    Right,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}
