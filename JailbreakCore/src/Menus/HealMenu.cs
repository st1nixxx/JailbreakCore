using Jailbreak.Shared;
using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace JailbreakCore;

public class HealMenu(ISwiftlyCore _core, Extensions _extensions)
{
    private readonly ISwiftlyCore _Core = _core;
    private readonly Extensions _Extensions = _extensions;

    public void Display(JBPlayer prisoner)
    {
        var warden = JailbreakCore.JBPlayerManagement.GetWarden();
        if (warden == null)
            return;

        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(warden.Player)["heal_request_menu<title>", prisoner.Controller.PlayerName]);

        var yesOption = new ButtonMenuOption(_Core.Translation.GetPlayerLocalizer(warden.Player)["accept<option>"]);
        yesOption.Click += async (sender, args) =>
        {
            _Core.Scheduler.NextTick(() =>
            {
                _Extensions.PrintToChatAll("heal_accepted", true, IPrefix.JB, prisoner.Controller.PlayerName);
                prisoner.Pawn.ItemServices?.GiveItem<CBaseEntity>("weapon_healthshot");
                _Core.MenusAPI.CloseMenuForPlayer(warden.Player, menu);
            });
        };

        var noOption = new ButtonMenuOption(_Core.Translation.GetPlayerLocalizer(warden.Player)["refuse<option>"]);
        noOption.Click += async (sender, args) =>
        {
            _Core.Scheduler.NextTick(() =>
            {
                prisoner.Print(IHud.Chat, "heal_refused", null, 0, true, IPrefix.JB);
                _Core.MenusAPI.CloseMenuForPlayer(warden.Player, menu);
            });
        };

        menu.AddOption(yesOption);
        menu.AddOption(noOption);

        _Core.MenusAPI.OpenMenuForPlayer(warden.Player, menu);
    }
}
