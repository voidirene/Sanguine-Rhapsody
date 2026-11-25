using UnityEngine;

public enum ButtonEvent
{
    None,
    Tap,
    Hold
}

// For figuring out if a button input was a tap/press or a hold...
public class HoldDetector
{
    private bool _tracking;     // keeps track of if a press is currently being figured out
    private float _pressTime;

    public ButtonEvent Tick(bool pressedThisFrame, bool isHeld, float holdThreshold, bool holdPossible)
    {
        if (pressedThisFrame)
        {
            if (!holdPossible)
            {
                _tracking = false;
                return ButtonEvent.Tap;
            }

            _tracking = true;
            _pressTime = Time.time;
        }

        if (!_tracking)
            return ButtonEvent.None;

        // Released before the threshold: it was a tap
        if (!isHeld)
        {
            _tracking = false;
            return ButtonEvent.Tap;
        }

        // If it's still down when we reach the threshold it's a hold
        if (Time.time - _pressTime >= holdThreshold)
        {
            _tracking = false;
            return ButtonEvent.Hold;
        }

        return ButtonEvent.None;
    }

    public void Cancel()
    {
        _tracking = false;
    }
}
