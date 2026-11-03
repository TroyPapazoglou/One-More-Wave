using UnityEngine;
using UnityEngine.UI;

public class ExitMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup exitMenuCanvasGroup;

    [SerializeField] private Button YesButton;
    [SerializeField] private Button NoButton;

    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] private GameplayUI gameplayUI;

    [Header("Animation Properties")]
    [SerializeField] private float exitMenuDuration = 0.5f;

    private bool isExitMenuOpen;
    private bool isTransitioning;

    public void Open()
    {
        if (isExitMenuOpen || isTransitioning)
            return;             

        exitMenuCanvasGroup.blocksRaycasts = true;

        mainMenuUI.SetRaycasts(false);

        YesButton.interactable = true;
        NoButton.interactable = true;

        isTransitioning = true;
        isExitMenuOpen = true;

        mainMenuUI.SetPlayInteractable(false);
        mainMenuUI.HideHighscore();

        gameplayUI.HideTimeSurvived();

        GameManager.Instance.GetPlayer().FadeOut(
            timeToFade: exitMenuDuration
        );

        mainMenuUI.FadeOut(exitMenuDuration, () =>
        {
            UIFader.Fade(
                exitMenuCanvasGroup,
                1f,
                exitMenuDuration,
                onComplete: () =>
                {
                    isTransitioning = false;
                }
            );
        });
    }

    public void OnExitCancelled()
    {
        if (!isExitMenuOpen || isTransitioning)
            return;

        exitMenuCanvasGroup.blocksRaycasts = false;

        isTransitioning = true;
        isExitMenuOpen = false;

        YesButton.interactable = false;
        NoButton.interactable = false;

        GameManager.Instance.GetPlayer().FadeIn(2.5f, 0.5f);

        UIFader.Fade(
            exitMenuCanvasGroup,
            0f,
            exitMenuDuration,
            onComplete: () =>
            {
                isTransitioning = false;

                mainMenuUI.ShowMenu();
            }
        );
    }

    public void OnExit()
    {
        if (!isExitMenuOpen || isTransitioning)
            return;

        isTransitioning = true;
        isExitMenuOpen = false;

        YesButton.interactable = false;
        NoButton.interactable = false;   

        UIFader.Fade(
            exitMenuCanvasGroup,
            0f,
            exitMenuDuration,
            onComplete: () =>
            {
                isTransitioning = false;

                Application.Quit();

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        );
    }
}
