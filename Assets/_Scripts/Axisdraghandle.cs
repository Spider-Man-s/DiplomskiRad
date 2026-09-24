using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // remove/adjust if your XRI version uses a different namespace

/// <summary>
/// Editor-gizmo-style single-axis drag, driven by transform math - no
/// ConfigurableJoint, no physics tug-of-war.
///
/// Setup:
/// 2. On the parent's XRGrabInteractable, UNCHECK "Track Position" and
///    "Track Rotation". This makes it fire select events only - it will
///    no longer move the object itself. This script takes full control
///    of position instead.
/// 3. Handles stay exactly as they are: MeshCollider + MeshRenderer only.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class AxisDragHandle : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private Rigidbody rb;

    [Header("Handles (collider-only children)")]
    [SerializeField] private Transform xHandle;
    [SerializeField] private Transform yHandle;
    [SerializeField] private Transform zHandle;

    private Transform interactorTransform;
    private Vector3 axisDirection;   // world-space direction of the active axis, captured at grab time
    private Vector3 grabStartHandPos;
    private Vector3 grabStartObjectPos;
    private bool dragging;

    public void Start()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            Debug.LogError("AxisDragHandle: Missing required components.");
            return;
        }
    }

    public void enableTracking()
    {
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
    }

    public void disableTracking()
    {
        grabInteractable.trackPosition = false;
        grabInteractable.trackRotation = false;
    }
    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        grabInteractable.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        interactorTransform = (args.interactorObject as Component)?.transform;
        if (interactorTransform == null) return;

        Transform nearest = GetNearestHandle(interactorTransform.position);

        // Local axis direction in world space, captured once at grab start -
        // this is what makes it "the parent's own axis" rather than world XYZ.
        axisDirection = nearest == xHandle ? transform.right
                       : nearest == yHandle ? transform.up
                       : transform.forward;

        grabStartHandPos = interactorTransform.position;
        grabStartObjectPos = rb.position;
        dragging = true;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        dragging = false;
        interactorTransform = null;
    }

    private void FixedUpdate()
    {
        if (!dragging || interactorTransform == null) return;

        Vector3 handDelta = interactorTransform.position - grabStartHandPos;
        float distanceAlongAxis = Vector3.Dot(handDelta, axisDirection);
        Vector3 targetPos = grabStartObjectPos + axisDirection * distanceAlongAxis;

        // Kinematic Rigidbody + MovePosition: still gets proper collision
        // response against other objects, but nothing here is physics-driven.
        rb.MovePosition(targetPos);
    }

    private Transform GetNearestHandle(Vector3 grabPosition)
    {
        Transform nearest = xHandle;
        float nearestDist = Vector3.SqrMagnitude(grabPosition - xHandle.position);

        float yDist = Vector3.SqrMagnitude(grabPosition - yHandle.position);
        if (yDist < nearestDist) { nearest = yHandle; nearestDist = yDist; }

        float zDist = Vector3.SqrMagnitude(grabPosition - zHandle.position);
        if (zDist < nearestDist) { nearest = zHandle; nearestDist = zDist; }

        return nearest;
    }
}