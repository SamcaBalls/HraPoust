using System.Collections;
using UnityEngine;
using Mirror;
using Steamworks;

public class ConnectionMonitor : MonoBehaviour
{
    [Header("Nastaven�")]
    [SerializeField] private float checkInterval = 2f;
    [SerializeField] private float connectionTimeout = 10f;
    [SerializeField] private bool autoDisconnect = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    [SerializeField] private ReturnToLobbyManager rtlMan;

    private float lastPacketTime;
    private bool isMonitoring = false;
    private Coroutine monitorCoroutine;

    public System.Action OnConnectionLost;

    private static ConnectionMonitor instance; // ?? singleton instance

    private void Start()
    {
        NetworkClient.RegisterHandler<NetworkPingMessage>(OnPingMessage);
    }

    private void OnEnable()
    {
        if (NetworkClient.isConnected)
            StartMonitoring();
    }

    private void OnDisable()
    {
        StopMonitoring();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;

        StopMonitoring();
    }

    private void Update()
    {
        if (NetworkClient.isConnected && !isMonitoring)
            StartMonitoring();
        else if (!NetworkClient.isConnected && isMonitoring)
            StopMonitoring();
    }

    public void StartMonitoring()
    {
        if (isMonitoring) return;

        lastPacketTime = Time.time;
        isMonitoring = true;

        if (monitorCoroutine != null)
            StopCoroutine(monitorCoroutine);

        monitorCoroutine = StartCoroutine(MonitorConnection());

        if (showDebugLogs)
            Debug.Log("[ConnectionMonitor] Zah�jeno monitorov�n� p�ipojen�");
    }

    public void StopMonitoring()
    {
        if (!isMonitoring) return;

        isMonitoring = false;

        if (monitorCoroutine != null)
        {
            StopCoroutine(monitorCoroutine);
            monitorCoroutine = null;
        }
        if(rtlMan.gameObject.activeSelf == true)
        {
        rtlMan.SendToLobby();

        }

        if (showDebugLogs)
            Debug.Log("[ConnectionMonitor] Monitorov�n� zastaveno");
    }

    private IEnumerator MonitorConnection()
    {
        while (isMonitoring)
        {
            yield return new WaitForSeconds(checkInterval);

            if (!NetworkServer.active && NetworkClient.isConnected)
            {
                float timeSinceLastPacket = Time.time - lastPacketTime;

                if (showDebugLogs && timeSinceLastPacket > connectionTimeout * 0.5f)
                    Debug.LogWarning($"[ConnectionMonitor] Dlouh� doba bez odpov�di: {timeSinceLastPacket:F1}s");

                if (timeSinceLastPacket > connectionTimeout)
                {
                    HandleConnectionLost();
                    yield break;
                }

                if (SteamManager.Initialized && !CheckSteamConnection())
                {
                    HandleConnectionLost();
                    yield break;
                }
            }
        }
    }

    private bool CheckSteamConnection()
    {
        try
        {
            if (!SteamAPI.IsSteamRunning())
            {
                if (showDebugLogs)
                    Debug.LogError("[ConnectionMonitor] Steam nen� spu�t�n�!");
                return false;
            }

            if (SteamLobby.Instance != null && SteamLobby.Instance.lobbyID != 0)
            {
                var lobby = new CSteamID(SteamLobby.Instance.lobbyID);
                int numMembers = SteamMatchmaking.GetNumLobbyMembers(lobby);

                if (numMembers <= 0)
                {
                    if (showDebugLogs)
                        Debug.LogWarning("[ConnectionMonitor] Lobby m� 0 �len� - pravd�podobn� neexistuje");
                    return false;
                }
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ConnectionMonitor] Chyba p�i kontrole Steam p�ipojen�: {e.Message}");
            return false;
        }
    }

    private void HandleConnectionLost()
    {
        Debug.LogError("[ConnectionMonitor] Ztr�ta p�ipojen� detekov�na!");

        OnConnectionLost?.Invoke();

        if (autoDisconnect)
            DisconnectClient();

        StopMonitoring();
    }

    private void DisconnectClient()
    {
        if (showDebugLogs)
            Debug.Log("[ConnectionMonitor] Prov�d�m odpojen� klienta...");

        try
        {
            rtlMan?.SendToLobby(); // ?? bezpe�n�j�� null check
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ConnectionMonitor] Chyba p�i odpojov�n�: {e.Message}");
        }
    }

    private void OnPingMessage(NetworkPingMessage msg)
    {
        lastPacketTime = Time.time;
    }

    public void ResetConnectionTimer() => lastPacketTime = Time.time;

    public float TimeSinceLastPacket => Time.time - lastPacketTime;
    public bool IsMonitoring => isMonitoring;
}
