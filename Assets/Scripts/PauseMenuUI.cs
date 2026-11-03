using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup pauseMenuCanvasGroup;

    [SerializeField] private Button ResumeButton;
    [SerializeField] private Button QuitButton;

    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private GameplayUI gameplayUI;
    [SerializeField] private MainMenuUI mainMenuUI;

    [Header("Animation Properties")]
    [SerializeField] private float pauseMenuDuration = 0.5f;

    public void Toggle()
    {
        pauseManager.Toggle();

        bool isPaused = PauseManager.IsPaused;

        ResumeButton.interactable = isPaused;
        QuitButton.interactable = isPaused;

        pauseMenuCanvasGroup.blocksRaycasts = isPaused;

        UIFader.Fade(
            pauseMenuCanvasGroup,
            isPaused ? 1f : 0f,
            pauseMenuDuration,
            useUnscaledTime: true
        );
    }

    public void OnResumePressed()
    {
        CloseMenu();
    }

    private void CloseMenu()
    {
        pauseManager.Resume();

        ResumeButton.interactable = false;
        QuitButton.interactable = false;

        pauseMenuCanvasGroup.blocksRaycasts = false;

        UIFader.Fade(
            pauseMenuCanvasGroup,
            0f,
            pauseMenuDuration,
            useUnscaledTime: true
        );
    }

    public void OnQuitRun()
    {
        CloseMenu();

        gameplayUI.HideTimeSurvived();

        GameManager.Instance.QuitRun();

        StartCoroutine(ReturnSequence());
      
    }

    private IEnumerator ReturnSequence()
    {
        yield return StartCoroutine(GameManager.Instance.OnPlayerKilled());

        StartCoroutine(EnemyManager.Instance.FadeEnemies(() =>
        {
            GameManager.Instance.GetPlayer().FadeOut(
                endDelay: 1,
                onComplete: () =>
                {
                    GameManager.Instance.CameraManager.ResetCamera();

                    GameManager.Instance.GetPlayer().Show();

                    GameManager.Instance.GetPlayer().Movement.EnableMainMenuInput();

                    GameManager.Instance.UIManager.ShowMenu();

                    GameManager.Instance.GetPlayer().FadeIn(2.5f, .5f);
                }
            );
        }));
    }
}
