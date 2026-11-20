using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Serializable]
    public struct BaseStat
    {
        public StatType stat;
        public float value;
    }

    [SerializeField] private List<BaseStat> baseStats = new List<BaseStat>
    {
        new BaseStat { stat = StatType.Damage, value = 1f },
        new BaseStat { stat = StatType.EcstasySlowerDrain, value = 1f },
        new BaseStat { stat = StatType.AttackSpeed, value = 1f },
        new BaseStat { stat = StatType.AttackSize, value = 1f },
        new BaseStat { stat = StatType.MoveSpeed, value = 1f }
    };

    private readonly Dictionary<StatType, float> _baseStatLookup = new Dictionary<StatType, float>();
    private readonly List<StatModifier> _modifiers = new List<StatModifier>();
    
    
    public event Action<StatType> OnStatChanged;

    private void Awake()
    {
        // Populate the dictionary
        foreach (BaseStat entry in baseStats)
            _baseStatLookup[entry.stat] = entry.value;
    }
    private void Start()
    {
        ApplyAllStats();
    }
    
    public void ApplyAllStats()
    {
        foreach (StatType stat in _baseStatLookup.Keys)
            OnStatChanged?.Invoke(stat);
    }
    
    public float GetBaseStatValue(StatType stat)
    {
        if (_baseStatLookup.TryGetValue(stat, out float value))
            return value;

        Debug.LogWarning($"No base value set for {stat} stat. Returning 0!");
        return 0f;
    }

    public float GetFinal(StatType stat)
    {
        float additives = 0f;
        float multiplicatives = 1f;

        for (int i = 0; i < _modifiers.Count; i++)
        {
            StatModifier mod = _modifiers[i];
            if (mod.Stat != stat) continue;

            if (mod.Type == ModifierType.Additive)
                additives += mod.Value;
            else
                multiplicatives *= mod.Value;
        }

        return GetBaseStatValue(stat) * (1f + additives) * multiplicatives;
    }

    public void AddModifier(StatModifier modifier)
    {
        _modifiers.Add(modifier);
        OnStatChanged?.Invoke(modifier.Stat);
    }
    public void RemoveModifier(StatModifier modifier)
    {
        if (_modifiers.Remove(modifier))
            OnStatChanged?.Invoke(modifier.Stat);
    }
    
    public void RemoveAllModifiers()
    {
        HashSet<StatType> removed = new HashSet<StatType>();
    
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            removed.Add(_modifiers[i].Stat);
            _modifiers.RemoveAt(i);
        }
    
        foreach (StatType stat in removed)
            OnStatChanged?.Invoke(stat);
    }
}