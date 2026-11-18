using System;
using System.Collections.Generic;
using UnityEngine;

// What things we can lock
[Flags]
public enum LockFlags
{
    None = 0,
    Movement = 1 << 0,
    Actions = 1 << 1, // includes attacks, dodging, transformations, etc
}

// For locking input during certain gaeplay moments (e g taking damage, dying, transformation animation)
public class PlayerLock : MonoBehaviour
{
    private readonly Dictionary<object, LockFlags> _locks = new Dictionary<object, LockFlags>();
    private LockFlags _combined = LockFlags.None;

    private readonly List<object> _deadOwners = new List<object>();

    // NOTE: Make sure to ALWAYS Unlock() when destroying the object that started the Lock, OnDisable() is a good place to do it!
    public void Lock(object lockOwner, LockFlags lockedCategory)
    {
        if (lockOwner == null)
        {
            Debug.LogWarning("Lock() must have a lock Owner!", this);
            return;
        }

        if (lockedCategory == LockFlags.None)
        {
            Debug.LogWarning("You can't Lock() nothing!", this);
            return;
        }

        // WIP: is it better to just overwrite the existing lock in the same category?
        if (_locks.TryGetValue(lockOwner, out LockFlags existing) && existing == lockedCategory)
            return;

        _locks[lockOwner] = lockedCategory;
        ApplyLocking();
    }

    public void Unlock(object owner)
    {
        if (owner == null)
            return;

        _locks.Remove(owner); //WIP: should we unlock per-category? Or is it okay if one run of Unlock() unlocks everything from that owner
        ApplyLocking();
    }

    public bool IsLocked(LockFlags lockedCategory)
    {
        return (_combined & lockedCategory) != 0;
    }

    // Emergency reset
    public void ClearAllLocks()
    {
        if (_locks.Count == 0)
            return;

        _locks.Clear();
        ApplyLocking();
    }

    // --- SAFETY NET FOR RELEASING LOCKS FROM DEAD OWNERS, can comment this block out if we know what we're doing
    private void Update()
    {
        if (_locks.Count == 0)
            return;

        _deadOwners.Clear();
        foreach (object owner in _locks.Keys)
        {
            if (owner is UnityEngine.Object unityObject && unityObject == null)
                _deadOwners.Add(owner);
        }

        if (_deadOwners.Count == 0)
            return;

        foreach (object owner in _deadOwners)
            _locks.Remove(owner);

        Debug.LogWarning($"[PlayerLocks] Released {_deadOwners.Count} lock(s) held by destroyed object(s). Every owner should call Unlock() in OnDisable.", this);

        ApplyLocking();
    }
    // ---

    private void ApplyLocking()
    {
        _combined = LockFlags.None;
        foreach (LockFlags flags in _locks.Values)
            _combined |= flags;
    }
}