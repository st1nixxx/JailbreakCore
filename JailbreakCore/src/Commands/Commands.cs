using Jailbreak;
using Jailbreak.Shared;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Players;
namespace JailbreakCore;

public class Commands(ISwiftlyCore core)
{
    private readonly ISwiftlyCore _Core = core;
    private WardenCommands WConfig => JailbreakCore.Config.Warden.Commands;
    private PrisonerCommands PConfig => JailbreakCore.Config.Prisoner.Commands;
    private int MaxHealTries => JailbreakCore.Config.Prisoner.HealTriesXRound;
    private int MaxSurrenderTries = JailbreakCore.Config.Prisoner.SurrenderTriesXRound;
    public void RegisterPrisonerCommands()
    {
        var commands = new Dictionary<List<string>, ICommandService.CommandListener>
        {
            { PConfig.HealRequest, Command_HealRequest },
            { PConfig.Surrender, Command_Surrender },
            { PConfig.LRMenu, Command_LRMenu }
        };

        foreach (var (aliases, handler) in commands)
        {
            foreach (var alias in aliases)
            {
                _Core.Command.RegisterCommand(alias, handler);
            }
        }
    }
    public void RegisterWardenCommands()
    {
        var commands = new Dictionary<List<string>, ICommandService.CommandListener>
        {
            { WConfig.TakeWarden, Command_TakeWarden },
            { WConfig.GiveUpWarden, Command_GiveUpWarden },
            { WConfig.ToggleBox, Command_ToggleBox },
            { WConfig.WardenMenu, Command_WardenMenu },
            { WConfig.SDMenu, Command_SpecialDay }
        };

        foreach (var (aliases, handler) in commands)
        {
            foreach (var alias in aliases)
            {
                _Core.Logger.LogInformation($"Registering {alias} command.");
                _Core.Command.RegisterCommand(alias, handler);
            }
        }
    }
    private void Command_HealRequest(ICommandContext context)
    {
        if (context.Sender is not IPlayer player)
            return;

        var jbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(player);

        if (!JailbreakCore.healTries.ContainsKey(jbPlayer))
            JailbreakCore.healTries[jbPlayer] = 0;

        if (jbPlayer.Role != IJBRole.Prisoner)
        {
            jbPlayer.Print(IHud.Chat, "not_prisoner_heal", null, 0, true, IPrefix.JB);
            return;
        }

        if (JailbreakCore.healTries[jbPlayer] > MaxHealTries)
        {
            jbPlayer.Print(IHud.Chat, "max_heal_tries", null, 0, true, IPrefix.JB);
            return;
        }


        var warden = JailbreakCore.JBPlayerManagement.GetWarden();
        if (warden == null)
        {
            jbPlayer.Print(IHud.Chat, "no_warden_heal", null, 0, true, IPrefix.JB);
            return;
        }

        jbPlayer.Print(IHud.Chat, "heal_request_sent", null, 0, true, IPrefix.JB);
        JailbreakCore.healTries[jbPlayer]++;

    }
    private void Command_Surrender(ICommandContext context)
    {
        if (context.Sender is not IPlayer player)
            return;

        var jbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(player);

        if (!JailbreakCore.surrenderTries.ContainsKey(jbPlayer))
            JailbreakCore.surrenderTries[jbPlayer] = 0;

        if (!jbPlayer.IsRebel) // this automaticly exclude the CT's too, so it's okay.
        {
            jbPlayer.Print(IHud.Chat, "not_rebel_surrender", null, 0, true, IPrefix.JB);
            return;
        }

        if (JailbreakCore.surrenderTries[jbPlayer] > MaxSurrenderTries)
        {
            jbPlayer.Print(IHud.Chat, "max_surrender_tries", null, 0, true, IPrefix.JB);
            return;
        }

        var warden = JailbreakCore.JBPlayerManagement.GetWarden();
        if (warden == null)
        {
            jbPlayer.Print(IHud.Chat, "no_warden_surrender", null, 0, true, IPrefix.JB);
            return;
        }

        jbPlayer.Print(IHud.Chat, "surrender_sent", null, 0, true, IPrefix.JB);
        JailbreakCore.surrenderTries[jbPlayer]++;

        JailbreakCore.SurrenderMenu.Display(jbPlayer);
    }
    private void Command_LRMenu(ICommandContext context)
    {
        if (context.Sender is not IPlayer player)
            return;

        var jbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(player);
        if (jbPlayer == null)
            return;

        if (jbPlayer.Role != IJBRole.Prisoner)
            return;


        List<JBPlayer> prisoners = JailbreakCore.JBPlayerManagement.GetAllPrisoners(excludeRebels: false, exlcudeFreedays: false);
        if (prisoners.Count > 1)
        {
            jbPlayer.Print(IHud.Chat, "lr_not_avaliable", null, 0, true, IPrefix.LR);
            return;
        }

        if (jbPlayer.IsRebel)
        {
            jbPlayer.Print(IHud.Chat, "cant_lr_as_rebel", null, 0, true, IPrefix.LR);
            return;
        }

        JailbreakCore.LRMenu.Display(jbPlayer);

    }
    private void Command_TakeWarden(ICommandContext context)
    {
        if (context.Sender is not IPlayer player)
            return;

        JBPlayer? jbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(player);
        JBPlayer? currentWarden = JailbreakCore.JBPlayerManagement.GetWarden();

        if (JailbreakCore.SpecialDay.GetActiveDay() != null ||
            JailbreakCore.LastRequest.GetActiveRequest() != null)
            return;

        if (!jbPlayer.Controller.PawnIsAlive)
            return;

        if (jbPlayer.Role == IJBRole.Prisoner)
        {
            jbPlayer.Print(IHud.Chat, "cant_become_warden_as_t", null, 0, true, IPrefix.JB);
            return;
        }

        if (currentWarden != null)
        {
            jbPlayer.Print(IHud.Chat, "already_warden", null, 0, true, IPrefix.JB, currentWarden.Controller.PlayerName);
            return;
        }

        jbPlayer.SetWarden(true);
        jbPlayer.Print(IHud.Chat, "warden_take", null, 0, true, IPrefix.JB);

        JailbreakCore.Extensions.PrintToAlertAll("warden_take_alert", jbPlayer.Controller.PlayerName);

        //if (!string.IsNullOrEmpty(JailbreakCore.Config.Sounds.WardenTake.Path))
        //{
        //    foreach (var otherJbPlayer in JailbreakCore.JBPlayerManagement.GetAllPlayers())
        //    {
        //        otherJbPlayer.PlaySound(JailbreakCore.Config.Sounds.WardenTake.Path, JailbreakCore.Config.Sounds.WardenTake.Volume);
        //    }
        //}
    }

