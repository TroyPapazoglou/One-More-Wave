using PrimeTween;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sequence = PrimeTween.Sequence;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI OneText;
    [SerializeField] private TextMeshProUGUI MoreText;
    [SerializeField] private TextMeshProUGUI WaveText;
    [SerializeField] private Image playImage;
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI playText;
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private TextMeshProUGUI timeSurvivedText;
    [SerializeField] private CanvasGroup menuCanvas;
    [SerializeField] private CanvasGroup inGameCanvas;

    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;


    [Header("Animation Properties")]
    [SerializeField] private float OneTextDuration = 3;
    [SerializeField] private float MoreTextDuration = 3;
    [SerializeField] private float WaveTextDuration = 4;
    [SerializeField] private float PlayButtonDuration = 2;
    [SerializeField] private float highScoreDuration = 4;

    private Sequence introSequence;  

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnGameStarted += OnGameStarted;
        scoreManager.scoreChanged += OnScoreChanged;      

        ShowMenuCanvas();

    }

    private void OnScoreChanged(string score)
    {
        timeSurvivedText.text = score;
    }
        
    private void OnGameStarted()
    {
        if (introSequence.isAlive)
            introSequence.Stop();

        FadeAlphaIfNeeded(menuCanvas, 0f, 1f, () =>
        {
            OnTransitionEnded();

            FadeAlphaIfNeeded(highscoreText, 0f, 1f);
            FadeAlphaIfNeeded(timeSurvivedText, 1f, 1f);
        });
    }

    private void OnTransitionEnded()
    {
        GameManager.Instance.MenuTransitionEnded();
    }

    public void FadeAlphaIfNeeded(UnityEngine.Object target, float endAlpha, float duration, Action onComplete = null)
    {
        if (target == null)
        {
            onComplete?.Invoke();
            return;
        }

        float currentAlpha;

        switch (target)
        {
            case CanvasGroup canvasGroup:
                currentAlpha = canvasGroup.alpha;
                break;

            case TextMeshProUGUI text:
                currentAlpha = text.alpha;
                break;

            case SpriteRenderer sprite:
                currentAlpha = sprite.color.a;
                break;

            default:
                Debug.LogWarning(
                    $"FadeAlphaIfNeeded does not support {target.GetType().Name}."
                );
                onComplete?.Invoke();
                return;
        }

        if (Mathf.Approximately(currentAlpha, endAlpha))
        {
            onComplete?.Invoke();
            return;
        }

        switch (target)
        {
            case CanvasGroup canvasGroup:
                Tween.Alpha(canvasGroup, endAlpha, duration)
                    .OnComplete(onComplete);
                break;

            case TextMeshProUGUI text:
                Tween.Alpha(text, endAlpha, duration)
                    .OnComplete(onComplete);
                break;

            case SpriteRenderer sprite:
                Tween.Alpha(sprite, endAlpha, duration)
                    .OnComplete(onComplete);
                break;
        }
    }


    public void ShowMenuCanvas()
    {
        playButton.interactable = true;
        menuCanvas.alpha = 1;

        Color playImgColour = playImage.color;
        playImgColour.a = 0;

        OneText.alpha = 0;
        MoreText.alpha = 0;
        WaveText.alpha = 0;
        playImage.color = playImgColour;
        playText.alpha = 0;
        highscoreText.alpha = 0;

        introSequence = Sequence.Create();

        introSequence
            .Chain(Tween.Alpha(OneText, 1, OneTextDuration, startDelay: 1.5f))
            .Insert(2, Tween.Alpha(MoreText, 1, MoreTextDuration))
            .Insert(3, Tween.Alpha(WaveText, 1, WaveTextDuration))
            .Insert(4, Tween.Alpha(playImage, 1, PlayButtonDuration))
            .Insert(4, Tween.Alpha(playText, 1, PlayButtonDuration))
            .Insert(6, Tween.Alpha(highscoreText, 1, highScoreDuration));
    }

}
   

