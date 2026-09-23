using UnityEngine;

/// <summary>
/// Attach to the root of each hand prefab (Left Hand / Right Hand).
/// Caches all colliders once on Awake and exposes methods to enable/disable
/// them cheaply at runtime. Self-registers with HandColliderManager so a
/// UI button (or any script/event) can toggle every spawned hand at once.
/// </summary>
public class HandColliderToggle : MonoBehaviour
{
    [Tooltip("Optional: assign if all hand colliders live under ONE Rigidbody " +
             "(e.g. a single driven 'physics hand'). Lets us use the cheap " +
             "Rigidbody.detectCollisions flag instead of looping colliders.")]
    [SerializeField] private Rigidbody handRigidbody;

    private Collider[] handColliders;
    private bool collidersEnabled = true;

    private void Awake()
    {
        // Cache once - never call GetComponentsInChildren at toggle time.
        handColliders = GetComponentsInChildren<Collider>(includeInactive: true);
    }

    private void OnEnable()
    {
        HandColliderManager.Instance?.Register(this);
    }

    private void OnDisable()
    {
        HandColliderManager.Instance?.Unregister(this);
    }

    public void SetCollidersEnabled(bool state)
    {
        if (collidersEnabled == state) return;
        collidersEnabled = state;

        // Cheapest path: one Rigidbody governs every child collider.
        if (handRigidbody != null)
        {
            handRigidbody.detectCollisions = state;
            return;
        }

        // Fallback: independent colliders (e.g. per-bone rigidbodies/joints,
        // no single shared Rigidbody).
        for (int i = 0; i < handColliders.Length; i++)
        {
            if (handColliders[i] != null)
                handColliders[i].enabled = state;
        }
    }

    public bool CollidersEnabled => collidersEnabled;
}