    private void Command_GiveUpWarden(ICommandContext context)
    {
        if (context.Sender is not IPlayer player)
            return;

        JBPlayer jbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(player);
        JBPlayer? currentWarden = JailbreakCore.JBPlayerManagement.GetWarden();

        if (JailbreakCore.SpecialDay.GetActiveDay() != null ||
            JailbreakCore.LastRequest.GetActiveRequest() != null)
            return;

        if (jbPlayer.Role == IJBRole.Prisoner)
        {
            jbPlayer.Print(IHud.Chat, "you_are_not_warden", null, 0, true, IPrefix.JB);
            return;
        }

        if (currentWarden != jbPlayer)
        {
            jbPlayer.Print(IHud.Chat, "you_are_not_warden", null, 0, true, IPrefix.JB);
            return;
        }

        currentWarden.SetWarden(false);
        currentWarden.Print(IHud.Chat, "you_gave_up_on_warden", null, 0, true, IPrefix.JB);

        JailbreakCore.Extensions.PrintToAlertAll("warden_gave_up", currentWarden.Controller.PlayerName,
        JailbreakCore.Config.Warden.Commands.TakeWarden.FirstOrDefault()!);

        //if (!string.IsNullOrEmpty(JailbreakCore.Config.Sounds.WardenRemoved.Path))
        //{
        //    foreach (var otherJbPlayer in JailbreakCore.JBPlayerManagement.GetAllPlayers())
        //    {
        //        otherJbPlayer.PlaySound(JailbreakCore.Config.Sounds.WardenRemoved.Path, JailbreakCore.Config.Sounds.WardenRemoved.Volume);
        //    }
        //}

        _Core.Scheduler.DelayBySeconds(5.0f, () =>
        {
            if (JailbreakCore.JBPlayerManagement.GetWarden() == null)
            {
                JailbreakCore.Extensions.AssignRandomWarden();
                //if (!string.IsNullOrEmpty(JailbreakCore.Config.Sounds.WardenTake.Path))
                //{
                //    foreach (var otherJbPlayer in JailbreakCore.JBPlayerManagement.GetAllPlayers())
                //    {
                //        otherJbPlayer.PlaySound(JailbreakCore.Config.Sounds.WardenTake.Path, JailbreakCore.Config.Sounds.WardenTake.Volume);
                //    }
                //}
                JailbreakCore.Extensions.PrintToCenterAll("warden_take_alert", JailbreakCore.JBPlayerManagement.GetWarden()?.Controller.PlayerName ?? "");
            }
        });
    }

    private void Command_ToggleBox(ICommandContext context)
    {
        if (context.Sender is not IPlayer player)
            return;

        JBPlayer jbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(player);

        if (JailbreakCore.SpecialDay.GetActiveDay() != null ||
            JailbreakCore.LastRequest.GetActiveRequest() != null)
            return;

        if (!jbPlayer.IsWarden)
        {
            jbPlayer.Print(IHud.Chat, "you_are_not_warden", null, 0, true, IPrefix.JB);
            return;
        }

        JailbreakCore.g_IsBoxActive = !JailbreakCore.g_IsBoxActive;
        JailbreakCore.Extensions.ToggleBox(JailbreakCore.g_IsBoxActive, jbPlayer.Controller.PlayerName);
    }

    private void Command_WardenMenu(ICommandContext context)
    {
        if (context.Sender is not IPlayer player)
            return;

        var jbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(player);
        if (!jbPlayer.IsWarden)
        {
            jbPlayer.Print(IHud.Chat, "you_are_not_warden", null, 0, true, IPrefix.JB);
            return;
        }

        if (JailbreakCore.SpecialDay.GetActiveDay() != null || JailbreakCore.LastRequest.GetActiveRequest() != null)
            return;

        JailbreakCore.WardenMenu.Display(jbPlayer);
    }
    private void Command_SpecialDay(ICommandContext context)
    {
        if (context.Sender is not IPlayer player)
            return;

        var jbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(player);

        if (!jbPlayer.IsWarden)
        {
            jbPlayer.Print(IHud.Chat, "you_are_not_warden", null, 0, true, IPrefix.SD);
            return;
        }

        JailbreakCore.SDMenu.Display(jbPlayer);
    }
}
