using Jailbreak.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Plugins;
using Tomlyn.Extensions.Configuration;

namespace LastRequests;

[PluginMetadata(Id = "LastRequests", Version = "1.0.0", Name = "LastRequests", Author = "T3Marius", Description = "No description.")]
public partial class LastRequests : BasePlugin
{
    private ServiceProvider? _provider;
    public static IJailbreakApi Api = null!;
    public static LRConfig Config { get; set; } = new();
    public static Library Library { get; set; } = null!;
    public LastRequests(ISwiftlyCore core) : base(core)
    {
    }
    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
        Api = interfaceManager.GetSharedInterface<IJailbreakApi>("Jailbreak.Core");
    }
    public override void Load(bool hotReload)
    {
        Core.Configuration
            .InitializeTomlWithModel<LRConfig>("config.toml", "LastRequests")
            .Configure(builder =>
            {
                builder.AddTomlFile("config.toml", false, true);
            });

        ServiceCollection services = new();
        services.AddSwiftly(Core)
                .AddSingleton<Library>()
                .AddOptionsWithValidateOnStart<LRConfig>().BindConfiguration("LastRequests");

        _provider = services.BuildServiceProvider();
        Config = _provider.GetRequiredService<IOptions<LRConfig>>().Value;
        Library = _provider.GetRequiredService<Library>();

        Register();
    }

    public override void Unload()
    {
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
    private void Register()
    {
        if (Config.KnifeFight.Enable)
            Api.LastRequest.Register(new KnifeFight(Core, Api, Library));

        if (Config.GrenadeWar.Enable)
            Api.LastRequest.Register(new GrenadeWar(Core, Api, Library));

        if (Config.RussianRoulette.Enable)
            Api.LastRequest.Register(new RussianRoulette(Core, Api, Library));
    }
}
public class KnifeFight(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ILastRequest
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    public string Name => Core.Localizer["knife_fight_lr<name>"];
    public string Description => string.Empty;

    public IJBPlayer? Prisoner { get; set; } = null;
    public IJBPlayer? Guardian { get; set; } = null;

    public string SelectedWeaponName { get; set; } = string.Empty;
    public string SelectedWeaponID { get; set; } = string.Empty;

    public IReadOnlyList<(string DisplayName, string ClassName)> GetAvailableWeapons() =>
        new List<(string, string)>
        {
          ("Knife", "weapon_knife") // "Knife" is shown in the menu and "weapon_knife" is given to the players.
        };

    public string? SelectedType { get; set; } = string.Empty;
    public IReadOnlyList<string> GetAvailableTypes() => new List<string> { "Normal", "Gravity", "Speed", "OneShot" };
    public bool IsPrepTimerActive { get; set; }
    public bool IsOneShotEnable = false;
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
    public void Start(IJBPlayer guardian, IJBPlayer prisoner)
    {
        Guardian = guardian;
        Prisoner = prisoner;


    }
    public void End(IJBPlayer? winner, IJBPlayer? loser)
    {

    }
}
public class GrenadeWar(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ILastRequest
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    private GrenadeWar_LR Config => LastRequests.Config.GrenadeWar;

    public string Name => Core.Localizer["grenade_war_lr<name>"];
    public string Description => string.Empty;

    public IJBPlayer? Prisoner { get; set; } = null;
    public IJBPlayer? Guardian { get; set; } = null;

    public string SelectedWeaponName { get; set; } = string.Empty;
    public string SelectedWeaponID { get; set; } = string.Empty;

    public IReadOnlyList<(string DisplayName, string ClassName)> GetAvailableWeapons() =>
        new List<(string, string)>
        {
            ("HE Grenade", "weapon_hegrenade") // only he are allowed for now!
        };

    public string? SelectedType { get; set; } = string.Empty;
    public IReadOnlyList<string> GetAvailableTypes() => new List<string>(); 
    public bool IsPrepTimerActive { get; set; }

    private HashSet<ushort> AllowedWeaponsDefIndex = new();
    private int grenadesThrown = 0;
    private bool canThrow = true;

    public void Start(IJBPlayer guardian, IJBPlayer prisoner)
    {
        Guardian = guardian;
        Prisoner = prisoner;

        if (Guardian == null || Prisoner == null)
            return;
        grenadesThrown = 0;
        canThrow = true;

        if (LastRequests.WeaponItemDefinitionIndices.TryGetValue(SelectedWeaponID, out int defIndex))
        {
            AllowedWeaponsDefIndex.Add((ushort)defIndex);
        }
        Guardian.StripWeapons(keepKnife: false);
        Prisoner.StripWeapons(keepKnife: false);

        Core.Scheduler.NextTick(() =>
        {
            for (int i = 0; i < Config.GrenadeCount; i++)
            {
                Guardian.PlayerPawn.ItemServices?.GiveItem(SelectedWeaponID);
                Prisoner.PlayerPawn.ItemServices?.GiveItem(SelectedWeaponID);
            }
        });

        Core.Event.OnItemServicesCanAcquireHook += OnCanAcquire;

        Core.GameEvent.HookPost<EventGrenadeThrown>(OnGrenadeThrown);

        Api.Utilities.PrintToChatAll($"grenade_war_started", true, IPrefix.LR, Guardian.Controller.PlayerName, Prisoner.Controller.PlayerName, SelectedWeaponName);
    }

    public void End(IJBPlayer? winner, IJBPlayer? loser)
    {
        Core.Event.OnItemServicesCanAcquireHook -= OnCanAcquire;
        Core.GameEvent.UnhookPost<EventGrenadeThrown>();

        if (Guardian != null && Guardian.IsValid)
        {
            Guardian.StripWeapons(keepKnife: true);
        }

        if (Prisoner != null && Prisoner.IsValid)
        {
            Prisoner.StripWeapons(keepKnife: true);
        }
    }

    private void OnCanAcquire(IOnItemServicesCanAcquireHookEvent @event)
    {
        var econItem = @event.EconItemView;
        if (!AllowedWeaponsDefIndex.Contains(econItem.ItemDefinitionIndex))
        {
            @event.SetAcquireResult(AcquireResult.NotAllowedByProhibition);
        }
    }

    private HookResult OnGrenadeThrown(EventGrenadeThrown @event)
    {
        var thrower = @event.UserIdPlayer;
        if (thrower == null)
            return HookResult.Continue;

        var jbThrower = Api.Players.GetPlayer(thrower);
        if (jbThrower == null || (jbThrower != Guardian && jbThrower != Prisoner))
            return HookResult.Continue;

        grenadesThrown++;

        if (grenadesThrown % 2 == 0)
        {
            // delay for next throw!
            canThrow = false;
            Core.Scheduler.DelayBySeconds(Config.ThrowDelay, () =>
            {
                canThrow = true;
            });
        }

        return HookResult.Continue;
    }
}
public class RussianRoulette(ISwiftlyCore _core, IJailbreakApi _api, Library _library) : ILastRequest
{
    private readonly ISwiftlyCore Core = _core;
    private readonly IJailbreakApi Api = _api;
    private readonly Library Library = _library;
    private RussianRoulette_LR Config => LastRequests.Config.RussianRoulette;

