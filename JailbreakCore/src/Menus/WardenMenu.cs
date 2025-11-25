using Jailbreak.Shared;
using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace JailbreakCore;

public class WardenMenu(ISwiftlyCore _core, Extensions _extensions)
{
    private readonly ISwiftlyCore _Core = _core;
    private readonly Extensions _Extensions = _extensions;

    public void Display(JBPlayer player)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["warden_menu<title>"]);

        var toggleCells = new ToggleMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["toggle_cells<option>"]);
        toggleCells.Click += async (sender, args) =>
        {
            var isToggled = toggleCells.GetDisplayText(args.Player, 0).Contains("✔");
            JailbreakCore.Extensions.ToggleCells(isToggled, args.Player.Controller.PlayerName);
        };

        var freedaySubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["add_freeday<option>"], FreedayMenu(player, menu));
        var colorSubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["set_color<option>"], ColorSelectPlayerMenu(player, menu));
        var gamesSubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["games<option>"], GamesMenu(player, menu));
        var toolsSubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["tools<option>"], ToolsMenu(player, menu));
        var weaponSubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["weapon_menu<option>"], WeaponMenu(player, menu));

        menu.AddOption(toggleCells);
        menu.AddOption(freedaySubMenu);
        menu.AddOption(gamesSubMenu);
        menu.AddOption(toolsSubMenu);
        menu.AddOption(weaponSubMenu);
        //menu.AddOption(colorSubMenu);

        _Core.MenusAPI.OpenMenuForPlayer(player.Player, menu);
    }
    private IMenuAPI ColorSelectPlayerMenu(JBPlayer player, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["select_player<title>"], parent);

        List<JBPlayer> prisoners = JailbreakCore.JBPlayerManagement.GetAllPrisoners(excludeRebels: true, exlcudeFreedays: true);
        if (prisoners.Count == 0)
        {
            menu.AddOption(new TextMenuOption("No valid prisoner!"));
        }

        foreach (var prisoner in prisoners)
        {
            var setColorSubMenu = new SubmenuMenuOption(prisoner.Controller.PlayerName, SetColorMenu(player, parent, prisoner));
            menu.AddOption(setColorSubMenu);
        }

        return menu;
    }
    private IMenuAPI SetColorMenu(JBPlayer player, IMenuAPI parent, JBPlayer prisoner)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["select_color<title>"], parent);

        var inputOption = new InputMenuOption(16, (value) => !string.IsNullOrWhiteSpace(value) && value.Length >= 3, "blue...red..etc..", "red");

        inputOption.ValueChanged += (sender, args) =>
        {
            var color = System.Drawing.Color.FromName(inputOption.GetValue(args.Player));
            prisoner.SetColor(Color.FromBuiltin(color));
            inputOption.SetValue(args.Player, color.ToString());
        };

        menu.AddOption(inputOption);

        return menu;
    }
    private IMenuAPI FreedayMenu(JBPlayer player, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["select_player<title>"], parent);

        List<JBPlayer> prisoners = JailbreakCore.JBPlayerManagement.GetAllPrisoners(excludeRebels: true, exlcudeFreedays: true);
        if (prisoners.Count == 0)
        {
            menu.AddOption(new TextMenuOption("No valid prisoner!"));
        }
        foreach (var prisoner in prisoners)
        {
            var option = new ButtonMenuOption(prisoner.Controller.PlayerName);
            option.Click += async (sender, args) =>
            {
                prisoner.SetFreeday(true);
                JailbreakCore.Extensions.PrintToChatAll("freeday_given", showPrefix: true, IPrefix.JB, prisoner.Controller.PlayerName);
            };

            menu.AddOption(option);
        }

        return menu;
    }

    private IMenuAPI GamesMenu(JBPlayer player, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["games_menu<title>"], parent);

        var boxMenuSubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["box_menu<option>"], BoxMenu(player, menu));
        menu.AddOption(boxMenuSubMenu);

        // Stop Game option
        if (JailbreakCore.SpecialDay.IsGameActive())
        {
            var stopGameOption = new ButtonMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["stop_game<option>"]);
            stopGameOption.Click += async (sender, args) =>
            {
                JailbreakCore.SpecialDay.StopGame(player);
                _Core.MenusAPI.CloseMenuForPlayer(args.Player, menu);
            };
            menu.AddOption(stopGameOption);
        }

        IReadOnlyList<ISpecialDay> Days = JailbreakCore.SpecialDay.GetAllDays();

        if (Days.Count == 0)
        {
            menu.AddOption(new TextMenuOption("No games available!"));
        }

        foreach (var day in Days)
        {
            var option = new ButtonMenuOption(day.Name);
            option.Click += async (sender, args) =>
            {
                JailbreakCore.SpecialDay.Select(player, day.Name);
                _Core.MenusAPI.CloseMenuForPlayer(args.Player, menu);
            };
            menu.AddOption(option);
        }

        return menu;
    }

    private IMenuAPI BoxMenu(JBPlayer player, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["box_menu<title>"], parent);

        var toggleBoxAll = new ToggleMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["toggle_box_all<option>"]);
        toggleBoxAll.Click += async (sender, args) =>
        {
            var isToggled = toggleBoxAll.GetDisplayText(args.Player, 0).Contains("✔");
            JailbreakCore.Extensions.ToggleBox(isToggled, args.Player.Controller.PlayerName);
        };

        var giveBoxSubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["give_box<option>"], GiveBoxPlayerMenu(player, menu));
        var removeBoxSubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["remove_box<option>"], RemoveBoxPlayerMenu(player, menu));

        menu.AddOption(toggleBoxAll);
        menu.AddOption(giveBoxSubMenu);
        menu.AddOption(removeBoxSubMenu);

        return menu;
    }

    private IMenuAPI GiveBoxPlayerMenu(JBPlayer player, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["select_player<title>"], parent);

        foreach (var jbPlayer in JailbreakCore.JBPlayerManagement.GetAllPlayers())
        {
            if (jbPlayer.Controller.IsHLTV || jbPlayer.Player.IsFakeClient)
                continue;

            var option = new ButtonMenuOption(jbPlayer.Controller.PlayerName);
            option.Click += async (sender, args) =>
            {
                JailbreakCore.Extensions.GiveBox(jbPlayer, player.Controller.PlayerName);
                _Core.MenusAPI.CloseMenuForPlayer(args.Player, menu);
            };

            menu.AddOption(option);
        }

        return menu;
    }

    private IMenuAPI RemoveBoxPlayerMenu(JBPlayer player, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["select_player<title>"], parent);

        foreach (var jbPlayer in JailbreakCore.JBPlayerManagement.GetAllPlayers())
        {
            if (jbPlayer.Controller.IsHLTV || jbPlayer.Player.IsFakeClient)
                continue;

            var option = new ButtonMenuOption(jbPlayer.Controller.PlayerName);
            option.Click += async (sender, args) =>
            {
                JailbreakCore.Extensions.RemoveBox(jbPlayer, player.Controller.PlayerName);
                _Core.MenusAPI.CloseMenuForPlayer(args.Player, menu);
            };

            menu.AddOption(option);
        }

        return menu;
    }

    private IMenuAPI ToolsMenu(JBPlayer player, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["tools_menu<title>"], parent);

        var countdownSubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["countdown_options<option>"], CountdownMenu(player, menu));
        var microphoneSubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["microphone_options<option>"], MicrophoneMenu(player, menu));

        menu.AddOption(countdownSubMenu);
        menu.AddOption(microphoneSubMenu);

        return menu;
    }

    private IMenuAPI CountdownMenu(JBPlayer player, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["countdown_menu<title>"], parent);

        var countdown5 = new ButtonMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["countdown_5<option>"]);
        countdown5.Click += async (sender, args) =>
        {
            _Core.MenusAPI.CloseMenuForPlayer(args.Player, menu);
            JailbreakCore.Extensions.PlayCountdown(5);
        };

        var countdown10 = new ButtonMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["countdown_10<option>"]);
        countdown10.Click += async (sender, args) =>
        {
            _Core.MenusAPI.CloseMenuForPlayer(args.Player, menu);
            JailbreakCore.Extensions.PlayCountdown(10);
        };

        menu.AddOption(countdown5);
        menu.AddOption(countdown10);

        return menu;
    }

    private IMenuAPI MicrophoneMenu(JBPlayer player, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["microphone_menu<title>"], parent);

        var toggleMicAll = new ToggleMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["toggle_mic_all<option>"]);
        toggleMicAll.Click += async (sender, args) =>
        {
            var isToggled = toggleMicAll.GetDisplayText(args.Player, 0).Contains("✔");
            JailbreakCore.Extensions.ToggleMicrophone(isToggled, args.Player.Controller.PlayerName);
        };

        var giveMicSubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["give_mic<option>"], GiveMicrophonePlayerMenu(player, menu));
        var removeMicSubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["remove_mic<option>"], RemoveMicrophonePlayerMenu(player, menu));

        menu.AddOption(toggleMicAll);
        menu.AddOption(giveMicSubMenu);
        menu.AddOption(removeMicSubMenu);

        return menu;
    }

    private IMenuAPI GiveMicrophonePlayerMenu(JBPlayer player, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["select_player<title>"], parent);

        foreach (var jbPlayer in JailbreakCore.JBPlayerManagement.GetAllPlayers())
        {
            if (jbPlayer.Controller.IsHLTV || jbPlayer.Player.IsFakeClient)
                continue;

            // Only show prisoners (Team.T = 2), skip warden and CTs
            if (jbPlayer.Controller.TeamNum != 2)
                continue;

            var option = new ButtonMenuOption(jbPlayer.Controller.PlayerName);
            option.Click += async (sender, args) =>
            {
                JailbreakCore.Extensions.GiveMicrophone(jbPlayer, player.Controller.PlayerName);
                _Core.MenusAPI.CloseMenuForPlayer(args.Player, menu);
            };

            menu.AddOption(option);
        }

        return menu;
    }

    private IMenuAPI RemoveMicrophonePlayerMenu(JBPlayer player, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["select_player<title>"], parent);

        foreach (var jbPlayer in JailbreakCore.JBPlayerManagement.GetAllPlayers())
        {
            if (jbPlayer.Controller.IsHLTV || jbPlayer.Player.IsFakeClient)
                continue;

            // Only show prisoners (Team.T = 2), skip warden and CTs
            if (jbPlayer.Controller.TeamNum != 2)
                continue;

            var option = new ButtonMenuOption(jbPlayer.Controller.PlayerName);
            option.Click += async (sender, args) =>
            {
                JailbreakCore.Extensions.RemoveMicrophone(jbPlayer, player.Controller.PlayerName);
                _Core.MenusAPI.CloseMenuForPlayer(args.Player, menu);
            };

            menu.AddOption(option);
        }

        return menu;
    }

    private IMenuAPI WeaponMenu(JBPlayer player, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["weapon_menu<title>"], parent);

        var m4a1s = new ButtonMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["weapon_m4a1s<option>"]);
        m4a1s.Click += async (sender, args) =>
        {
            var jbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(args.Player);
            jbPlayer.PlayerPawn.ItemServices?.RemoveItems();
            _Core.Scheduler.NextTick(() =>
            {
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_knife");
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_m4a1_silencer");
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_deagle");
            });
            _Core.MenusAPI.CloseMenuForPlayer(args.Player, menu);
        };

        var m4a4 = new ButtonMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["weapon_m4a4<option>"]);
        m4a4.Click += async (sender, args) =>
        {
            var jbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(args.Player);
            jbPlayer.PlayerPawn.ItemServices?.RemoveItems();
            _Core.Scheduler.NextTick(() =>
            {
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_knife");
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_m4a1");
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_deagle");
            });
            _Core.MenusAPI.CloseMenuForPlayer(args.Player, menu);
        };

        var ak47 = new ButtonMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["weapon_ak47<option>"]);
        ak47.Click += async (sender, args) =>
        {
            var jbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(args.Player);
            jbPlayer.PlayerPawn.ItemServices?.RemoveItems();
            _Core.Scheduler.NextTick(() =>
            {
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_knife");
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_ak47");
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_deagle");
            });
            _Core.MenusAPI.CloseMenuForPlayer(args.Player, menu);
        };

        var awp = new ButtonMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["weapon_awp<option>"]);
        awp.Click += async (sender, args) =>
        {
            var jbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(args.Player);
            jbPlayer.PlayerPawn.ItemServices?.RemoveItems();
            _Core.Scheduler.NextTick(() =>
            {
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_knife");
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_awp");
                jbPlayer.PlayerPawn.ItemServices?.GiveItem("weapon_deagle");
            });
            _Core.MenusAPI.CloseMenuForPlayer(args.Player, menu);
        };

        menu.AddOption(m4a1s);
        menu.AddOption(m4a4);
        menu.AddOption(ak47);
        menu.AddOption(awp);

        return menu;
    }
}
