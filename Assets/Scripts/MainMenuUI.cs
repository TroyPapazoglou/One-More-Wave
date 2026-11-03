using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sequence = PrimeTween.Sequence;

public class MainMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI OneText;
    [SerializeField] private TextMeshProUGUI MoreText;
    [SerializeField] private TextMeshProUGUI WaveText;

    [SerializeField] private Image playImage;
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI playText;

    [SerializeField] private TextMeshProUGUI highscoreText;

    [SerializeField] private CanvasGroup mainMenuCanvasGroup;

    [SerializeField] private GameplayUI gameplayUI;

    [Header("Animation Properties")]
    [SerializeField] private float OneTextDuration = 3f;
    [SerializeField] private float MoreTextDuration = 3f;
    [SerializeField] private float WaveTextDuration = 4f;
    [SerializeField] private float PlayButtonDuration = 2f;
    [SerializeField] private float highScoreDuration = 4f;
    [SerializeField] private float mainMenuDuration = 0.5f;

    private Sequence animationSequence;

    public bool IsAnimating => animationSequence.isAlive;

    private void OnDestroy()
    {
        if (animationSequence.isAlive)
            animationSequence.Stop();
    }

    public void ShowMenu()
    {
        playButton.interactable = true;

        mainMenuCanvasGroup.alpha = 1f;
        mainMenuCanvasGroup.blocksRaycasts = true;

        OneText.alpha = 0f;
        MoreText.alpha = 0f;
        WaveText.alpha = 0f;

        playImage.color = new Color(
            playImage.color.r,
            playImage.color.g,
            playImage.color.b,
            0f
        );

        playText.alpha = 0f;
        highscoreText.alpha = 0f;

        animationSequence = Sequence.Create();

        animationSequence
            .Chain(UIFader.FadeTween(
                OneText, 1f, OneTextDuration, startDelay: 1.5f))

            .Insert(2f, UIFader.FadeTween(
                MoreText, 1f, MoreTextDuration))

            .Insert(3f, UIFader.FadeTween(
                WaveText, 1f, WaveTextDuration))

            .Insert(4f, UIFader.FadeTween(
                playImage, 1f, PlayButtonDuration))

            .Insert(4f, UIFader.FadeTween(
                playText, 1f, PlayButtonDuration))

            .Insert(6f, UIFader.FadeTween(
                highscoreText, 1f, highScoreDuration));
    }  
    

    public void OnGameStarted()
    {
        mainMenuCanvasGroup.blocksRaycasts = false;

        if (animationSequence.isAlive)
            animationSequence.Stop();

        UIFader.Fade(highscoreText, 0f, 1f);

        UIFader.Fade(
            mainMenuCanvasGroup,
            0f,
            mainMenuDuration,
            onComplete: () =>
            {              

                GameManager.Instance.MenuTransitionEnded();               

                gameplayUI.ShowTimeSurvived();
            }
        );
    }

    public void HideHighscore()
    {
        UIFader.Fade(highscoreText, 0f, mainMenuDuration);
    }

    public void SetPlayInteractable(bool interactable)
    {
        playButton.interactable = interactable;
    }

    public void SetRaycasts(bool enabled)
    {
        mainMenuCanvasGroup.blocksRaycasts = enabled;
    }

    public void FadeOut(float duration, Action onComplete = null)
    {
        UIFader.Fade(
            mainMenuCanvasGroup,
            0f,
            duration,
            onComplete: onComplete
        );
    }

    public void StopIntroAnimation()
    {
        if (animationSequence.isAlive)
            animationSequence.Stop();
    }
}
