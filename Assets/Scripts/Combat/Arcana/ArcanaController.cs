using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class ArcanaController : MonoBehaviour
{
    public enum ArcanaState
    {
        Idle,
        Presenting, 
        Paired
    }
    public ArcanaState State { get; private set; } = ArcanaState.Idle;
    
    [Header("Arcana")]
    public ArcanaData characterArcana;
    public ArcanaData drawnArcana;
    public ArcanaData stashedArcana;
    public ArcanaData pairedArcana;
    
    [Space(10)]
    [Header("Deck")]
    public List<ArcanaData> ArcanaInPool = new List<ArcanaData>();
    public List<ArcanaData> ArcanaDiscardPile = new List<ArcanaData>();
    
    [Space(10)]
    [Header("Redraw Cooldown")]
    [Tooltip("How long it takes to redraw arcana after discarding")]
    [SerializeField] private float redrawCooldown = 2f;
    
    [Space(10)]
    [Header("Pairs")]
    [SerializeField] private List<ArcanaPairData> allPairs = new List<ArcanaPairData>();
    public ArcanaPairData ActivePair { get; private set; }
    
    // Transformation variables
    private bool IsTransformed => _transformation.currentForm == TransformationController.CharacterForm.Transformed;
    private bool _wasTransformed;
    
    // Redraw cooldown variables
    private float _nextDrawAllowedTime;
    private float _activeCooldownDuration;
    private static float Now => Time.time;
    public bool CanDraw => Now >= _nextDrawAllowedTime;
    public float CooldownRemaining => Mathf.Max(0f, _nextDrawAllowedTime - Now);
    
    
    private StarterAssetsInputs _input;
    private TransformationController _transformation;
    private ArcanaPairLibrary _pairLibrary;
    
    
    void Awake()
    {
        _input = GetComponentInParent<StarterAssetsInputs>();
        _transformation = transform.parent.GetComponentInChildren<TransformationController>();

        // Build the Pair Library
        _pairLibrary = new ArcanaPairLibrary(allPairs);
        
        if (ArcanaInPool.Count == 0)
        {
            Debug.LogWarning("Character doesn't have any Secondary Arcana In Pool!!");
            return;
        }
        _pairLibrary.Validate(characterArcana, ArcanaInPool);
        ShuffleArcanaPool(ArcanaInPool);
    }

    // Update is called once per frame
    void Update()
    {
        // Detect transformation ending and reset everything
        if (_wasTransformed && !IsTransformed)
            ResetArcana();
        _wasTransformed = IsTransformed;
        
        if (_input.drawArcana && IsTransformed) // you use the same input to draw and play arcanas
        {
            if (drawnArcana == null)
                DrawArcana();
            else
                PlayArcana();
        }
        if (_input.discardArcana && IsTransformed)
        {
            DiscardArcana();
        }
        if (_input.stashArcana && IsTransformed) // you use the same input to stash and load arcanas
        {
            if (stashedArcana == null)
                StashArcana();
            else
                LoadArcana();
        }

        ResetArcanaInput();
    }

    void DrawArcana()
    {
        if (State != ArcanaState.Idle)
            return;
        if (!CanDraw)
        {
            Debug.Log($"Redraw on cooldown: {CooldownRemaining:F1}s left");
            return;
        }
        
        // Shuffle the discard pile back in if there's nothing left
        if (ArcanaInPool.Count == 0)
        {
            Debug.Log("No Arcana In Pool, reshuffling...");
            RecycleDiscardPile();
        }
        
        drawnArcana = ArcanaInPool[0];
        ArcanaInPool.RemoveAt(0);
        
        Debug.Log($"Drew: {drawnArcana.name}");
        State = ArcanaState.Presenting;
    }

    // WIP: Ask Kenneth if we want the played Arcana to have a duration time, or if it just lasts indefinitely until replaced
    void PlayArcana()
    {
        if (State != ArcanaState.Presenting)
            return;

        // Check our pair
        ArcanaPairData pair = _pairLibrary.GetPair(characterArcana, drawnArcana);
        if (pair == null)
        {
            Debug.LogError($"CAN'T PLAY CARD. No pair found for {drawnArcana.name}");
            return;
        }
        
        pairedArcana = drawnArcana;
        drawnArcana = null;
        ActivePair = pair;
 
        State = ArcanaState.Paired;
    }

    void StashArcana()
    {
        if (State != ArcanaState.Presenting)
            return;
 
        if (stashedArcana != null)
        {
            Debug.Log("Arcana save slot is already full!");
            return;
        }
 
        stashedArcana = drawnArcana;
        drawnArcana = null;
 
        State = ArcanaState.Idle;
    }
    void LoadArcana()
    {
        if (State != ArcanaState.Idle || stashedArcana == null)
            return;
 
        drawnArcana = stashedArcana;
        stashedArcana = null;
 
        State = ArcanaState.Presenting;
    }

    void DiscardArcana()
    {
        if (State != ArcanaState.Presenting)
            return;
 
        ArcanaDiscardPile.Add(drawnArcana);
        drawnArcana = null;

        StartRedrawCooldown();
        State = ArcanaState.Idle;
    }
    
    private void StartRedrawCooldown()
    {
        _activeCooldownDuration = redrawCooldown;
        _nextDrawAllowedTime = Now + _activeCooldownDuration;
    }
    
    private void ShuffleArcanaPool(List<ArcanaData> list)
    {
        // Fisher-Yates shuffle?
        // for (int i = list.Count - 1; i > 0; i--)
        // {
        //     int j = UnityEngine.Random.Range(0, i + 1);
        //     (list[i], list[j]) = (list[j], list[i]);
        // }
        // return list;
        
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = (UnityEngine.Random.Range(0, n) % n); 
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    // Moves ONLY the discard pile back into the pool (so stashed cards can stay where they are, like when running out of cards to draw mid-transforamtion)
    private void RecycleDiscardPile()
    {
        ArcanaInPool.AddRange(ArcanaDiscardPile);
        ArcanaDiscardPile.Clear();
        ShuffleArcanaPool(ArcanaInPool); // WIP: should we make it so that the last arcana that was drawn can't be first after the discard pile goes back into the pool?
    }
    // Move EVERY arcana back into the pool (like when transformation ends)
    private void ResetArcana()
    {
        // Add the arcanas that may be in circulation into the discard pile
        foreach (ArcanaData arcana in new[] { drawnArcana, stashedArcana, pairedArcana })
            if (arcana != null)
                ArcanaDiscardPile.Add(arcana);
        drawnArcana = null;
        stashedArcana = null;
        pairedArcana = null;
        ActivePair = null;
        
        // Add the (now full) discard pile back to the pool
        RecycleDiscardPile();
        
        _nextDrawAllowedTime = 0f;
        State = ArcanaState.Idle;
    }
    
    private void ResetArcanaInput()
    {
        _input.drawArcana = false;
        _input.discardArcana = false;
        _input.stashArcana = false;
    }
}
