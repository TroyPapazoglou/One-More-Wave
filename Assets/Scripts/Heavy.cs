using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Heavy : Enemy
{
    public override float SpawnAvailability => 40;
    public override float SpawnWeight => 40;   

    private enum HeavyState
    {
        Grounded,
        TakingOff,
        Airborne,
        Landing
    }

    [Header("Leap")]
    [SerializeField] private float leapDuration = 0.8f;
    [SerializeField] private float offscreenPadding = 1f;
    [SerializeField] private float landingDelay = 0.35f;
    [SerializeField] private float predictionTime = 0.25f;
    [SerializeField, Range(0f, 1f)]
    private float predictionChance = 0.5f;
    [SerializeField, Range(0f, 1f)] private float airborneOpacity = 0.5f;
    [SerializeField] private float fadeDuration = 0.2f;

    [Header("Landing")]
    [SerializeField] private float slamRecoveryDuration = 0.8f;
    [SerializeField] private float slamDuration = 0.5f;
    [SerializeField] private float pushRadius = 2f;
    [SerializeField] private float pushForce = 8f;
    [SerializeField] private LayerMask enemyLayers;

    [Header("Moving Player Slam")]
    [SerializeField] private float movingPlayerThreshold = 0.1f;
    [SerializeField] private float slamDistanceAhead = 4f;
    [SerializeField] private float randomRadius = 2f;

    [Header("Finisher")]
    [SerializeField] private float crushLeadTime = 0.15f;

    [Header("References")]
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Collider2D[] gameplayColliders;
    [SerializeField] private GameObject landingMarker;
    [SerializeField] private GameObject landingEffect;

    private HeavyState state = HeavyState.Grounded; 
    private Camera mainCamera;
    private Vector2 landingPosition;
    private float landingZoneWidth;
    private Vector2 startSize;

    public override void Reset()
    {
        base.Reset();
        transform.localScale = startSize;
        rb.simulated = true;
        state = HeavyState.Grounded;
        if (landingMarker != null)
            landingMarker.SetActive(true);
    }

    public override void Disable()
    {
        base.Disable();

        StopAllCoroutines();
        Tween.StopAll(onTarget: transform);

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        SetGameplayCollision(false);

        if (landingMarker != null)
        {
            landingMarker.SetActive(false);
            landingMarker.transform.SetParent(transform);
            landingMarker.transform.localPosition = Vector3.zero;
        }
    }

    protected override void Awake()   
    {
        base.Awake();
        mainCamera = Camera.main;
        startSize = transform.localScale;
    }

    public override bool Move(Player player)
    {
        if (!base.Move(player))
            return false;

        if (state != HeavyState.Grounded)
            return true;       

        BeginLeap(player);

        return true;
    }

    public override void FadeOutVisuals(float duration, float delay = 0f, System.Action onComplete = null)
    {
        base.FadeOutVisuals(duration, delay, onComplete);

        if (landingMarker != null)
            landingMarker.SetActive(false);
    }

    protected override SpriteRenderer[] GetVisualRenderers()
    {        
        return new SpriteRenderer[]
        {
            body           
        };
    }

    protected override void SetVisualRootsActive(bool active)
    {
        base.SetVisualRootsActive(active);

        if (landingMarker != null)
            landingMarker.SetActive(active);
    }

    public void BeginLeap(Player player)
    {
        if (state != HeavyState.Grounded)
            return;

        if (player == null || mainCamera == null)
            return;

        StartCoroutine(LeapRoutine(player));
    }

    private IEnumerator LeapRoutine(Player player)
    {        
        state = HeavyState.TakingOff;

        landingPosition = ChooseLandingPosition(player);

        if (landingMarker != null)
        {
            landingMarker.transform.SetParent(null);
            landingMarker.transform.position = landingPosition;
            landingMarker.SetActive(true);

            SpriteRenderer markerSprite = landingMarker.GetComponentInChildren<SpriteRenderer>();
            landingZoneWidth = markerSprite.bounds.size.x;
        }

        SetGameplayCollision(false);
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;            
       

        Vector3 start = transform.position;
        Vector3 end = new Vector3(
            start.x,
            mainCamera.ViewportToWorldPoint(
                new Vector3(0.5f, 1f, 0f)).y + offscreenPadding,
            start.z
        );

        yield return StartCoroutine(FadeTo(airborneOpacity, fadeDuration));

        float elapsed = 0f;

        while (elapsed < leapDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / leapDuration);
            transform.position = Vector3.Lerp(start, end, progress);

            yield return null;
        }

        transform.position = end;
        state = HeavyState.Airborne;      

        yield return new WaitForSeconds(landingDelay);

        state = HeavyState.Landing;
              
        Vector3 landingTarget = new Vector3(
            landingPosition.x,
            landingPosition.y,
            transform.position.z
        );

        Vector3 descentStart = transform.position;
        body.enabled = true;

        float descentElapsed = 0f;
        bool crushChecked = false;
        bool playerCrushed = false; 

        while (descentElapsed < slamDuration)
        {
            descentElapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                descentElapsed / slamDuration
            );
                     
            progress *= progress;

            transform.position = Vector3.Lerp(
                descentStart,
                landingTarget,
                progress
            );

            if (!crushChecked && descentElapsed >= slamDuration - crushLeadTime)
            {
                crushChecked = true;

                if (player != null && player.TryGetComponent(out Collider2D playerCol))
                {
                    Vector2 closest = playerCol.ClosestPoint(landingPosition);
                    playerCrushed = Vector2.Distance(closest, landingPosition) <= landingZoneWidth * 0.5f;
                }

                if (playerCrushed)
                {
                    PlayCrushDeathAnimation(player);
                    player.KillPlayerCrushed(this);
                    StartCoroutine(FadeTo(1f, fadeDuration));
                }
             
            }
            yield return null;
        }

        transform.position = landingTarget;

        yield return StartCoroutine(FadeTo(1f, fadeDuration));

        if (landingMarker != null)
        {
            landingMarker.SetActive(false);
            landingMarker.transform.SetParent(transform);
            landingMarker.transform.localPosition = Vector3.zero;
        }        

        PushNearbyEnemies();

        if (landingEffect != null)
        {
            Instantiate(landingEffect, landingPosition, Quaternion.identity);
        }

        rb.constraints |= RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        rb.position = landingPosition;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = true;

        SetGameplayCollision(true);
                
        yield return new WaitForSeconds(slamRecoveryDuration);

        rb.constraints &= ~(RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY);
        state = HeavyState.Grounded;
    }

    private Vector2 ChooseLandingPosition(Player player)
    {
        Vector2 target = player.transform.position;

        if (player.TryGetComponent<Rigidbody2D>(out var playerRb))
        {
            Vector2 velocity = playerRb.linearVelocity;

            if (velocity.magnitude > movingPlayerThreshold)
            {
                Vector2 direction = velocity.normalized;

                Vector2 targetCentre = target + direction * slamDistanceAhead;

                target = targetCentre + Random.insideUnitCircle * randomRadius;
            }
        }

        return LevelBounds.Instance.ClampPosition(target);
    } 

    private void SetGameplayCollision(bool enabled)
    {
        foreach (Collider2D col in gameplayColliders)
        {
            if (col != null)
                col.enabled = enabled;
        }
    }

    private void PushNearbyEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            landingPosition,
            pushRadius,
            enemyLayers
        );               

        foreach (Collider2D hit in hits)
        {
            if (hit.transform.root == transform.root)
                continue;

            Rigidbody2D otherRb = hit.attachedRigidbody;

            if (otherRb == null)                      
                continue;            

            if (otherRb.bodyType != RigidbodyType2D.Dynamic)                        
                continue;            

            Vector2 direction = otherRb.position - landingPosition;

            if (direction.sqrMagnitude < 0.001f)
                direction = Random.insideUnitCircle.normalized;
            else
                direction.Normalize();

            Enemy enemy = hit.GetComponentInParent<Enemy>();

            if (enemy is Heavy)
                continue;

            if (enemy != null)
            {
                enemy.ApplyKnockback(direction, pushForce, 0.3f);
            }        
        }
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (body == null)
            yield break;

        Color color = body.color;
        float startAlpha = color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            body.color = color;

            yield return null;
        }

        color.a = targetAlpha;
        body.color = color;
    }

    public override void PlayDeathAnimation(Vector2 contactPoint, Player player, Enemy enemy)
    {
        base.PlayDeathAnimation(contactPoint, player, enemy);

        DeathManager.Instance.PlayDeathAnimation(EnemyType.Heavy, contactPoint, player, enemy);
    }

    public void PlayCrushDeathAnimation(Player player)
    {
        DeathManager.Instance.PlayHeavyCrushAnimation(player, this);
    }
}
