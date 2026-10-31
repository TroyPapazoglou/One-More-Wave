using PrimeTween;
using System.Collections;
using System.Linq;
using UnityEngine;

public class DeathFadeSequence : MonoBehaviour
{
    public static DeathFadeSequence Instance { get; private set; }

    [SerializeField] private GameObject[] walls;        
    [SerializeField] private float wallFadeDuration = 0.5f;
    [SerializeField] private float enemyFadeDuration = 0.25f;
    [SerializeField] private float enemyStagger = 0.1f; 

    private void Awake() => Instance = this;

    public IEnumerator Play(Player player, Enemy killer)
    {
        foreach (GameObject wall in walls)
        {
            if (wall != null)
                FadeAll(wall, wallFadeDuration, 0f);
        }

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

        float total = wallFadeDuration;

        for (int i = 0; i < ordered.Count; i++)
        {
            float start = wallFadeDuration + i * enemyStagger;

            ordered[i].FadeOutVisuals(enemyFadeDuration, start);

            total = start + enemyFadeDuration;
        }

        yield return new WaitForSeconds(total);
    }

    private void FadeAll(GameObject target, float duration, float delay)
    {
        foreach (SpriteRenderer sr in target.GetComponentsInChildren<SpriteRenderer>())
            Tween.Alpha(sr, 0f, duration, startDelay: delay);
    }
}
