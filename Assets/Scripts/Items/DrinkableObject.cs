using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DrinkableObject : MonoBehaviour
{
    public bool TestFill;
    public bool spillable;
    public int maxCapacity = 20;

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
        Capacity = maxCapacity;
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
                Capacity -= Random.Range(0, maxCapacity / 2);
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
}

