using System.Collections.Generic;
using UnityEngine;

public class RagdollHandler : MonoBehaviour
{
    [SerializeField] private Animator animator; // animátor postavy
    [SerializeField] private Rigidbody mainRigidbody; // hlavní rigidbody (pokud máš)
    [SerializeField] private Rigidbody head;
    [SerializeField] private Collider pickUpTrigger;
    public List<Rigidbody> limbRigidbodies = new List<Rigidbody>();


    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    public bool isRagdoll = false;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = GetComponentsInChildren<Collider>(true);

        if (mainRigidbody == null)
            mainRigidbody = GetComponent<Rigidbody>();

        SetRagdoll(false, Vector3.zero);


        pickUpTrigger.enabled = true;
    }

    public void SetRagdoll(bool on, Vector3 force)
    {
        isRagdoll = on;
        pickUpTrigger.enabled = on;

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

    
    public Rigidbody GetClosestLimb(Vector3 point)
    {
        Rigidbody closest = null;
        float minDist = float.MaxValue;

        foreach (var rb in limbRigidbodies)
        {
            float dist = Vector3.Distance(point, rb.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = rb;
            }
        }

        return closest;
    }


    public void ToggleRagdoll()
    {
        SetRagdoll(!isRagdoll, Vector3.zero);
    }
}
