[System.Serializable]
public class PlayerInfo
{
    public ulong steamID;
    public string playerName;
    public bool isAlive;

    public PlayerInfo(ulong steamID, string playerName)
    {
        this.steamID = steamID;
        this.playerName = playerName;
        this.isAlive = true;
    }
}