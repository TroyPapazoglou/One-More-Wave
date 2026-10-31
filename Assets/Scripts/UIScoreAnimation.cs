using UnityEngine;
using TMPro;
using PrimeTween;
using System;

public class UIScoreAnimation : MonoBehaviour
{
    [Header("References")]

    [SerializeField] private TextMeshProUGUI timeSurvivedText;
    [SerializeField] private TextMeshProUGUI timeBeatText;
    [SerializeField] private TextMeshProUGUI highscoreText;

    [Header("Scroll Animation Properties")]
    [SerializeField] private float animationDuration = 5f;

    private Vector2 highscoreStartPos;
    private Vector2 timeBestedStartPos;
    private Vector2 highscoreSize;

    private Sequence highscoreSequence;  

    private Vector2 highscoreEndPos = new(0, 540);
    private Vector2 highscoreEndSize = new(600, 100);



 
    void Start()
    {
        highscoreStartPos = highscoreText.rectTransform.anchoredPosition;
        timeBestedStartPos = timeBeatText.rectTransform.anchoredPosition;        
        
        highscoreSize = new Vector2(highscoreText.rectTransform.rect.width, highscoreText.rectTransform.rect.height);
        
    }


    public Sequence AnimateTo(int targetSeconds, TextMeshProUGUI timeText)
    {
        targetSeconds = Mathf.Max(0, targetSeconds);

        int startSeconds = ReadDisplayedTime(timeText);

        int difference = Mathf.Abs(targetSeconds - startSeconds);

        float duration = animationDuration;

        return Sequence.Create(Tween.Custom(startSeconds, targetSeconds, duration, onValueChange: value =>
                {
                    UpdateText(Mathf.RoundToInt(value), timeText);
                }
            )
        );
    }


    private int ReadDisplayedTime(TextMeshProUGUI timeText)
    {
        if (timeText == null || string.IsNullOrWhiteSpace(timeText.text))
            return 0;

        // Get the final part after any label, e.g. "HIGH SCORE: 0:00"
        string displayedTime = timeText.text.Trim();
        int lastSpaceIndex = displayedTime.LastIndexOf(' ');

        if (lastSpaceIndex >= 0)
            displayedTime = displayedTime.Substring(lastSpaceIndex + 1);

        string[] parts = displayedTime.Split(':');

        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int minutes) ||
            !int.TryParse(parts[1], out int seconds))
            return 0;

        return minutes * 60 + seconds;
    }


    private void UpdateText(int totalSeconds, TextMeshProUGUI timeText)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        string time = $"{minutes}:{seconds:00}";

        if (timeText == highscoreText)
        {
            timeText.text = $"HIGH SCORE: {time}";
        }
        else if (timeText == timeBeatText)
        {
            timeText.text = $"+ {time}";
        }
    }

    public void SetHighscoreText(int highScore)
    {
        int minutes = highScore / 60;
        int seconds = highScore % 60;

        string time = $"{minutes}:{seconds:00}";

        highscoreText.text = $"HIGH SCORE: {time}";
    }


    public void PlayNewHighscore(int difference, int newHighscore, int bestScore)
    {
        UpdateText(bestScore, highscoreText);

        highscoreSequence = Sequence.Create(Tween.Alpha(highscoreText, 1, 1))
        .Group(Tween.UIAnchoredPosition(highscoreText.rectTransform, highscoreEndPos, 1))
        .Group(Tween.UISizeDelta(highscoreText.rectTransform, highscoreEndSize, 1))
        .Chain(Tween.UIAnchoredPosition(timeBeatText.rectTransform, Vector2.zero, 2))
        .Chain(AnimateTo(difference, timeBeatText))
        .Chain(AnimateTo(newHighscore, highscoreText))
        .Chain(Tween.UIAnchoredPosition(timeBeatText.rectTransform, timeBestedStartPos, 1))
        .Group(Tween.UISizeDelta(highscoreText.rectTransform, highscoreSize, 1, startDelay: 1f))
        .Group(Tween.UIAnchoredPosition(highscoreText.rectTransform, highscoreStartPos, 1, startDelay: 1f))
        .Group(Tween.Alpha(timeSurvivedText, 0, 1, startDelay: 1f))
        .Group(Tween.Alpha(highscoreText, 0, 1, startDelay: 1f)
        .OnComplete(() => 
        {
            RespawnFadeSequence.Instance.Play();
            UpdateText(0, timeBeatText);
        })
        );           
    }

    public void Play()
    {
        highscoreSequence = Sequence.Create(Tween.Alpha(timeSurvivedText, 0, 1)
        .OnComplete(() => { RespawnFadeSequence.Instance.Play(); })
        );
    }



}
