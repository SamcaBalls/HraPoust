using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;


public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Hráči v lobby")]
    public List<PlayerInfo> players = new List<PlayerInfo>();

    [Header("Debug")]
    [SerializeField] private bool showLogs = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    [Server]
    public void WritePlayers()
    {
        players.Clear();

        if (SteamLobby.Instance == null || SteamLobby.Instance.lobbyID == 0)
        {
            Debug.LogWarning("[GameManager] Nejsem v lobby!");
            return;
        }

        CSteamID lobby = new CSteamID(SteamLobby.Instance.lobbyID);
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobby);

        if (showLogs)
            Debug.Log($"[GameManager] Zapisuji {memberCount} hráčů z lobby");

        for (int i = 0; i < memberCount; i++)
        {
            CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(lobby, i);
            string name = SteamFriends.GetFriendPersonaName(memberID);

            PlayerInfo player = new PlayerInfo(memberID.m_SteamID, name);
            players.Add(player);

            if (showLogs)
                Debug.Log($"[GameManager] Přidán hráč: {name} (ID: {memberID.m_SteamID})");
        }

        RpcSyncPlayers();
    }

    [Server]
    public bool CanPlayerJoin(ulong steamID)
    {
        PlayerInfo player = GetPlayer(steamID);

        if (player == null)
        {
            if (showLogs)
                Debug.LogWarning($"[GameManager] Hráč {steamID} NENÍ v lobby listu!");
            return false;
        }

        if (!player.isAlive)
        {
            if (showLogs)
                Debug.LogWarning($"[GameManager] Hráč {player.playerName} je MRTVÝ!");
            return false;
        }

        if (showLogs)
            Debug.Log($"[GameManager] Hráč {player.playerName} může joinout ✓");

        return true;
    }

    [Command(requiresAuthority = false)]
    public void CmdCheckCanJoin(NetworkConnectionToClient conn = null)
    {
        if (conn == null) return;

        ulong steamID = GetSteamIDFromConnection(conn);
        bool canJoin = CanPlayerJoin(steamID);

        TargetJoinResponse(conn, canJoin);
    }

    [TargetRpc]
    void TargetJoinResponse(NetworkConnection target, bool canJoin)
    {
        if (canJoin)
        {
            Debug.Log("[GameManager] Můžu joinout! ✓");
        }
        else
        {
            Debug.LogWarning("[GameManager] NEMŮŽU joinout - jsem mrtvý nebo nejsem v lobby!");
        }
    }

    [ClientRpc]
    void RpcSyncPlayers()
    {
        if (isServer) return;

        if (showLogs)
            Debug.Log($"[GameManager CLIENT] Přijato {players.Count} hráčů");
    }

    [Server]
    public void SetPlayerDead(ulong steamID)
    {
        PlayerInfo player = GetPlayer(steamID);
        if (player != null)
        {
            player.isAlive = false;

            if (showLogs)
                Debug.Log($"[GameManager] {player.playerName} ZEMŘEL ☠");

            RpcSyncPlayers();
        }
    }

    [Server]
    public void SetPlayerAlive(ulong steamID)
    {
        PlayerInfo player = GetPlayer(steamID);
        if (player != null)
        {
            player.isAlive = true;

            if (showLogs)
                Debug.Log($"[GameManager] {player.playerName} ožil ❤");

            RpcSyncPlayers();
        }
    }

    public PlayerInfo GetPlayer(ulong steamID)
    {
        foreach (var player in players)
        {
            if (player.steamID == steamID)
                return player;
        }
        return null;
    }

    public int GetAliveCount()
    {
        int count = 0;
        foreach (var player in players)
        {
            if (player.isAlive) count++;
        }
        return count;
    }

    public bool IsPlayerAlive(ulong steamID)
    {
        PlayerInfo player = GetPlayer(steamID);
        return player != null && player.isAlive;
    }

    private ulong GetSteamIDFromConnection(NetworkConnectionToClient conn)
    {
        // TODO: Implementuj podle toho jak ukládáš SteamID u hráče
        return SteamUser.GetSteamID().m_SteamID;
    }

    [ContextMenu("Print Players")]
    public void DebugPrintPlayers()
    {
        Debug.Log("=== HRÁČI V LOBBY ===");
        foreach (var player in players)
        {
            Debug.Log($"{player.playerName} - " +
                     $"Alive: {(player.isAlive ? "✓" : "☠")} " +
                     $"(ID: {player.steamID})");
        }
        Debug.Log($"Celkem: {players.Count} | Živí: {GetAliveCount()}");
    }

    [Server]
    public void ResetAllPlayers()
    {
        foreach (var player in players)
        {
            player.isAlive = true;
        }

        if (showLogs)
            Debug.Log("[GameManager] Všichni hráči resetováni na ALIVE");

        RpcSyncPlayers();
    }

    [Server]
    public void ClearList()
    {
        players.Clear();
    }
}