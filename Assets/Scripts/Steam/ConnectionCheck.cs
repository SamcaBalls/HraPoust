using System.Collections;
using UnityEngine;
using Mirror;
using Steamworks;

/// <summary>
/// Monitoruje stav p�ipojen� klienta a automaticky ho odpoj� p�i ztr�t� spojen�
/// </summary>
public class ConnectionMonitor : MonoBehaviour
{
    [Header("Nastaven�")]
    [Tooltip("Jak �asto kontrolovat p�ipojen� (v sekund�ch)")]
    [SerializeField] private float checkInterval = 2f;

    [Tooltip("Timeout pro detekci ztr�ty p�ipojen� (v sekund�ch)")]
    [SerializeField] private float connectionTimeout = 10f;

    [Tooltip("Povolit automatick� odpojen� p�i ztr�t� spojen�")]
    [SerializeField] private bool autoDisconnect = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    [SerializeField] ReturnToLobbyManager rtlMan;

    private float lastPacketTime;
    private bool isMonitoring = false;
    private Coroutine monitorCoroutine;

    // Event pro notifikaci o ztr�t� p�ipojen�
    public System.Action OnConnectionLost;

    void Start()
    {
        // Registrace Mirror callback�
        NetworkClient.RegisterHandler<NetworkPingMessage>(OnPingMessage);
    }

    void OnEnable()
    {
        // Za��t monitorovat pokud u� je klient p�ipojen�
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
        // Automaticky za��t monitorovat p�i p�ipojen�
        if (NetworkClient.isConnected && !isMonitoring)
        {
            StartMonitoring();
        }
        // Zastavit monitorov�n� p�i odpojen�
        else if (!NetworkClient.isConnected && isMonitoring)
        {
            StopMonitoring();
        }
    }

    /// <summary>
    /// Spust� monitorov�n� p�ipojen�
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
            Debug.Log("[ConnectionMonitor] Zah�jeno monitorov�n� p�ipojen�");
    }

    /// <summary>
    /// Zastav� monitorov�n� p�ipojen�
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
            Debug.Log("[ConnectionMonitor] Monitorov�n� zastaveno");
    }

    /// <summary>
    /// Hlavn� coroutine pro monitorov�n� p�ipojen�
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
                    Debug.LogWarning($"[ConnectionMonitor] Dlouh� doba bez odpov�di: {timeSinceLastPacket:F1}s");
                }

                // Pokud p�ekro��me timeout, odpoj�me se
                if (timeSinceLastPacket > connectionTimeout)
                {
                    HandleConnectionLost();
                    yield break;
                }

                // Kontrola Steam p�ipojen�
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
    /// Kontrola Steam p�ipojen�
    /// </summary>
    private bool CheckSteamConnection()
    {
        try
        {
            // Kontrola jestli je Steam st�le p�ipojen�
            if (!SteamAPI.IsSteamRunning())
            {
                if (showDebugLogs)
                    Debug.LogError("[ConnectionMonitor] Steam nen� spu�t�n�!");
                return false;
            }

            // Kontrola lobby p�ipojen�
            if (SteamLobby.Instance != null && SteamLobby.Instance.lobbyID != 0)
            {
                CSteamID lobby = new CSteamID(SteamLobby.Instance.lobbyID);

                // Ov���me �e lobby st�le existuje
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

    /// <summary>
    /// Zpracov�n� ztr�ty p�ipojen�
    /// </summary>
    private void HandleConnectionLost()
    {
        Debug.LogError("[ConnectionMonitor] Ztr�ta p�ipojen� detekov�na!");

        // Vyvol�me event
        OnConnectionLost?.Invoke();

        // Automatick� odpojen�
        if (autoDisconnect)
        {
            DisconnectClient();
        }

        StopMonitoring();
    }

    /// <summary>
    /// Odpoj� klienta a vr�t� ho do menu
    /// </summary>
    private void DisconnectClient()
    {
        if (showDebugLogs)
            Debug.Log("[ConnectionMonitor] Prov�d�m odpojen� klienta...");

        try
        {
            rtlMan.SendToLobby();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ConnectionMonitor] Chyba p�i odpojov�n�: {e.Message}");
        }
    }

    /// <summary>
    /// Zobraz� zpr�vu o odpojen� (p�izp�sob podle tv�ho UI syst�mu)
    /// </summary>
    private void ShowDisconnectionMessage()
    {
        // Zde m��e� implementovat zobrazen� UI zpr�vy
        var menuComp = FindAnyObjectByType<MenuComponents>();
        if (menuComp != null)
        {
            // Nap��klad:
            // menuComp.ShowNotification("P�ipojen� ztraceno. Byli jste odpojeni ze hry.");
        }

        Debug.Log("[ConnectionMonitor] Klient byl odpojen z d�vodu ztr�ty p�ipojen�");
    }

    /// <summary>
    /// Handler pro ping zpr�vy - resetuje timeout
    /// </summary>
    private void OnPingMessage(NetworkPingMessage msg)
    {
        lastPacketTime = Time.time;
    }

    /// <summary>
    /// Manu�ln� resetov�n� �asu posledn�ho packetu (volej p�i p�ijet� jak�koliv zpr�vy)
    /// </summary>
    public void ResetConnectionTimer()
    {
        lastPacketTime = Time.time;
    }

    // Gettery pro debug
    public float TimeSinceLastPacket => Time.time - lastPacketTime;
    public bool IsMonitoring => isMonitoring;
}