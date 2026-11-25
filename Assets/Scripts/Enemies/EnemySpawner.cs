using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefab")]
    [SerializeField] private GameObject enemyPrefab;
    
    [Space(10)]
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnRate = 5f;
    [SerializeField] private int maxEnemies = 10;

    private ObjectPool<GameObject> _enemyPool;
    private float _nextSpawnTime;

    private void Awake()
    {
        if (enemyPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Either the enemy prefab is missing, or there is no spawn points!!", this);
            enabled = false;
            return;
        }
 
        _enemyPool = new ObjectPool<GameObject>(
            createFunc: CreateEnemy,
            actionOnGet: OnGetEnemy,
            actionOnRelease: OnReleaseEnemy,
            actionOnDestroy: OnDestroyEnemy,
            collectionCheck: true,
            defaultCapacity: maxEnemies,
            maxSize: maxEnemies);
    }
    private void OnDestroy()
    {
        _enemyPool?.Dispose();
    }

    private void Update()
    {
        if (Time.time < _nextSpawnTime)
            return;
 
        _nextSpawnTime = Time.time + spawnRate;
 
        if (_enemyPool.CountActive < maxEnemies)
            _enemyPool.Get();
    }
 
    private GameObject CreateEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab);
        enemy.SetActive(false);
 
        enemy.GetComponentInChildren<EnemyController>().SetPool(_enemyPool);
        return enemy;
    }
 
    private void OnGetEnemy(GameObject enemy)
    {
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
 
        enemy.transform.SetPositionAndRotation(point.position, point.rotation);
        enemy.SetActive(true);
    }
 
    private void OnReleaseEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
    }
 
    private void OnDestroyEnemy(GameObject enemy)
    {
        Destroy(enemy);
    }
}
