using System;
using Jailbreak.Shared;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;

namespace JailbreakCore;

public class LastRequest(ISwiftlyCore core)
{
    private readonly ISwiftlyCore _Core = core;
    private readonly List<ILastRequest> Requests = new();
    private ILastRequest? ActiveRequest;
    private CancellationTokenSource? PrepTimer;
    private Guid? ActiveLinkLaserId;
    private Guid? GuardianBeaconId;
    private Guid? PrisonerBeaconId;

    public IReadOnlyList<ILastRequest> GetRequests() => Requests;
    public ILastRequest? GetActiveRequest() => ActiveRequest;

    private static bool IsPrepTimeActive = false;

    public void Register(ILastRequest request)
    {
        Requests.Add(request);
    }
    public void SelectRequest(ILastRequest request, JBPlayer guardian, JBPlayer prisoner, string weaponName, string weaponId)
    {
        if (ActiveRequest != null)
        {
            prisoner.Print(IHud.Chat, "last_request_aleardy_active", null, 0, true, IPrefix.LR);
            return;
        }

        ActiveRequest = request;
        ActiveRequest.Prisoner = prisoner;
        ActiveRequest.Guardian = guardian;
        ActiveRequest.SelectedWeaponName = weaponName;
        ActiveRequest.SelectedWeaponID = weaponId;

        int prepDelay = Math.Max(0, JailbreakCore.Config.LastRequest.PrepCountdownSeconds);

        if (prepDelay <= 0)
        {
            request.IsPrepTimerActive = false;
            IsPrepTimeActive = false;
            StartRequest(guardian, prisoner);
            return;
        }

        request.IsPrepTimerActive = true;
        IsPrepTimeActive = true;

        BeginPrepVisuals(prisoner, guardian);
        SendPrepHtmlCountdown(request, guardian, prisoner, prepDelay);

        PrepTimer = _Core.Scheduler.RepeatBySeconds(1, () =>
        {
            if (ActiveRequest == null)
            {
                StopPrepVisuals(true, true);
                PrepTimer?.Cancel();
                PrepTimer = null;
                IsPrepTimeActive = false;
                return;
            }

            prepDelay--;

            if (prepDelay <= 0)
            {
                request.IsPrepTimerActive = false;
                IsPrepTimeActive = false;
                StopPrepVisuals(true, false);
                StartRequest(guardian, prisoner);

                guardian.Print(IHud.Html, null, "", showPrefix: false);
                prisoner.Print(IHud.Html, null, "", showPrefix: false);

                PrepTimer?.Cancel();
                PrepTimer = null;
            }
            else
            {
                SendPrepHtmlCountdown(request, guardian, prisoner, prepDelay);
            }
        });
    }
    private void StartRequest(IJBPlayer guardian, IJBPlayer prisoner)
    {
        JBPlayer? activeWarden = JailbreakCore.JBPlayerManagement.GetWarden();
        if (activeWarden != null)
            activeWarden.SetWarden(false);

        ActiveRequest?.Start(guardian, prisoner);
        if (ActiveRequest != null)
        {
            JailbreakCore.Extensions.PrintToChatAll("last_request_started", true, IPrefix.LR, ActiveRequest.Name, ActiveRequest.SelectedType!);
        }
    }
    public void EndRequest(IJBPlayer? winner = null, IJBPlayer? loser = null)
    {
        if (ActiveRequest != null)
        {
            ActiveRequest.End(winner, loser);
            ActiveRequest.Prisoner = null;
            ActiveRequest.Guardian = null;
            ActiveRequest = null;
        }

        StopPrepVisuals(true, true);
        PrepTimer?.Cancel();
        PrepTimer = null;
        IsPrepTimeActive = false;
    }
    public void OnPlayerDeath(IJBPlayer player)
    {
        if (ActiveRequest == null)
            return;

        if (player == ActiveRequest.Prisoner)
        {
            EndRequest(ActiveRequest.Guardian, ActiveRequest.Prisoner);
        }
        else if (player == ActiveRequest.Guardian)
        {
            EndRequest(ActiveRequest.Prisoner, ActiveRequest.Guardian);
        }
    }
    public HookResult OnTakeDamage(CTakeDamageInfo info, JBPlayer attacker, JBPlayer victim)
    {
        if (ActiveRequest == null)
            return HookResult.Continue;

        if ((attacker != ActiveRequest.Prisoner && attacker != ActiveRequest.Guardian) ||
            (victim != ActiveRequest.Prisoner && victim != ActiveRequest.Guardian))
        {
            info.Damage = 0;
            return HookResult.Handled;
        }

        if (IsPrepTimeActive)
        {
            info.Damage = 0;
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    private void BeginPrepVisuals(JBPlayer prisoner, JBPlayer guardian)
    {
        var config = JailbreakCore.Config.LastRequest;

        if (config.EnableLinkLaser && !ActiveLinkLaserId.HasValue)
        {
            try
            {
                ActiveLinkLaserId = JailbreakCore.Extensions.StartPlayerLinkLaser(prisoner, guardian);
            }
            catch (Exception ex)
            {
                _Core.Logger.LogWarning(ex, "Failed to start Last Request link laser between {Prisoner} and {Guardian}", prisoner.Controller.PlayerName, guardian.Controller.PlayerName);
            }
        }

        if (config.EnablePlayerBeacons)
        {
            if (!PrisonerBeaconId.HasValue)
            {
                PrisonerBeaconId = JailbreakCore.Extensions.CreateBeaconAnimationOnPlayer(prisoner, loop: true);
            }

            if (!GuardianBeaconId.HasValue)
            {
                GuardianBeaconId = JailbreakCore.Extensions.CreateBeaconAnimationOnPlayer(guardian, loop: true);
            }
        }
    }

    private void StopPrepVisuals(bool laserLink, bool beacon)
    {
        if (laserLink)
        {
            if (ActiveLinkLaserId.HasValue)
            {
                JailbreakCore.Extensions.StopPlayerLinkLaser(ActiveLinkLaserId.Value);
                ActiveLinkLaserId = null;
            }
        }
        if (beacon)
        {
            if (PrisonerBeaconId.HasValue)
            {
                JailbreakCore.Extensions.StopPlayerBeacon(PrisonerBeaconId.Value);
                PrisonerBeaconId = null;
            }

            if (GuardianBeaconId.HasValue)
            {
                JailbreakCore.Extensions.StopPlayerBeacon(GuardianBeaconId.Value);
                GuardianBeaconId = null;
            }
        }
    }

    private void SendPrepHtmlCountdown(ILastRequest request, JBPlayer guardian, JBPlayer prisoner, int secondsRemaining)
    {
        if (!JailbreakCore.Config.LastRequest.ShowHtmlCountdown)
            return;

        if (prisoner.IsValid)
        {
            prisoner.Print(IHud.Html, "last_request_starting_html", null, 5, false, IPrefix.LR, request.Name, secondsRemaining, guardian.Controller.PlayerName);
        }

        if (guardian.IsValid)
        {
            guardian.Print(IHud.Html, "last_request_starting_html", null, 5, false, IPrefix.LR, request.Name, secondsRemaining, prisoner.Controller.PlayerName);
        }
    }
}
