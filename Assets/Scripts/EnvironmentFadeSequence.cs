using PrimeTween;
using System.Collections;
using UnityEngine;

public class EnvironmentFadeSequence : MonoBehaviour
{
    [SerializeField] private GameObject[] walls;
    [SerializeField] private float wallFadeDuration = 0.5f;

    public IEnumerator Play()
    {
        foreach (GameObject wall in walls)
        {
            if (wall != null)
                FadeAll(wall, wallFadeDuration);
        }

        float total = wallFadeDuration;

        yield return new WaitForSeconds(total);
    }

    private void FadeAll(GameObject target, float duration)
    {
        foreach (SpriteRenderer sr in target.GetComponentsInChildren<SpriteRenderer>())
            UIFader.Fade(sr, 0f, duration);
    }  
}
