using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;   

    [Header("Difficulty Settings")]
    [SerializeField] private float spawnInterval = 2.0f;
    [SerializeField] private float minimumInterval = 0.65f;
    [SerializeField] private float difficultyScaling = 0.015f;

    private bool isRunning = false;
    private Coroutine difficultyCoroutine;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.GetPlayer().OnPlayerKilled += OnPlayerKilled;
        GameManager.Instance.OnQuitRun += OnQuitRun;
        StartDifficultyScaling();
    }

    private void OnDestroy()
    {
        GameManager.Instance.GetPlayer().OnPlayerKilled -= OnPlayerKilled;
        GameManager.Instance.OnQuitRun -= OnQuitRun;
        StopDifficultyScaling();
    }

    private void OnPlayerKilled()
    {
        StopDifficultyScaling();
    }

    private void OnQuitRun()
    {
        StopDifficultyScaling();
    }
   
    public void StartDifficultyScaling()
    {
        if (isRunning) return;

        isRunning = true;

        difficultyCoroutine ??= StartCoroutine(SpawnCoroutineLoop());
    }

    public void StopDifficultyScaling()
    {
        if (!isRunning) return;

        isRunning = false;
        if (difficultyCoroutine != null)
        {
            StopCoroutine(difficultyCoroutine);
            difficultyCoroutine = null;
        }
    }

    public IEnumerator SpawnCoroutineLoop()
    {
        
        int elapsedTime = ScoreManager.Instance.GetElapsedInSeconds();
        float newSpawnInterval = Mathf.Max(minimumInterval, spawnInterval - (difficultyScaling * elapsedTime));

        //Debug.Log("DifficultyManager: new spawninterval:" + newSpawnInterval);

        yield return new WaitForSeconds(newSpawnInterval);
        
    }


}
