using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public readonly struct AttackConditions
{
    public readonly Vector2 MoveInput;
    public readonly bool HoldingAttackInput;
    public readonly float TimeInCancelWindow; // Time in seconds we've been in the cancel window. 0 if idle. For combos that require pausing betweet attacks
    public readonly bool IsGrounded;
    public readonly bool IsBlocking;
    public readonly bool IsTransformed;
    
    public AttackConditions(Vector2 moveInput, bool holdingAttackInput, float timeInCancelWindow, bool isGrounded, bool isBlocking, bool isTransformed)
    {
        this.MoveInput = moveInput;
        this.HoldingAttackInput = holdingAttackInput;
        this.TimeInCancelWindow = timeInCancelWindow;
        this.IsGrounded = isGrounded;
        this.IsBlocking = isBlocking;
        this.IsTransformed = isTransformed;
    }
}