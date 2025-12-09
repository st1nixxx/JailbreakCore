using Jailbreak.Shared;
using Spectre.Console;
using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;

namespace JailbreakCore;

public class LRMenu(ISwiftlyCore _core, Extensions _extensions)
{
    private readonly ISwiftlyCore _Core = _core;
    private readonly Extensions _Extensions = _extensions;
    public void Display(JBPlayer prisoner)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(prisoner.Player)["last_request_menu<title>"]);

        foreach (var request in JailbreakCore.LastRequest.GetRequests())
        {
            var guardianMenu = OpenGuardianMenu(prisoner, request, menu);
            var option = new SubmenuMenuOption(request.Name, guardianMenu);
            menu.AddOption(option);
        }

        _Core.MenusAPI.OpenMenuForPlayer(prisoner.Player, menu);
    }
    private IMenuAPI OpenGuardianMenu(JBPlayer prisoner, ILastRequest request, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(prisoner.Player)["last_request_guardian_menu<title>"], parent);

        List<JBPlayer> guardians = JailbreakCore.JBPlayerManagement.GetAllGuardians().Where(g => g.Role == IJBRole.Guardian && g.Controller.PawnIsAlive).ToList();

        if (!guardians.Any())
        {
            menu.AddOption(new TextMenuOption(_Core.Translation.GetPlayerLocalizer(prisoner.Player)["last_request_no_guardian<option>"]));
        }

        foreach (var g in guardians)
        {
            if (request.GetAvailableTypes != null && request.GetAvailableTypes().Any())
            {
                var typeMenu = OpenTypeMenu(prisoner, g, request, menu);
                var option = new SubmenuMenuOption(g.Controller.PlayerName, typeMenu);

                menu.AddOption(option);
            }
            else
            {
                var weaponsMenu = OpenWeaponsMenu(prisoner, g, request, menu);
                var option = new SubmenuMenuOption(g.Controller.PlayerName, weaponsMenu);

                menu.AddOption(option);
            }
        }

        return menu;
    }
    private IMenuAPI OpenTypeMenu(JBPlayer prisoner, JBPlayer guardian, ILastRequest request, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(prisoner.Player)["last_request_type_menu<title>"], parent);

        foreach (var type in request.GetAvailableTypes())
        {
            var weaponsMenu = OpenWeaponsMenu(prisoner, guardian, request, menu);
            var option = new SubmenuMenuOption(type, weaponsMenu);
            option.Click += async (sender, args) =>
            {
                request.SelectedType = type;
            };

            menu.AddOption(option);
        }

        return menu;
    }
    private IMenuAPI OpenWeaponsMenu(JBPlayer prisoner, JBPlayer guardian, ILastRequest request, IMenuAPI parent)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(prisoner.Player)["last_request_weapons_menu<title>"], parent);

        IReadOnlyList<(string DisplayName, string ClassName)> avaliableWeapons = request.GetAvailableWeapons();

        foreach (var (displayName, className) in avaliableWeapons)
        {
            var option = new ButtonMenuOption(displayName);
            option.Click += async (sender, args) =>
            {
                _Core.Scheduler.NextTick(() =>
                {
                    request.SelectedWeaponName = displayName;
                    request.SelectedWeaponID = className;

                    JailbreakCore.LastRequest.SelectRequest(request, guardian, prisoner, displayName, className);
                });
            };

            menu.AddOption(option);
        }

        return menu;
    }
}
