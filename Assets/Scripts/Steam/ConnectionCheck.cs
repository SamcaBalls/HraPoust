using System.Collections;
using UnityEngine;
using Mirror;

public class ConnectionMonitor : MonoBehaviour
{
    [Header("Nastavení")]
    [Tooltip("Jak èasto kontrolovat pøipojení (v sekundách)")]
    [SerializeField] private float checkInterval = 2f;

    [Tooltip("Jak dlouho èekat než se hráè považuje za odpojeného (v sekundách)")]
    [SerializeField] private float connectionTimeout = 10f;

    [Tooltip("Automaticky odeslat hráèe zpìt do lobby")]
    [SerializeField] private bool autoDisconnect = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private float disconnectTimer = 0f;
    private ReturnToLobbyManager returnToLobbyManager;

    private static ConnectionMonitor instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        returnToLobbyManager = FindAnyObjectByType<ReturnToLobbyManager>();
        StartCoroutine(CheckConnectionLoop());
    }

    private IEnumerator CheckConnectionLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            // Je klient pøipojený?
            bool isConnected = NetworkClient.isConnected && NetworkClient.ready;

            if (isConnected)
            {
                disconnectTimer = 0f;
            }
            else
            {
                disconnectTimer += checkInterval;
                if (showDebugLogs)
                    Debug.Log($"[ConnectionMonitor] Odpojeno {disconnectTimer:F1}/{connectionTimeout}s");

                if (autoDisconnect && disconnectTimer >= connectionTimeout)
                {
                    if (showDebugLogs)
                        Debug.Log("[ConnectionMonitor] Hráè je odpojen pøíliš dlouho – návrat do lobby.");

                    if (returnToLobbyManager != null)
                        returnToLobbyManager.SendToLobby();
                    else
                        Debug.LogWarning("[ConnectionMonitor] Nenalezen ReturnToLobbyManager!");

                    yield break; // konec coroutine
                }
            }
        }
    }
}
