using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxCollider2D enemySpawnField;

    [Header("Enemy Type Prefabs")]
    [SerializeField] private List<Enemy> enemyList;

    [Header("Spawn Properties")]
    [SerializeField] private float minimumSpawnDistanceFromPlayer = 5;   

    private bool isRunning = false;
    private Coroutine spawnCoroutine;  
   

    private void Start()
    {
        GameManager.Instance.GetPlayer().OnPlayerKilled += OnPlayerKilled;
        GameManager.Instance.OnQuitRun += OnQuitRun;
        GameManager.Instance.OnMenuTransitionEnded += OnMenuTransitionEnded; 
    }

    private void OnDestroy()
    {
        GameManager.Instance.GetPlayer().OnPlayerKilled -= OnPlayerKilled;
        GameManager.Instance.OnQuitRun -= OnQuitRun;
        GameManager.Instance.OnMenuTransitionEnded -= OnMenuTransitionEnded;
    }

    private void OnMenuTransitionEnded()
    {
        StartSpawningEnemies();
    }

    private void OnPlayerKilled()
    {
        StopSpawningEnemies();
    }

    private void OnQuitRun()
    {
        StopSpawningEnemies();
    }

    public void StartSpawningEnemies()
    {
        if (isRunning) return;

        isRunning = true;
               
        spawnCoroutine ??= StartCoroutine(SpawnCoroutineLoop());      
    }

    private void StopSpawningEnemies()
    {
        if (!isRunning) return;

        isRunning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnCoroutineLoop()
    {
        while (isRunning)
        {
            SpawnEnemy(GetSpawnPoint());

            yield return StartCoroutine(DifficultyManager.Instance.SpawnCoroutineLoop());           
        }            
    }

    private void SpawnEnemy(Vector2 spawnPoint)
    {
        Enemy enemyPrefab = GetEnemy();

        if (enemyPrefab == null)
            return;
              
        if (EnemyManager.Instance.TryGetAvailableEnemy(
                enemyPrefab.GetType(), out Enemy pooledEnemy))
        {
            pooledEnemy.SpawnAt(spawnPoint);
            return;
        }
              
        Enemy newEnemy = Instantiate(
            enemyPrefab,
            spawnPoint,
            Quaternion.identity
        );

        EnemyManager.Instance.AddEnemy(newEnemy);
        newEnemy.ShowVisuals();
    }

    private Enemy GetEnemy()
    {
        if (enemyList.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: no enemy prefabs to spawn!");
            return null;
        }

        List<Enemy> availableEnemiesToSpawn = new();
        float totalWeight = 0f;

        float elapsedTime = ScoreManager.Instance.GetElapsedTime();

        foreach (Enemy enemy in enemyList)
        {
            if (enemy.SpawnAvailability <= elapsedTime)
            {
                float weight = GetCurrentWeight(enemy, elapsedTime);

                if (weight > 0f)
                {
                    availableEnemiesToSpawn.Add(enemy);
                    totalWeight += weight;
                }
            }
        }

        if (availableEnemiesToSpawn.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: no enemies currently available!");
            return null;
        }

        if (availableEnemiesToSpawn.Count == 1)
        {
            return availableEnemiesToSpawn[0];
        }

        return GetEnemyFromWeight(availableEnemiesToSpawn, totalWeight, elapsedTime);
    }

    private float GetCurrentWeight(Enemy enemy, float elapsedTime)
    {
        // Chaser: constant weight
        if (enemy is Chaser)
            return 100f;

        // Sprinter: unlocks at 20 seconds
        if (enemy is Sprinter)
            return GetWeight(elapsedTime, enemy.SpawnAvailability, enemy.SpawnWeight, 1.5f, 50f);

        // Heavy: unlocks at 40 seconds
        if (enemy is Heavy)
            return GetWeight(elapsedTime, enemy.SpawnAvailability, enemy.SpawnWeight, 1f, 35f);

        // Fallback for any other enemy type
        return enemy.SpawnWeight;
    }

    private Enemy GetEnemyFromWeight(List<Enemy> enemyList, float totalWeight, float elapsedTime)
    {
        float randomValue = Random.Range(0, totalWeight);
        float cumulativeWeight = 0;
       
        foreach (Enemy enemy in enemyList)
        {
            cumulativeWeight += GetCurrentWeight(enemy, elapsedTime);

            if (randomValue <= cumulativeWeight)            
                return enemy;            

        }

        Debug.LogWarning("EnemySpawner: No enemy provided was within weight range");
        return enemyList[enemyList.Count - 1];
    }

    float GetWeight(float elapsedTime, float unlockTime, float startingWeight, float growthRate, float maxWeight)
    {
        if (elapsedTime < unlockTime)
            return 0f;

        float timeSinceUnlock = elapsedTime - unlockTime;

        return Mathf.Min(
            maxWeight,
            startingWeight + growthRate * timeSinceUnlock
        );
    }

    private Vector2 GetRandomPosition()
    {
        Bounds bounds = enemySpawnField.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);

        return new Vector2(randomX, randomY);
    }

    private bool IsValidSpawnPoint(Vector2 spawnPoint)
    {
        if (Vector2.Distance(EnemyManager.Instance.GetPlayerPosition(), spawnPoint) > minimumSpawnDistanceFromPlayer)
        {
            return true;
        }       

        return false;

    }

    private Vector2 GetSpawnPoint()
    {
        Vector2 spawnPoint;

        do
        {
            spawnPoint = GetRandomPosition();
        }
        while (!IsValidSpawnPoint(spawnPoint));
        

        return spawnPoint;
    }
}
