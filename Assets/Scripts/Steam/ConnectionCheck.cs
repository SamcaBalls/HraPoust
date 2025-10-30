using System.Collections;
using UnityEngine;
using Mirror;

public class ConnectionMonitor : MonoBehaviour
{
    [Header("Nastaven�")]
    [Tooltip("Jak �asto kontrolovat p�ipojen� (v sekund�ch)")]
    [SerializeField] private float checkInterval = 2f;

    [Tooltip("Jak dlouho �ekat ne� se hr�� pova�uje za odpojen�ho (v sekund�ch)")]
    [SerializeField] private float connectionTimeout = 10f;

    [Tooltip("Automaticky odeslat hr��e zp�t do lobby")]
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

            // Je klient p�ipojen�?
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
                        Debug.Log("[ConnectionMonitor] Hr�� je odpojen p��li� dlouho � n�vrat do lobby.");

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
