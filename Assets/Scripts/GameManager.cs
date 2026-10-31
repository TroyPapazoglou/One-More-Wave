using System;
using UnityEngine;
using UnityEngine.Timeline;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private RespawnFadeSequence respawnFadeSequence;   
    [SerializeField] private Player player;

    public event Action OnGameStarted;
    public event Action OnMenuTransitionEnded;

    public CameraManager CameraManager => cameraManager;
    public UIManager UIManager => uiManager;

    private void Awake()
    {
        Instance = this;
    }

    public Player GetPlayer()
    {  return player; }    

    public void StartGame()
    {
        OnGameStarted?.Invoke();
    }

    public void MenuTransitionEnded()
    {
        OnMenuTransitionEnded?.Invoke();
    }
}
