using PrimeTween;
using System.Collections;
using System.Linq;
using UnityEngine;

public class DeathFadeSequence : MonoBehaviour
{
    public static DeathFadeSequence Instance { get; private set; }

    [SerializeField] private float enemyFadeDuration = 0.25f;
    [SerializeField] private float enemyStagger = 0.1f; 

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

        float total = 0;

        for (int i = 0; i < ordered.Count; i++)
        {
            float start = i * enemyStagger;

            ordered[i].FadeOutVisuals(enemyFadeDuration, start);

            total = start + enemyFadeDuration;
        }

        yield return new WaitForSeconds(total);
    } 
}
