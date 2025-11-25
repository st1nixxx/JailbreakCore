using Jailbreak.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Helpers;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.ProtobufDefinitions;
using SwiftlyS2.Shared.SchemaDefinitions;
using Tomlyn.Extensions.Configuration;

namespace SpecialDays;

[PluginMetadata(Id = "SpecialDays", Version = "1.0.0", Name = "SpecialDays", Author = "T3Marius", Description = "No description.")]
public partial class SpecialDays : BasePlugin
{
    private ServiceProvider? _provider;
    public static IJailbreakApi Api = null!;
    public static DaysConfig Config { get; set; } = new();
    public static Library Library { get; set; } = null!;
    public SpecialDays(ISwiftlyCore core) : base(core)
    {
    }
    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
        Api = interfaceManager.GetSharedInterface<IJailbreakApi>("Jailbreak.Core");

        Register();
    }
    public override void Load(bool hotReload)
    {
        Core.Configuration
            .InitializeJsonWithModel<DaysConfig>("config.json", "SpecialDays")
            .Configure(builder =>
            {
                builder.AddJsonFile("config.json", false, true);
            });

        ServiceCollection services = new();
        services.AddSwiftly(Core)
                .AddSingleton<Library>()
                .AddOptionsWithValidateOnStart<DaysConfig>().BindConfiguration("SpecialDays");

        _provider = services.BuildServiceProvider();
        Config = _provider.GetRequiredService<IOptions<DaysConfig>>().Value;
        Library = _provider.GetRequiredService<Library>();
    }

    public override void Unload()
    {
        Unregister();
    }
    public void Register()
    {
        if (Config.FFA.Enable)
            Api.SpecialDay.Register(new FFA_Day(Core, Api, Library));

        if (Config.Teleport.Enable)
            Api.SpecialDay.Register(new Teleport_Day(Core, Api, Library));

        if (Config.NoScope.Enable)
            Api.SpecialDay.Register(new NoScope_Day(Core, Api, Library));

        if (Config.OneInTheChamber.Enable)
            Api.SpecialDay.Register(new OneInTheChamber_Day(Core, Api, Library));

        if (Config.HeadshotOnly.Enable)
            Api.SpecialDay.Register(new HeadshotOnly_Day(Core, Api, Library));

        if (Config.HideAndSeek.Enable)
            Api.SpecialDay.Register(new HideAndSeek_Day(Core, Api, Library));

        if (Config.KnifeFight.Enable)
            Api.SpecialDay.Register(new KnifeFight_Day(Core, Api, Library));

        if (Config.KnifeFight.Enable && Config.KnifeFight.Speed > 1.0f)
            Api.SpecialDay.Register(new KnifeFight_Speed_Day(Core, Api, Library));

        if (Config.KnifeFight.Enable && Config.KnifeFight.Gravity < 1.0f)
            Api.SpecialDay.Register(new KnifeFight_Gravity_Day(Core, Api, Library));
    }
    public void Unregister()
    {
        if (Config.FFA.Enable)
            Api.SpecialDay.Unregister(new FFA_Day(Core, Api, Library));

        if (Config.Teleport.Enable)
            Api.SpecialDay.Unregister(new Teleport_Day(Core, Api, Library));

        if (Config.NoScope.Enable)
            Api.SpecialDay.Unregister(new NoScope_Day(Core, Api, Library));

        if (Config.OneInTheChamber.Enable)
            Api.SpecialDay.Unregister(new OneInTheChamber_Day(Core, Api, Library));

        if (Config.HeadshotOnly.Enable)
            Api.SpecialDay.Unregister(new HeadshotOnly_Day(Core, Api, Library));

        if (Config.HideAndSeek.Enable)
            Api.SpecialDay.Unregister(new HideAndSeek_Day(Core, Api, Library));

        if (Config.KnifeFight.Enable)
            Api.SpecialDay.Unregister(new KnifeFight_Day(Core, Api, Library));

        if (Config.KnifeFight.Enable && Config.KnifeFight.Speed > 1.0f)
            Api.SpecialDay.Unregister(new KnifeFight_Speed_Day(Core, Api, Library));

        if (Config.KnifeFight.Enable && Config.KnifeFight.Gravity < 1.0f)
            Api.SpecialDay.Unregister(new KnifeFight_Gravity_Day(Core, Api, Library));
    }
    public static readonly Dictionary<string, int> WeaponItemDefinitionIndices = new()
    {
        // Pistols
        { "weapon_deagle", 1 },
        { "weapon_elite", 2 },
        { "weapon_fiveseven", 3 },
        { "weapon_glock", 4 },
        { "weapon_tec9", 30 },
        { "weapon_hkp2000", 32 },
        { "weapon_p250", 36 },
        { "weapon_usp_silencer", 61 },
        { "weapon_cz75a", 63 },
        { "weapon_revolver", 64 },

        // Rifles
        { "weapon_ak47", 7 },
        { "weapon_aug", 8 },
        { "weapon_awp", 9 },
        { "weapon_famas", 10 },
        { "weapon_g3sg1", 11 },
        { "weapon_galilar", 13 },
        { "weapon_m249", 14 },
        { "weapon_m4a1", 16 },
        { "weapon_mac10", 17 },
        { "weapon_p90", 19 },
        { "weapon_mp5sd", 23 },
        { "weapon_ump45", 24 },
        { "weapon_xm1014", 25 },
        { "weapon_bizon", 26 },
        { "weapon_mag7", 27 },
        { "weapon_negev", 28 },
        { "weapon_sawedoff", 29 },
        { "weapon_mp7", 33 },
        { "weapon_mp9", 34 },
        { "weapon_nova", 35 },
        { "weapon_scar20", 38 },
        { "weapon_sg556", 39 },
        { "weapon_ssg08", 40 },
        { "weapon_m4a1_silencer", 60 },

        // Grenades
        { "weapon_flashbang", 43 },
        { "weapon_hegrenade", 44 },
        { "weapon_smokegrenade", 45 },
        { "weapon_molotov", 46 },
        { "weapon_decoy", 47 },
        { "weapon_incgrenade", 48 },

        // Knives and Equipment
        { "weapon_taser", 31 },
        { "weapon_knifegg", 41 },
        { "weapon_knife", 42 },
        { "weapon_c4", 49 },
        { "weapon_knife_t", 59 },
        { "weapon_bayonet", 500 },
        { "weapon_knife_css", 503 },
        { "weapon_knife_flip", 505 },
        { "weapon_knife_gut", 506 },
        { "weapon_knife_karambit", 507 },
        { "weapon_knife_m9_bayonet", 508 },
        { "weapon_knife_tactical", 509 },
        { "weapon_knife_falchion", 512 },
        { "weapon_knife_survival_bowie", 514 },
        { "weapon_knife_butterfly", 515 },
        { "weapon_knife_push", 516 },
        { "weapon_knife_cord", 517 },
        { "weapon_knife_canis", 518 },
        { "weapon_knife_ursus", 519 },
        { "weapon_knife_gypsy_jackknife", 520 },
        { "weapon_knife_outdoor", 521 },
        { "weapon_knife_stiletto", 522 },
        { "weapon_knife_widowmaker", 523 },
        { "weapon_knife_skeleton", 525 },
        { "weapon_knife_kukri", 526 },

        // Utility
        { "item_kevlar", 50 },
        { "item_assaultsuit", 51 },
        { "item_heavyassaultsuit", 52 },
        { "item_defuser", 55 },
        { "ammo_50ae", 0 }
    };
}
public class KnifeFight_Gravity_Day(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ISpecialDay
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    private KnifeFightConfig Config => SpecialDays.Config.KnifeFight;
    private int DelayCooldown = SpecialDays.Config.KnifeFight.DelayCooldown;
    public string Name => Core.Localizer["knife_fight_gravity_day<name>"];
    public string Description => Core.Localizer["knife_fight_gravity_day<description>"];
    private CancellationTokenSource? token = null;
    private bool g_IsTimerActive = false;
    private IDisposable? damageHook = null;
    private HashSet<ushort> AllowedWeaponsDefIndex = new(GetAllowedWeapons());
    private static IEnumerable<ushort> GetAllowedWeapons()
    {
        var knifes = new[]
        {
            41,
            42,
            59,
            500,
            503,
            505,
            506,
            507,
            508,
            509,
            512,
            514,
            515,
            516,
            517,
            518,
            519,
            520,
            521,
            522,
            523,
            525,
            526
        };

        foreach (var knife in knifes)
            yield return (ushort)knife;
    }
    public void Start()
    {
        damageHook = Api.Hooks.HookTakeDamage(OnTakeDamage);
        Core.Event.OnItemServicesCanAcquireHook += CanAcquireFunc;

        Api.Utilities.ToggleCells(true, "");
        Library.ToggleFriendlyFire(true);

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player.IsFakeClient) continue;
            var jbPlayer = Api.Players.GetPlayer(player);
            if (jbPlayer == null)
                continue;

            jbPlayer.PlayerPawn.ItemServices?.RemoveItems();
            Core.Scheduler.NextTick(() =>
            {
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_knife");
            });

            Library.SetGravity(jbPlayer, Config.Gravity);
        }

        token = Core.Scheduler.RepeatBySeconds(1.0f, () =>
        {
            DelayCooldown--;
            if (DelayCooldown > 0)
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["day_starting", Name, DelayCooldown]);
                g_IsTimerActive = true;
            }
            else
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["prefix"] + Core.Localizer["game_started_message"]);
                token?.Cancel();
                token = null;
                g_IsTimerActive = false;

            }
        });

    }
    public void End()
    {
        damageHook?.Dispose();
        damageHook = null;

        Library.ToggleFriendlyFire(false);

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            var jbPlayer = Api.Players.GetPlayer(player);
            if (jbPlayer != null)
                Library.SetGravity(jbPlayer, 1.0f);
        }

        token?.Cancel();
        token = null;

        Core.Event.OnItemServicesCanAcquireHook -= CanAcquireFunc;
    }
    public void CanAcquireFunc(IOnItemServicesCanAcquireHookEvent @event)
    {
        var econItem = @event.EconItemView;
        if (!AllowedWeaponsDefIndex.Contains(econItem.ItemDefinitionIndex))
        {
            @event.SetAcquireResult(AcquireResult.NotAllowedByProhibition);
            return;
        }
    }
    public HookResult OnTakeDamage(DamageHookContext context)
    {
        var info = context.Info;
        if (g_IsTimerActive)
        {
            info.Damage = 0;
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }
}
public class KnifeFight_Speed_Day(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ISpecialDay
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    private KnifeFightConfig Config => SpecialDays.Config.KnifeFight;
    private int DelayCooldown = SpecialDays.Config.KnifeFight.DelayCooldown;
    public string Name => Core.Localizer["knife_fight_speed_day<name>"];
    public string Description => Core.Localizer["knife_fight_speed_day<description>"];
    private CancellationTokenSource? token = null;
    private bool g_IsTimerActive = false;
    private IDisposable? damageHook = null;
    private HashSet<ushort> AllowedWeaponsDefIndex = new(GetAllowedWeapons());
    private static IEnumerable<ushort> GetAllowedWeapons()
    {
        var knifes = new[]
        {
            41,
            42,
            59,
            500,
            503,
            505,
            506,
            507,
            508,
            509,
            512,
            514,
            515,
            516,
            517,
            518,
            519,
            520,
            521,
            522,
            523,
            525,
            526
        };

        foreach (var knife in knifes)
            yield return (ushort)knife;
    }
    public void Start()
    {
        damageHook = Api.Hooks.HookTakeDamage(OnTakeDamage);
        Core.Event.OnItemServicesCanAcquireHook += CanAcquireFunc;

        Api.Utilities.ToggleCells(true, "");
        Library.ToggleFriendlyFire(true);

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player.IsFakeClient) continue;
            var jbPlayer = Api.Players.GetPlayer(player);
            if (jbPlayer == null)
                continue;

            jbPlayer.PlayerPawn.ItemServices?.RemoveItems();
            Core.Scheduler.NextTick(() =>
            {
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_knife");
            });

            Library.SetSpeed(jbPlayer, Config.Speed);
        }

        token = Core.Scheduler.RepeatBySeconds(1.0f, () =>
        {
            DelayCooldown--;
            if (DelayCooldown > 0)
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["day_starting", Name, DelayCooldown]);
                g_IsTimerActive = true;
            }
            else
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["prefix"] + Core.Localizer["game_started_message"]);
                token?.Cancel();
                token = null;
                g_IsTimerActive = false;

            }
        });

    }
    public void End()
    {
        damageHook?.Dispose();
        damageHook = null;

        Library.ToggleFriendlyFire(false);

        token?.Cancel();
        token = null;

        Core.Event.OnItemServicesCanAcquireHook -= CanAcquireFunc;

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            var jbPlayer = Api.Players.GetPlayer(player);
            if (jbPlayer != null)
                Library.SetSpeed(jbPlayer, 1.0f);
        }
    }
    public void CanAcquireFunc(IOnItemServicesCanAcquireHookEvent @event)
    {
        var econItem = @event.EconItemView;
        if (!AllowedWeaponsDefIndex.Contains(econItem.ItemDefinitionIndex))
        {
            @event.SetAcquireResult(AcquireResult.NotAllowedByProhibition);
            return;
        }
    }
    public HookResult OnTakeDamage(DamageHookContext context)
    {
        var info = context.Info;
        if (g_IsTimerActive)
        {
            info.Damage = 0;
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }
}
public class KnifeFight_Day(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ISpecialDay
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    private KnifeFightConfig Config => SpecialDays.Config.KnifeFight;
    private int DelayCooldown = SpecialDays.Config.KnifeFight.DelayCooldown;
    public string Name => Core.Localizer["knife_fight_day<name>"];
    public string Description => Core.Localizer["knife_fight_day<description>"];
    private CancellationTokenSource? token = null;
    private bool g_IsTimerActive = false;
    private IDisposable? damageHook = null;
    private HashSet<ushort> AllowedWeaponsDefIndex = new(GetAllowedWeapons());
    private static IEnumerable<ushort> GetAllowedWeapons()
    {
        var knifes = new[]
        {
            41,
            42,
            59,
            500,
            503,
            505,
            506,
            507,
            508,
            509,
            512,
            514,
            515,
            516,
            517,
            518,
            519,
            520,
            521,
            522,
            523,
            525,
            526
        };

        foreach (var knife in knifes)
            yield return (ushort)knife;
    }
    public void Start()
    {
        damageHook = Api.Hooks.HookTakeDamage(OnTakeDamage);
        Core.Event.OnItemServicesCanAcquireHook += CanAcquireFunc;

        Api.Utilities.ToggleCells(true, "");
        Library.ToggleFriendlyFire(true);

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player.IsFakeClient) continue;
            var jbPlayer = Api.Players.GetPlayer(player);
            if (jbPlayer == null)
                continue;

            jbPlayer.PlayerPawn.ItemServices?.RemoveItems();
            Core.Scheduler.NextTick(() =>
            {
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_knife");
            });
        }

        token = Core.Scheduler.RepeatBySeconds(1.0f, () =>
        {
            DelayCooldown--;
            if (DelayCooldown > 0)
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["day_starting", Name, DelayCooldown]);
                g_IsTimerActive = true;
            }
            else
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["prefix"] + Core.Localizer["game_started_message"]);
                token?.Cancel();
                token = null;
                g_IsTimerActive = false;

            }
        });

    }
    public void End()
    {
        damageHook?.Dispose();
        damageHook = null;

        Library.ToggleFriendlyFire(false);

        token?.Cancel();
        token = null;

        Core.Event.OnItemServicesCanAcquireHook -= CanAcquireFunc;
    }
    public void CanAcquireFunc(IOnItemServicesCanAcquireHookEvent @event)
    {
        var econItem = @event.EconItemView;
        if (!AllowedWeaponsDefIndex.Contains(econItem.ItemDefinitionIndex))
        {
            @event.SetAcquireResult(AcquireResult.NotAllowedByProhibition);
            return;
        }
    }
    public HookResult OnTakeDamage(DamageHookContext context)
    {
        var info = context.Info;
        if (g_IsTimerActive)
        {
            info.Damage = 0;
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }
}
public class HideAndSeek_Day(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ISpecialDay
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    private HideAndSeekConfig Config => SpecialDays.Config.HideAndSeek;
    private int DelayCooldown = SpecialDays.Config.HideAndSeek.DelayCooldown;
    public string Name => Core.Localizer["hide_and_seek_day<name>"];
    public string Description => Core.Localizer["hide_and_seek_day<description>"];
    private CancellationTokenSource? token = null;
    private bool g_IsTimerActive = false;
    private IDisposable? damageHook = null;
    private HashSet<ushort> AllowedWeaponsDefIndex = new(GetAllowedWeapons());
    private static IEnumerable<ushort> GetAllowedWeapons()
    {
        var knifes = new[]
        {
            41,
            42,
            59,
            500,
            503,
            505,
            506,
            507,
            508,
            509,
            512,
            514,
            515,
            516,
            517,
            518,
            519,
            520,
            521,
            522,
            523,
            525,
            526
        };

        foreach (var knife in knifes)
            yield return (ushort)knife;
    }
    public void Start()
    {
        damageHook = Api.Hooks.HookTakeDamage(OnTakeDamage);
        Api.Utilities.ToggleCells(true, "");

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player.IsFakeClient) continue;
            var jbPlayer = Api.Players.GetPlayer(player);
            if (jbPlayer == null)
                continue;

            jbPlayer.PlayerPawn.ItemServices?.RemoveItems();
            Core.Scheduler.NextTick(() =>
            {
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_knife");
            });

            if (jbPlayer.Role == IJBRole.Guardian)
            {
                Library.Freeze(jbPlayer, true);
            }
        }

        token = Core.Scheduler.RepeatBySeconds(1.0f, () =>
        {
            DelayCooldown--;
            if (DelayCooldown > 0)
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["day_starting", Name, DelayCooldown]);
                g_IsTimerActive = true;
            }
            else
            {
                List<IPlayer> terrorists = Core.PlayerManager.GetAllPlayers().Where(p => p.Controller.TeamNum == (int)Team.T && !p.IsFakeClient).ToList();

                List<IPlayer> cts = Core.PlayerManager.GetAllPlayers().Where(p => p.Controller.TeamNum == (int)Team.CT && !p.IsFakeClient).ToList();

                foreach (var t in terrorists)
                {
                    IJBPlayer? prisoner = Api.Players.GetPlayer(t);
                    if (prisoner != null)
                        Library.Freeze(prisoner, true);
                }
                foreach (var ct in cts)
                {
                    IJBPlayer? guardian = Api.Players.GetPlayer(ct);

                    if (guardian != null)
                        Library.Freeze(guardian, false);
                }

                Core.PlayerManager.SendMessage(MessageType.CenterHTML, "");
                token?.Cancel();
                token = null;
                g_IsTimerActive = false;

            }
        });

        Core.Event.OnItemServicesCanAcquireHook += CanAcquireFunc;

    }
    public void CanAcquireFunc(IOnItemServicesCanAcquireHookEvent @event)
    {
        var econItem = @event.EconItemView;
        if (!AllowedWeaponsDefIndex.Contains(econItem.ItemDefinitionIndex))
        {
            @event.SetAcquireResult(AcquireResult.NotAllowedByProhibition);
            return;
        }
    }
    public HookResult OnTakeDamage(DamageHookContext context)
    {
        var info = context.Info;
        if (g_IsTimerActive)
        {
            info.Damage = 0;
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }
    public void End()
    {
        damageHook?.Dispose();
        damageHook = null;
        token?.Cancel();
        token = null;

        Core.Event.OnItemServicesCanAcquireHook -= CanAcquireFunc;

        foreach (var terrorist in Core.PlayerManager.GetAllPlayers().Where(t => t.Controller.TeamNum == (int)Team.T && !t.IsFakeClient))
        {
            IJBPlayer? prisoner = Api.Players.GetPlayer(terrorist);
            if (prisoner != null)
                Library.Freeze(prisoner, false);
        }
    }
}
public class HeadshotOnly_Day(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ISpecialDay
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    private HeadshotOnlyConfig Config => SpecialDays.Config.HeadshotOnly;
    private int DelayCooldown = SpecialDays.Config.HeadshotOnly.DelayCooldown;
    public string Name => Core.Localizer["headshot_only_day<name>"];
    public string Description => Core.Localizer["headshot_only_day<description>"];
    private CancellationTokenSource? token = null;
    private bool g_IsTimerActive = false;
    private IDisposable? damageHook = null;
    private HashSet<ushort> AllowedWeaponsDefIndex = new(GetAllowedWeapons());
    private static IEnumerable<ushort> GetAllowedWeapons()
    {
        string weaponName = SpecialDays.Config.HeadshotOnly.Weapon;

        var knifes = new[]
        {
            41,
            42,
            59,
            500,
            503,
            505,
            506,
            507,
            508,
            509,
            512,
            514,
            515,
            516,
            517,
            518,
            519,
            520,
            521,
            522,
            523,
            525,
            526
        };
        if (SpecialDays.WeaponItemDefinitionIndices.TryGetValue(weaponName, out int defIndex))
        {
            yield return (ushort)defIndex;
        }

        foreach (var knife in knifes)
            yield return (ushort)knife;
    }
    public void Start()
    {
        damageHook = Api.Hooks.HookTakeDamage(OnTakeDamage);
        Library.ToggleFriendlyFire(true);
        Api.Utilities.ToggleCells(true, "");

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player.IsFakeClient) continue;
            var jbPlayer = Api.Players.GetPlayer(player);
            if (jbPlayer == null)
                continue;

            jbPlayer.PlayerPawn.ItemServices?.RemoveItems();
            Core.Scheduler.NextTick(() =>
            {
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_knife");
                jbPlayer.PlayerPawn.ItemServices?.GiveItem(Config.Weapon);
            });
        }

        token = Core.Scheduler.RepeatBySeconds(1.0f, () =>
        {
            DelayCooldown--;
            if (DelayCooldown > 0)
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["day_starting", Name, DelayCooldown]);
                g_IsTimerActive = true;
            }
            else
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["prefix"] + Core.Localizer["game_started_message"]);
                token?.Cancel();
                token = null;
                g_IsTimerActive = false;

            }
        });

        Core.Event.OnItemServicesCanAcquireHook += CanAcquireFunc;

    }
    public void CanAcquireFunc(IOnItemServicesCanAcquireHookEvent @event)
    {
        var econItem = @event.EconItemView;
        if (!AllowedWeaponsDefIndex.Contains(econItem.ItemDefinitionIndex))
        {
            @event.SetAcquireResult(AcquireResult.NotAllowedByProhibition);
            return;
        }
    }
    public HookResult OnTakeDamage(DamageHookContext context)
    {
        var info = context.Info;
        if (g_IsTimerActive)
        {
            info.Damage = 0;
            return HookResult.Handled;
        }

        if (info.ActualHitGroup != HitGroup_t.HITGROUP_HEAD || info.DamageType == DamageTypes_t.DMG_SLASH)
        {
            info.Damage = 0;
            return HookResult.Handled;
        }


        return HookResult.Continue;
    }
    public void End()
    {
        Library.ToggleFriendlyFire(false);

        damageHook?.Dispose();
        damageHook = null;
        token?.Cancel();
        token = null;

        Core.Event.OnItemServicesCanAcquireHook -= CanAcquireFunc;
    }
}
public class OneInTheChamber_Day(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ISpecialDay
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    private OneInTheChamberConfig Config => SpecialDays.Config.OneInTheChamber;
    private int DelayCooldown = SpecialDays.Config.OneInTheChamber.DelayCooldown;
    public string Name => Core.Localizer["one_in_the_chamber_day<name>"];
    public string Description => Core.Localizer["one_in_the_chamber_day<description>"];
    private CancellationTokenSource? token = null;
    private bool g_IsTimerActive = false;
    private IDisposable? damageHook = null;
    private HashSet<ushort> AllowedWeaponsDefIndex { get; } = new(GetAllowedWeapons());
    private static IEnumerable<ushort> GetAllowedWeapons()
    {
        var pistols = new[]
        {
            ItemDefinitionIndex.Deagle
        };

        var knifes = new[]
        {
            41,
            42,
            59,
            500,
            503,
            505,
            506,
            507,
            508,
            509,
            512,
            514,
            515,
            516,
            517,
            518,
            519,
            520,
            521,
            522,
            523,
            525,
            526
        };
        foreach (var pistol in pistols)
            yield return (ushort)pistol;

        foreach (var knife in knifes)
            yield return (ushort)knife;
    }
    public void Start()
    {
        damageHook = Api.Hooks.HookTakeDamage(OnTakeDamage);
        Library.ToggleFriendlyFire(true);
        Api.Utilities.ToggleCells(true, "");

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player.IsFakeClient) continue;
            var jbPlayer = Api.Players.GetPlayer(player);
            if (jbPlayer == null)
                continue;

            jbPlayer.PlayerPawn.ItemServices?.RemoveItems();
            Core.Scheduler.NextTick(() =>
            {
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_knife");
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_deagle");

                var activeWeapon = jbPlayer.PlayerPawn.WeaponServices?.ActiveWeapon.Value;
                if (activeWeapon != null)
                {
                    activeWeapon.ReserveAmmo[0] = 0;
                    activeWeapon.Clip1 = 1;
                    activeWeapon.Clip1Updated();
                }
            });
        }

        token = Core.Scheduler.RepeatBySeconds(1.0f, () =>
        {
            DelayCooldown--;
            if (DelayCooldown > 0)
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["day_starting", Name, DelayCooldown]);
                g_IsTimerActive = true;
            }
            else
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["prefix"] + Core.Localizer["game_started_message"]);
                token?.Cancel();
                token = null;
                g_IsTimerActive = false;

            }
        });

        Core.Event.OnItemServicesCanAcquireHook += CanAcquireFunc;
        Core.GameEvent.HookPre<EventPlayerDeath>(OnPlayerDeath);

    }
    public void CanAcquireFunc(IOnItemServicesCanAcquireHookEvent @event)
    {
        var econItem = @event.EconItemView;
        if (!AllowedWeaponsDefIndex.Contains(econItem.ItemDefinitionIndex))
        {
            @event.SetAcquireResult(AcquireResult.NotAllowedByProhibition);
            return;
        }
    }
    public HookResult OnPlayerDeath(EventPlayerDeath @event)
    {
        IPlayer attacker = Core.PlayerManager.GetPlayer(@event.Attacker);
        if (attacker == null)
            return HookResult.Continue;

        if (attacker.IsFakeClient || attacker.PlayerPawn == null)
            return HookResult.Continue;

        IJBPlayer? jbAttacer = Api.Players.GetPlayer(attacker);
        if (jbAttacer == null)
            return HookResult.Continue;

        if (@event.Weapon == "deagle")
            return HookResult.Continue;

        var weapons = jbAttacer.PlayerPawn.WeaponServices?.MyWeapons;
        if (weapons == null)
            return HookResult.Continue;

        foreach (var handle in weapons)
        {
            var weapon = handle.Value;
            if (weapon == null)
                continue;

            Core.Scheduler.NextTick(() =>
            {
                weapon.Clip1 += 1;
                weapon.Clip1Updated();
            });
        }

        return HookResult.Continue;
    }
    public HookResult OnTakeDamage(DamageHookContext context)
    {
        if (g_IsTimerActive)
        {
            context.Info.Damage = 0;
            return HookResult.Handled;
        }

        var activeWeapon = context.Attacker.PlayerPawn?.WeaponServices?.ActiveWeapon.Value;

        if (activeWeapon?.DesignerName == "weapon_deagle")
        {
            context.Info.Damage = 1000;
            Core.Scheduler.NextTick(() =>
            {
                activeWeapon.Clip1 += 1;
                activeWeapon.Clip1Updated();
            });
            return HookResult.Continue;
        }

        return HookResult.Continue;
    }
    public void End()
    {
        Library.ToggleFriendlyFire(false);

        damageHook?.Dispose();
        damageHook = null;
        token?.Cancel();
        token = null;

        Core.Event.OnItemServicesCanAcquireHook -= CanAcquireFunc;
        Core.GameEvent.UnhookPre<EventPlayerDeath>();
    }

}
public class NoScope_Day(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ISpecialDay
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    private NoScopeConfig Config => SpecialDays.Config.NoScope;
    private int DelayCooldown = SpecialDays.Config.NoScope.DelayCooldown;
    public string Name => Core.Localizer["noscope_day<name>"];
    public string Description => Core.Localizer["noscope_day<description>"];
    private CancellationTokenSource? token = null;
    private bool g_IsTimerActive = false;
    private IDisposable? damageHook = null;
    private List<string> ScopeRifles = ["weapon_awp", "weapon_ssg08", "weapon_scar20", "weapon_g3sg1"];
    private HashSet<ushort> NoScopeWeaponsDefIndex { get; } = new(GetAlloweWeapons());
    private static IEnumerable<ushort> GetAlloweWeapons()
    {
        var rifles = new[]
        {
            ItemDefinitionIndex.Awp,
            ItemDefinitionIndex.Ssg08,
            ItemDefinitionIndex.Scar20,
            ItemDefinitionIndex.G3sg1
        };

        foreach (var rifle in rifles)
            yield return (ushort)rifle;
    }

    public void Start()
    {
        damageHook = Api.Hooks.HookTakeDamage(OnTakeDamage);
        Library.ToggleFriendlyFire(true);
        Api.Utilities.ToggleCells(true, "");

        string randomWeapon = ScopeRifles[new Random().Next(ScopeRifles.Count)];

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player.IsFakeClient) continue;
            var jbPlayer = Api.Players.GetPlayer(player);
            if (jbPlayer == null)
                continue;

            jbPlayer.PlayerPawn.ItemServices?.RemoveItems();
            Core.Scheduler.NextTick(() =>
            {
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_knife");
                jbPlayer.PlayerPawn.ItemServices?.GiveItem(randomWeapon);

                Library.SetGravity(jbPlayer, Config.Gravity);
                Library.SetSpeed(jbPlayer, Config.Speed);
            });
        }

        token = Core.Scheduler.RepeatBySeconds(1.0f, () =>
        {
            DelayCooldown--;
            if (DelayCooldown > 0)
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["prefix"] + Core.Localizer["day_starting", Name, DelayCooldown]);
                g_IsTimerActive = true;
            }
            else
            {
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, Core.Localizer["prefix"] + Core.Localizer["game_started_message"]);
                token?.Cancel();
                token = null;
                g_IsTimerActive = false;

            }
        });

        Core.Event.OnTick += OnTick;
        Core.Event.OnItemServicesCanAcquireHook += CanAcquireFunc;
    }
    public void OnTick()
    {
        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player.IsFakeClient) continue;
            var activeWeapon = player.PlayerPawn?.WeaponServices?.ActiveWeapon.Value;
            if (activeWeapon == null)
                return;

            if (ScopeRifles.Contains(activeWeapon.DesignerName))
            {
                activeWeapon.NextSecondaryAttackTick.Value = Core.Engine.GlobalVars.TickCount + 500;
                activeWeapon.NextSecondaryAttackTickUpdated();
            }
        }
    }
    public void CanAcquireFunc(IOnItemServicesCanAcquireHookEvent @event)
    {
        var econItem = @event.EconItemView;
        if (!NoScopeWeaponsDefIndex.Contains(econItem.ItemDefinitionIndex))
        {
            @event.SetAcquireResult(AcquireResult.NotAllowedByProhibition);
            return;
        }

    }
    public HookResult OnTakeDamage(DamageHookContext context)
    {
        if (g_IsTimerActive)
        {
            context.Info.Damage = 0;
            return HookResult.Handled;
        }
        return HookResult.Continue;
    }
    public void End()
    {
        damageHook?.Dispose();
        damageHook = null;

        token?.Cancel();
        token = null;

        Library.ToggleFriendlyFire(false);

        Core.Event.OnTick -= OnTick;
        Core.Event.OnItemServicesCanAcquireHook -= CanAcquireFunc;

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player.IsFakeClient) continue;
            var jbPlayer = Api.Players.GetPlayer(player);
            if (jbPlayer == null)
                continue;

            Core.Scheduler.NextTick(() =>
            {
                Library.SetGravity(jbPlayer, 1.0f);
                Library.SetSpeed(jbPlayer, 1.0f);
            });
        }
    }

}
public class Teleport_Day(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ISpecialDay
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    private int DelayCooldown = SpecialDays.Config.FFA.DelayCooldwon;
    public string Name => Core.Localizer["teleport_day<name>"];
    public string Description => Core.Localizer["teleport_day<description>"];
    private CancellationTokenSource? token = null;
    private bool g_IsTimerActive = false;
    private IDisposable? damageHook = null;

    public void Start()
    {
        damageHook = Api.Hooks.HookTakeDamage(OnTakeDamage);
        Library.ToggleFriendlyFire(true);
        Api.Utilities.ToggleCells(true, "");

        Core.Engine.ExecuteCommand($"sv_teamid_overhead 0");
        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            var jbPlayer = Api.Players.GetPlayer(player);
            if (jbPlayer != null)
                Library.ShowGunsMenu(jbPlayer);
        }


        token = Core.Scheduler.RepeatBySeconds(1.0f, () =>
        {
            DelayCooldown--;
            if (DelayCooldown > 0)
            {
                Core.PlayerManager.SendMessage(MessageType.Chat, Core.Localizer["prefix"] + Core.Localizer["day_starting", Name, DelayCooldown]);
                g_IsTimerActive = true;
            }
            else
            {
                Core.PlayerManager.SendMessage(MessageType.Chat, Core.Localizer["prefix"] + Core.Localizer["game_started_message"]);
                token?.Cancel();
                token = null;
                g_IsTimerActive = false;
            }
        });

        Core.GameEvent.HookPre<EventPlayerHurt>(OnPlayerHurt);
    }
    public HookResult OnPlayerHurt(EventPlayerHurt @event)
    {
        IPlayer victimPlayer = @event.UserIdPlayer;
        IPlayer attackerPlayer = Core.PlayerManager.GetPlayer(@event.Attacker);

        if (victimPlayer == null || attackerPlayer == null || victimPlayer == attackerPlayer)
            return HookResult.Continue;

        if (victimPlayer.IsFakeClient || attackerPlayer.IsFakeClient)
            return HookResult.Continue;

        if (victimPlayer.PlayerPawn == null || attackerPlayer.PlayerPawn == null)
            return HookResult.Continue;

        var jbVictim = Api.Players.GetPlayer(victimPlayer);
        var jbAttacker = Api.Players.GetPlayer(attackerPlayer);

        if (jbVictim == null || jbAttacker == null || jbAttacker == jbVictim)
            return HookResult.Continue;

        CCSPlayerPawn attackerPawn = jbAttacker.PlayerPawn;


        Vector savedAttackerPos = new Vector(attackerPawn.AbsOrigin!.Value.X, attackerPawn.AbsOrigin.Value.Y, attackerPawn.AbsOrigin.Value.Z);
        Vector victimPos = jbVictim.PlayerPawn.AbsOrigin!.Value;

        jbAttacker.Player.Teleport(victimPos, new QAngle(), new Vector());
        jbVictim.Player.Teleport(savedAttackerPos, new QAngle(), new Vector());


        return HookResult.Continue;
    }
    public HookResult OnTakeDamage(DamageHookContext context)
    {
        if (g_IsTimerActive)
        {
            context.Info.Damage = 0;
            return HookResult.Handled;
        }
        return HookResult.Continue;
    }
    public void End()
    {
        damageHook?.Dispose();
        damageHook = null;
        token?.Cancel();
        token = null;

        Library.ToggleFriendlyFire(false);

        Core.Engine.ExecuteCommand($"sv_teamid_overhead 1");
        Core.GameEvent.UnhookPre<EventPlayerHurt>();
    }

}
public class FFA_Day(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ISpecialDay
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    private int DelayCooldown = SpecialDays.Config.FFA.DelayCooldwon;
    public string Name => Core.Localizer["ffa_day<name>"];
    public string Description => Core.Localizer["ffa_day<description>"];
    private CancellationTokenSource? token = null;
    private bool g_IsTimerActive = false;
    private IDisposable? damageHook = null;

    public void Start()
    {
        damageHook = Api.Hooks.HookTakeDamage(OnTakeDamage);
        Library.ToggleFriendlyFire(true);
        Api.Utilities.ToggleCells(true, "");

        Core.Engine.ExecuteCommand($"sv_teamid_overhead 0");
        
        // Delay showing menu slightly to ensure weapons are removed
        Core.Scheduler.DelayBySeconds(0.5f, () =>
        {
            foreach (var player in Core.PlayerManager.GetAllPlayers())
            {
                var jbPlayer = Api.Players.GetPlayer(player);
                if (jbPlayer != null && !player.IsFakeClient)
                    Library.ShowGunsMenu(jbPlayer);
            }
        });

        token = Core.Scheduler.RepeatBySeconds(1.0f, () =>
        {
            DelayCooldown--;
            if (DelayCooldown > 0)
            {
                Core.PlayerManager.SendMessage(MessageType.Chat, Core.Localizer["prefix"] + Core.Localizer["day_starting", Name, DelayCooldown]);
                g_IsTimerActive = true;
            }
            else
            {
                Core.PlayerManager.SendMessage(MessageType.Chat, Core.Localizer["prefix"] + Core.Localizer["game_started_message"]);
                token?.Cancel();
                token = null;
                g_IsTimerActive = false;
            }
        });

    }
    public HookResult OnTakeDamage(DamageHookContext context)
    {
        if (g_IsTimerActive)
        {
            context.Info.Damage = 0;
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }
    public void End()
    {
        damageHook?.Dispose();
        damageHook = null;
        token?.Cancel();
        token = null;

        Library.ToggleFriendlyFire(false);

        Core.Engine.ExecuteCommand($"sv_teamid_overhead 1");
    }
}
