//using AudioApi;
using Jailbreak.Shared;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace JailbreakCore;

public class JBPlayer : IDisposable, IJBPlayer
{
    public IPlayer Player { get; set; }
    public CCSPlayerController Controller { get; set; }
    public CCSPlayerPawn PlayerPawn { get; set; }
    public CBasePlayerPawn Pawn { get; set; }
    public IJBRole Role { get; set; } = IJBRole.None;
    public bool IsWarden => Role == IJBRole.Warden;
    public bool IsRebel => Role == IJBRole.Rebel;
    public bool IsFreeday => Role == IJBRole.Freeday;
    public bool IsValid => Controller.IsValid && PlayerPawn.IsValid && Player.IsValid && Pawn.IsValid;
    public string WardenModel => JailbreakCore.Config.Models.Warden;
    public string GuardianModel => JailbreakCore.Config.Models.Guardian;
    public string PrisonerModel => JailbreakCore.Config.Models.Prisoner;
    public Color DefaultColor => Color.FromHex("#FFFFFFFF");
    private readonly ISwiftlyCore _Core;
    public JBPlayer(IPlayer player, CCSPlayerController controller, CCSPlayerPawn playerPawn, CBasePlayerPawn pawn, ISwiftlyCore core)
    {
        Player = player;
        Controller = controller;
        PlayerPawn = playerPawn;
        Pawn = pawn;
        _Core = core;
    }
    public void SetWarden(bool state)
    {
        if (state && !IsWarden)
        {
            SetRole(IJBRole.Warden);
            ConfigureWarden();

            if (!Controller.IsHLTV && IsValid)
            {
                if (JailbreakCore.Config.Warden.ShowMenuOnSet)
                    JailbreakCore.WardenMenu.Display(this);
            }
        }
        else
        {
            ClearWaden();

            if (Controller.TeamNum == (int)Team.CT)
                SetRole(IJBRole.Guardian);
            else if (Controller.TeamNum == (int)Team.T)
                SetRole(IJBRole.Prisoner);
            else
                SetRole(IJBRole.None);
        }
    }
    public void SetRebel(bool state)
    {
        if (state && Role == IJBRole.Prisoner)
        {
            var color = new Color(
            JailbreakCore.Config.Colors.RebelColor[0],
            JailbreakCore.Config.Colors.RebelColor[1],
            JailbreakCore.Config.Colors.RebelColor[2]);

            SetColor(color);
            SetRole(IJBRole.Rebel);
        }
        else
        {
            SetColor(DefaultColor);

            if (Controller.TeamNum == (int)Team.T)
                SetRole(IJBRole.Prisoner);
            else if (Controller.TeamNum == (int)Team.CT)
                SetRole(IJBRole.Guardian);
            else
                SetRole(IJBRole.None);
        }
    }
    public void SetFreeday(bool state)
    {
        if (state && Role == IJBRole.Prisoner)
        {
            var color = new Color(
            JailbreakCore.Config.Colors.FreedayColor[0],
            JailbreakCore.Config.Colors.FreedayColor[1],
            JailbreakCore.Config.Colors.FreedayColor[2]);

            SetColor(color);
            SetRole(IJBRole.Freeday);
        }
        else
        {
            SetColor(DefaultColor);

            if (Controller.TeamNum == (int)Team.T)
                SetRole(IJBRole.Prisoner);
            else if (Controller.TeamNum == (int)Team.CT)
                SetRole(IJBRole.Guardian);
            else
                SetRole(IJBRole.None);
        }
    }
    public void SetRole(IJBRole role)
    {
        Role = role;
    }
    public void SetVisible(bool state)
    {
        if (!IsValid)
            return;

        if (state)
        {
            SetColor(DefaultColor);
        }
        else
        {
            SetColor(new Color(0, 0, 0));
        }
    }
    public void StripWeapons(bool keepKnife)
    {
        if (!IsValid)
            return;

        if (keepKnife)
        {
            Pawn.ItemServices?.RemoveItems();
            _Core.Scheduler.NextTick(() => Pawn.ItemServices?.GiveItem<CBaseEntity>("weapon_knife"));
        }
        else
        {
            Pawn.ItemServices?.RemoveItems();
        }
    }
    public void OnPlayerSpawn()
    {
        if (Controller.TeamNum == (int)Team.T)
            SetRole(IJBRole.Prisoner);
        else if (Controller.TeamNum == (int)Team.CT)
            SetRole(IJBRole.Guardian);
        else
        {
            SetRole(IJBRole.None);
        }
        _Core.Scheduler.NextTick(() =>
        {
            if (Role == IJBRole.Prisoner)
            {
                if (!string.IsNullOrEmpty(PrisonerModel))
                    PlayerPawn.SetModel(PrisonerModel);
            }
            else if (Role == IJBRole.Guardian)
            {
                if (!string.IsNullOrEmpty(GuardianModel))
                    PlayerPawn.SetModel(GuardianModel);
            }
        });
    }
    public void OnChangeTeam(Team team)
    {
        if (team == Team.T)
        {
            if (IsWarden)
                SetWarden(false);
            else
                SetRole(IJBRole.Prisoner);
        }
        else if (team == Team.CT)
        {
            if (Role != IJBRole.Warden)
            {
                SetRole(IJBRole.Guardian);
            }
        }
        else
        {
            if (IsWarden)
                SetWarden(false);

            SetRole(IJBRole.None);
        }
    }
    public void Print(IHud hud, string? key = "", string? message = "", int duration = 5, bool showPrefix = true, IPrefix prefixType = IPrefix.JB, params object[] args)
    {
        string prefix = "";
        if (showPrefix)
        {
            switch (prefixType)
            {
                case IPrefix.LR:
                    prefix = _Core.Translation.GetPlayerLocalizer(Player)["lr_prefix"];
                    break;
                case IPrefix.SD:
                    prefix = _Core.Translation.GetPlayerLocalizer(Player)["sd_prefix"];
                    break;
                case IPrefix.JB:
                    prefix = _Core.Translation.GetPlayerLocalizer(Player)["jb_prefix"];
                    break;
            }
        }
        switch (hud)
        {
            case IHud.Chat:
                if (showPrefix)
                {
                    if (key != null && message == null)
                        Player.SendMessage(MessageType.Chat, prefix + _Core.Translation.GetPlayerLocalizer(Player)[key, args]);
                    else if (message != null && key == null)
                        Player.SendMessage(MessageType.Chat, prefix + message);
                }
                else
                {
                    if (key != null && message == null)
                        Player.SendMessage(MessageType.Chat, _Core.Translation.GetPlayerLocalizer(Player)[key, args]);
                    else if (message != null && key == null)
                        Player.SendMessage(MessageType.Chat, message);
                }
                break;

            case IHud.Alert:
                if (key != null && message == null)
                    Player.SendMessage(MessageType.Alert, _Core.Translation.GetPlayerLocalizer(Player)[key, args]);
                else if (message != null && key == null)
                    Player.SendMessage(MessageType.Alert, message);
                break;

            case IHud.Center:
                if (key != null && message == null)
                    Player.SendMessage(MessageType.Center, _Core.Translation.GetPlayerLocalizer(Player)[key, args]);
                else if (message != null && key == null)
                    Player.SendMessage(MessageType.Center, message);
                break;

            case IHud.InstructorHint:
                if (key != null && message == null)
                {
                    JailbreakCore.Extensions.ShowInstructorHint(this, _Core.Translation.GetPlayerLocalizer(Player)[key]);
                }
                else if (message != null && key == null)
                {
                    JailbreakCore.Extensions.ShowInstructorHint(this, message);
                }
                break;
            case IHud.Html:
                if (key != null && message == null)
                {
                    if (duration > 5)
                    {
                        CancellationTokenSource? tempToken = null;
                        tempToken = _Core.Scheduler.RepeatBySeconds(3, () =>
                        {
                            duration--;
                            if (duration > 0)
                            {
                                Player.SendMessage(MessageType.CenterHTML, _Core.Translation.GetPlayerLocalizer(Player)[key, args]);
                            }
                            else
                            {
                                Player.SendMessage(MessageType.CenterHTML, "");
                                tempToken?.Cancel();
                            }
                        });
                    }
                    else
                    {
                        Player.SendMessage(MessageType.CenterHTML, _Core.Translation.GetPlayerLocalizer(Player)[key, args]);
                    }

                }
                else if (message != null && key == null)
                {
                    if (duration > 5)
                    {
                        CancellationTokenSource? tempToken = null;
                        tempToken = _Core.Scheduler.RepeatBySeconds(3, () =>
                        {
                            duration--;
                            if (duration > 0)
                            {
                                Player.SendMessage(MessageType.CenterHTML, message);
                            }
                            else
                            {
                                Player.SendMessage(MessageType.CenterHTML, "");
                                tempToken?.Cancel();
                            }
                        });
                    }
                    else
                    {
                        Player.SendMessage(MessageType.CenterHTML, message);
                    }
                }
                break;

        }
    }
    private void ConfigureWarden()
    {
        _Core.Scheduler.NextTick(() =>
        {
            if (!IsValid || !PlayerPawn.IsValid)
                return;

            // Color rendering causes crash - disabled for now
            //try
            //{
            //    var color = new Color(
            //    JailbreakCore.Config.Colors.WardenColor[0],
            //    JailbreakCore.Config.Colors.WardenColor[1],
            //    JailbreakCore.Config.Colors.WardenColor[2]);
            //
            //    SetColor(color);
            //}
            //catch
            //{
            //    // Ignore color setting errors
            //}

            //if (!string.IsNullOrEmpty(WardenModel))
            //    PlayerPawn.SetModel(WardenModel);
        });
    }
    private void ClearWaden()
    {
        _Core.Scheduler.NextTick(() =>
        {
            SetColor(DefaultColor);

            if (Controller.TeamNum == (int)Team.CT)
            {
                if (!string.IsNullOrEmpty(GuardianModel))
                    PlayerPawn.SetModel(GuardianModel);
            }
            else if (Controller.TeamNum == (int)Team.T)
            {
                if (!string.IsNullOrEmpty(PrisonerModel))
                    PlayerPawn.SetModel(PrisonerModel);
            }
        });
    }
    public void SetColor(Color color)
    {
        _Core.Scheduler.NextTick(() =>
        {
            if (!IsValid || !PlayerPawn.IsValid)
                return;

            PlayerPawn.RenderMode = RenderMode_t.kRenderTransColor;
            PlayerPawn.RenderModeUpdated();
            PlayerPawn.Render = color;
            PlayerPawn.RenderUpdated();

        });
    }
    /*public void PlaySound(string mp3path, float volume)
    {
        IAudioChannelController controller = JailbreakCore.Audio.UseChannel("jailbreak_core");
        IAudioSource source = JailbreakCore.Audio.DecodeFromFile(Path.Combine(_Core.PluginDataDirectory, mp3path));

        controller.SetSource(source);
        controller.SetVolume(Player.PlayerID, volume);
        controller.Play(Player.PlayerID);
    }
    */
    public void Dispose()
    {
        SetRole(IJBRole.None);
    }

}
