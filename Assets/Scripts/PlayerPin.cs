using UnityEngine;

public class PlayerPin : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Tilt Settings")]
    [SerializeField] private float tiltAngle = 25f;
    [SerializeField] private float rotationSmoothTime = 0.15f;
    [SerializeField] private float centeredTiltAngle = 5f;

    [Header("Position Settings")]
    [SerializeField] private float leftX = -15f;
    [SerializeField] private float rightX = 15f;
    [SerializeField] private float upY = 0.8f;
    [SerializeField] private float downY = 0.35f;
    [SerializeField] private float positionSmoothTime = 0.15f;
    [SerializeField] private float centeredXOffset = 0.1f;

    [Header("Return To Centre")]
    [SerializeField] private float timeBeforeCentering = 2f;
    [SerializeField] private float centeringDuration = 1f;

    private Vector3 startingPosition;
    private Vector3 currentPosition;
    private Vector3 positionVelocity;

    private float currentAngle;
    private float angularVelocity;
    private float targetAngle;

    private float movementTimer;
    private float centeringAmount;
    private float centeringVelocity;
    private int previousHorizontalDirection;

    private void Awake()
    {
        startingPosition = transform.localPosition;
        currentPosition = startingPosition;
    }


    private void FixedUpdate()
    {
        float horizontalInput = playerMovement.MovementInput.x;
        float verticalInput = playerMovement.MovementInput.y;

        int horizontalDirection = 0;

        if (horizontalInput < -0.01f)
            horizontalDirection = 1;
        else if (horizontalInput > 0.01f)
            horizontalDirection = -1;
             
        if (horizontalDirection != 0 && horizontalDirection != previousHorizontalDirection)
        {
            movementTimer = 0f;
            centeringAmount = 0f;
            centeringVelocity = 0f;
        }

        if (horizontalDirection != 0)
        {
            movementTimer += Time.fixedDeltaTime;
        }
        else
        {
            movementTimer = 0f;
        }

        previousHorizontalDirection = horizontalDirection;
              
        float targetCentering = movementTimer >= timeBeforeCentering
            ? 1f
            : 0f;

        centeringAmount = Mathf.SmoothDamp(
            centeringAmount,
            targetCentering,
            ref centeringVelocity,
            centeringDuration
        );
              
        if (horizontalDirection < 0)
            targetAngle = Mathf.Lerp(tiltAngle, centeredTiltAngle, centeringAmount);
        else if (horizontalDirection > 0)
            targetAngle = Mathf.Lerp(-tiltAngle, -centeredTiltAngle, centeringAmount);
        else
            targetAngle = 0f;

        currentAngle = Mathf.SmoothDampAngle(
            currentAngle,
            targetAngle,
            ref angularVelocity,
            rotationSmoothTime
        );

        transform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
             
        Vector3 targetPosition = startingPosition;

        if (horizontalDirection < 0)
        {
            targetPosition.x = Mathf.Lerp(
                leftX,
                startingPosition.x - centeredXOffset,
                centeringAmount
            );
        }
        else if (horizontalDirection > 0)
        {
            targetPosition.x = Mathf.Lerp(
                rightX,
                startingPosition.x + centeredXOffset,
                centeringAmount
            );
        }
              
        if (verticalInput > 0.01f)
            targetPosition.y = upY;
        else if (verticalInput < -0.01f)
            targetPosition.y = downY;

        currentPosition = Vector3.SmoothDamp(
            currentPosition,
            targetPosition,
            ref positionVelocity,
            positionSmoothTime
        );

        transform.localPosition = currentPosition;
    }
}
