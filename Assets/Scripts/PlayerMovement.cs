using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody2D))]

[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Joystick joystick;


    private Rigidbody2D rb;
    private Player player;
    private PlayerInput playerInput;
    private Vector2 movementInput;

    public Joystick Joystick => joystick;
    public Vector2 MovementInput => movementInput;
  


    private void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Menu.Enable();
              
        playerInput.Player.Move.performed += OnMoveStarted;
        playerInput.Player.Move.canceled += OnMoveCancelled;
        playerInput.Player.Pause.performed += OnPausePerformed;

        playerInput.Menu.Escape.performed += OnExitPerformed;
    }   

    private void OnDestroy()
    {      
        playerInput.Player.Move.performed -= OnMoveStarted;
        playerInput.Player.Move.canceled -= OnMoveCancelled;
        playerInput.Player.Pause.performed -= OnPausePerformed;

        playerInput.Menu.Escape.performed -= OnExitPerformed;

        playerInput.Dispose();
     
    }
      
    void Start()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();
        gameObject.GetComponent<Transform>();

        GameManager.Instance.OnGameStarted += OnGameStart;
        GameManager.Instance.GetPlayer().OnPlayerKilled += OnPlayerKilled;
        GameManager.Instance.OnQuitRun += OnQuitRun;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameStarted -= OnGameStart;
        GameManager.Instance.GetPlayer().OnPlayerKilled -= OnPlayerKilled;
        GameManager.Instance.OnQuitRun -= OnQuitRun;
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

    void OnExitPerformed(InputAction.CallbackContext ctx)
    {
        GameManager.Instance.ExitPressed();
    }

    void OnPausePerformed(InputAction.CallbackContext ctx)
    {     
        GameManager.Instance.PausePressed();
    }

    private void OnGameStart()
    {
        playerInput.Menu.Escape.Disable();
        playerInput.Menu.Disable();

        playerInput.Player.Enable();
    }

    private void OnPlayerKilled()
    {
        playerInput.Player.Disable();
        playerInput.Menu.Enable();
        playerInput.Menu.Escape.Disable();
    }

    private void OnQuitRun()
    {
        playerInput.Player.Disable();
        playerInput.Menu.Enable();
        playerInput.Menu.Escape.Disable();
    }

    public void EnableMainMenuInput()
    {
        playerInput.Player.Disable();
        playerInput.Menu.Enable();
        playerInput.Menu.Escape.Enable();
    }



}
