using UnityEngine;
using PrimeTween;
using Sequence = PrimeTween.Sequence;
using System;
using Random = UnityEngine.Random;


public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance;


    [Header("Sprinter Death")]
    [SerializeField] private int slashCount = 5;
    [SerializeField] private float slashDuration = 0.1f;
    [SerializeField] private float slashGap = 0.3f;        // pause after a slash before teleporting
    [SerializeField] private float teleportRadius = 5f;
    [SerializeField] private float overshoot = 2f;
    [SerializeField] private float trailRestartDelay = 0.05f;

    [Header("Heavy Death")]
    [SerializeField] private float heavyRiseDuration = 0.6f;
    [SerializeField] private float heavyHangTime = 0.4f;     // pause off screen before the drop
    [SerializeField] private float heavyFallDuration = 0.25f;
    [SerializeField] private float heavySquashDuration = 0.15f;
    [SerializeField] private float offscreenMargin = 3f;
    [SerializeField] private ParticleSystem HeavyImpactParticle;

    [Header("References")]
    [SerializeField] private ParticleSystem ChaserContactParticle;
    [SerializeField] private ParticleSystem ChaserPulseSwirlParticle;
    [SerializeField] private TrailRenderer SprinterArcTrail;
    [SerializeField] private TrailRenderer SprinterSpikesTrail;
    [SerializeField] private TrailRenderer SprinterSmoothSpikesTrail;
    [SerializeField] private ParticleSystem SprinterImpactParticle;
    [SerializeField] private ParticleSystem SprinterSlashParticle;

    public event Action OnDeathAnimationFinished;

    private Camera mainCamera;
    private Sequence DeathTween;


    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
    }

    public void PlayDeathAnimation(EnemyType type, Vector2 contactPoint, Player player, Enemy enemy)
    {
        if (DeathTween.isAlive)
            DeathTween.Stop();

        switch (type)
        {
            case EnemyType.Chaser:
                PlayChaserDeathAnimation(contactPoint, player, enemy);
                break;
            case EnemyType.Sprinter:
                PlaySprinterDeathAnimation(contactPoint, player, enemy);
                break;
            case EnemyType.Heavy:
                PlayHeavyDeathAnimation(contactPoint, player, enemy);
                break;
        }
    }

    public void PlayChaserDeathAnimation(Vector2 contactPoint, Player player, Enemy enemy)
    {
        ChaserContactParticle.transform.position = contactPoint;
        ChaserContactParticle.Play();

        Vector3 playerPos = player.transform.position;
        Vector3 swirlPos = ChaserPulseSwirlParticle.transform.position;

        ChaserPulseSwirlParticle.transform.position = new Vector3(playerPos.x, playerPos.y, swirlPos.z);

        DeathTween = Sequence.Create(
            Tween.Position(enemy.transform, player.transform.position, .5f, startDelay: 1f))   // 0s to 2s
              .Chain(Tween.Scale(enemy.transform, 1.5f, 1f))                                        // 2s to 3s
              .InsertCallback(1.5f, () => Tween.ShakeCamera(Camera.main, strengthFactor: .8f))
              .InsertCallback(2, () => player.HidePin())
              .InsertCallback(1.7f, () =>                                                           // swirl starts halfway through the enemy scale
              {
                  Vector3 p = player.transform.position;
                  ChaserPulseSwirlParticle.transform.position = new Vector3(p.x, p.y, ChaserPulseSwirlParticle.transform.position.z);
                  ChaserPulseSwirlParticle.Play();
                  Tween.ShakeCamera(Camera.main, strengthFactor: 1.2f, frequency: 50, duration: 2.5f);
              })
              .Insert(2.5f, Tween.Scale(ChaserPulseSwirlParticle.transform, 1.5f, 1.5f))            // 2.5s to 4s: swirl grows
              .Insert(2.5f, Tween.EulerAngles(                                                      // spin while it grows
                  ChaserPulseSwirlParticle.transform,
                  Vector3.zero,
                  new Vector3(0, 0, 360),
                  1.5f,
                  Ease.Linear))
              .InsertCallback(4.2f, () => player.Hide())                 // 4s: player disappears
              .Insert(4.2f, Tween.Scale(ChaserPulseSwirlParticle.transform, Vector3.zero, 0.5f))      // 4s to 4.5s: swirl shrinks to 0
              .OnComplete(() => { enemy.FadeOutVisuals(0.1f, 0, () => OnDeathAnimationFinished?.Invoke()); })
              .Insert(4.2f, Tween.Scale(enemy.transform, Vector3.zero, 0.5f));


    }

    public void PlaySprinterDeathAnimation(Vector2 contactPoint, Player player, Enemy enemy)
    {
        SprinterArcTrail.transform.SetParent(enemy.transform, false);
        SprinterSpikesTrail.transform.SetParent(enemy.transform, false);
        SprinterSmoothSpikesTrail.transform.SetParent(enemy.transform, false);

        SetTrails(true);

        Vector3 playerPos = player.transform.position;
        float z = enemy.transform.position.z;

        // ---------- first slash (from where the enemy already is) ----------
        float slashStart = 1f;
        Vector3 from = enemy.transform.position;
        Vector3 firstEnd = GetSlashEnd(from, playerPos, z);    

        DeathTween = Sequence.Create(
            Tween.Position(enemy.transform, from, GetSlashEnd(from, playerPos, z),
                slashDuration, startDelay: slashStart));

        // ---------- extra slashes ----------
        for (int i = 1; i < slashCount; i++)
        {
            float teleportTime = slashStart + slashDuration + slashGap;
            slashStart = teleportTime + trailRestartDelay;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 teleportPos = new Vector3(
                playerPos.x + Mathf.Cos(angle) * teleportRadius,
                playerPos.y + Mathf.Sin(angle) * teleportRadius,
                z);
            Vector3 endPos = GetSlashEnd(teleportPos, playerPos, z);
            Quaternion faceRot = FacingRotation(teleportPos, endPos);

            // trails off, jump, wipe whatever was drawn
            DeathTween.InsertCallback(teleportTime, () =>
            {
                Tween.ShakeCamera(Camera.main, strengthFactor: 1f, frequency: 50, startDelay:.075f);
                SetTrails(false);
                enemy.transform.SetPositionAndRotation(teleportPos, faceRot);
                ClearTrails();
            });

            // trails back on, then slash
            DeathTween.InsertCallback(slashStart, () => SetTrails(true));
            DeathTween.Insert(slashStart, Tween.Position(enemy.transform, teleportPos, endPos, slashDuration));
        }

        // ---------- impact after the final slash lands ----------
        float impactTime = slashStart + slashDuration;
        DeathTween.InsertCallback(impactTime, () =>
        {
            SprinterImpactParticle.transform.position = new Vector3(
                player.transform.position.x,
                player.transform.position.y,
                SprinterImpactParticle.transform.position.z);

            SprinterSlashParticle.transform.position = new Vector3(
                player.transform.position.x,
                player.transform.position.y,
                SprinterSlashParticle.transform.position.z);

            SprinterImpactParticle.Play();
            SprinterSlashParticle.Play();

            player.Hide();

            enemy.FadeOutVisuals(1, 1f, () => 
            {
                OnDeathAnimationFinished?.Invoke();
                SprinterArcTrail.transform.SetParent(null, false);
                SprinterSpikesTrail.transform.SetParent(null, false);
                SprinterSmoothSpikesTrail.transform.SetParent(null, false);
                SetTrails(false);
            });

            Tween.ShakeCamera(Camera.main, strengthFactor: 1f);
        });
    }

    private Vector3 GetSlashEnd(Vector3 from, Vector3 playerPos, float z)
    {
        Vector3 dir = playerPos - from;
        dir.z = 0f;
        dir.Normalize();

        Vector3 end = playerPos + dir * overshoot;
        end.z = z;
        return end;
    }

    private void SetTrails(bool on)
    {
        SprinterArcTrail.enabled = on;
        SprinterSpikesTrail.enabled = on;
        SprinterSmoothSpikesTrail.enabled = on;
    }

    private void ClearTrails()
    {
        SprinterArcTrail.Clear();
        SprinterSpikesTrail.Clear();
        SprinterSmoothSpikesTrail.Clear();
    }

    private Quaternion FacingRotation(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, angle - 90f);
    }

    public void PlayHeavyDeathAnimation(Vector2 contactPoint, Player player, Enemy enemy)
    {
        float topY = mainCamera.transform.position.y + mainCamera.orthographicSize + offscreenMargin;
                
        enemy.transform.rotation = Quaternion.identity;  
        SpriteRenderer playerSprite = player.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer enemySprite = enemy.GetComponentInChildren<SpriteRenderer>();

        Vector3 enemyPos = enemy.transform.position;
        Vector3 playerPos = player.transform.position;
               
        float playerBottom = playerSprite.bounds.min.y;
        float playerTop = playerSprite.bounds.max.y;
        float pivotToBottom = playerPos.y - playerBottom;
        float pivotToTop = playerTop - playerPos.y;
        float enemyBottomOffset = enemyPos.y - enemySprite.bounds.min.y;

        Vector3 playerStartScale = player.transform.localScale;

        Vector3 riseEnd = new (enemyPos.x, topY, enemyPos.z);
        Vector3 dropStart = new (playerPos.x, topY, enemyPos.z);
        Vector3 landPos = new (playerPos.x, playerTop + enemyBottomOffset, enemyPos.z);
        Vector3 impactPos = new (playerPos.x, playerBottom, HeavyImpactParticle.transform.position.z);

        DeathTween = Sequence.Create(
            Tween.Position(enemy.transform, enemyPos, riseEnd, heavyRiseDuration, Ease.InQuad, startDelay: 1f))
                .ChainCallback(() => enemy.transform.position = dropStart)
                .ChainDelay(heavyHangTime)
                .Chain(Tween.Position(enemy.transform, dropStart, landPos, heavyFallDuration, Ease.InQuad))
                .ChainCallback(() =>
                {
                    HeavyImpactParticle.transform.position = impactPos;
                    HeavyImpactParticle.Play();
                    Tween.ShakeCamera(Camera.main, strengthFactor: 2f, frequency: 50);
                })
                .Chain(Tween.Custom(1f, 0f, heavySquashDuration, s =>
                {              
                    Transform p = player.transform;
                    p.localScale = new Vector3(playerStartScale.x, playerStartScale.y * s, playerStartScale.z);
                    p.position = new Vector3(playerPos.x, playerBottom + pivotToBottom * s, playerPos.z);
                               
                    float currentTop = playerBottom + (pivotToBottom + pivotToTop) * s;
                    enemy.transform.position = new Vector3(dropStart.x, currentTop + enemyBottomOffset, enemyPos.z);
                }, Ease.OutQuad))
                .ChainCallback(() => 
                {
                    player.Hide();
                    enemy.FadeOutVisuals(1, 0, () => OnDeathAnimationFinished?.Invoke());
                });
    }

    public void PlayHeavyCrushAnimation(Player player, Enemy enemy)
    {
        SpriteRenderer playerSprite = player.GetComponentInChildren<SpriteRenderer>();

        Vector3 playerPos = player.transform.position;
        Vector3 startScale = player.transform.localScale;
        float playerBottom = playerSprite.bounds.min.y;
        float pivotToBottom = playerPos.y - playerBottom;

        HeavyImpactParticle.transform.position = new Vector3(
            enemy.transform.position.x, playerBottom, HeavyImpactParticle.transform.position.z);

        HeavyImpactParticle.Play();

        Tween.ShakeCamera(Camera.main, strengthFactor: 2f, frequency: 50);

        DeathTween = Sequence.Create(
            Tween.Custom(1f, 0f, heavySquashDuration, s =>
            {
                Transform p = player.transform;
                p.localScale = new Vector3(startScale.x, startScale.y * s, startScale.z);
                p.position = new Vector3(playerPos.x, playerBottom + pivotToBottom * s, playerPos.z);
            }, Ease.OutQuad))
            .ChainCallback(() => 
            { 
                player.Hide();
                enemy.FadeOutVisuals(1, 0, () => OnDeathAnimationFinished?.Invoke());
            });
    }
  
}
