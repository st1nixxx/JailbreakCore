//using AudioApi;
using Jailbreak;
using Jailbreak.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.GameEvents;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;
using Tomlyn.Extensions.Configuration;

namespace JailbreakCore;

[PluginMetadata(Id = "JailbreakCore", Version = "1.0.0", Name = "JailbreakCore", Author = "T3Marius", Description = "No description.")]
public partial class JailbreakCore : BasePlugin
{
    private ServiceProvider? _provider;

    public static Extensions Extensions = null!;
    public static JBPlayerManagement JBPlayerManagement = null!;
    public static LastRequest LastRequest = null!;
    public static SpecialDay SpecialDay = null!;
    public static WardenMenu WardenMenu = null!;
    public static SDMenu SDMenu = null!;
    public static LRMenu LRMenu = null!;
    public static SurrenderMenu SurrenderMenu = null!;
    public static HealMenu HealMenu = null!;
    public static Hooks Hooks = null!;
    //public static IAudioApi Audio = null!;
    public static JailbreakConfig Config { get; set; } = new JailbreakConfig();

    public static bool g_IsBoxActive = false;
    public static bool g_AreCellsOpened = false;

    public static Dictionary<JBPlayer, int> surrenderTries = new();
    public static Dictionary<JBPlayer, int> healTries = new();

