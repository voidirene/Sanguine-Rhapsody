using UnityEngine;

[CreateAssetMenu(fileName = "ArcanaPairData", menuName = "Scriptable Objects/ArcanaPairData")]
public class ArcanaPairData : ScriptableObject
{
    [Header("Arcana Pair")]
    public ArcanaData arcanaA;
    public ArcanaData arcanaB;

    // Text fields if we want them???
    [Header("Info")]
    [TextArea] public string title;
    [TextArea] public string description;
    
    public StatModifier[] effects;
}
