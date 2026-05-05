using UnityEngine;
using System.Collections.Generic;


public class Box : MonoBehaviour
{
    [Header("Box parts (1–6 order)")]
    public List<Transform> boxParts = new List<Transform>();

    [Header("Hinges (h1–h5 order)")]
    public List<Transform> hinges = new List<Transform>();

    [Header("Snapping")]
    public float snapMin = 85f;
    public float snapMax = 95f;

    class PartState
    {
        public Transform part;
        public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

        public Transform hinge;
        public Vector3 localAxis;

        public Vector3 startDirLocal;
        public float radius;

        public Quaternion startRot;

        public bool isGrabbed;
    }

    List<PartState> states = new List<PartState>();

    void Start()
    {
        for (int i = 0; i < boxParts.Count; i++)
        {
            var part = boxParts[i];
            if (part == null) continue;

            PartState s = new PartState();
            s.part = part;
            s.grab = part.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

            int partID = i + 1;

            AssignMapping(partID, s);

            if (s.hinge != null)
            {
                // store everything in hinge local space
                Vector3 worldDir = (part.position - s.hinge.position).normalized;

                s.startDirLocal = s.hinge.InverseTransformDirection(worldDir);
                s.radius = Vector3.Distance(part.position, s.hinge.position);
                s.startRot = part.rotation;
            }

            // XR setup
            if (s.grab != null)
            {
                s.grab.trackPosition = false;
                s.grab.trackRotation = false;

                int index = states.Count;
                s.grab.selectEntered.AddListener(_ => states[index].isGrabbed = true);
                s.grab.selectExited.AddListener(_ => OnRelease(states[index]));
            }

            states.Add(s);
        }
    }

    void LateUpdate()
    {
        foreach (var s in states)
        {
            if (!s.isGrabbed) continue;

            ApplyConstraint(s);
        }
    }

    void AssignMapping(int id, PartState s)
    {
        // YOUR EXACT LAYOUT:

        switch (id)
        {
            case 1:
                s.hinge = hinges[0]; // h1
                s.localAxis = Vector3.right;
                break;

            case 2:
                s.hinge = hinges[1]; // h2
                s.localAxis = Vector3.forward;
                break;

            case 3:
                s.hinge = null; // center
                break;

            case 4:
                s.hinge = hinges[2]; // h3
                s.localAxis = -Vector3.forward;
                break;

            case 5:
                s.hinge = hinges[3]; // h4
                s.localAxis = -Vector3.right;
                break;

            case 6:
                s.hinge = hinges[4]; // h5
                s.localAxis = Vector3.right;
                break;
        }
    }

    void ApplyConstraint(PartState s)
    {
        // PART 3 → only Y rotation
        if (s.hinge == null)
        {
            Vector3 e = s.part.localEulerAngles;
            s.part.localEulerAngles = new Vector3(0f, e.y, 0f);
            return;
        }

        // convert current direction into hinge space
        Vector3 worldDir = (s.part.position - s.hinge.position).normalized;
        Vector3 localDir = s.hinge.InverseTransformDirection(worldDir);

        // project onto hinge plane
        localDir = Vector3.ProjectOnPlane(localDir, s.localAxis).normalized;

        // angle in hinge space
        float angle = Vector3.SignedAngle(s.startDirLocal, localDir, s.localAxis);

        Quaternion localRot = Quaternion.AngleAxis(angle, s.localAxis);

        // back to world
        Quaternion worldRot = s.hinge.rotation * localRot;

        Vector3 newDir = worldRot * s.startDirLocal;

        s.part.position = s.hinge.position + newDir * s.radius;
        s.part.rotation = worldRot * s.startRot;
    }

    void OnRelease(PartState s)
    {
        s.isGrabbed = false;

        if (s.hinge == null) return;

        Vector3 worldDir = (s.part.position - s.hinge.position).normalized;
        Vector3 localDir = s.hinge.InverseTransformDirection(worldDir);

        float angle = Vector3.SignedAngle(s.startDirLocal, localDir, s.localAxis);

        if (Mathf.Abs(angle) >= snapMin && Mathf.Abs(angle) <= snapMax)
        {
            float target = Mathf.Sign(angle) * 90f;

            Quaternion localRot = Quaternion.AngleAxis(target, s.localAxis);
            Quaternion worldRot = s.hinge.rotation * localRot;

            Vector3 newDir = worldRot * s.startDirLocal;

            s.part.position = s.hinge.position + newDir * s.radius;
            s.part.rotation = worldRot * s.startRot;
        }
    }
}