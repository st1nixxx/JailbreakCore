using Jailbreak.Shared;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;

namespace JailbreakCore;

public enum DayOfWeek
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday
}

public class DayCycle
{
    private readonly ISwiftlyCore _Core;
    private DayOfWeek _currentDay = DayOfWeek.Monday;
    private bool _isFreedayActive = false;

    public DayCycle(ISwiftlyCore core)
    {
        _Core = core;
    }

    public DayOfWeek GetCurrentDay() => _currentDay;
    public bool IsFreedayActive() => _isFreedayActive;

    public string GetDayName()
    {
        return _currentDay switch
        {
            DayOfWeek.Monday => "Monday",
            DayOfWeek.Tuesday => "Tuesday",
            DayOfWeek.Wednesday => "Wednesday",
            DayOfWeek.Thursday => "Thursday",
            DayOfWeek.Friday => "Friday",
            _ => "Unknown"
        };
    }

    public string GetDayStatus()
    {
        if (_currentDay == DayOfWeek.Friday)
            return "Freeday";
        
        return "Normal";
    }

    public void NextDay()
    {
        _currentDay = _currentDay switch
        {
            DayOfWeek.Monday => DayOfWeek.Tuesday,
            DayOfWeek.Tuesday => DayOfWeek.Wednesday,
            DayOfWeek.Wednesday => DayOfWeek.Thursday,
            DayOfWeek.Thursday => DayOfWeek.Friday,
            DayOfWeek.Friday => DayOfWeek.Monday,
            _ => DayOfWeek.Monday
        };

        // Friday is auto-freeday
        _isFreedayActive = (_currentDay == DayOfWeek.Friday);

        // UpdateHUD(); // Disabled - HUD was broken
    }

    public void OnRoundStart()
    {
        // Auto-freeday on Friday
        if (_currentDay == DayOfWeek.Friday && _isFreedayActive)
        {
            ApplyFreeday();
        }

        // UpdateHUD(); // Disabled - HUD was broken
    }

    public void OnRoundEnd()
    {
        NextDay();
    }

    private void ApplyFreeday()
    {
        // Open cells
        JailbreakCore.Extensions.ToggleCells(true, "");

        // Give freeday to all prisoners
        foreach (var jbPlayer in JailbreakCore.JBPlayerManagement.GetAllPlayers())
        {
            if (jbPlayer.Controller.TeamNum == 2 && !jbPlayer.IsFreeday)  // Team.T = 2
            {
                jbPlayer.SetFreeday(true);
            }
        }

        JailbreakCore.Extensions.PrintToChatAll("freeday_friday", true, IPrefix.JB);
    }

    public void UpdateHUD()
    {
        string dayName = GetDayName();
        string dayStatus = GetDayStatus();
        string hudMessage = $"<font class='fontSize-l' color='#FFD700'>Current Day: {dayName}</font><br><font class='fontSize-m' color='#87CEEB'>({dayStatus})</font>";

        foreach (var jbPlayer in JailbreakCore.JBPlayerManagement.GetAllPlayers())
        {
            if (jbPlayer.Player.IsFakeClient || jbPlayer.Controller.IsHLTV)
                continue;

            // Display HUD at top-left using HTML
            jbPlayer.Print(IHud.Html, null, hudMessage, duration: 999, showPrefix: false);
        }
    }

    public void Reset()
    {
        _currentDay = DayOfWeek.Monday;
        _isFreedayActive = false;
        // UpdateHUD(); // Disabled - HUD was broken
    }
}
