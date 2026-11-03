using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Controllers")]
    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] private ExitMenuUI exitMenuUI;
    [SerializeField] private PauseMenuUI pauseMenuUI;
    [SerializeField] private PersistentMenuButton persistentMenuButton;

    private bool isTransitioning;

    private void Start()
    {
        mainMenuUI.ShowMenu();

        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnGameStarted += OnGameStarted;
        GameManager.Instance.OnExitPressed += OnExitPressed;
        GameManager.Instance.OnPausePressed += OnPausePressed;
        GameManager.Instance.GetPlayer().OnPlayerKilled += OnPlayerKilled;
        GameManager.Instance.OnQuitRun += OnQuitRun;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnGameStarted -= OnGameStarted;
        GameManager.Instance.OnExitPressed -= OnExitPressed;
        GameManager.Instance.OnPausePressed -= OnPausePressed;
        GameManager.Instance.GetPlayer().OnPlayerKilled -= OnPlayerKilled;
        GameManager.Instance.OnQuitRun -= OnQuitRun;
    }

    private void OnGameStarted()
    {
        isTransitioning = true;

        persistentMenuButton.SetMainMenuState(false);
        mainMenuUI.StopIntroAnimation();
        mainMenuUI.OnGameStarted();
    }

    private void OnExitPressed()
    {      
        mainMenuUI.StopIntroAnimation();
        exitMenuUI.Open();
    }

    private void OnPausePressed()
    {       
        pauseMenuUI.Toggle();
    }

    private void OnPlayerKilled()
    {
        SetTransitioning(true);
    }

    private void OnQuitRun()
    {
        SetTransitioning(true);
    }

    public void ShowMenu()
    {
        SetTransitioning(false);
        persistentMenuButton.SetMainMenuState(true);
        mainMenuUI.ShowMenu();
    }

    public void OnPersistentMenuButtonPressed(bool isMainMenu)
    {
        if (isTransitioning)
            return;

        if (isMainMenu)
        {
            OnExitPressed();
        }
        else
        {
            OnPausePressed();
        }
    }

    public void EnablePersistentMenuButton()
    {
        persistentMenuButton.Enable();
    }

    public void SetTransitioning(bool value)
    {
        isTransitioning = value;
    }
}
