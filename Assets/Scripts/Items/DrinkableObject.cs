using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using UnityEngine.UIElements;

public class DrinkableObject : MonoBehaviour
{
    public bool TestFill;
    public bool spillable;
    public int maxCapacity = 20;
    [SerializeField] LayerMask groundMask;
    [SerializeField] GameObject spillParticle;
    [SerializeField] ParticleManager particleManager;

    [SerializeField] int _capacity;
    public int Capacity
    {
        get => _capacity;
        set
        {
            if (_capacity == value) return;
            _capacity = Mathf.Clamp(value, 0, maxCapacity);
            OnCapacityChanged();
        }
    }

    [SerializeField] int minLoss = 1;
    [SerializeField] int maxLoss = 10;
    [SerializeField] Water water;

    private void Start()
    {
        Capacity = Random.Range(maxCapacity/3, maxCapacity);
        particleManager = FindAnyObjectByType<ParticleManager>();
        if (TestFill) StartCoroutine(TryFill());
    }

    public virtual void ChangeCapacity(bool spill)
    {
        if (!spillable) return;

        if (spill)
        {
            if (TestFill)
            {
                Capacity = Random.Range(0, maxCapacity);
            }
            else
            {
                if(Capacity != 0)
                {
                    Capacity -= Random.Range(minLoss , maxLoss);
                    particleManager.SpawnParticle(spillParticle, gameObject.transform.position);
                }
            }
            if(Capacity < 0) Capacity = 0;
        }
        else
        {
            Capacity = maxCapacity;
        }
    }

    private void OnCapacityChanged()
    {
        if (water != null)
            water.ChangeWaterLevel();
    }

    IEnumerator TryFill()
    {
        while (true) 
        {
            if (TestFill)
            {
                ChangeCapacity(true);
            }
            yield return new WaitForSeconds(1);

        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        // Kontrola, jestli vrstva kolidujícího objektu je v groundMask
        if (((1 << collision.collider.gameObject.layer) & groundMask) != 0 && spillable)
        {
            Debug.Log("Dotkl jsem se země!");
            ChangeCapacity(true);
        }
    }
}

