using Gamekit3D;
using StarterAssets;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private bool canAttack = true;
    
    private MeleeWeapon _meleeWeapon;
    private RangeWeapon _rangeWeapon;
    private StarterAssetsInputs _input;
    private Animator _animator;
    
    private static readonly int _hashLightAttack = Animator.StringToHash("LightAttack");
    private static readonly int _hashHeavyAttack = Animator.StringToHash("HeavyAttack");

    void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
        _animator = GetComponent<Animator>();
        
        _meleeWeapon = GetComponentInChildren<MeleeWeapon>();
        _rangeWeapon = GetComponentInChildren<RangeWeapon>();

        if (!_meleeWeapon && !_rangeWeapon)
        {
            Debug.LogWarning($"{name} needs a MeleeWeapon or RangeWeapon.", this);
            enabled = false;
            return;
        }

        _meleeWeapon?.SetOwner(gameObject);
    } 

    void Update()
    {
        if (_input.lightAttack && canAttack)
        {
            BeginAttack(_hashLightAttack, false);
            
            _animator.SetTrigger(_hashLightAttack);
        }
        
        if (_input.heavyAttack && canAttack)
        {
            BeginAttack(_hashHeavyAttack, true);
            
            _animator.SetTrigger(_hashHeavyAttack);
        }

        ResetCombatInputs();
    }
    
    private void BeginAttack(int animationHash, bool isHeavyAttack)
    {
        if (!_meleeWeapon)
            return;
        
        _animator.SetTrigger(animationHash);
    }
    
    // Called by Animation Events
    public void OnMeleeAttackStart()
    {
        _meleeWeapon?.BeginAttack(false);
    }

    // Called by Animation Events
    public void OnMeleeAttackEnd()
    {
        _meleeWeapon?.EndAttack();
    }
    
    void ResetCombatInputs()
    {
        _input.lightAttack = false;
        _input.heavyAttack = false;
    }
}