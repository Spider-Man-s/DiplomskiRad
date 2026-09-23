using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene-wide switchboard for hand colliders. Place ONE instance in your
/// scene (e.g. on a "Managers" GameObject that exists before hands spawn).
/// Wire UI Buttons directly to EnableAllHandColliders() / DisableAllHandColliders(),
/// or call SetAllHandsCollidersEnabled(bool) from any script/event.
/// </summary>
public class HandColliderManager : MonoBehaviour
{
    public static HandColliderManager Instance { get; private set; }

    private readonly List<HandColliderToggle> registeredHands = new List<HandColliderToggle>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(HandColliderToggle hand)
    {
        if (!registeredHands.Contains(hand))
            registeredHands.Add(hand);
    }

    public void Unregister(HandColliderToggle hand)
    {
        registeredHands.Remove(hand);
    }

    // Two explicit no-argument methods so they show up cleanly in the
    // Button's OnClick() dropdown. UnityEvents don't let you reliably set
    // a bool argument per-button in the inspector, so avoid exposing a
    // single SetAll(bool) directly to UI - use these wrappers instead.
    public void EnableAllHandColliders() => SetAllHandsCollidersEnabled(true);
    public void DisableAllHandColliders() => SetAllHandsCollidersEnabled(false);

    public void SetAllHandsCollidersEnabled(bool state)
    {
        for (int i = 0; i < registeredHands.Count; i++)
        {
            if (registeredHands[i] != null)
                registeredHands[i].SetCollidersEnabled(state);
        }
    }
}
