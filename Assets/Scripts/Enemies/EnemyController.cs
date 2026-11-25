using System;
using Gamekit3D;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

// like the crudest, simplest and LEAST efficient enemy controller known to man
public class EnemyController : MonoBehaviour
{
    public bool active = true;
    
    public LayerMask whatIsGround; 
    public LayerMask whatIsPlayer;
    
    // Attacking variables
    public AttackData enemyAttack;
    public float timeBetweenAttacks;
    private bool _isAttacking;
    private float _attackEndTime;
    public float attackRange;
    public bool playerInRange;
    
    [SerializeField] private float returnToLocomotionBlend = 0.15f;
    
    [SerializeField] private float repathInterval = 0.2f;
    private float _nextRepathTime;
    
    private Transform _player;
    private NavMeshAgent _agent;
    private EnemyMeleeWeapon _weapon;
    private IObjectPool<GameObject> _enemyPool;
    private Animator _animator;
    
    private static readonly int HashLocomotion = Animator.StringToHash("walk cycle");
    
    private Action<EnemyController> OnEnemyDestroyed;
    
    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _animator = transform.parent.GetComponentInChildren<Animator>();
        _agent = transform.parent.GetComponentInChildren<NavMeshAgent>();
        _weapon = transform.parent.GetComponentInChildren<EnemyMeleeWeapon>();
        _weapon.SetOwner(this.gameObject);
    }

    private void Update()
    {
        if (active)
        {
            if (_isAttacking && Time.time >= _attackEndTime)
                ResetAttacking();
            
            transform.LookAt(_player.position);
            float distance = Vector3.Distance(_player.position, transform.parent.position);
            if (distance <= attackRange)
                playerInRange = true;
            else
                playerInRange = false;
        
            if (!playerInRange)
                ChasePlayer();
            else
                StartAttack();
        }
    }
    
    private void ChasePlayer()
    {
        if (_agent.isStopped)
        {
            _agent.isStopped = false;
            _nextRepathTime = 0f;
        }
 
        if (Time.time >= _nextRepathTime) // This is here because we don't want to set the agent path every frame. expensiv
        {
            _nextRepathTime = Time.time + repathInterval;
            _agent.SetDestination(_player.position);
        }
    }

    private void StartAttack()
    {
        _agent.isStopped = true;
        
        if (_weapon)
            _weapon.EndAttack();   // close any open hit window
        
        if (!_isAttacking)
        {
            _isAttacking = true;
            _attackEndTime = Time.time + enemyAttack.clip.length;
            
            _weapon.BeginAttack(false);
            _animator.CrossFadeInFixedTime(enemyAttack.StateHash, enemyAttack.crossFadeTime);
        }
    }
    private void StopAttack()
    {
        _weapon.EndAttack();
    }

    private void ResetAttacking()
    {
        _isAttacking = false;
        _animator.CrossFadeInFixedTime(HashLocomotion, returnToLocomotionBlend);
    }

    public void SetPool(IObjectPool<GameObject> pool)
    {
        _enemyPool = pool;   
    }

    public void DestroyEnemy()
    {
        OnEnemyDestroyed?.Invoke(this); // call this BEFORE releasing/destroying the enemy
 
        if (_enemyPool != null)
            _enemyPool.Release(gameObject);
        else
            Destroy(gameObject);// ideally we'll never have to directly destroy...
    }
}
