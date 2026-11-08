using System.Collections;
using UnityEngine;


public class Sprinter : Enemy
{
    [SerializeField] private CircleCollider2D detectionCollider;

    [Header("Charge")]
    [SerializeField] private float chargeTime = 2f;
    [SerializeField] private SpriteRenderer warningSpriteRenderer;
    [SerializeField] private float warningPulseSpeed = 5f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float angleOffset = -90f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float overshootDistance = 6f;
    [SerializeField] private float dashMass = 1000f;

    [Header("Stun")]
    [SerializeField] private float stunTime = 1f;

    private enum State { Moving, Charging, Dashing, Stunned }
    private State state;
    private bool dashBlocked;
    private float originalMass;

    public override float SpawnAvailability => 20;
    public override float SpawnWeight => 50;
    private Coroutine dashCouroutine = null;
    private Coroutine warningCoroutine;
    private Color originalBodyColor;
    private Vector3 originalWarningScale;


    protected override void Awake()
    {
        base.Awake();

        originalBodyColor = visual.color;
        originalWarningScale = warningSpriteRenderer.transform.localScale;

        warningSpriteRenderer.enabled = false;
    }

    public override void Reset()
    {
        base.Reset();
        rb.simulated = true;
    }

    public override void Disable()
    {
        base.Disable();
        EndChargeVisuals();
        if (dashCouroutine != null) StopCoroutine(dashCouroutine);
        dashCouroutine = null;
        if (originalMass > 0f) rb.mass = originalMass;
        state = State.Moving;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        rb.freezeRotation = false;
    }

    public override bool Move(Player player)
    {
        if (state != State.Moving)
            return true;

        if (!base.Move(player))
            return false;

        Vector2 direction = (Vector2)(player.transform.position - transform.position);

        rb.linearVelocity = direction.normalized * moveSpeed;

        float targetAngle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg
            + angleOffset;

        float angleDifference = Mathf.DeltaAngle(
            rb.rotation,
            targetAngle
        );

        rb.angularVelocity = Mathf.Clamp(
            angleDifference / Time.fixedDeltaTime,
            -rotationSpeed,
            rotationSpeed
        );

        return true;
    }

    private void LateUpdate()
    {
        if (warningSpriteRenderer != null)
        {
            warningSpriteRenderer.transform.rotation = Quaternion.identity;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryBeginDash(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryBeginDash(collision);
    }
     

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (state != State.Dashing) return;
              
        if (collision.gameObject.TryGetComponent<Player>(out _)) return;
        if (collision.gameObject.TryGetComponent<Enemy>(out _)) return;

        dashBlocked = true;  
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (state != State.Dashing) return;

        if (collision.gameObject.TryGetComponent<Player>(out _)) return;
        if (collision.gameObject.TryGetComponent<Enemy>(out _)) return;

        dashBlocked = true;
    }

    private void TryBeginDash(Collider2D collision)
    {
        if (state != State.Moving) return;

        if (collision.TryGetComponent(out Player player))
        {
            rb.linearVelocity = Vector2.zero;
            BeginDashing(player);
        }
    }


    public void BeginDashing(Player player)
    {
        if (dashCouroutine != null) StopCoroutine(dashCouroutine);
        dashCouroutine = StartCoroutine(DashRoutine(player));
    }

    private void BeginChargeVisuals(float chargeDuration)
    {
        EndChargeVisuals();

        warningSpriteRenderer.enabled = true;
        warningSpriteRenderer.transform.localScale = originalWarningScale * 0.5f;

        warningCoroutine = StartCoroutine(
            ChargeWarningRoutine(chargeDuration)
        );
    }

    private IEnumerator ChargeWarningRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);
         
            float popProgress = Mathf.Clamp01(elapsed / 0.18f);
            float pop = Mathf.Lerp(
                0.3f,
                1.3f,
                Mathf.Sin(popProgress * Mathf.PI * 0.5f)
            );

            float pulseSpeed = Mathf.Lerp(2.5f, 7f, progress);
            float pulseStrength = Mathf.Lerp(0.12f, 0.25f, progress);

            float pulse = 1f + Mathf.Sin(
                elapsed * pulseSpeed * Mathf.PI * 2f
            ) * pulseStrength;
                      
            float finalPop = 1f;

            if (progress > 0.85f)
            {
                float finalProgress = Mathf.InverseLerp(0.85f, 1f, progress);
                finalPop = Mathf.Lerp(1f, 1.5f, finalProgress);
            }

            warningSpriteRenderer.transform.localScale = finalPop * pop * pulse * originalWarningScale;
                                 

            float flashSpeed = Mathf.Lerp(2f, 7f, progress);

            float flashAmount = Mathf.PingPong(elapsed * flashSpeed, 1f);
            flashAmount = Mathf.SmoothStep(0f, 1f, flashAmount);
            flashAmount *= Mathf.Lerp(0.4f, 0.8f, progress);

            if (progress > 0.85f)
            {
                flashAmount = Mathf.Lerp(
                    flashAmount,
                    1f,
                    Mathf.InverseLerp(0.85f, 1f, progress)
                );
            }

            visual.color = Color.Lerp(
                originalBodyColor,
                Color.white,
                flashAmount
            );

            yield return null;
        }

        warningCoroutine = null;
    }

    private void EndChargeVisuals()
    {
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }

        warningSpriteRenderer.enabled = false;
        warningSpriteRenderer.transform.localScale = originalWarningScale;
        visual.color = originalBodyColor;
    }

    private IEnumerator DashRoutine(Player player)
    {
        rb.constraints |= RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        state = State.Charging;
        BeginChargeVisuals(chargeTime);
        rb.linearVelocity = Vector2.zero;

        float elapsed = 0f;
        while (elapsed < chargeTime)
        {
            yield return new WaitForFixedUpdate();

            Vector2 toPlayer = (Vector2)player.transform.position - rb.position;
            float targetAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg + angleOffset;
            float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newAngle);

            elapsed += Time.fixedDeltaTime;
        }
                       
        Vector2 start = rb.position;
        Vector2 toTarget = (Vector2)player.transform.position - start;
        Vector2 dashDir = toTarget.normalized;
        float dashDistance = toTarget.magnitude + overshootDistance;

        rb.constraints &= ~(RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY);
        EndChargeVisuals();
        rb.freezeRotation = true;
        state = State.Dashing;
        dashBlocked = false;

        originalMass = rb.mass;
        rb.mass = dashMass;

        while (!dashBlocked && Vector2.Distance(start, rb.position) < dashDistance)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            yield return new WaitForFixedUpdate();
        }

        rb.mass = originalMass;
        rb.linearVelocity = Vector2.zero;

        if (dashBlocked)
        {
            rb.constraints |= RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
            state = State.Stunned;
            yield return new WaitForSeconds(stunTime);           
        }

        rb.constraints &= ~(RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY);
        rb.freezeRotation = false;
        state = State.Moving;
        dashBlocked = false;
    }

    public override void PlayDeathAnimation(Vector2 contactPoint, Player player, Enemy enemy)
    {
        base.PlayDeathAnimation(contactPoint, player, enemy);

        DeathManager.Instance.PlayDeathAnimation(EnemyType.Sprinter, contactPoint, player, enemy);
    }   
}
