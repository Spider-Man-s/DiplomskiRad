using UnityEngine;

public class ColocationMesh : MonoBehaviour
{
    [Header("Objects to change")]
    [SerializeField] private Renderer[] renderersToChange;

    [Header("Materials")]
    [SerializeField] private Material visibleMaterial;
    [SerializeField] private Material hiddenMaterial;

    private bool lastState;

    private void Start()
    {
        lastState = FusionBoot.ColocationAvatarsVisible;
        ApplyMaterial();
    }

    private void ApplyMaterial()
    {
        Material materialToApply =
            FusionBoot.ColocationAvatarsVisible ? visibleMaterial : hiddenMaterial;

        foreach (Renderer r in renderersToChange)
        {
            if (r == null)
                continue;

            Material[] mats = r.materials;

            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = materialToApply;
            }

            r.materials = mats;
        }
    }
}
