using Fusion;
using UnityEngine;

public class LocalUIOnly : NetworkBehaviour
{

    [SerializeField] private GameObject uiRoot;

    public override void Spawned()
    {
        uiRoot.SetActive(Object.HasStateAuthority);
    }
}