using System;
using System.Collections;
using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(EcstasyMeter))]
public class TransformationController : MonoBehaviour
{
    public enum CharacterForm
    {
        Normal,
        Transformed
    }

    public CharacterForm currentForm = CharacterForm.Normal;

    [Header("Transformation Settings")] [SerializeField]
    private AnimationClip transformationAnimationClip;

    [SerializeField] private float transformationCrossFadeTime = 0.1f; // Crossfade time into transformation
    [SerializeField] private float locomotionCrossFadeTime = 0.15f; // Crossfade time into locomotion

    private StarterAssetsInputs _input;
    private PlayerLock _lock;
    private EcstasyMeter _ecstasyMeter;
    private Animator _animator;

    private float _transformationAnimLength;
    private static readonly int HashLocomotion = Animator.StringToHash("Idle Walk Run Blend");
    private static readonly int HashTransformation = Animator.StringToHash("Transformation");
    
    public event Action OnChangeFormToNormal;

    private void Awake()
    {
        _input = GetComponentInParent<StarterAssetsInputs>();
        _lock = GetComponentInParent<PlayerLock>();
        _animator = GetComponentInParent<Animator>();

        _ecstasyMeter = GetComponent<EcstasyMeter>();

        _transformationAnimLength = transformationAnimationClip.length;
    }

    private void OnEnable()
    {
        if (_ecstasyMeter != null)
            _ecstasyMeter.OnEmptied += HandleMeterEmptied;
    }

    private void OnDisable()
    {
        if (_ecstasyMeter != null)
            _ecstasyMeter.OnEmptied -= HandleMeterEmptied;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_input.transformation)
        {
            TryTransformation();
        }

        ResetTransformationInput();
    }

    // Checks if Transformation can be triggered now
    private void TryTransformation()
    {
        // Can't tranform if...
        if (_lock.IsLocked(LockFlags.Actions)) // ...we're locked from doing actions
            return;
        if (!_ecstasyMeter.IsAtThreshold) // ...the ecstasy meter hasn't reached the threshold
            return;
        if (currentForm != CharacterForm.Normal) // ...the character isn't in their normal form
            return;

        // If nothing is blocking us from transforming, go for it
        StartCoroutine(TriggerTransformation());
    }

    private IEnumerator TriggerTransformation()
    {
        Debug.Log("Transformation ACTIVE");

        _animator.CrossFadeInFixedTime(HashTransformation, transformationCrossFadeTime);
        _ecstasyMeter.SetMode(EcstasyMeter.MeterMode.Paused);
        _lock.Lock(this, LockFlags.Movement);
        _lock.Lock(this, LockFlags.Actions);

        // Wait for the transformation Anim to end
        yield return
            new WaitForSeconds(
                _transformationAnimLength); // WIP: this could maybe be an Animation Event instead?? it's more precise and more robust most likely

        currentForm = CharacterForm.Transformed; // WIP: Should this be set before or after the WaitForSeconds???
        _animator.CrossFadeInFixedTime(HashLocomotion, locomotionCrossFadeTime);
        _ecstasyMeter.SetMode(EcstasyMeter.MeterMode.Draining); // Start draining ecstasy
        _lock.Unlock(this);
    }

    private void HandleMeterEmptied()
    {
        if (currentForm != CharacterForm.Transformed)
            return;

        ExitTransformation();
    }

    private void ExitTransformation()
    {
        Debug.Log("Transformation INACTIVE");

        currentForm = CharacterForm.Normal;
        _ecstasyMeter.SetMode(EcstasyMeter.MeterMode.Filling);
        OnChangeFormToNormal?.Invoke(); // Might not be needed anymore after some code changes...
    }

    private void ResetTransformationInput()
    {
        _input.transformation = false;
    }
}