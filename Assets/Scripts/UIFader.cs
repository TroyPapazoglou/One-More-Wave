
using PrimeTween;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class UIFader
{
    private static readonly Dictionary<UnityEngine.Object, Tween> activeFades = new();

    public static void Fade(UnityEngine.Object target, float endAlpha, float duration, bool useUnscaledTime = false, Action onComplete = null)
    {        
        if (target == null)
        {
            onComplete?.Invoke();
            return;
        }

        float currentAlpha;

        switch (target)
        {
            case CanvasGroup group:
                currentAlpha = group.alpha;
                break;

            case TextMeshProUGUI text:
                currentAlpha = text.alpha;
                break;

            case SpriteRenderer sprite:
                currentAlpha = sprite.color.a;
                break;

            case Image image:
                currentAlpha = image.color.a;
                break;

            default:
                Debug.LogWarning($"UIFader does not support {target.GetType().Name}.");
                onComplete?.Invoke();
                return;
        }
              
        if (activeFades.TryGetValue(target, out Tween previousTween))
        {
            if (previousTween.isAlive)
                previousTween.Stop();

            activeFades.Remove(target);
        }

        if (Mathf.Approximately(currentAlpha, endAlpha))
        {
            onComplete?.Invoke();
            return;
        }

        Tween tween;

        switch (target)
        {
            case CanvasGroup group:
                tween = Tween.Alpha(group, endAlpha, duration,
                    useUnscaledTime: useUnscaledTime);
                break;

            case TextMeshProUGUI text:
                tween = Tween.Alpha(text, endAlpha, duration,
                    useUnscaledTime: useUnscaledTime);
                break;

            case SpriteRenderer sprite:
                tween = Tween.Alpha(sprite, endAlpha, duration,
                    useUnscaledTime: useUnscaledTime);
                break;

            case Image image:
                tween = Tween.Alpha(image, endAlpha, duration,
                    useUnscaledTime: useUnscaledTime);
                break;

            default:
                return;
        }

        activeFades[target] = tween;

        tween.OnComplete(() =>
        {
            activeFades.Remove(target);
            onComplete?.Invoke();
        });
        
    }


    public static Tween FadeTween(UnityEngine.Object target, float endAlpha, float duration, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false)
    {
        if (target == null)
            return default;

        switch (target)
        {
            case SpriteRenderer sprite:
                return Tween.Alpha(
                    sprite, endAlpha, duration,
                    startDelay: startDelay,
                    endDelay: endDelay,
                    useUnscaledTime: useUnscaledTime);

            case CanvasGroup group:
                return Tween.Alpha(
                    group, endAlpha, duration,
                    startDelay: startDelay,
                    endDelay: endDelay,
                    useUnscaledTime: useUnscaledTime);

            case TextMeshProUGUI text:
                return Tween.Alpha(
                    text, endAlpha, duration,
                    startDelay: startDelay,
                    endDelay: endDelay,
                    useUnscaledTime: useUnscaledTime);

            case UnityEngine.UI.Image image:
                return Tween.Alpha(
                    image, endAlpha, duration,
                    startDelay: startDelay,
                    endDelay: endDelay,
                    useUnscaledTime: useUnscaledTime);

            default:
                Debug.LogWarning(
                    $"UIFader does not support {target.GetType().Name}."
                );
                return default;
        }
    }

}
