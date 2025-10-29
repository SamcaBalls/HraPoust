using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class RagdollDrag : NetworkBehaviour
{
    [SerializeField] private Transform handTransform; // Ruka hr·Ëe
    [SerializeField] private float grabRange = 2f;

    private FixedJoint currentJoint;
    private RagdollHandler targetRagdoll;
    private NetworkIdentity targetIdentity;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out RagdollHandler rag))
        {
            if (rag.isRagdoll != true) return;
            targetRagdoll = rag;

            // najdi NetworkIdentity
            if (other.TryGetComponent(out NetworkIdentity id))
                targetIdentity = id;
            else
                targetIdentity = rag.GetComponentInParent<NetworkIdentity>();

            Debug.Log("Veöel");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out RagdollHandler rag) && rag == targetRagdoll)
        {
            targetRagdoll = null;
        
            targetIdentity = null;

            Debug.Log("Odeöel");
        }

    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.E) && targetRagdoll != null)
        {
            Debug.Log("Zkusim");
            if (currentJoint == null)
                CmdGrabRagdoll(targetIdentity);
            else
                CmdRelease();
        }else if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Ragdoll je null");
        }
    }

    [Command]
    void CmdGrabRagdoll(NetworkIdentity ragdollNetId)
    {
        Debug.Log("Chyt·m");
        if (ragdollNetId == null) return;

        var rag = ragdollNetId.GetComponent<RagdollHandler>();
        if (rag == null) return;

        // Najdi nejbliûöÌ konËetinu
        Rigidbody closestLimb = rag.GetClosestLimb(transform.position);
        if (closestLimb == null) return;

        // Spusù p¯ipojenÌ na vöech klientech
        RpcAttach(closestLimb.gameObject);
    }

    [ClientRpc]
    void RpcAttach(GameObject limb)
    {
        if (currentJoint != null) Destroy(currentJoint);

        Rigidbody limbRB = limb.GetComponent<Rigidbody>();
        if (limbRB == null) return;

        // Ujisti se, ûe ruka m· Rigidbody
        var jointRB = handTransform.GetComponent<Rigidbody>();
        if (jointRB == null)
        {
            jointRB = handTransform.gameObject.AddComponent<Rigidbody>();
            jointRB.mass = 1f;
            jointRB.isKinematic = true;
        }

        // P¯ipojÌme konËetinu k ruce
        currentJoint = limbRB.gameObject.AddComponent<FixedJoint>();
        currentJoint.connectedBody = jointRB;
        currentJoint.breakForce = 600f;
        currentJoint.breakTorque = 600f;
    }

    [Command]
    void CmdRelease() => RpcRelease();

    [ClientRpc]
    void RpcRelease()
    {
        Debug.Log("Release");
        if (currentJoint != null)
        {
            Destroy(currentJoint);
            currentJoint = null;
        }
    }
}
