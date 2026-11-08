
using System.Collections.Generic;
using UnityEngine;

public class OffscreenEnemyIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform arrowPrefab;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Camera mainCamera;

    [Header("Detection")]
    [SerializeField] private float detectionConeAngle = 90f;
    [SerializeField] private float minimumMovementSpeed = 0.2f;

    [Header("Indicator")]
    [SerializeField] private float edgePadding = 35f;
    [SerializeField] private float lingerDuration = 0.6f;

    private Player player;
    private Rigidbody2D playerRb;

    private readonly Dictionary<Enemy, RectTransform> activeIndicators = new();
    private readonly Dictionary<Enemy, float> lingerTimers = new();
    private readonly Stack<RectTransform> indicatorPool = new();

    private float halfConeAngleCos;

    private void Awake()
    {
        halfConeAngleCos = Mathf.Cos(
            detectionConeAngle * 0.5f * Mathf.Deg2Rad
        );
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayer();
        playerRb = player.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (player == null || playerRb == null)
            return;

        Vector2 velocity = playerRb.linearVelocity;

        bool isMoving = velocity.magnitude > minimumMovementSpeed;

        Vector2 movementDirection = isMoving
            ? velocity.normalized
            : Vector2.zero;

        foreach (Enemy enemy in EnemyManager.Instance.Enemies)
        {
            if (enemy == null || !enemy.IsSpawned)
            {
                ReturnIndicator(enemy);
                continue;
            }

            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(
                enemy.transform.position
            );
                      
            if (IsOnScreen(viewportPosition))
            {
                ReturnIndicator(enemy);
                continue;
            }

            bool isInCone = isMoving &&
                IsInsideDetectionCone(enemy, movementDirection);

            if (isInCone)
            {
                lingerTimers[enemy] = lingerDuration;

                if (!activeIndicators.ContainsKey(enemy))
                    GetIndicator(enemy);
            }
            else if (activeIndicators.ContainsKey(enemy))
            {
                lingerTimers[enemy] -= Time.deltaTime;

                if (lingerTimers[enemy] <= 0f)
                    ReturnIndicator(enemy);
            }
        }

        UpdateIndicators();
    }

    private bool IsInsideDetectionCone(Enemy enemy, Vector2 movementDirection)
    {
        Vector2 toEnemy = (Vector2)enemy.transform.position - (Vector2)player.transform.position;

        if (toEnemy.sqrMagnitude < 0.001f)
            return false;

        float dot = Vector2.Dot(
            movementDirection,
            toEnemy.normalized
        );

        return dot >= halfConeAngleCos;
    }

    private bool IsOnScreen(Vector3 viewportPosition)
    {
        return viewportPosition.z > 0f &&
               viewportPosition.x >= 0f &&
               viewportPosition.x <= 1f &&
               viewportPosition.y >= 0f &&
               viewportPosition.y <= 1f;
    }

    private void GetIndicator(Enemy enemy)
    {
        RectTransform arrow;

        if (indicatorPool.Count > 0)
        {
            arrow = indicatorPool.Pop();
        }
        else
        {
            arrow = Instantiate(arrowPrefab, canvasRect);
        }

        arrow.gameObject.SetActive(true);

        activeIndicators.Add(enemy, arrow);
    }

    private void ReturnIndicator(Enemy enemy)
    {
        if (enemy == null || !activeIndicators.TryGetValue(enemy, out RectTransform arrow))
            return;

        activeIndicators.Remove(enemy);
        lingerTimers.Remove(enemy);

        arrow.gameObject.SetActive(false);
        indicatorPool.Push(arrow);
    }

    private void UpdateIndicators()
    {
        foreach (var pair in activeIndicators)
        {
            Enemy enemy = pair.Key;
            RectTransform arrow = pair.Value;

            if (enemy == null)
                continue;

            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(enemy.transform.position);

            Vector2 screenDirection;

            if (viewportPosition.z < 0f)
            {             
                screenDirection = new Vector2(0.5f - viewportPosition.x, 0.5f - viewportPosition.y);
            }
            else
            {
                screenDirection = new Vector2(viewportPosition.x - 0.5f, viewportPosition.y - 0.5f);
            }

            if (screenDirection.sqrMagnitude < 0.001f)
                continue;

            screenDirection.Normalize();
                      
            float halfWidth = canvasRect.rect.width * 0.5f - edgePadding;
            float halfHeight = canvasRect.rect.height * 0.5f - edgePadding;

            float xDistance = Mathf.Abs(screenDirection.x) > 0.001f
                ? halfWidth / Mathf.Abs(screenDirection.x)
                : float.PositiveInfinity;

            float yDistance = Mathf.Abs(screenDirection.y) > 0.001f
                ? halfHeight / Mathf.Abs(screenDirection.y)
                : float.PositiveInfinity;

            float edgeDistance = Mathf.Min(xDistance, yDistance);

            Vector2 edgePosition = screenDirection * edgeDistance;

            arrow.anchoredPosition = edgePosition;
                      
            float angle = Mathf.Atan2(
                screenDirection.y,
                screenDirection.x
            ) * Mathf.Rad2Deg - 90f;

            arrow.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