    public string Name => Core.Localizer["russian_roulette_lr<name>"];
    public string Description => string.Empty;

    public IJBPlayer? Prisoner { get; set; } = null;
    public IJBPlayer? Guardian { get; set; } = null;

    public string SelectedWeaponName { get; set; } = string.Empty;
    public string SelectedWeaponID { get; set; } = string.Empty;

    public IReadOnlyList<(string DisplayName, string ClassName)> GetAvailableWeapons() =>
        new List<(string, string)>
        {
            ("Revolver", "weapon_revolver")
        };

    public string? SelectedType { get; set; } = string.Empty;
    public IReadOnlyList<string> GetAvailableTypes() => new List<string>();
    public bool IsPrepTimerActive { get; set; }

    private int currentRound = 0;
    private int bulletPosition = 0;
    private bool isGuardianTurn = true;
    private CancellationTokenSource? turnTimer = null;

    public void Start(IJBPlayer guardian, IJBPlayer prisoner)
    {
        Guardian = guardian;
        Prisoner = prisoner;

        if (Guardian == null || Prisoner == null)
            return;

        currentRound = 0;
        bulletPosition = new Random().Next(1, Config.MaxRounds + 1);
        isGuardianTurn = new Random().Next(2) == 0;

        Guardian.StripWeapons(keepKnife: false);
        Prisoner.StripWeapons(keepKnife: false);

        Core.Scheduler.NextTick(() =>
        {
            Guardian.PlayerPawn.ItemServices?.GiveItem("weapon_revolver");
            Prisoner.PlayerPawn.ItemServices?.GiveItem("weapon_revolver");

            SetRevolverAmmo(Guardian, 1);
            SetRevolverAmmo(Prisoner, 1);
        });

        Api.Utilities.PrintToChatAll($"russian_roulette_started", true, IPrefix.LR, Guardian.Controller.PlayerName, Prisoner.Controller.PlayerName);
        Api.Utilities.PrintToChatAll($"Bullet is in chamber {bulletPosition}/{Config.MaxRounds}", false, IPrefix.LR);

        StartTurn();
    }

