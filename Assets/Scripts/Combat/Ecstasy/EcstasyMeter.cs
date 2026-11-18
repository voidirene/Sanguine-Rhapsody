using System;
using UnityEngine;

[RequireComponent(typeof(EcstasyMeterSettings))]
public class EcstasyMeter : MonoBehaviour
{
    public enum MeterMode
    {
        Filling,
        Draining,
        Paused
    }
    private EcstasyMeterSettings _settings;
    private TransformationController _transformationController;
    [SerializeField, Min(0f)] private float startingValue = 0f;
    
    // WIP: this is always set to TRUE. in the future we need to actually detect if enemies are around
    public bool isEnemyAggroed = true;


    [Range(0, 100)] public float EcstasyValue;
    public float Max => _settings.MaxValue;
    public float Normalized => Max > 0f ? EcstasyValue / Max : 0f;
    public bool IsAtThreshold => EcstasyValue >= _settings.TransformThreshold;
    public MeterMode Mode { get; private set; } = MeterMode.Filling;
    public bool PassiveFillActive { get; private set; }
    
    // Events
    public event Action<float> OnValueChanged;
    public event Action OnThresholdReached;
    public event Action OnThresholdLost;
    public event Action OnEmptied;

    private void Awake()
    {
        _settings = GetComponent<EcstasyMeterSettings>();
        if (_settings == null)
        {
            Debug.LogError("EcstasyMeter has no EcstasyMeterSettings on the same gabeobject as itself!", this);
            enabled = false;
            return;
        }
        _transformationController = GetComponent<TransformationController>();
        
        EcstasyValue = Mathf.Clamp(startingValue, 0f, _settings.MaxValue);
        
        // DEBUG, delete later
        // replace with tracking aggro in update
        SetPassiveFillActive(true);
    }

    private void Update()
    {
        // Should we have this in FixedUpdate instead? Since we're working with deltaTime
        switch (Mode)
        {
            case MeterMode.Filling:
                if (PassiveFillActive)
                    ApplyChange(_settings.PassiveFillPerSecond * Time.deltaTime);
                break;

            case MeterMode.Draining:
                ApplyChange(-_settings.DrainPerSecond * Time.deltaTime);
                break;

            case MeterMode.Paused:
                break;
        }
    }
    

    public void SetMode(MeterMode mode)
    {
        Mode = mode;
    }
    // Ask Kenneth: do we want the ecstasy bar to fill up over time when there's at least one enemy aggroed on the player? If not we don't need this
    public void SetPassiveFillActive(bool active)
    {
        PassiveFillActive = active;
    }
    
    // Instantly add to the ecstasy meter (with a combo hit for example)
    public void Add(float amount)
    {
        ApplyChange(amount);
    }
    // Changing meter math
    private void ApplyChange(float amount)
    {
        float currentValue = EcstasyValue;
        float newValue = Mathf.Clamp(currentValue + amount, 0f, Max);
        
        if (Mathf.Approximately(currentValue, newValue)) // this is mostly because we'er dealing with floats... might not be needed, needs more testing
            return;

        EcstasyValue = newValue;
        OnValueChanged?.Invoke(Normalized);

        CheckThreshold();

        if (currentValue > 0f && newValue <= 0f) // Fire OnEmptied to return character to normal
            OnEmptied?.Invoke();
    }

    private bool wasAtThreshold;
    private void CheckThreshold()
    {
        bool isAtThreshold = IsAtThreshold; // are we at the threshold right now?
        if (isAtThreshold == wasAtThreshold) return; // were we already at the threshold? if yes, don't do anything

        wasAtThreshold = isAtThreshold; // if no, this is the first time we reach it!

        if (isAtThreshold) OnThresholdReached?.Invoke();
        else OnThresholdLost?.Invoke();
    }
}