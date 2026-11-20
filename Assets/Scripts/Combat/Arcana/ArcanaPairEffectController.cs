using System;
using System.Collections.Generic;
using UnityEngine;

class ActivePair
{
    public ArcanaPairData Pair;
    public readonly List<ActiveModifier> Modifiers = new List<ActiveModifier>();
}
class ActiveModifier
{
    public StatModifier Modifier;
    public float Duration; // 0 means it will last until the transformation ends
    public float EndTime;
    public bool IsTimed => Duration > 0f;
}

public class ArcanaPairEffectController : MonoBehaviour
{
    private readonly List<ActivePair> _activePairs = new List<ActivePair>();
    public int ActivePairCount => _activePairs.Count;
    
    private static float Now => Time.time; // this WILL be modified if we tweak timeScale. if we don't want this, use unscaledTime
    
    private ArcanaController _arcanaController;
    private PlayerStats _stats;
    
    public event Action<ArcanaPairData> OnPairEnded; // Fires once ONLY when the pair's LAST modifier is gone.

    public void Awake()
    {
        _arcanaController = GetComponent<ArcanaController>();
        _stats = transform.parent.GetComponentInChildren<PlayerStats>();
    }

    public void OnEnable()
    {
        if (_arcanaController == null) return;
        _arcanaController.OnPairActivated += ApplyPair;
        _arcanaController.OnArcanaReset += EndAllPairs;
    }
    public void OnDisable()
    {
        if (_arcanaController == null) return;
        _arcanaController.OnPairActivated -= ApplyPair;
        _arcanaController.OnArcanaReset -= EndAllPairs;
    }

    private void Update()
    {
        // Countdown Timer
        for (int i = _activePairs.Count - 1; i >= 0; i--)
        {
            ActivePair active = _activePairs[i];

            for (int j = active.Modifiers.Count - 1; j >= 0; j--)
            {
                ActiveModifier effect = active.Modifiers[j];
                if (!effect.IsTimed || Now < effect.EndTime) continue;

                active.Modifiers.RemoveAt(j);
                _stats.RemoveModifier(effect.Modifier);
                
                Debug.Log("Modifier Timed out");
            }

            // Last modifier gone
            if (active.Modifiers.Count == 0)
            {
                _activePairs.RemoveAt(i);
                OnPairEnded?.Invoke(active.Pair);
            }
        }
    }

    public void ApplyPair(ArcanaPairData pair)
    {
        if (pair == null || _stats == null) return;
        
        ActivePair active = FindActive(pair);
        
        bool isNew = (active == null);
        if (isNew)
        {
            active = new ActivePair { Pair = pair };
        }
        else
        {
            RemoveAllModifiersOf(active); // This will make it so if the same pair is replayed, it restarts its modifiers' timers
        }
        
        foreach (StatModifier effect in pair.effects) // Add the new modifier(s)
        {
            if (effect == null) continue;

            _stats.AddModifier(effect);
            active.Modifiers.Add(new ActiveModifier
            {
                Modifier = effect,
                Duration = effect.Duration,
                EndTime = Now + effect.Duration
            });
            Debug.Log($"Added modifier: {effect.Value}, Duration: {effect.Duration}");
        }
        
        if (active.Modifiers.Count == 0) // Edge case but... we don't really need to track pairs thatdon't have valid modifiers
        {
            if (!isNew) _activePairs.Remove(active);
            return;
        }   

        if (isNew) _activePairs.Add(active);
    }
    
    private ActivePair FindActive(ArcanaPairData pair)
    {
        foreach (ActivePair active in _activePairs)
            if (active.Pair == pair)
                return active;

        return null;
    }
    
    public void EndAllPairs()
    {
        for (int i = _activePairs.Count - 1; i >= 0; i--)
        {
            ActivePair active = _activePairs[i];
            _activePairs.RemoveAt(i);
            RemoveAllModifiersOf(active);
            OnPairEnded?.Invoke(active.Pair);
        }
    }
    private void RemoveAllModifiersOf(ActivePair active)
    {
        foreach (ActiveModifier effect in active.Modifiers)
            _stats.RemoveModifier(effect.Modifier);
        active.Modifiers.Clear();
    }
}