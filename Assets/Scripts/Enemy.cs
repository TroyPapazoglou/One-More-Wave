using PrimeTween;
using UnityEngine;

public enum EnemyType
{
    Chaser,
    Sprinter,
    Heavy

}

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 9f;
    [SerializeField] protected GameObject visual;
    protected Rigidbody2D rb;
    public virtual float SpawnAvailability => 0; // 0 meaning can spawn from start of game
    public virtual float SpawnWeight => 100; // 100 meaning if it were this enemy and one other it would be 50/50 chance to spawn 

    private float knockbackTimer;
    private Vector2 knockbackVelocity;


    private Sequence visualFadeSequence;

    private Vector3 scale;

    public bool IsSpawned { get; private set; } = true;

    protected virtual void Awake()
    {
        if(EnemyManager.Instance != null)
            EnemyManager.Instance.AddEnemy(this);

        rb = GetComponent<Rigidbody2D>();

        scale = transform.localScale;
    }

    private void Start()
    {
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.AddEnemy(this);
    }

    public virtual bool Move(Player player) 
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
            rb.linearVelocity = knockbackVelocity;
            return false; 
        }

        return true;
    }

    public virtual void Reset() 
    {
        transform.localScale = scale;
    }


    public virtual void Disable() 
    {
        knockbackTimer = 0;
    }

    public virtual void PlayDeathAnimation(Vector2 contactPoint, Player player, Enemy enemy) { }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic)
            return;

        knockbackVelocity = direction.normalized * force;
        knockbackTimer = duration;
    }

    //public Tween HideEnemy(float delay, System.Action onComplete = null)
    //{
    //    Tween tween = Tween.Alpha(visual, 0f, 2f, startDelay: delay);

    //    if (onComplete != null)
    //        tween.OnComplete(onComplete);

    //    return tween;
    //}

    protected virtual SpriteRenderer[] GetVisualRenderers()
    {
        if (visual == null)
            return System.Array.Empty<SpriteRenderer>();

        return visual.GetComponentsInChildren<SpriteRenderer>(true);
    }

    protected virtual void SetVisualRootsActive(bool active)
    {
        if (visual != null)
            visual.SetActive(active);
    }

    public void ShowVisuals()
    {
        // Stop a previous fade if this enemy is being reused.
        if (visualFadeSequence.isAlive)
            visualFadeSequence.Stop();

        SetVisualRootsActive(true);

        foreach (SpriteRenderer sr in GetVisualRenderers())
        {
            if (sr == null)
                continue;

            Color colour = sr.color;
            colour.a = 1f;
            sr.color = colour;
        }

        SetCollidersEnabled(true);
        IsSpawned = true;
    }

    public virtual void FadeOutVisuals(float duration, float delay = 0f, System.Action onComplete = null)
    {
        if (!IsSpawned)
            return;

        IsSpawned = false;
             
        SetCollidersEnabled(false);

        if (visualFadeSequence.isAlive)
            visualFadeSequence.Stop();

        SetVisualRootsActive(true);

        SpriteRenderer[] renderers = GetVisualRenderers();

        if (renderers.Length == 0)
        {
            SetVisualRootsActive(false);
            return;
        }

        visualFadeSequence = Sequence.Create();

        foreach (SpriteRenderer sr in renderers)
        {
            if (sr == null)
                continue;

            visualFadeSequence.Group(
                Tween.Alpha(sr, 0f, duration, startDelay: delay)
            );
        }

        visualFadeSequence.OnComplete(() =>
        {
            SetVisualRootsActive(false);
            onComplete?.Invoke();
        });
    }

    public void SpawnAt(Vector2 position)
    {      
        if (visualFadeSequence.isAlive)
            visualFadeSequence.Stop();
              
        Reset();

        transform.position = position;

        ShowVisuals();
    }

    private void SetCollidersEnabled(bool enabled)
    {
        foreach (Collider2D enemyCollider in
                 GetComponentsInChildren<Collider2D>(true))
        {
            if (enemyCollider != null)
                enemyCollider.enabled = enabled;
        }
    }


}
