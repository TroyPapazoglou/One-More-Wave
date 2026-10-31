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
    }

    private void OnGameStart()
    {
        StartCamera();
    }

    private void OnPlayerKilled()
    {
        StopCamera();
    }

    public void ResetCamera()
    {
        transform.position = startPos;       
    }
    
    public void StartCamera()
    {
        if (isRunning) return;

        isRunning = true;

        cameraCoroutine ??= StartCoroutine(CameraUpdateLoop());
    }

    private IEnumerator CameraUpdateLoop()
    {
        while (isRunning)
        {          
            yield return new WaitForEndOfFrame();

            if (camera == null) yield break;

            Vector2 playerPos = EnemyManager.Instance.GetPlayerPosition();

            Vector3 newCamPos = new(playerPos.x, playerPos.y, camTransform.position.z);
            camTransform.position = Vector3.SmoothDamp(camTransform.position, newCamPos, ref currentVelocity, smoothTime);
        }
    }

    public void StopCamera()
    {
        if (!isRunning) return;      

        isRunning = false;
        if (cameraCoroutine != null)
        {
            StopCoroutine(cameraCoroutine);
            cameraCoroutine = null;
        }
    }
}
