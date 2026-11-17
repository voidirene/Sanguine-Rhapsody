using Gamekit3D;
using StarterAssets;
using UnityEngine;

// NOTE: certain things like the actual attack logic are triggered through the animator using functions like OnMeleeAttackStart
// Make sure all animation clips have events attached to them to trigger these events otherwise while the animations play, no damage will be dealt
public class WeaponController : MonoBehaviour
{
    [SerializeField] private bool canAttack = true;
    
    [Space(10)]
    // Allowing the player to press a button earlier and still count
    [Header("Input Buffer")]
    [SerializeField] private float bufferDuration = 0.2f;
    private AttackInput _bufferedInput = AttackInput.None;
    private float _bufferedTime;
    
    [Space(10)]
    [Header("Combo")]
    [Tooltip("The idle node. Its transitions are the attacks you can start from standing.")]
    [SerializeField] private AttackData rootAttack;
    [SerializeField] private float returnToLocomotionBlend = 0.15f;
    private AttackData _currentAttack; // rootAttack when idle
    private float _elapsed; // seconds since _currentAttack started
    private bool IsIdle => _currentAttack == rootAttack;
    
    private ThirdPersonController _controller;   
    private MeleeWeapon _meleeWeapon;
    private RangeWeapon _rangeWeapon;
    private StarterAssetsInputs _input;
    private Animator _animator;
    
    private static readonly int HashLocomotion = Animator.StringToHash("Idle Walk Run Blend");

    private void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
        _animator = GetComponent<Animator>();
        
        _controller = GetComponent<ThirdPersonController>();
        _meleeWeapon = GetComponentInChildren<MeleeWeapon>();
        _rangeWeapon = GetComponentInChildren<RangeWeapon>();
        if (!_meleeWeapon && !_rangeWeapon)
        {
            Debug.LogWarning($"{name} needs a MeleeWeapon or RangeWeapon.", this);
            enabled = false;
            return;
        }
        if (_meleeWeapon) _meleeWeapon.SetOwner(gameObject);
        
