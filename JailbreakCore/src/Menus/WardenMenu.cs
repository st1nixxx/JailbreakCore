using Jailbreak.Shared;
using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Natives;

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

        var toggleBox = new ToggleMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["toggle_box<option>"]);
        toggleBox.Click += async (sender, args) =>
        {
            var isToggled = toggleBox.GetDisplayText(args.Player, 0).Contains("✔");
            JailbreakCore.Extensions.ToggleBox(isToggled, args.Player.Controller.PlayerName);
        };

        var freedaySubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["add_freeday<option>"], FreedayMenu(player, menu));
        var colorSubMenu = new SubmenuMenuOption(_Core.Translation.GetPlayerLocalizer(player.Player)["set_color<option>"], ColorSelectPlayerMenu(player, menu));

        menu.AddOption(toggleCells);
        menu.AddOption(toggleBox);
        menu.AddOption(freedaySubMenu);
        menu.AddOption(colorSubMenu);

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
}
