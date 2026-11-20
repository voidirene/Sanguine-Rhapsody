using System;
using UnityEngine;

public enum StatType
{
    Damage,
    EcstasySlowerDrain,
    AttackSpeed,
    AttackSize,
    MoveSpeed
}
public enum ModifierType
{
    Additive,
    Multiplicative
}
 
[Serializable]
public struct ModifierDefinition
{
    public StatType stat;
    public ModifierType type;
    public float value;
    public float duration;
}
 
[CreateAssetMenu(fileName = "StatModifier", menuName = "Scriptable Objects/StatModifier")]
public class StatModifier : ScriptableObject
{
    public StatType Stat;
    public ModifierType Type;
    public float Value;
    public float Duration;
 
    public StatModifier(StatType stat, ModifierType type, float value, float duration)
    {
        Stat = stat;
        Type = type;
        Value = value;
        Duration = duration;
    }
}
