using System.Collections;
using UnityEngine;


public class Sprinter : Enemy
{
    [SerializeField] private CircleCollider2D detectionCollider;

    [Header("Charge")]
    [SerializeField] private float chargeTime = 2f;
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

    public override void Reset()
    {
        base.Reset();
        rb.simulated = true;
    }

    public override void Disable()
    {
        base.Disable();
        if (dashCouroutine != null) StopCoroutine(dashCouroutine);
        dashCouroutine = null;
        if (originalMass > 0f) rb.mass = originalMass;
        state = State.Moving;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (state != State.Moving) return;

        if (collision.gameObject.TryGetComponent(out Player player))
        {          
            rb.linearVelocity = Vector2.zero;
            BeginDashing(player);
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (state != State.Dashing) return;
              
        if (collision.gameObject.TryGetComponent<Player>(out _)) return;
        if (collision.gameObject.TryGetComponent<Enemy>(out _)) return;

        dashBlocked = true;  
    }

    public void BeginDashing(Player player)
    {
        if (dashCouroutine != null) StopCoroutine(dashCouroutine);
        dashCouroutine = StartCoroutine(DashRoutine(player));
    }

    private IEnumerator DashRoutine(Player player)
    {     
        state = State.Charging;
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
            state = State.Stunned;
            yield return new WaitForSeconds(stunTime);           
        }

        state = State.Moving;
        dashBlocked = false;
    }

    public override void PlayDeathAnimation(Vector2 contactPoint, Player player, Enemy enemy)
    {
        base.PlayDeathAnimation(contactPoint, player, enemy);

        DeathManager.Instance.PlayDeathAnimation(EnemyType.Sprinter, contactPoint, player, enemy);
    }   
}
