using Mirror;
using UnityEngine;
using System.Collections;

public class DrinkableObject : NetworkBehaviour
{
    public bool TestFill;
    public bool spillable;
    public int maxCapacity = 20;

    [SerializeField] LayerMask groundMask;
    [SerializeField] GameObject spillParticle;

    [SyncVar] public int Capacity;

    [SerializeField] int minLoss = 1;
    [SerializeField] int maxLoss = 10;
    [SerializeField] Water water;

    public override void OnStartServer()
    {
        Capacity = Random.Range(maxCapacity / 3, maxCapacity);
        if (TestFill) StartCoroutine(TryFill());
    }

    public override void OnStartClient()
    {
        // Např. aktualizace vizuálu po připojení
        if (water != null)
            water.ChangeWaterLevel();
    }

    public virtual void ChangeCapacity(bool spill)
    {
        if (!spillable) return;
        if (!isServer) return; // jen server mění hodnoty

        if (spill && Capacity > 0)
        {
            Capacity -= Random.Range(minLoss, maxLoss);
            if (Capacity < 0) Capacity = 0;
            RpcPlaySpillEffect(transform.position);
        }
        else if (!spill)
        {
            Capacity = maxCapacity;
        }

        if (water != null)
            water.ChangeWaterLevel();
    }

    IEnumerator TryFill()
    {
        while (true)
        {
            ChangeCapacity(true);
            yield return new WaitForSeconds(1);
        }
    }

    [ClientRpc]
    void RpcPlaySpillEffect(Vector3 pos)
    {
        Instantiate(spillParticle, pos, Quaternion.identity);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.collider.gameObject.layer) & groundMask) != 0 && spillable)
        {
            if (isServer)
            {
                Debug.Log("Dotkl jsem se země!");
                ChangeCapacity(true);
            }
        }
    }
}
