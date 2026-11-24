using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefab")]
    [SerializeField] private GameObject enemy;
    
    [Space(10)]
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnRate = 5f;
    [SerializeField] private int maxEnemies = 10;

    private IObjectPool<GameObject> _enemyPool;
    private List<EnemyController> _currentEnemies;
    private float _timeSinceLastSpawn;

    void Awake()
    {
        
    }
    
    void Start()
    {
        _enemyPool = new ObjectPool<GameObject>(CreateEnemy);
    }
    
    void Update()
    {
        if (_currentEnemies.Count < maxEnemies && Time.time > _timeSinceLastSpawn) // WIP: implement counting current enemies so we can stop spawning
        {
            _enemyPool.Get();
            enemy.GetComponentInChildren<EnemyController>().SetPool(_enemyPool);
            _timeSinceLastSpawn = Time.time + spawnRate;
        }
    }

    private GameObject CreateEnemy()
    {
        GameObject enemy = Instantiate(this.enemy);
        _currentEnemies.Add(enemy.GetComponentInChildren<EnemyController>());
        return enemy;
    }
}
