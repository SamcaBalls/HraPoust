using Mirror;
using UnityEngine;

public class GamePlayManager : NetworkBehaviour
{
    [SerializeField] GameObject bowlPrefab;

    public void Start()
    {
        Debug.Log("[GamePlayManager] Server initialized.");

        SpawnWaterObject();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            SpawnWaterObject();
        }
    }

    public void SpawnWaterObject()
    {
        Vector3 pos = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        GameObject obj = Instantiate(bowlPrefab, pos, Quaternion.identity);
        NetworkServer.Spawn(obj);
    }

}
