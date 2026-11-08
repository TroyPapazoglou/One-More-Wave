using System;
using System.Collections;
using Unity.Collections.Tests.CoreCLR.TestJobs;
using UnityEngine;
using UnityEngine.Timeline;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private RespawnFadeSequence respawnFadeSequence;   
    [SerializeField] private EnvironmentFadeSequence environmentFadeSequence;   
    [SerializeField] private Player player;

    [Header("Mobile Controls")]
    [SerializeField] private bool showMobileUI = false;


    public event Action OnGameStarted;
    public event Action OnMenuTransitionEnded;

    public event Action OnExitPressed;
    public event Action OnPausePressed;

    public event Action OnQuitRun;

    public CameraManager CameraManager => cameraManager;
    public UIManager UIManager => uiManager;
    public EnvironmentFadeSequence EnvironmentFadeSequence => environmentFadeSequence;

    private bool IsMobile => IsPlatformMobile();    


    private void Awake()
    {
        Instance = this;

        if (IsPlatformMobile())
            Application.targetFrameRate = 120;
    }

    private void Start()
    {
        if(IsPlatformMobile())
        {
            player.Movement.Joystick.enabled = true;
            uiManager.EnablePersistentMenuButton();
        }
    }

    public Player GetPlayer()
    {  return player; }    

    public bool IsPlatformMobile()   
    {
#if UNITY_EDITOR       
        return showMobileUI;
#else           
            return Application.isMobilePlatform;
#endif
    }

    public void StartGame()
    {
        OnGameStarted?.Invoke();
    }

    public void MenuTransitionEnded()
    {
        uiManager.SetTransitioning(false);
        OnMenuTransitionEnded?.Invoke();
    }

    public void ExitPressed()
    {
        OnExitPressed?.Invoke();
    }

    public void PausePressed()
    {      
        OnPausePressed?.Invoke();
    }

    public void QuitRun()
    {
        OnQuitRun?.Invoke();
    }

    public IEnumerator OnPlayerKilled()
    {
        yield return StartCoroutine(environmentFadeSequence.Play());
    }
}