    public JailbreakCore(ISwiftlyCore core) : base(core)
    {

    }
    public override void ConfigureSharedInterface(IInterfaceManager interfaceManager)
    {
        var apiService = new Api();

        interfaceManager.AddSharedInterface<IJailbreakApi, Api>("Jailbreak.Core", apiService);
    }

    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
        //Audio = interfaceManager.GetSharedInterface<IAudioApi>("audio");
    }
    public override void Load(bool hotReload)
    {
        if (hotReload)
        {
            Hooks?.Unregister();
            _provider?.Dispose();
        }

        Core.Configuration
            .InitializeTomlWithModel<JailbreakConfig>("config.toml", "JailbreakCore")
            .Configure(builder =>
            {
                builder.AddTomlFile("config.toml", optional: false, reloadOnChange: true);
            });

        ServiceCollection services = new();
        services.AddSwiftly(Core)
                .AddSingleton<Extensions>()
                .AddSingleton<JBPlayerManagement>()
                .AddSingleton<LastRequest>()
                .AddSingleton<SpecialDay>()
                .AddSingleton<Commands>()
                .AddSingleton<Hooks>()
                .AddSingleton<WardenMenu>()
                .AddSingleton<SDMenu>()
                .AddSingleton<LRMenu>()
                .AddSingleton<SurrenderMenu>()
                .AddSingleton<HealMenu>()
                .AddOptionsWithValidateOnStart<JailbreakConfig>().BindConfiguration("JailbreakCore");

        _provider = services.BuildServiceProvider();

        Config = _provider.GetRequiredService<IOptions<JailbreakConfig>>().Value;
        Extensions = _provider.GetRequiredService<Extensions>();
        JBPlayerManagement = _provider.GetRequiredService<JBPlayerManagement>();
        LastRequest = _provider.GetRequiredService<LastRequest>();
        SpecialDay = _provider.GetRequiredService<SpecialDay>();
        Hooks = _provider.GetRequiredService<Hooks>();
        WardenMenu = _provider.GetRequiredService<WardenMenu>();
        SDMenu = _provider.GetRequiredService<SDMenu>();
        LRMenu = _provider.GetRequiredService<LRMenu>();
        SurrenderMenu = _provider.GetRequiredService<SurrenderMenu>();
        HealMenu = _provider.GetRequiredService<HealMenu>();

        var commmands = _provider.GetRequiredService<Commands>();

        commmands.RegisterWardenCommands();
        commmands.RegisterPrisonerCommands();
        Hooks.Register();
    }

    public override void Unload()
    {
        Hooks?.Unregister();
        _provider?.Dispose();
        Extensions?.CleanupAllLasers();
        Extensions?.CleanupAllBeacons();
    }

    #region Events
    [GameEventHandler(HookMode.Pre)]
    public HookResult EventPlayerDeath(EventPlayerDeath @event)
    {
        IPlayer player = @event.UserIdPlayer;
        IPlayer attacker = Core.PlayerManager.GetPlayer(@event.Attacker);

        if (player == null || player.PlayerPawn == null || attacker == null || attacker.PlayerPawn == null)
            return HookResult.Continue;

        JBPlayer jbAttacker = JBPlayerManagement.GetOrCreate(attacker);
        JBPlayer jbVictim = JBPlayerManagement.GetOrCreate(player);

        if (jbAttacker == jbVictim)
            return HookResult.Continue;

        if (jbVictim.IsValid)
            LastRequest.OnPlayerDeath(jbVictim);

        if (LastRequest.GetActiveRequest() != null || SpecialDay.GetActiveDay() != null)
            return HookResult.Continue;

        if (jbVictim.IsWarden)
        {
            jbVictim.SetWarden(false);
            jbAttacker.SetRebel(true);

            Extensions.PrintToChatAll("warden_killed_by", true, IPrefix.JB, jbAttacker.Controller.PlayerName);
            Extensions.PrintToAlertAll("warden_died", Config.Warden.Commands.TakeWarden.FirstOrDefault()!);

            if (!string.IsNullOrEmpty(Config.Sounds.WardenKilled.Path))
            {
                foreach (var p in JBPlayerManagement.GetAllPlayers())
                {
                    //p.PlaySound(Config.Sounds.WardenKilled.Path, Config.Sounds.WardenKilled.Volume);
                }
            }

            Core.Scheduler.DelayBySeconds(5, () =>
            {
                if (JBPlayerManagement.GetWarden() == null)
                {
                    Extensions.AssignRandomWarden();
                    Extensions.PrintToCenterAll("warden_take_alert", JBPlayerManagement.GetWarden()?.Player.Controller.PlayerName ?? " ");
                }
            });
        }

        if (jbVictim.IsRebel)
        {
            jbVictim.SetRebel(false);
            Extensions.PrintToCenterAll("rebel_killed_by", jbVictim.Controller.PlayerName, jbAttacker.Controller.PlayerName);
        }

        return HookResult.Continue;
    }
    [GameEventHandler(HookMode.Pre)]
    public HookResult EventPlayerTeam(EventPlayerTeam @event)
    {
        IPlayer player = @event.UserIdPlayer;
        if (player == null)
            return HookResult.Continue;

        JBPlayer jbPlayer = JBPlayerManagement.GetOrCreate(player);
        jbPlayer.OnChangeTeam((Team)@event.Team);

        return HookResult.Continue;
    }
    [GameEventHandler(HookMode.Pre)]
    public HookResult EventPlayerDisconnect(EventPlayerDisconnect @event)
    {
        IPlayer player = @event.UserIdPlayer;
        if (player == null)
            return HookResult.Continue;

        var jbPlayer = JBPlayerManagement.GetOrCreate(player);

        if (healTries.ContainsKey(jbPlayer))
            healTries.Remove(jbPlayer);

        if (surrenderTries.ContainsKey(jbPlayer))
            surrenderTries.Remove(jbPlayer);

        JBPlayerManagement.Remove(player);

        return HookResult.Continue;
    }
    [GameEventHandler(HookMode.Pre)]
    public HookResult EventPlayerPing(EventPlayerPing @event)
    {
        IPlayer player = @event.UserIdPlayer;
        if (player == null)
            return HookResult.Continue;

        var jbPlayer = JBPlayerManagement.GetOrCreate(player);

        if (!jbPlayer.IsWarden)
            return HookResult.Continue;

        var pingPos = new Vector(@event.X, @event.Y, @event.Z);

        Extensions.CreateWardenBeacon(jbPlayer, pingPos, radius: 64.0f, height: 5.0f, segments: 32, duration: 30.0f);

        return HookResult.Continue;
    }
    [GameEventHandler(HookMode.Post)]
    public HookResult EventRoundStart(EventRoundStart @event)
    {
        JBPlayer? currentWarden = JBPlayerManagement.GetWarden();
        List<JBPlayer> currentRebels = JBPlayerManagement.GetAllRebels();
        List<JBPlayer> currentFreedays = JBPlayerManagement.GetAllFreedays();

        surrenderTries.Clear();
        healTries.Clear();

        if (currentWarden != null)
            currentWarden.SetWarden(false);

        foreach (var rebel in currentRebels)
            rebel.SetRebel(false);

        foreach (var freeday in currentFreedays)
            freeday.SetFreeday(false);

        Extensions.ToggleBox(false);
        SpecialDay.OnRoundStart();

        Extensions.ToggleBunnyhoop(false);

        if (SpecialDay.GetActiveDay() == null)
        {
            Core.Scheduler.DelayBySeconds(5.0f, () =>
            {
                if (JBPlayerManagement.GetWarden() == null)
                {
                    Extensions.AssignRandomWarden();
                    Extensions.PrintToCenterAll("warden_take_alert", JBPlayerManagement.GetWarden()?.Controller.PlayerName ?? "");

                    if (!string.IsNullOrEmpty(Config.Sounds.WardenTake.Path))
                    {
                        foreach (var otherJbPlayer in JBPlayerManagement.GetAllPlayers())
                        {
                            //otherJbPlayer.PlaySound(Config.Sounds.WardenTake.Path, Config.Sounds.WardenTake.Volume);
                        }
                    }
                }
            });

            if (Config.Bunnyhoop.Enable && Config.Bunnyhoop.RoundStartCooldown > 1)
            {
                Core.Scheduler.DelayBySeconds(Config.Bunnyhoop.RoundStartCooldown, () =>
                {
                    Extensions.ToggleBunnyhoop(true);
                });
            }
            else if (Config.Bunnyhoop.Enable && Config.Bunnyhoop.RoundStartCooldown <= 1)
            {
                Extensions.ToggleBunnyhoop(true);
            }
        }
        else
        {
            if (Config.Bunnyhoop.EnableOnSpecialDay)
                Extensions.ToggleBunnyhoop(true);
        }

        Core.Scheduler.NextTick(() =>
        {
            if (Config.Prisoner.PrisonerMuteDuration > 1)
            {
                foreach (var player in Core.PlayerManager.GetAllPlayers().Where(p => p.Controller.TeamNum == (int)Team.T))
                {
                    player.VoiceFlags = VoiceFlagValue.Muted;
                    JBPlayer jbPlayer = JBPlayerManagement.GetOrCreate(player);
                    Core.Scheduler.DelayBySeconds(Config.Prisoner.PrisonerMuteDuration, () =>
                    {
                        jbPlayer.Print(IHud.Chat, "prisoner_unmuted", Config.Prisoner.PrisonerMuteDuration.ToString(), 0, true, IPrefix.JB);
                        player.VoiceFlags = VoiceFlagValue.ListenAll;
                    });
                }
            }
        });

        return HookResult.Continue;
    }
    [GameEventHandler(HookMode.Pre)]
    public HookResult EventRoundEnd(EventRoundEnd @event)
    {
        SpecialDay.OnRoundEnd();
        LastRequest.EndRequest(null, null);

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            JBPlayer jbPlayer = JBPlayerManagement.GetOrCreate(player);
            if (Config.Prisoner.UnmutePrisonerOnRoundEnd && player.VoiceFlags == VoiceFlagValue.Muted)
            {
                jbPlayer.Print(IHud.Chat, "prisoner_unmuted_round_end", null, 0, true, IPrefix.JB);
                player.VoiceFlags = VoiceFlagValue.ListenAll;
            }
        }

        return HookResult.Continue;
    }
    [GameEventHandler(HookMode.Pre)]
    public HookResult EventPlayerHurt(EventPlayerHurt @event)
    {
        IPlayer victimPlayer = @event.UserIdPlayer;
        IPlayer attackerPlayer = Core.PlayerManager.GetPlayer(@event.Attacker);

        if (attackerPlayer == null || victimPlayer == null || attackerPlayer == victimPlayer)
            return HookResult.Continue;

        if (attackerPlayer.PlayerPawn == null || victimPlayer.PlayerPawn == null)
            return HookResult.Continue;

        JBPlayer jbAttacker = JBPlayerManagement.GetOrCreate(attackerPlayer);

        if (jbAttacker.Role == IJBRole.Prisoner)
            return HookResult.Continue;

        if (SpecialDay.GetActiveDay() != null)
            return HookResult.Continue;

        if (LastRequest.GetActiveRequest() != null)
            return HookResult.Continue;

        if (!jbAttacker.IsRebel && jbAttacker.Controller.TeamNum == (int)Team.T)
        {
            jbAttacker.SetRebel(true);
            Extensions.PrintToAlertAll("became_rebel", jbAttacker.Controller.PlayerName);

            foreach (var otherJbPlayer in JBPlayerManagement.GetAllPlayers())
            {
                if (!string.IsNullOrEmpty(Config.Sounds.Rebel.Path))
                {
                    //otherJbPlayer.PlaySound(Config.Sounds.Rebel.Path, Config.Sounds.Rebel.Volume);
                }
            }
        }

        return HookResult.Continue;
    }
    [GameEventHandler(HookMode.Pre)]
    public HookResult EventWeaponFire(EventWeaponFire @event)
    {
        IPlayer player = @event.UserIdPlayer;
        if (player == null)
            return HookResult.Continue;

        JBPlayer jbPlayer = JBPlayerManagement.GetOrCreate(player);

        if (SpecialDay.GetActiveDay() != null)
            return HookResult.Continue;

        if (LastRequest.GetActiveRequest() != null)
            return HookResult.Continue;

        if (@event.Weapon.Contains("knife"))
            return HookResult.Continue;

        if (jbPlayer.Role == IJBRole.Prisoner && !jbPlayer.IsRebel && jbPlayer.Controller.TeamNum == (int)Team.T)
        {
            jbPlayer.SetRebel(true);
            Extensions.PrintToAlertAll("became_rebel", jbPlayer.Controller.PlayerName);

            foreach (var otherJbPlayer in JBPlayerManagement.GetAllPlayers())
            {
                if (!string.IsNullOrEmpty(Config.Sounds.Rebel.Path))
                {
                    //otherJbPlayer.PlaySound(Config.Sounds.Rebel.Path, Config.Sounds.Rebel.Volume);
                }
            }
        }

        return HookResult.Continue;
    }
    [GameEventHandler(HookMode.Post)]
    public HookResult EventPlayerSpawned(EventPlayerSpawned @event)
    {
        IPlayer player = @event.UserIdPlayer;
        if (player == null)
            return HookResult.Continue;

        JBPlayer jbPlayer = JBPlayerManagement.GetOrCreate(player);
        jbPlayer.OnPlayerSpawn();

        return HookResult.Continue;
    }
    [EventListener<EventDelegates.OnTick>]
    public void OnTick()
    {
        Extensions.TickDynamicEffects();

        var warden = JBPlayerManagement.GetWarden();
        if (warden == null || !warden.IsValid || !warden.Controller.PawnIsAlive || warden.PlayerPawn == null)
        {
            // No warden or warden is invalid/dead - cleanup any existing laser
            if (warden != null)
                Extensions.RemoveWardenLaser(warden);
            return;
        }

        // Check if warden is holding E key (bitwise check to allow other buttons)
        if ((warden.Player.PressedButtons & GameButtonFlags.E) != 0)
        {
            // Get warden's eye position and forward vector
            var absOrigin = warden.PlayerPawn.AbsOrigin;
            if (!absOrigin.HasValue)
                return;

            var eyePos = new Vector(absOrigin.Value.X, absOrigin.Value.Y, absOrigin.Value.Z + 64); // Add eye height

            var eyeAngles = warden.PlayerPawn.EyeAngles;

            // Calculate forward direction from angles
            double pitch = eyeAngles.Pitch * Math.PI / 180.0;
            double yaw = eyeAngles.Yaw * Math.PI / 180.0;

            float cosPitch = (float)Math.Cos(pitch);
            float sinPitch = (float)Math.Sin(pitch);
            float cosYaw = (float)Math.Cos(yaw);
            float sinYaw = (float)Math.Sin(yaw);

            // Calculate end position (4096 units ahead in view direction)
            float distance = 4096.0f;
            var endPos = new Vector(
                eyePos.X + distance * cosPitch * cosYaw,
                eyePos.Y + distance * cosPitch * sinYaw,
                eyePos.Z - distance * sinPitch
            );

            // Create or update the laser
            Extensions.UpdateWardenLaser(warden, eyePos, endPos);
        }
        else
        {
            // Not holding E - remove laser if it exists
            Extensions.RemoveWardenLaser(warden);
        }
    }
    #endregion

}
