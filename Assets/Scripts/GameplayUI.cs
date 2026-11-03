using TMPro;
using UnityEngine;

public class GameplayUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI timeSurvivedText;
    [SerializeField] private ScoreManager scoreManager;

    [Header("Animation Properties")]
    [SerializeField] private float fadeDuration = 0.5f;

    private void OnEnable()
    {
        if (scoreManager != null)
            scoreManager.scoreChanged += OnScoreChanged;
    }

    private void OnDisable()
    {
        if (scoreManager != null)
            scoreManager.scoreChanged -= OnScoreChanged;
    }

    private void OnScoreChanged(string score)
    {
        timeSurvivedText.text = score;
    }

    public void ShowTimeSurvived()
    {
        UIFader.Fade(
            timeSurvivedText,
            1f,
            fadeDuration
        );
    }

    public void HideTimeSurvived()
    {
        UIFader.Fade(
            timeSurvivedText,
            0f,
            fadeDuration
        );
    }
}
