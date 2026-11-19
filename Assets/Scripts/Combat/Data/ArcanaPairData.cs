using UnityEngine;

[CreateAssetMenu(fileName = "ArcanaData", menuName = "Scriptable Objects/ArcanaPairData")]
public class ArcanaPairData : ScriptableObject
{
    public ArcanaData arcanaA;
    public ArcanaData arcanaB;
    public string effect; // WIP: figure out a type for this later
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
