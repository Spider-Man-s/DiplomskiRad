using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates only the 12 real cuboid edges (no triangle diagonals), plus optional corner markers.
/// Attach this to the same center-pivot object as BoxCalibrationRig.
/// </summary>
public class WireframeBoxRenderer : MonoBehaviour
{
    [SerializeField] private Vector3 boxSizeMeters = new Vector3(0.40f, 0.12f, 0.25f);

    [Header("Edges")]
    [SerializeField] private Material edgeMaterial;
    [SerializeField, Min(0.0005f)] private float edgeWidthMeters = 0.003f;

    [Header("Corners")]
    [SerializeField] private bool showCorners = true;
    [SerializeField, Min(0.001f)] private float cornerSizeMeters = 0.012f;
    [SerializeField] private Material cornerMaterial;
    [SerializeField] private Material referenceCornerMaterial;

    [Header("Generated hierarchy")]
    [SerializeField] private string generatedRootName = "GeneratedCalibrationWireframe";

    public Vector3 BoxSizeMeters
    {
        get => boxSizeMeters;
        set => boxSizeMeters = value;
    }

    private Transform generatedRoot;

    private static readonly int[,] EdgePairs =
    {
        {0,1}, {1,2}, {2,3}, {3,0},
        {4,5}, {5,6}, {6,7}, {7,4},
        {0,4}, {1,5}, {2,6}, {3,7}
    };

    private void Awake()
    {
        Rebuild();
    }

    [ContextMenu("Rebuild Wireframe")]
    public void Rebuild()
    {
        if (!Application.isPlaying && gameObject.scene.name == null)
            return;

        ClearGenerated();

        GameObject rootObject = new GameObject(generatedRootName);
        generatedRoot = rootObject.transform;
        generatedRoot.SetParent(transform, false);

        Vector3[] corners = GetCorners();

        for (int i = 0; i < EdgePairs.GetLength(0); i++)
        {
            GameObject edge = new GameObject($"Edge_{i:00}");
            edge.transform.SetParent(generatedRoot, false);

            LineRenderer lr = edge.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.positionCount = 2;
            lr.SetPosition(0, corners[EdgePairs[i, 0]]);
            lr.SetPosition(1, corners[EdgePairs[i, 1]]);
            lr.startWidth = edgeWidthMeters;
            lr.endWidth = edgeWidthMeters;
            lr.numCapVertices = 2;
            lr.numCornerVertices = 2;

            if (edgeMaterial != null)
                lr.sharedMaterial = edgeMaterial;
        }

        if (showCorners)
        {
            for (int i = 0; i < corners.Length; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = i == 0 ? "Corner_0_SHARED_ORIGIN" : $"Corner_{i}";
                marker.transform.SetParent(generatedRoot, false);
                marker.transform.localPosition = corners[i];
                marker.transform.localRotation = Quaternion.identity;
                marker.transform.localScale = Vector3.one * (i == 0 ? cornerSizeMeters * 1.5f : cornerSizeMeters);

                Collider collider = marker.GetComponent<Collider>();
                if (collider != null)
                    Destroy(collider);

                MeshRenderer mr = marker.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    if (i == 0 && referenceCornerMaterial != null)
                        mr.sharedMaterial = referenceCornerMaterial;
                    else if (cornerMaterial != null)
                        mr.sharedMaterial = cornerMaterial;
                }
            }
        }
    }

    private Vector3[] GetCorners()
    {
        Vector3 h = boxSizeMeters * 0.5f;

        // Index 0 is bottom-left-front and therefore the Shared Space reference corner.
        return new[]
        {
            new Vector3(-h.x, -h.y, -h.z), // 0 BLF
            new Vector3( h.x, -h.y, -h.z), // 1 BRF
            new Vector3( h.x,  h.y, -h.z), // 2 TRF
            new Vector3(-h.x,  h.y, -h.z), // 3 TLF
            new Vector3(-h.x, -h.y,  h.z), // 4 BLB
            new Vector3( h.x, -h.y,  h.z), // 5 BRB
            new Vector3( h.x,  h.y,  h.z), // 6 TRB
            new Vector3(-h.x,  h.y,  h.z), // 7 TLB
        };
    }

    private void ClearGenerated()
    {
        Transform existing = transform.Find(generatedRootName);
        if (existing == null)
            return;

        if (Application.isPlaying)
            Destroy(existing.gameObject);
        else
            DestroyImmediate(existing.gameObject);
    }
}