        if (!rootAttack)
        {
            Debug.LogError($"{name} needs a Root AttackData assigned.", this);
            enabled = false;
            return;
        }
        _currentAttack = rootAttack;
    } 

    private void Update()
    {
        CaptureInput();
        ExpireBuffer();
        TickCombo();
        ResetCombatInputs();
    }

    #region Input
    private void CaptureInput()
    {
        if (!canAttack)
            return;

        // Heavy is checked second, so if both land on the same frame, heavy wins
        // Ask Kenneth: do we want priorities like this?
        if (_input.block)
            BufferInput(AttackInput.Block);
        if (_input.lightAttack)
            BufferInput(AttackInput.Light);
        if (_input.heavyAttack)
            BufferInput(AttackInput.Heavy);
    }
    
    private void BufferInput(AttackInput input)
    {
        _bufferedInput = input;        // latest press overwrites the old one
        _bufferedTime = Time.time;
        Debug.Log($"Buffered {input}");
    }
    
    // True only while a press is stored and still fresh
    private bool HasBufferedInput =>
        _bufferedInput != AttackInput.None &&
        Time.time - _bufferedTime <= bufferDuration;

    // READ BUFFERED INPUT FROM HERE!
    private AttackInput PeekBuffer()
    {
        return HasBufferedInput ? _bufferedInput : AttackInput.None;
    }
    
    // Call when a transition fires, so one press can't start two attacks
    private void ConsumeBuffer()
    {
        _bufferedInput = AttackInput.None;
    }

    // Clear buffer if the input has timed out
    private void ExpireBuffer()
    {
        if (_bufferedInput != AttackInput.None && !HasBufferedInput)
        {
            Debug.Log($"Buffer expired ({_bufferedInput})");
            _bufferedInput = AttackInput.None;
        }
    }
    #endregion

    #region Combo
    private void TickCombo()
    {
        // Kill the combo (in case of e.g getting hurt, dying, cutscene, etc)
        if (!canAttack)
        {
            if (!IsIdle) AbortCombo();
            return;
        }

        // If not attacking right now, check if there's input, then try to start a combo chain
        if (IsIdle)
        {
            TryChain();
            return;
        }

        // Count time passing since the start of an attack
        _elapsed += Time.deltaTime * _animator.speed;
        float progress = _elapsed / Mathf.Max(_currentAttack.Duration, 0.01f);

        // If enough time has passed for us to enter exitTime, return to locomotion
        if (progress >= _currentAttack.exitTime)
        {
            FinishAttack();
            return;
        }

        // If enough time has passed since the start of the attack, try to chain another attack
        if (progress >= _currentAttack.cancelWindowStart)
            TryChain();
    }
    
    private void TryChain()
    {
        //No attack
        AttackInput input = PeekBuffer();
        if (input == AttackInput.None)
            return;
        
        AttackConditions conditions = SetConditions(); // should we call this separately on Update()? No need right?
        ComboTransition transition = FindNext(_currentAttack, input, conditions);
        if (transition != null)
            StartAttack(transition.nextAttack);
    }

    private ComboTransition FindNext(AttackData currentAttack, AttackInput input, in AttackConditions conditions)
    {
        ComboTransition bestMatch = null;
        
        foreach (var t in currentAttack.transitions)
        {
            if (t.input != input) continue;
            if (t.nextAttack == null) continue;
            if (!t.ConditionsMet(conditions)) continue;

            if (bestMatch == null || t.priority > bestMatch.priority) // for the first matching item, save it, and for the next ones save them only if their priority is higher
                bestMatch = t;
        }
        
        return bestMatch;
    }

    
    private void StartAttack(AttackData next)
    {
        Debug.Log($"Start {next.name} (from {_currentAttack.name})");

        if (_meleeWeapon)
            _meleeWeapon.EndAttack();   // close any open hit window

        _currentAttack = next;
        _elapsed = 0f;

        _animator.CrossFadeInFixedTime(next.StateHash, next.crossFadeTime);

        ConsumeBuffer();
    }

    private void FinishAttack()
    {
        Debug.Log($"Finish {_currentAttack.name}");

        if (_meleeWeapon) _meleeWeapon.EndAttack();

        _currentAttack = rootAttack;
        _elapsed = 0f;

        _animator.CrossFadeInFixedTime(HashLocomotion, returnToLocomotionBlend);
    }

    // Aborting may happen because of the character getting hurt, dying, a cutscene, etc
    private void AbortCombo()
    {
        if (_meleeWeapon) _meleeWeapon.EndAttack();

        _currentAttack = rootAttack;
        _elapsed = 0f;
        ConsumeBuffer();
    }
    #endregion

    #region Conditions

    private AttackConditions SetConditions()
    {
        // Movement
        Vector2 moveInput = _input.move;
        // Holding attack input
        bool holdingAttackInput = _input.lightAttack || _input.heavyAttack; // WIP: this might not be too robust, test it a lot
        // Pause between attacks (0 when idle or when still mid-attack)
        float windowOpensAt = _currentAttack.cancelWindowStart * _currentAttack.Duration;
        float timeInCancelWindow = IsIdle ? 0f : Mathf.Max(0f, _elapsed - windowOpensAt);
        // Airborne/Jumping
        bool isGrounded = _controller.Grounded;
        // Dodge
        bool justDodged = _input.sprint; // WIP: this currently just checks if we're sprinting. sprint and dodge are the same button, but dodge is a tap while sprint is a hold. implement dodge functionality
        // Blocking
        bool isBlocking = _input.block;

        return new AttackConditions(moveInput, holdingAttackInput, timeInCancelWindow, isGrounded, justDodged, isBlocking);
    }

    #endregion
    
    // ---------- Animation events ----------
    public void OnMeleeAttackStart()
    {
        _meleeWeapon?.BeginAttack(false);
    }
    public void OnMeleeAttackEnd()
    {
        _meleeWeapon?.EndAttack();
    }
    
    private void ResetCombatInputs()
    {
        _input.lightAttack = false;
        _input.heavyAttack = false;
    }
}