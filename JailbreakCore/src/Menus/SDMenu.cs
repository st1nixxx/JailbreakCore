using Jailbreak.Shared;
using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Natives;

namespace JailbreakCore;

public class SDMenu(ISwiftlyCore _core, Extensions _extensions)
{
    private readonly ISwiftlyCore _Core = _core;
    private readonly Extensions _Extensions = _extensions;

    public void Display(JBPlayer player)
    {
        var menu = _Extensions.CreateMenu(_Core.Translation.GetPlayerLocalizer(player.Player)["sd_menu<title>"]);

        IReadOnlyList<ISpecialDay> Days = JailbreakCore.SpecialDay.GetAllDays();

        foreach (var day in Days)
        {
            var option = new ButtonMenuOption(day.Name);
            option.Click += async (sender, args) =>
            {
                _Core.Scheduler.NextTick(() => JailbreakCore.SpecialDay.Select(player, day.Name));
            };
            menu.AddOption(option);
        }

        _Core.MenusAPI.OpenMenuForPlayer(player.Player, menu);
    }
}
