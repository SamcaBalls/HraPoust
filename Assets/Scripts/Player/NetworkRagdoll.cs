using Mirror;
using UnityEngine;

public class NetworkRagdoll : NetworkBehaviour
{
    [SerializeField] RagdollHandler ragdollHandler;

    [Command]
    public void SetRagdoll(bool on)
    {
        ragdollHandler.SetRagdoll(on, Vector3.forward*1);
    }

}
