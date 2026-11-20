using System;
using System.Collections.Generic;
using UnityEngine;

public class ArcanaPairLibrary
{
    private readonly struct PairKey : IEquatable<PairKey>
    {
        private readonly ArcanaData _a;
        private readonly ArcanaData _b;

        public PairKey(ArcanaData a, ArcanaData b)
        {
            _a = a;
            _b = b;
        }

        // (A=A, B=B) or (A=B, B=A)
        public bool Equals(PairKey other) =>
            (_a == other._a && _b == other._b) ||
            (_a == other._b && _b == other._a);
        // checks the type, then hands off to Equals(PairKey). might not be needed tbh
        public override bool Equals(object obj) => obj is PairKey other && Equals(other);

        public override int GetHashCode() => (_a != null ? _a.GetHashCode() : 0) ^ (_b != null ? _b.GetHashCode() : 0); // ^ is XOR. a ^ b always equals b ^ a
    }

    private readonly Dictionary<PairKey, ArcanaPairData> _lookup = new Dictionary<PairKey, ArcanaPairData>();

    public ArcanaPairLibrary(IEnumerable<ArcanaPairData> pairs)
    {
        foreach (ArcanaPairData pair in pairs)
        {
            // Do some checks...
            if (pair == null)
            {
                Debug.LogWarning($"Arcana Pair is empty!! {pair.name}");
                continue;
            }
            if (pair.arcanaA == null || pair.arcanaB == null)
            {
                Debug.LogWarning($"Arcana Pair has an empty arcana slot!! {pair.name}");
                continue;
            }

            var key = new PairKey(pair.arcanaA, pair.arcanaB);
            if (_lookup.ContainsKey(key))
            {
                Debug.LogWarning($"Arcana Pair duplicate for {pair.arcanaA.title} + {pair.arcanaB.title}!!");
                continue;
            }
            
            // ...and if everything looks good, add to the dictionary
            _lookup.Add(key, pair);
        }
    }

    public ArcanaPairData GetPair(ArcanaData a, ArcanaData b)
    {
        if (a == null || b == null)
            return null;

        _lookup.TryGetValue(new PairKey(a, b), out ArcanaPairData pair);
        return pair;
    }

    // Check that baseArcana has a pair SO asset with every arcana in the pool
    public bool Validate(ArcanaData baseArcana, IEnumerable<ArcanaData> pool)
    {
        if (baseArcana == null)
        {
            Debug.LogError("Character has no base arcana!!!");
            return false;
        }

        bool allPresent = true;
        foreach (ArcanaData card in pool)
        {
            if (card == null)
                continue;

            if (GetPair(baseArcana, card) == null)
            {
                Debug.LogError($"Missing pair SO asset!: {baseArcana.title} + {card.title}");
                allPresent = false;
            }
        }
        return allPresent;
    }
}