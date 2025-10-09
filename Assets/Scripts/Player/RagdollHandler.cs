using UnityEngine;

public class RagdollHandler : MonoBehaviour
{
    [SerializeField] private Animator animator; // animátor postavy
    [SerializeField] private Rigidbody mainRigidbody; // hlavní rigidbody (pokud máš)
    [SerializeField] private Rigidbody head;

    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private bool isRagdoll = false;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = GetComponentsInChildren<Collider>(true);

        if (mainRigidbody == null)
            mainRigidbody = GetComponent<Rigidbody>();

        SetRagdoll(false, Vector3.zero);
    }

    public void SetRagdoll(bool on, Vector3 force)
    {
        isRagdoll = on;

        if (animator != null)
            animator.enabled = !on;

        if (mainRigidbody != null)
        {
            mainRigidbody.isKinematic = on;
            mainRigidbody.detectCollisions = !on;
        }

        foreach (var rb in ragdollRigidbodies)
        {
            if (rb == mainRigidbody) continue;
            rb.isKinematic = !on;
        }

        foreach (var col in ragdollColliders)
            col.enabled = on;

        if (on && force != Vector3.zero)
        {
            foreach (var rb in ragdollRigidbodies)
            {
                if (rb == mainRigidbody) continue;
                rb.AddForce(force, ForceMode.Impulse);
            }
            head.AddForce(force*4, ForceMode.Impulse);
        }
    }

    public void ToggleRagdoll()
    {
        SetRagdoll(!isRagdoll, Vector3.zero);
    }
}
