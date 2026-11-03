using System;
using System.Collections;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("References")]
    [SerializeField] private UIScoreAnimation uiScoreAnimation;

    [Header("Time Elapsed")]
    [SerializeField] private float elapsedTime = 0;

    public event Action<string> scoreChanged;

    private readonly string scoreKey = "HighScore";
    private bool isRunning = false;
    private Coroutine timeCoroutine;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += OnGameStarted;
        GameManager.Instance.OnMenuTransitionEnded += OnMenuTransitionEnded;
        GameManager.Instance.GetPlayer().OnPlayerKilled += OnPlayerKilled;
        GameManager.Instance.OnQuitRun += OnQuitRun;
        DeathManager.Instance.OnDeathAnimationFinished += OnDeathAnimationFinished;
        uiScoreAnimation.SetHighscoreText(Mathf.FloorToInt(LoadBestTime()));
    }

    private void OnGameStarted()
    {
        elapsedTime = 0;
    }

    private void OnDeathAnimationFinished()
    {
        CheckNewHighScore(elapsedTime);
    }

    private void OnPlayerKilled()
    {
        StopStopWatch();
    }

    private void OnQuitRun()
    {
        StopStopWatch();
    }

    private void OnMenuTransitionEnded()
    {
        StartStopWatch();
    }

    private void StartStopWatch()
    {
        if (isRunning) return;

        isRunning = true;

        timeCoroutine = StartCoroutine(RunStopWatch());
    }

    private void StopStopWatch()
    {
        if (!isRunning) return;

        isRunning = false;
        if(timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
            timeCoroutine = null;
        }
    }

    private IEnumerator RunStopWatch()
    {
        while (isRunning)
        {
            elapsedTime += Time.deltaTime;
            scoreChanged?.Invoke(FormatTime(elapsedTime));
            yield return null;
        }
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    public int GetElapsedInSeconds()
    {
        return Mathf.RoundToInt(elapsedTime);
    }

    private string FormatTime(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTime);
      
        return string.Format("TIME SURVIVED: {0}:{1:00}", (int)timeSpan.TotalMinutes, timeSpan.Seconds);       
    }

    private void CheckNewHighScore(float totalSeconds)
    {
        float previousHighScore = LoadBestTime();

        if (totalSeconds > previousHighScore)
        {
            int difference = Mathf.FloorToInt(
                totalSeconds - previousHighScore
            );

            int newHighscore = Mathf.FloorToInt(totalSeconds);
            int bestScore = Mathf.FloorToInt(previousHighScore);

            SaveScore(totalSeconds);

            uiScoreAnimation.PlayNewHighscore(difference, newHighscore, bestScore);
        }
        else
        {
            uiScoreAnimation.Play();
        }
    }

    private void SaveScore(float newScore)
    {
        PlayerPrefs.SetFloat(scoreKey, newScore);
        PlayerPrefs.Save(); 
    }

    public void ResetSavedScore()
    {
        PlayerPrefs.SetFloat(scoreKey, 0);
        PlayerPrefs.Save(); 
    }

    public float LoadBestTime()    
    {       
        return PlayerPrefs.GetFloat(scoreKey, 0.0f);
    }

    public void ResetScore()
    {
        PlayerPrefs.DeleteKey(scoreKey);
    }



}
