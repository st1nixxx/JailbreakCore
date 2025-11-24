using Jailbreak.Shared;
using JailbreakCore;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

namespace Jailbreak;

public class JBPlayerManagement(ISwiftlyCore _core)
{
    private readonly ISwiftlyCore _Core = _core;
    private Dictionary<int, JBPlayer> JBPlayers = new();
    public JBPlayer GetOrCreate(IPlayer player)
    {
        if (player == null || player.PlayerPawn == null || player.Pawn == null || player.Controller == null || player.IsFakeClient)
            throw new ArgumentException("Invalid IPlayer");

        if (!JBPlayers.TryGetValue(player.PlayerID, out JBPlayer? jbPlayer))
        {
            jbPlayer = new JBPlayer(player, player.Controller, player.PlayerPawn, player.Pawn, _Core);
            JBPlayers[player.PlayerID] = jbPlayer;
        }

        return jbPlayer;
    }
    public JBPlayer? GetWarden()
    {
        return JBPlayers.Values.FirstOrDefault(p => p.IsWarden && p.IsValid);
    }
    public List<JBPlayer> GetAllRebels()
    {
        return JBPlayers.Values.Where(p => p.IsRebel && p.IsValid).ToList();
    }
    public List<JBPlayer> GetAllFreedays()
    {
        return JBPlayers.Values.Where(p => p.IsFreeday && p.IsValid).ToList();
    }
    public List<JBPlayer> GetAllPrisoners(bool excludeRebels = true, bool exlcudeFreedays = true)
    {
        return JBPlayers.Values.Where(p => p.IsValid && p.Role == IJBRole.Prisoner && excludeRebels ? !p.IsRebel : p.IsRebel && exlcudeFreedays ? !p.IsFreeday : p.IsFreeday).ToList();
    }
    public List<JBPlayer> GetAllGuardians()
    {
        return JBPlayers.Values.Where(p => p.IsValid && p.Role == IJBRole.Guardian).ToList();
    }
    public List<JBPlayer> GetAllPlayers()
    {
        return JBPlayers.Values.Where(p => p.IsValid).ToList();
    }
    public void Remove(IPlayer player)
    {
        if (JBPlayers.TryGetValue(player.PlayerID, out JBPlayer? jbPlayer))
        {
            jbPlayer.Dispose();
            JBPlayers.Remove(player.PlayerID);
        }
    }
}
