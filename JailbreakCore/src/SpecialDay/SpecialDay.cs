using System.ComponentModel;
using Jailbreak.Shared;
using SwiftlyS2.Shared;

namespace JailbreakCore;

public class SpecialDay
{
    private readonly List<ISpecialDay> Games = new();
    private ISpecialDay? ActiveGame;
    
    public IReadOnlyList<ISpecialDay> GetAllDays() => Games;
    public ISpecialDay? GetActiveDay() => ActiveGame;
    public bool IsGameActive() => ActiveGame != null;

    public void Register(ISpecialDay day)
    {
        Games.Add(day);
    }
    public void Unregister(ISpecialDay day)
    {
        Games.Remove(day);
    }
    public void Select(JBPlayer player, string name)
    {
        // Check if a game is already active
        if (ActiveGame != null)
        {
            player.Print(IHud.Chat, "game_already_active", null, 0, true, IPrefix.SD, ActiveGame.Name);
            return;
        }

        var selectedGame = Games.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (selectedGame != null)
        {
            // Start game instantly
            ActiveGame = selectedGame;
            
            // Remove all weapons from players first
            RemoveAllWeapons();
            
            // Start the game
            ActiveGame.Start();
            JailbreakCore.Extensions.PrintToChatAll("game_started", true, IPrefix.SD, player.Controller.PlayerName, ActiveGame.Name);
        }
    }

    public void StopGame(JBPlayer player)
    {
        if (ActiveGame == null)
        {
            player.Print(IHud.Chat, "no_active_game", null, 0, true, IPrefix.SD);
            return;
        }

        string gameName = ActiveGame.Name;
        ActiveGame.End();
        ActiveGame = null;

        JailbreakCore.Extensions.PrintToChatAll("game_stopped", true, IPrefix.SD, player.Controller.PlayerName, gameName);
    }

    private void RemoveAllWeapons()
    {
        foreach (var jbPlayer in JailbreakCore.JBPlayerManagement.GetAllPlayers())
        {
            if (jbPlayer.PlayerPawn.IsValid && jbPlayer.PlayerPawn.ItemServices != null)
            {
                jbPlayer.PlayerPawn.ItemServices.RemoveItems();
            }
        }
    }

    public void OnRoundStart()
    {
        // Games persist across rounds unless manually stopped
        // No auto-start on round start
    }
    
    public void OnRoundEnd()
    {
        // Stop game on round end
        if (ActiveGame != null)
        {
            ActiveGame.End();
            ActiveGame = null;
        }
    }
    
    public void EndDay()
    {
        if (ActiveGame != null)
        {
            ActiveGame.End();
            ActiveGame = null;
        }
    }
}
