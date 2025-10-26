using System.Collections;
using UnityEngine;
using Mirror;
using Steamworks;

/// <summary>
/// Monitoruje stav pøipojení klienta a automaticky ho odpojí pøi ztrátì spojení
/// </summary>
public class ConnectionMonitor : MonoBehaviour
{
    [Header("Nastavení")]
    [Tooltip("Jak èasto kontrolovat pøipojení (v sekundách)")]
    [SerializeField] private float checkInterval = 2f;

    [Tooltip("Timeout pro detekci ztráty pøipojení (v sekundách)")]
    [SerializeField] private float connectionTimeout = 10f;

    [Tooltip("Povolit automatické odpojení pøi ztrátì spojení")]
    [SerializeField] private bool autoDisconnect = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    [SerializeField] ReturnToLobbyManager rtlMan;

    private float lastPacketTime;
    private bool isMonitoring = false;
    private Coroutine monitorCoroutine;

    // Event pro notifikaci o ztrátì pøipojení
    public System.Action OnConnectionLost;

    void Start()
    {
        // Registrace Mirror callbackù
        NetworkClient.RegisterHandler<NetworkPingMessage>(OnPingMessage);
    }

    void OnEnable()
    {
        // Zaèít monitorovat pokud už je klient pøipojený
        if (NetworkClient.isConnected)
        {
            StartMonitoring();
        }
    }

    void OnDisable()
    {
        StopMonitoring();
    }

    void OnDestroy()
    {
        StopMonitoring();
    }

    void Update()
    {
        // Automaticky zaèít monitorovat pøi pøipojení
        if (NetworkClient.isConnected && !isMonitoring)
        {
            StartMonitoring();
        }
        // Zastavit monitorování pøi odpojení
        else if (!NetworkClient.isConnected && isMonitoring)
        {
            StopMonitoring();
        }
    }

    /// <summary>
    /// Spustí monitorování pøipojení
    /// </summary>
    public void StartMonitoring()
    {
        if (isMonitoring) return;

        lastPacketTime = Time.time;
        isMonitoring = true;

        if (monitorCoroutine != null)
            StopCoroutine(monitorCoroutine);

        monitorCoroutine = StartCoroutine(MonitorConnection());

        if (showDebugLogs)
            Debug.Log("[ConnectionMonitor] Zahájeno monitorování pøipojení");
    }

    /// <summary>
    /// Zastaví monitorování pøipojení
    /// </summary>
    public void StopMonitoring()
    {
        if (!isMonitoring) return;

        isMonitoring = false;

        if (monitorCoroutine != null)
        {
            StopCoroutine(monitorCoroutine);
            monitorCoroutine = null;
        }

        if (showDebugLogs)
            Debug.Log("[ConnectionMonitor] Monitorování zastaveno");
    }

    /// <summary>
    /// Hlavní coroutine pro monitorování pøipojení
    /// </summary>
    private IEnumerator MonitorConnection()
    {
        while (isMonitoring)
        {
            yield return new WaitForSeconds(checkInterval);

            // Kontrola pouze pokud jsme klient (ne server/host)
            if (!NetworkServer.active && NetworkClient.isConnected)
            {
                float timeSinceLastPacket = Time.time - lastPacketTime;

                if (showDebugLogs && timeSinceLastPacket > connectionTimeout * 0.5f)
                {
                    Debug.LogWarning($"[ConnectionMonitor] Dlouhá doba bez odpovìdi: {timeSinceLastPacket:F1}s");
                }

                // Pokud pøekroèíme timeout, odpojíme se
                if (timeSinceLastPacket > connectionTimeout)
                {
                    HandleConnectionLost();
                    yield break;
                }

                // Kontrola Steam pøipojení
                if (SteamManager.Initialized)
                {
                    if (!CheckSteamConnection())
                    {
                        HandleConnectionLost();
                        yield break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Kontrola Steam pøipojení
    /// </summary>
    private bool CheckSteamConnection()
    {
        try
        {
            // Kontrola jestli je Steam stále pøipojený
            if (!SteamAPI.IsSteamRunning())
            {
                if (showDebugLogs)
                    Debug.LogError("[ConnectionMonitor] Steam není spuštìný!");
                return false;
            }

            // Kontrola lobby pøipojení
            if (SteamLobby.Instance != null && SteamLobby.Instance.lobbyID != 0)
            {
                CSteamID lobby = new CSteamID(SteamLobby.Instance.lobbyID);

                // Ovìøíme že lobby stále existuje
                int numMembers = SteamMatchmaking.GetNumLobbyMembers(lobby);

                if (numMembers <= 0)
                {
                    if (showDebugLogs)
                        Debug.LogWarning("[ConnectionMonitor] Lobby má 0 èlenù - pravdìpodobnì neexistuje");
                    return false;
                }
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ConnectionMonitor] Chyba pøi kontrole Steam pøipojení: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Zpracování ztráty pøipojení
    /// </summary>
    private void HandleConnectionLost()
    {
        Debug.LogError("[ConnectionMonitor] Ztráta pøipojení detekována!");

        // Vyvoláme event
        OnConnectionLost?.Invoke();

        // Automatické odpojení
        if (autoDisconnect)
        {
            DisconnectClient();
        }

        StopMonitoring();
    }

    /// <summary>
    /// Odpojí klienta a vrátí ho do menu
    /// </summary>
    private void DisconnectClient()
    {
        if (showDebugLogs)
            Debug.Log("[ConnectionMonitor] Provádím odpojení klienta...");

        try
        {
            rtlMan.SendToLobby();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ConnectionMonitor] Chyba pøi odpojování: {e.Message}");
        }
    }

    /// <summary>
    /// Zobrazí zprávu o odpojení (pøizpùsob podle tvého UI systému)
    /// </summary>
    private void ShowDisconnectionMessage()
    {
        // Zde mùžeš implementovat zobrazení UI zprávy
        var menuComp = FindAnyObjectByType<MenuComponents>();
        if (menuComp != null)
        {
            // Napøíklad:
            // menuComp.ShowNotification("Pøipojení ztraceno. Byli jste odpojeni ze hry.");
        }

        Debug.Log("[ConnectionMonitor] Klient byl odpojen z dùvodu ztráty pøipojení");
    }

    /// <summary>
    /// Handler pro ping zprávy - resetuje timeout
    /// </summary>
    private void OnPingMessage(NetworkPingMessage msg)
    {
        lastPacketTime = Time.time;
    }

    /// <summary>
    /// Manuální resetování èasu posledního packetu (volej pøi pøijetí jakékoliv zprávy)
    /// </summary>
    public void ResetConnectionTimer()
    {
        lastPacketTime = Time.time;
    }

    // Gettery pro debug
    public float TimeSinceLastPacket => Time.time - lastPacketTime;
    public bool IsMonitoring => isMonitoring;
}