    private void SetRevolverAmmo(IJBPlayer player, int ammo)
    {
        var weapons = player.PlayerPawn.WeaponServices?.MyWeapons;
        if (weapons == null) return;

        foreach (var handle in weapons)
        {
            var weapon = handle.Value;
            if (weapon?.DesignerName == "weapon_revolver")
            {
                weapon.Clip1 = ammo;
                weapon.Clip1Updated();
                weapon.ReserveAmmo[0] = 0;
            }
        }
    }

    private void StartTurn()
    {
        currentRound++;

        if (currentRound > Config.MaxRounds)
        {
            Api.Utilities.PrintToChatAll("Russian Roulette ended in a tie!", true, IPrefix.LR);
            Api.LastRequest.EndActive();
            return;
        }

        var currentPlayer = isGuardianTurn ? Guardian : Prisoner;
        var otherPlayer = isGuardianTurn ? Prisoner : Guardian;

        if (currentPlayer == null || !currentPlayer.IsValid)
        {
            Api.LastRequest.EndActive();
            return;
        }

        Api.Utilities.PrintToChatAll($"Round {currentRound}: {currentPlayer.Controller.PlayerName}'s turn!", true, IPrefix.LR);

        turnTimer = Core.Scheduler.DelayBySeconds(Config.TurnDelay, () =>
        {
            if (currentRound == bulletPosition)
            {
                if (currentPlayer.IsValid && currentPlayer.PlayerPawn != null)
                {
                    currentPlayer.PlayerPawn.CommitSuicide(false, true);
                    Api.Utilities.PrintToChatAll($"{currentPlayer.Controller.PlayerName} lost Russian Roulette!", true, IPrefix.LR);
                }
                Api.LastRequest.EndActive();
            }
            else
            {
                Api.Utilities.PrintToChatAll($"{currentPlayer.Controller.PlayerName} survived! Click!", false, IPrefix.LR);
                isGuardianTurn = !isGuardianTurn;
                StartTurn();
            }
        });
    }

    public void End(IJBPlayer? winner, IJBPlayer? loser)
    {
        turnTimer?.Cancel();
        turnTimer = null;

        if (Guardian != null && Guardian.IsValid)
        {
            Guardian.StripWeapons(keepKnife: true);
        }

        if (Prisoner != null && Prisoner.IsValid)
        {
            Prisoner.StripWeapons(keepKnife: true);
        }
    }
}
