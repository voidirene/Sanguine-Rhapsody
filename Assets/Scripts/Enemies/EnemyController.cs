using System;
using Gamekit3D;
using UnityEngine;
using UnityEngine.AI;

// like the crudest, simplest and LEAST efficient enemy controller known to man
public class EnemyController : MonoBehaviour
{
    public bool active = true;
    public LayerMask whatIsGround; 
    public LayerMask whatIsPlayer;
    
    // Attacking variables
    public float timeBetweenAttacks;
    private bool _isAttacking;
    public float attackRange;
    public bool playerInRange;
    
    private Transform _player;
    private NavMeshAgent _agent;
    private EnemyMeleeWeapon _weapon;
    
    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _agent = transform.parent.GetComponentInChildren<NavMeshAgent>();
        _weapon = transform.parent.GetComponentInChildren<EnemyMeleeWeapon>();
        _weapon.SetOwner(this.gameObject);
    }

    private void Update()
    {
        if (active)
        {
            transform.LookAt(_player.position);
            float distance = Vector3.Distance(_player.position, transform.parent.position);
            if (distance <= attackRange)
                playerInRange = true;
            else
                playerInRange = false;
        
            if (!playerInRange)
                ChasePlayer();
            else
                AttackPlayer();
        }
    }
    
    private void ChasePlayer()
    {
        _agent.SetDestination(_player.position);
    }

    private void AttackPlayer()
    {
        _agent.SetDestination(transform.position);
        
        if (!_isAttacking)
        {
            _isAttacking = true;
            _weapon.BeginAttack(false);
            
            Invoke(nameof(StopAttack), 0.3f);
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void StopAttack()
    {
        _weapon.EndAttack();
    }

    private void ResetAttack()
    {
        _isAttacking = false;
    }
}
