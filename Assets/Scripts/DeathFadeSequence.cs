using PrimeTween;
using System.Collections;
using System.Linq;
using UnityEngine;

public class DeathFadeSequence : MonoBehaviour
{
    public static DeathFadeSequence Instance { get; private set; }

    [SerializeField] private float enemyFadeDuration = 0.25f;
    [SerializeField] private float enemyStagger = 0.1f;
    [SerializeField] private float maxWaitDuration = 2f;

    private void Awake() => Instance = this;


    public IEnumerator Play(Player player, Enemy killer)
    {
        yield return StartCoroutine(GameManager.Instance.OnPlayerKilled());

        Vector2 playerPos = player.transform.position;

        var ordered = EnemyManager.Instance.Enemies
            .Where(e =>
                e != null &&
                e != killer &&
                e.IsSpawned)
            .OrderByDescending(e =>
                Vector2.SqrMagnitude(
                    (Vector2)e.transform.position - playerPos
                ))
            .ToList();

        if (ordered.Count == 0)
            yield break;
               
        float stagger = enemyStagger;

        if (ordered.Count > 1)
        {
            float availableStaggerTime = maxWaitDuration - enemyFadeDuration;

            stagger = Mathf.Min(
                enemyStagger,
                Mathf.Max(0f, availableStaggerTime / (ordered.Count - 1))
            );
        }

        for (int i = 0; i < ordered.Count; i++)
        {
            float start = i * stagger;

            ordered[i].FadeOutVisuals(enemyFadeDuration, start);
        }

        yield return new WaitForSeconds(maxWaitDuration);
    }

}
