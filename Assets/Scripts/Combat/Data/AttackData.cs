using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackData", menuName = "Scriptable Objects/AttackData")]
public class AttackData : ScriptableObject
{
    [Header("Animation Settings")]
    public AnimationClip clip; // The attack's animation clip
    public string stateName; // Which Animator state to play
    public int StateHash { get; private set; }
    [Tooltip("Tick if the Animator state is named differently from the clip on purpose")]
    public bool allowStateNameMismatch;
    [DoNotSerialize][Min(0f)] public float Duration => clip != null ? clip.length : 0f;
    
    [Space(10)]
    [Header("Transition Settings")]
    public float crossFadeTime = 0.1f; // Per-attack blending; Heavy attacks often want longer crossfades
    [Range(0f, 1f)]
    public float cancelWindowStart = 0.5f; // From here on, the player may chain into the nextAttack attack
    [Range(0f, 1f)]
    public float exitTime = 0.9f; // When the attack is considered finished, and you can return to 
    
    [Space(10)]
    [Header("Damage & Follow-up Attacks")]
    [Min(0f)] public float damageMultiplier = 1; // Scales the weapon’s base damage
    public List<ComboTransition> transitions; // List of follow-up attacks

    void OnEnable()
    {
        RefreshHash();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        crossFadeTime = Mathf.Max(0f, crossFadeTime);
        damageMultiplier = Mathf.Max(0f, damageMultiplier);
        
        // Just in case these two are changed through code and not the inspector
        cancelWindowStart = Mathf.Clamp01(cancelWindowStart);
        exitTime = Mathf.Clamp01(exitTime);

        // The attack can't end before its cancel window opens.
        // Raising cancelWindowStart past exitTime pushes exitTime up with it.
        exitTime = Mathf.Max(exitTime, cancelWindowStart);
        
        // OPTIONAL: default the state name to the clip's name
        if (clip != null && string.IsNullOrEmpty(stateName))
            stateName = clip.name;

        if (clip != null && !allowStateNameMismatch && stateName != clip.name)
            Debug.LogWarning($"{name}: stateName '{stateName}' doesn't match clip '{clip.name}'. " +
                             "Fix it, or tick Allow State Name Mismatch if that's intended.", this);

        if (clip == null && !string.IsNullOrEmpty(stateName))
            Debug.LogWarning($"{name}: has a state name but no clip, so the controller " +
                             "has no clip.length to compute progress from.", this);
        
        RefreshHash();
    }
#endif
    
    void RefreshHash()
    {
        StateHash = string.IsNullOrEmpty(stateName) ? 0 : Animator.StringToHash(stateName);
    }
}

[Serializable]
public class ComboTransition
{
    public AttackInput input;
    public AttackData nextAttack;
    [Tooltip("In the case of multiple transitions having their requirements fulfilled, the one with the highest priority will be executed.")]
    public int priority;
    
    // Refers to certain extra inputs that change which combo is performed
    [Header("Transition Requirements")]
    public bool movingForward;
    public bool movingBackward;
    public bool movingLeft;
    public bool movingRight;
    public bool holdingAttackInput;
    [Min(0f)] public float minTimeInCancelWindow;   // in seconds
    public AirRequirement air = AirRequirement.Any;
    public bool dodged;
    public bool blocking;
    public bool transformation;
    
    
    public bool ConditionsMet(in AttackConditions conditions)
    {
        // Go through all conditions, and if one isnt met return false, otherwise return true
        // Movement
        if (movingForward && conditions.MoveInput.y <= 0.5f) return false;
        if (movingBackward && conditions.MoveInput.y >= -0.5f) return false;
        if (movingLeft && conditions.MoveInput.x >= -0.5f) return false;
        if (movingRight && conditions.MoveInput.x <= 0.5f) return false;
        // Holding attack input
        if (holdingAttackInput && !conditions.HoldingAttackInput) return false;
        // Pause between attacks
        if (minTimeInCancelWindow > conditions.TimeInCancelWindow) return false;
        // Airborne/Jumping
        if (air == AirRequirement.Grounded && !conditions.IsGrounded) return false;
        if (air == AirRequirement.Airborne && conditions.IsGrounded) return false;
        // Dodge
        if (dodged && !conditions.JustDodged) return false;
        // Blocking
        if (blocking && !conditions.IsBlocking) return false;
        if (transformation && !conditions.IsTransformed) return false;
        
        return true;
    }
}

public enum AirRequirement { Any, Grounded, Airborne }