using PrimeTween;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public class Joystick : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image stickImage;
    [SerializeField] private OnScreenStick onScreenStick;

    [Header("Properties")]
    [SerializeField] private float timeToFade = .5f;
    [SerializeField] private float startDelay = 0;

    private float backgroundImgStartAlpha;
    private float stickStartAlpha;
    private Sequence fadeSequence;

    private void Awake()
    {
        backgroundImgStartAlpha = backgroundImage.color.a;
        stickStartAlpha = stickImage.color.a;

        Hide();         
        
    }

    private void Start()
    {       
        GameManager.Instance.OnQuitRun += OnGameEnd;
        GameManager.Instance.OnGameStarted += OnGameStarted;
        GameManager.Instance.GetPlayer().OnPlayerKilled += OnGameEnd;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnQuitRun -= OnGameEnd;
        GameManager.Instance.OnGameStarted -= OnGameStarted;
        GameManager.Instance.GetPlayer().OnPlayerKilled -= OnGameEnd;
    }

    private void OnGameStarted()
    {       
        FadeIn();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        onScreenStick.enabled = true;       
    }

    private void OnGameEnd()
    {
        FadeOut();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        onScreenStick.enabled = false;
    } 

    public void FadeIn()
    {
        if (fadeSequence.isAlive)
            fadeSequence.Stop();       

        fadeSequence = Sequence.Create(
            Tween.Alpha(backgroundImage, backgroundImgStartAlpha, timeToFade, startDelay: startDelay))
            .Group(Tween.Alpha(stickImage, stickStartAlpha, timeToFade, startDelay: startDelay)           
        );

    }

    public void FadeOut()
    {
        if (fadeSequence.isAlive)
            fadeSequence.Stop();

        fadeSequence = Sequence.Create(
           Tween.Alpha(backgroundImage, 0, timeToFade, startDelay: startDelay))
          .Group(Tween.Alpha(stickImage, 0, timeToFade, startDelay: startDelay));      
       
    }

    private void Hide()
    {
        Color newBackgroundColor = backgroundImage.color;
        newBackgroundColor.a = 0;

        Color newStickColor = stickImage.color;
        newStickColor.a = 0;

        backgroundImage.color = newBackgroundColor;
        stickImage.color = newStickColor;
    }

}
