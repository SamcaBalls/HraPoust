using Mirror;
using UnityEngine;

public class ParticleManager : NetworkBehaviour
{
    [Server]
    public void SpawnParticle(GameObject particle, Vector3 position)
    {
        GameObject obj = Instantiate(particle, position, Quaternion.identity);
        NetworkServer.Spawn(obj);
    }
}
