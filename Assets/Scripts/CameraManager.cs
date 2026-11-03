using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Lag Settings")]
    [Range(0, 1)]
    [SerializeField] private float smoothTime = .3f;
    [SerializeField] private Vector3 startPos = new(0,5.48f,-10);

    //local variables
    private Vector3 currentVelocity = Vector3.zero;
    private bool isRunning = false;

    private Camera camera;
    private Transform camTransform;  
    private Coroutine cameraCoroutine;    


    private void Awake()
    {
        camera = Camera.main;

        if(camera != null)
            camTransform = camera.gameObject.transform;
    }
        
    private void Start()
    {
        startPos = transform.position;
        GameManager.Instance.OnGameStarted += OnGameStart;
        GameManager.Instance.GetPlayer().OnPlayerKilled += OnPlayerKilled;
        GameManager.Instance.OnQuitRun += OnQuitRun;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStarted -= OnGameStart;
        GameManager.Instance.GetPlayer().OnPlayerKilled -= OnPlayerKilled;
        GameManager.Instance.OnQuitRun -= OnQuitRun;
    }

    private void OnGameStart()
    {
        StartCamera();
    }

    private void OnPlayerKilled()
    {
        StopCamera();
    }

    private void OnQuitRun()
    {
        StopCamera();
    }

    public void ResetCamera()
    {
        transform.position = startPos;       
    }

    public void StartCamera()
    {
        isRunning = true;
    }

    public void StopCamera()
    {
        isRunning = false;
    }

    private void LateUpdate()
    {
        if (!isRunning || camera == null) return;

        Vector2 playerPos = EnemyManager.Instance.GetPlayerPosition();

        Vector3 newCamPos = new(playerPos.x, playerPos.y, camTransform.position.z);
        camTransform.position = Vector3.SmoothDamp(
            camTransform.position, newCamPos, ref currentVelocity, smoothTime,
            Mathf.Infinity, Time.deltaTime);
    }
}
