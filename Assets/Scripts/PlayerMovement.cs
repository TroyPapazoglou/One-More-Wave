using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody2D))]

[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Player player;
    private PlayerInput playerInput;
    private Vector2 movementInput;


    private void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Enable();
              
        playerInput.PlayerControls.Move.performed += OnMoveStarted;
        playerInput.PlayerControls.Move.canceled += OnMoveCancelled;
    }

    private void OnDestroy()
    {      
        playerInput.PlayerControls.Move.performed -= OnMoveStarted;
        playerInput.PlayerControls.Move.canceled -= OnMoveCancelled;

        playerInput.Disable();
    }
      
    void Start()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();
        gameObject.GetComponent<Transform>();
    }
   
    void FixedUpdate()
    {           
        if(player.PlayerState == PlayerState.Alive)
            rb.linearVelocity = movementInput * moveSpeed;
    }

    void OnMoveStarted(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();      
    }

    void OnMoveCancelled(InputAction.CallbackContext ctx)
    {
        movementInput = Vector2.zero;
    }
}
