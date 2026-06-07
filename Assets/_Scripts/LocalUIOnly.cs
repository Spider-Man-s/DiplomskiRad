using Fusion;
using UnityEngine;

public class LocalUIOnly : NetworkBehaviour
{
    public override void Spawned()
    {
        gameObject.SetActive(Object.HasInputAuthority);
    }
}