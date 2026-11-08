using UnityEngine;

public class LevelBounds : MonoBehaviour
{
    public static LevelBounds Instance { get; private set; }

    [Header("Playable Area")]
    [SerializeField] private BoxCollider2D playableBounds;

    public Bounds Bounds => playableBounds.bounds;

    private void Awake()
    {
        Instance = this;
    }

    public Vector2 ClampPosition(Vector2 position)
    {
        Bounds bounds = playableBounds.bounds;

        position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);

        return position;
    }

    public Vector2 GetRandomPosition()
    {
        Bounds bounds = playableBounds.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);

        return new Vector2(randomX, randomY);
    }

    public float GetMaxDistanceInDirection(Vector2 position, Vector2 direction)
    {
        Bounds bounds = playableBounds.bounds;

        float maxDistance = float.PositiveInfinity;

        if (direction.x > 0)
            maxDistance = Mathf.Min(maxDistance, (bounds.max.x - position.x) / direction.x);
        else if (direction.x < 0)
            maxDistance = Mathf.Min(maxDistance, (bounds.min.x - position.x) / direction.x);

        if (direction.y > 0)
            maxDistance = Mathf.Min(maxDistance, (bounds.max.y - position.y) / direction.y);
        else if (direction.y < 0)
            maxDistance = Mathf.Min(maxDistance, (bounds.min.y - position.y) / direction.y);

        return Mathf.Max(0f, maxDistance);
    }
}