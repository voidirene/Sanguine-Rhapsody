using UnityEngine;

public class EcstasyMeterSettings : MonoBehaviour
{
    [Header("Range")]
    [SerializeField, Min(1f)] private float maxValue = 100f;
 
    [Tooltip("Fill rate while at least one enemy is aggro'ed")]
    [SerializeField, Min(0f)] private float passiveFillPerSecond = 1.5f;
 
    [Tooltip("Drain rate during transformation mode")]
    [SerializeField, Min(0f)] private float drainPerSecond = 6f;
 
    [Header("Ecstasy gain per hit on enemies")]
    [SerializeField, Min(0f)] private float baseGainPerHit = 2f;
    
    [Space(10)]
    [Header("Transformation")]
    [Tooltip("At what % of the bar should the player be able to transform?")]
    [SerializeField, Min(0f)] private float transformThreshold = 70f;
    
    public float MaxValue => maxValue;
    public float TransformThreshold => transformThreshold;
    public float PassiveFillPerSecond => passiveFillPerSecond;
    public float DrainPerSecond => drainPerSecond;
    public float BaseGainPerHit => baseGainPerHit;
 
    private void OnValidate()
    {
        transformThreshold = Mathf.Min(transformThreshold, maxValue);
    }
}
