using UnityEngine;

[CreateAssetMenu(fileName = "ArcanaPairData", menuName = "Scriptable Objects/ArcanaPairData")]
public class ArcanaPairData : ScriptableObject
{
    [Header("Arcana Pair")]
    public ArcanaData arcanaA;
    public ArcanaData arcanaB;

    [Header("Info")]
    [TextArea] public string description;
    
    public string effect; // WIP: figure out a type for this later
}
