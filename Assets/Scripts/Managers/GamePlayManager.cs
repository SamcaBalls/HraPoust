using Mirror;
using UnityEngine;

public class GamePlayManager : NetworkBehaviour
{
    [SerializeField] GameObject bowlPrefab;

    public void Start()
    {
        Debug.Log("[GamePlayManager] Server initialized.");

        Vector3 pos = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        GameObject obj = Instantiate(bowlPrefab, pos, Quaternion.identity);
        NetworkServer.Spawn(obj);
    }

}
