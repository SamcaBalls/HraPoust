using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : NetworkBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop;
        public bool is3D = false;
    }

    [SerializeField] private List<Sound> sounds = new();
    private Dictionary<string, Sound> soundDict = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        foreach (var s in sounds)
            soundDict[s.name] = s;
    }

    // 🔹 Lokální přehrání
    public void PlayLocal(string name, Vector3? position = null)
    {

        if (!soundDict.TryGetValue(name, out var s)) return;

        Debug.Log("Hraje");

        if (s.is3D && position.HasValue)
        {
            AudioSource.PlayClipAtPoint(s.clip, position.Value, s.volume);
        }
        else
        {
            sfxSource.pitch = s.pitch;
            sfxSource.volume = s.volume;
            sfxSource.PlayOneShot(s.clip);
        }
    }

    // 🔹 Lokální hudba
    public void PlayMusic(string name)
    {
        if (!soundDict.TryGetValue(name, out var s)) return;
        musicSource.clip = s.clip;
        musicSource.loop = s.loop;
        musicSource.volume = s.volume;
        musicSource.pitch = s.pitch;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    // 🔹 Síťové přehrání (všichni hráči)
    [Command(requiresAuthority = false)]
    public void CmdPlayGlobal(string name, bool is3D, Vector3 position)
    {
        RpcPlayGlobal(name, is3D, position);
    }

    [ClientRpc]
    public void RpcPlayGlobal(string name, bool is3D, Vector3 position)
    {
        Debug.Log("Hraju: " + name);

        if (is3D)
            PlayLocal(name, position);
        else
            PlayLocal(name);
    }

    // 🔹 Síťové přehrání jen v okolí (např. výbuch)
    [Command(requiresAuthority = false)]
    public void CmdPlayInRange(Vector3 pos, string name, float range)
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            var obj = conn.identity;
            if (obj == null) continue;

            if (Vector3.Distance(obj.transform.position, pos) <= range)
            {
                TargetPlay(conn, name, pos);
            }
        }
    }

    [TargetRpc]
    public void TargetPlay(NetworkConnection conn, string name, Vector3 pos)
    {
        PlayLocal(name, pos);
    }
}
