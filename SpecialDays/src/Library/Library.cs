using Jailbreak.Shared;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Convars;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SpecialDays;

public class Library(ISwiftlyCore _core)
{
    private readonly ISwiftlyCore Core = _core;
    public static readonly Dictionary<string, string> GlobalRifles = new()
    {
        { "AK-47", "weapon_ak47" },
        { "AWP", "weapon_awp" },
        { "M4A4", "weapon_m4a1" },
        { "M4A1-S", "weapon_m4a1_silencer" },
        { "SG 553", "weapon_sg553" },
        { "AUG", "weapon_aug" },
        { "SSG 08", "weapon_ssg08" },
        { "Negev", "weapon_negev" },
        { "M249", "weapon_m249" },
        { "FAMAS", "weapon_famas" },
        { "Galil AR", "weapon_galilar" },
        { "MP5-SD", "weapon_mp5sd" },
        { "PP-Bizon", "weapon_bizon" },
        { "UMP-45", "weapon_ump45" },
        { "MP9", "weapon_mp9" },
        { "P90", "weapon_p90" },
        { "MP7", "weapon_mp7" },
        { "MAC-10", "weapon_mac10"},
        { "SG556", "weapon_sg556"},
        { "G3SG1",  "weapon_g3sg1"},
        { "SCAR-20", "weapon_scar20" },
        { "XM1014", "weapon_xm1014"},
        { "MAG-7", "weapon_mag7"},
        { "Sawed-Off", "weapon_sawedoff"},
        { "Nova", "weapon_nova"},
    };
    public static readonly Dictionary<string, string> GlobalPistols = new()
    {
        { "Desert Eagle", "weapon_deagle" },
        { "Glock-18", "weapon_glock" },
        { "P2000", "weapon_hkp2000" },
        { "USP-S", "weapon_usp_silencer" },
        { "TEC-9", "weapon_tec9" },
        { "P250", "weapon_p250" },
        { "CZ75-Auto", "weapon_cz75a" },
        { "Dual Berettas", "weapon_elite" },
        { "Five-SeveN", "weapon_fiveseven" },
        { "R8 Revolver", "weapon_revolver" },
    };

    public void ShowGunsMenu(IJBPlayer player)
    {
        var config = new MenuConfiguration
        {
            Title = Core.Translation.GetPlayerLocalizer(player.Player)["guns_menu<title>"],
            HideTitle = false,
            HideFooter = false,
            PlaySound = true,
            MaxVisibleItems = 5,
            AutoIncreaseVisibleItems = true,
            FreezePlayer = false,
            AutoCloseAfter = 0f
        };
        var keyBinds = new MenuKeybindOverrides
        {
            Select = KeyBind.E,
            Move = KeyBind.S,
            MoveBack = KeyBind.W,
            Exit = KeyBind.Tab
        };
        var menu = Core.MenusAPI.CreateMenu(
            configuration: config,
            keybindOverrides: keyBinds,
            parent: null,
            optionScrollStyle: MenuOptionScrollStyle.CenterFixed,
            optionTextStyle: MenuOptionTextStyle.TruncateEnd

        );

        foreach (var rifle in GlobalRifles)
        {
            var button = new SubmenuMenuOption(rifle.Key, PistolMenu(player, rifle.Value, menu));
            menu.AddOption(button);
        }

        Core.MenusAPI.OpenMenuForPlayer(player.Player, menu);
    }
    private IMenuAPI PistolMenu(IJBPlayer player, string weaponId, IMenuAPI parent)
    {
        var config = new MenuConfiguration
        {
            Title = Core.Translation.GetPlayerLocalizer(player.Player)["guns_menu<title>"],
            HideTitle = false,
            HideFooter = false,
            PlaySound = true,
            MaxVisibleItems = 5,
            AutoIncreaseVisibleItems = true,
            FreezePlayer = false,
            AutoCloseAfter = 0f
        };

        var keyBinds = new MenuKeybindOverrides
        {
            Select = KeyBind.E,
            Move = KeyBind.S,
            MoveBack = KeyBind.W,
            Exit = KeyBind.Tab
        };

        var menu = Core.MenusAPI.CreateMenu(
            configuration: config,
            keybindOverrides: keyBinds,
            parent: parent,
            optionScrollStyle: MenuOptionScrollStyle.CenterFixed,
            optionTextStyle: MenuOptionTextStyle.TruncateEnd
        );

        foreach (var pistol in GlobalPistols)
        {
            var button = new ButtonMenuOption(pistol.Key);
            button.Click += async (sender, args) =>
            {
                var pawn = player.Pawn;
                pawn.ItemServices?.RemoveItems();

                Core.Scheduler.NextTick(() =>
                {
                    pawn.ItemServices?.GiveItem<CBaseEntity>(pistol.Value);
                    pawn.ItemServices?.GiveItem<CBaseEntity>(weaponId);
                });
            };

            menu.AddOption(button);
        }

        return menu;
    }
    public void SetHealth(IJBPlayer player, int health)
    {
        player.PlayerPawn.MaxHealth = health;
        player.PlayerPawn.Health = health;

        player.PlayerPawn.HealthUpdated();
        player.PlayerPawn.MaxHealthUpdated();
    }
    public void SetGravity(IJBPlayer player, float gravity)
    {
        player.PlayerPawn.GravityScale = gravity;
        player.PlayerPawn.ActualGravityScale = gravity;

        player.PlayerPawn.GravityScaleUpdated();
    }
    public void SetSpeed(IJBPlayer player, float speed)
    {
        player.PlayerPawn.VelocityModifier = speed;
        player.PlayerPawn.VelocityModifierUpdated();
    }
    public void ToggleFriendlyFire(bool state)
    {
        IConVar<bool>? teammatesAreEnemies = Core.ConVar.Find<bool>("mp_teammates_are_enemies");
        teammatesAreEnemies?.SetInternal(state);

        int value = state ? 0 : 1;

        Core.Engine.ExecuteCommand($"sv_teamid_overhead {value}");
    }
    public void Freeze(IJBPlayer player, bool value)
    {
        if (value)
        {
            player.PlayerPawn.MoveType = MoveType_t.MOVETYPE_OBSOLETE;
            player.PlayerPawn.ActualMoveType = MoveType_t.MOVETYPE_OBSOLETE;
            player.PlayerPawn.MoveCollideUpdated();
        }
        else
        {
            player.PlayerPawn.MoveType = MoveType_t.MOVETYPE_WALK;
            player.PlayerPawn.ActualMoveType = MoveType_t.MOVETYPE_WALK;
            player.PlayerPawn.MoveCollideUpdated();
        }
    }
    public IMenuAPI CreateMenu(string title, IMenuAPI? parent)
    {
        var config = new MenuConfiguration
        {
            Title = title,
            HideTitle = false,
            HideFooter = false,
            PlaySound = true,
            MaxVisibleItems = 5,
            AutoIncreaseVisibleItems = true,
            FreezePlayer = false,
            AutoCloseAfter = 0f
        };

        var keyBinds = new MenuKeybindOverrides
        {
            Select = KeyBind.E,
            Move = KeyBind.S,
            MoveBack = KeyBind.W,
            Exit = KeyBind.Tab
        };

        var menu = _core.MenusAPI.CreateMenu(
            configuration: config,
            keybindOverrides: keyBinds,
            parent: parent,
            optionScrollStyle: MenuOptionScrollStyle.CenterFixed,
            optionTextStyle: MenuOptionTextStyle.TruncateEnd
        );

        return menu;
    }
}
