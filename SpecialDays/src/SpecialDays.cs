using Jailbreak.Shared;
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
            .InitializeTomlWithModel<DaysConfig>("config.toml", "SpecialDays")
            .Configure(builder =>
            {
                builder.AddTomlFile("config.toml", false, true);
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

        if (Config.FreezeTag.Enable)
            Api.SpecialDay.Register(new FreezeTag_Day(Core, Api, Library));
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

        if (Config.FreezeTag.Enable)
            Api.SpecialDay.Unregister(new FreezeTag_Day(Core, Api, Library));
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

                Core.PlayerManager.SendMessage(MessageType.CenterHTML, "");
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

                Core.PlayerManager.SendMessage(MessageType.CenterHTML, "");
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
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, "");
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
                List<IPlayer> terrorists = Core.PlayerManager.GetAllPlayers().Where(p => p.Controller.TeamNum == (int)Team.T).ToList();

                List<IPlayer> cts = Core.PlayerManager.GetAllPlayers().Where(p => p.Controller.TeamNum == (int)Team.CT).ToList();

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

        foreach (var terrorist in Core.PlayerManager.GetAllPlayers().Where(t => t.Controller.TeamNum == (int)Team.T))
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
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, "");
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
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, "");
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
public class FreezeTag_Day(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ISpecialDay
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    private FreezeTagConfig Config => SpecialDays.Config.FreezeTag;
    private int DelayCooldown = SpecialDays.Config.FreezeTag.DelayCooldown;
    public string Name => Core.Localizer["freeze_tag_day<name>"];
    public string Description => Core.Localizer["freeze_tag_day<description>"];
    private CancellationTokenSource? token = null;
    private bool g_IsTimerActive = false;
    private IDisposable? damageHook = null;
    
    // Track frozen prisoners
    private Dictionary<IJBPlayer, bool> frozenPrisoners = new();
    private Dictionary<IJBPlayer, (IJBPlayer unfreezer, float startTime)> unfreezingPrisoners = new();

    public void Start()
    {
        damageHook = Api.Hooks.HookTakeDamage(OnTakeDamage);
        Api.Utilities.ToggleCells(true, "");
        
        frozenPrisoners.Clear();
        unfreezingPrisoners.Clear();

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            var jbPlayer = Api.Players.GetPlayer(player);
            if (jbPlayer == null)
                continue;

            jbPlayer.PlayerPawn.ItemServices?.RemoveItems();
            Core.Scheduler.NextTick(() =>
            {
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_knife");
            });

            if (jbPlayer.Role == IJBRole.Prisoner)
            {
                frozenPrisoners[jbPlayer] = false;
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
                Core.PlayerManager.SendMessage(MessageType.CenterHTML, "");
                token?.Cancel();
                token = null;
                g_IsTimerActive = false;
                
                StartUnfreezeCheckTimer();
            }
        });

        Core.Event.OnItemServicesCanAcquireHook += CanAcquireFunc;
    }

    private void StartUnfreezeCheckTimer()
    {
        Core.Scheduler.RepeatBySeconds(0.1f, () =>
        {
            if (g_IsTimerActive)
                return;

            CheckUnfreezing();
            CheckWinCondition();
        });
    }

    private void CheckUnfreezing()
    {
        var currentTime = Core.Engine.GlobalVars.CurrentTime;
        List<IJBPlayer> toUnfreeze = new();

        foreach (var (frozen, data) in unfreezingPrisoners.ToList())
        {
            if (currentTime - data.startTime >= Config.UnfreezeTime)
            {
                toUnfreeze.Add(frozen);
            }
            else
            {
                var unfreezer = data.unfreezer;
                if (!unfreezer.IsValid || unfreezer.PlayerPawn == null || frozen.PlayerPawn == null)
                {
                    unfreezingPrisoners.Remove(frozen);
                    continue;
                }

                var distance = CalculateDistance(unfreezer.PlayerPawn.AbsOrigin!.Value, frozen.PlayerPawn.AbsOrigin!.Value);
                if (distance > Config.UnfreezeRadius)
                {
                    unfreezingPrisoners.Remove(frozen);
                    Api.Utilities.PrintToChatAll($"{unfreezer.Controller.PlayerName} stopped unfreezing {frozen.Controller.PlayerName}", false, IPrefix.JB);
                }
            }
        }
        foreach (var frozen in toUnfreeze)
        {
            UnfreezePrisoner(frozen);
            unfreezingPrisoners.Remove(frozen);
        }

        foreach (var prisoner in frozenPrisoners.Keys.ToList())
        {
            if (!frozenPrisoners[prisoner] || !prisoner.IsValid)
                continue;
            if (unfreezingPrisoners.ContainsKey(prisoner))
                continue;

            foreach (var helper in frozenPrisoners.Keys.Where(p => !frozenPrisoners[p] && p.IsValid && p.PlayerPawn != null))
            {
                if (prisoner.PlayerPawn == null || helper.PlayerPawn == null)
                    continue;

                var distance = CalculateDistance(helper.PlayerPawn.AbsOrigin!.Value, prisoner.PlayerPawn.AbsOrigin!.Value);
                
                if (distance <= Config.UnfreezeRadius)
                {
                    unfreezingPrisoners[prisoner] = (helper, currentTime);
                    Api.Utilities.PrintToChatAll($"{helper.Controller.PlayerName} is unfreezing {prisoner.Controller.PlayerName}!", false, IPrefix.JB);
                    break;
                }
            }
        }
    }

    private void CheckWinCondition()
    {
        var alivePrisoners = frozenPrisoners.Where(kvp => kvp.Key.IsValid && kvp.Key.Controller.PawnIsAlive).ToList();
        
        if (alivePrisoners.Count == 0)
            return;

        var allFrozen = alivePrisoners.All(kvp => kvp.Value);
        
        if (allFrozen)
        {
            Api.Utilities.PrintToChatAll("All prisoners are frozen! Guards win!", true, IPrefix.JB);
            
            foreach (var prisoner in alivePrisoners.Select(kvp => kvp.Key))
            {
                if (prisoner.IsValid && prisoner.PlayerPawn != null)
                {
                    prisoner.PlayerPawn.CommitSuicide(false, true);
                }
            }
            
            Api.SpecialDay.EndActive();
        }
    }

    private float CalculateDistance(Vector pos1, Vector pos2)
    {
        float dx = pos1.X - pos2.X;
        float dy = pos1.Y - pos2.Y;
        float dz = pos1.Z - pos2.Z;
        return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public HookResult OnTakeDamage(DamageHookContext context)
    {
        if (g_IsTimerActive)
        {
            context.Info.Damage = 0;
            return HookResult.Handled;
        }

        var attacker = context.Attacker;
        var victim = context.Victim;

        if (attacker.Role == IJBRole.Guardian && victim.Role == IJBRole.Prisoner)
        {
            if (frozenPrisoners.ContainsKey(victim) && !frozenPrisoners[victim])
            {
                FreezePrisoner(victim);
                Api.Utilities.PrintToChatAll($"{attacker.Controller.PlayerName} froze {victim.Controller.PlayerName}!", false, IPrefix.JB);
            }
            
            context.Info.Damage = 0;
            return HookResult.Handled;
        }

        context.Info.Damage = 0;
        return HookResult.Handled;
    }

    private void FreezePrisoner(IJBPlayer prisoner)
    {
        if (!prisoner.IsValid)
            return;

        frozenPrisoners[prisoner] = true;
        Library.Freeze(prisoner, true);
        
        prisoner.PlayerPawn.RenderMode = RenderMode_t.kRenderTransColor;
        prisoner.PlayerPawn.Render = new SwiftlyS2.Shared.Natives.Color(100, 150, 255, 150);
    }

    private void UnfreezePrisoner(IJBPlayer prisoner)
    {
        if (!prisoner.IsValid)
            return;

        frozenPrisoners[prisoner] = false;
        Library.Freeze(prisoner, false);
        
        prisoner.PlayerPawn.RenderMode = RenderMode_t.kRenderNormal;
        prisoner.PlayerPawn.Render = new SwiftlyS2.Shared.Natives.Color(255, 255, 255, 255);
        
        Api.Utilities.PrintToChatAll($"{prisoner.Controller.PlayerName} has been unfrozen!", false, IPrefix.JB);
    }

    public void CanAcquireFunc(IOnItemServicesCanAcquireHookEvent @event)
    {
        var econItem = @event.EconItemView;
        
        var knifes = new[]
        {
            41, 42, 59, 500, 503, 505, 506, 507, 508, 509, 512, 514, 515, 516, 517, 518, 519, 520, 521, 522, 523, 525, 526
        };

        if (!knifes.Contains(econItem.ItemDefinitionIndex))
        {
            @event.SetAcquireResult(AcquireResult.NotAllowedByProhibition);
        }
    }

    public void End()
    {
        damageHook?.Dispose();
        damageHook = null;
        
        token?.Cancel();
        token = null;

        Core.Event.OnItemServicesCanAcquireHook -= CanAcquireFunc;

        foreach (var prisoner in frozenPrisoners.Keys.ToList())
        {
            if (prisoner.IsValid && frozenPrisoners[prisoner])
            {
                Library.Freeze(prisoner, false);
                prisoner.PlayerPawn.RenderMode = RenderMode_t.kRenderNormal;
                prisoner.PlayerPawn.Render = new SwiftlyS2.Shared.Natives.Color(255, 255, 255, 255);
            }
        }

        frozenPrisoners.Clear();
        unfreezingPrisoners.Clear();
    }
}
