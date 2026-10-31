using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    private Player player;
    private readonly List<Enemy> enemyList = new List<Enemy>();
    private bool isRunning = false;
    private Coroutine enemyMoveCoroutine;

    public IReadOnlyList<Enemy> Enemies => enemyList;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnMenuTransitionEnded += OnMenuTransitionEnded;
        player.OnPlayerKilled += OnPlayerKilled;
    }

    private void OnMenuTransitionEnded()
    {
        StartEnemyMovement();
    }
     
    public void StartEnemyMovement()
    {
        if (isRunning) return;

        isRunning = true;

        enemyMoveCoroutine ??= StartCoroutine(EnemyMovementLoop());
    }

    public void StopEnemyMovement()
    {
        if (!isRunning) return;

        isRunning = false;
        if (enemyMoveCoroutine != null)
        {
            StopCoroutine(enemyMoveCoroutine);
            enemyMoveCoroutine = null;
        }
    }

    public IEnumerator EnemyMovementLoop()
    {
        while(isRunning)
        {
            if (player == null || !player.IsAlive()) yield break;

            for (int i = enemyList.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemyList[i];

                if (enemy == null)
                {
                    enemyList.RemoveAt(i);
                    continue;
                }

                if (!enemy.IsSpawned)
                    continue;

                enemy.Move(player);
            }
            yield return new WaitForFixedUpdate();
        }    
       

    }
    
    public void SetPlayer(Player newPlayer)
    {
        Debug.Log("added player");
        player = newPlayer;
    }

    public void AddEnemy(Enemy newEnemy)
    {
        if (!enemyList.Contains(newEnemy))
        {
            //Debug.Log("added enemy");
            enemyList.Add(newEnemy);
        }
    }

    public void OnPlayerKilled()
    {
        StopEnemyMovement();
        for (int i = enemyList.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemyList[i];
            if (enemy == null)
            {
                //enemyList.RemoveAt(i);
                continue;
            }
            enemy.Disable();
        }
    }   

    public Vector2 GetPlayerPosition()
    {
        if (player == null)
            Debug.LogWarning("EnemyManager: Player is null");
        return player ? player.gameObject.transform.position : Vector2.zero;
    }

    public bool TryGetAvailableEnemy(System.Type enemyType, out Enemy availableEnemy)
    {
        foreach (Enemy enemy in enemyList)
        {
            if (enemy == null)
                continue;

            if (enemy.IsSpawned)
                continue;
                      
            if (enemy.GetType() != enemyType)
                continue;

            availableEnemy = enemy;
            return true;
        }

        availableEnemy = null;
        return false;
    }

}
