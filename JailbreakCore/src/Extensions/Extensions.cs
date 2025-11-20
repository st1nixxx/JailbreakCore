using System.Globalization;
using Jailbreak.Shared;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace JailbreakCore;

public class Extensions(ISwiftlyCore core)
{
    private readonly ISwiftlyCore _Core = core;
    private readonly Dictionary<JBPlayer, CEnvBeam?> _wardenLasers = new();
    private readonly List<CEnvBeam?> _wardenBeacons = new();
    private readonly Dictionary<JBPlayer, List<CEnvBeam?>> _wardenBeaconsByPlayer = new();
    private static readonly Vector ANGLE_ZERO = new Vector(0, 0, 0);
    private static readonly Vector VEC_ZERO = new Vector(0, 0, 0);
    public void PrintToChatAll(string message, bool showPrefix, IPrefix prefixType)
    {
        string prefix = "";
        switch (prefixType)
        {
            case IPrefix.JB:
                prefix = "jb_prefix";
                break;

            case IPrefix.LR:
                prefix = "lr_prefix";
                break;

            case IPrefix.SD:
                prefix = "sd_prefix";
                break;

            default:
                prefix = "";
                break;
        }
        foreach (var player in _Core.PlayerManager.GetAllPlayers())
        {
            if (showPrefix)
                player.SendMessage(MessageType.Chat, _Core.Translation.GetPlayerLocalizer(player)[prefix] + message);
            else
                player.SendMessage(MessageType.Chat, message);
        }
    }
    public void PrintToChatAll(string key, bool showPrefix = true, IPrefix prefixType = IPrefix.JB, params object[] args)
    {
        string prefix = "";
        switch (prefixType)
        {
            case IPrefix.JB:
                prefix = "jb_prefix";
                break;

            case IPrefix.LR:
                prefix = "lr_prefix";
                break;

            case IPrefix.SD:
                prefix = "sd_prefix";
                break;

            default:
                prefix = "";
                break;
        }
        foreach (var player in _Core.PlayerManager.GetAllPlayers())
        {
            if (showPrefix)
                player.SendMessage(MessageType.Chat, _Core.Translation.GetPlayerLocalizer(player)[prefix] + _Core.Translation.GetPlayerLocalizer(player)[key, args]);
            else
                player.SendMessage(MessageType.Chat, _Core.Translation.GetPlayerLocalizer(player)[key, args]);
        }
    }
    public void PrintToAlertAll(string key, params object[] args)
    {
        foreach (var player in _Core.PlayerManager.GetAllPlayers())
        {
            player.SendMessage(MessageType.Alert, _Core.Translation.GetPlayerLocalizer(player)[key, args]);
        }
    }
    public void PrintToCenterAll(string key, params object[] args)
    {
        foreach (var player in _Core.PlayerManager.GetAllPlayers())
        {
            player.SendMessage(MessageType.Center, _Core.Translation.GetPlayerLocalizer(player)[key, args]);
        }
    }
    public void AssignRandomWarden()
    {
        List<IPlayer> validPlayers = _Core.PlayerManager.GetAllPlayers().Where(p => p.Controller?.TeamNum == (int)Team.CT && p.Controller.PawnIsAlive).ToList();

        _Core.Logger.LogDebug("AssignRandomWarden candidates {Count}", validPlayers.Count);

        if (validPlayers.Count == 0)
        {
            _Core.Logger.LogDebug("AssignRandomWarden aborted: no eligible CT players");
            return;
        }

        IPlayer randomPlayer = validPlayers[new Random().Next(validPlayers.Count)];

        if (randomPlayer != null && randomPlayer.Controller.PawnIsAlive == true && randomPlayer.Controller?.TeamNum == (int)Team.CT)
        {
            _Core.Logger.LogDebug("AssignRandomWarden selecting player {Player}", randomPlayer.Controller.PlayerName ?? "<unknown>");
            JBPlayer randomJbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(randomPlayer);
            randomJbPlayer.SetWarden(true);

            randomJbPlayer.Print(IHud.Chat, "warden_take", null, 0, true, IPrefix.JB);
        }
        else
        {
            _Core.Logger.LogDebug(
                "AssignRandomWarden rejected player; alive={Alive} team={Team}",
                randomPlayer?.Controller?.PawnIsAlive,
                randomPlayer?.Controller?.TeamNum
            );
        }
    }
    public void ShowInstructorHint(JBPlayer player, string text, int time = 5, float height = -40.0f, float range = -50.0f, bool follow = true,
    bool showOffScren = true, string iconOnScreen = "icon_bulb", string iconOffScreen = "icon_arrow_up", string cmd = "use_binding", bool showTextAlways = false, Color? color = null)
    {
        if (!player.IsValid)
            return;

        var hintColor = color ?? new Color(255, 0, 0);

        var gameInstructor = _Core.ConVar.Find<string>("sv_gameinstructor_enable");
        gameInstructor?.ReplicateToClient(player.Player.PlayerID, "1");

        _Core.Scheduler.NextTick(() =>
        {
            CreateInstructorHint(player, text, time, height, range, follow, showOffScren, iconOnScreen, iconOffScreen, cmd, showTextAlways, hintColor);
        });
    }
    private void CreateInstructorHint(JBPlayer player, string text, int time, float height, float range, bool follow, bool showOffScren, string iconOnScreen, string iconOffScreen, string cmd, bool showTextAlways, Color color)
    {
        var targetIndex = player.Controller.Index.ToString();
        var hintName = $"instructor_hint_{player.Player.PlayerID}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        var entity = _Core.EntitySystem.CreateEntity<CEnvInstructorHint>();
        if (entity == null)
            return;

        entity.Name = hintName;
        entity.HintTargetEntity = targetIndex;
        entity.Static = follow;
        entity.Timeout = time;
        entity.Caption = text.Replace("\n", " ");
        entity.Color = color;
        entity.ForceCaption = showTextAlways;
        entity.Icon_Onscreen = iconOnScreen;
        entity.Icon_Offscreen = iconOffScreen;
        entity.NoOffscreen = showOffScren;
        entity.Binding = cmd;
        entity.IconOffset = height;
        entity.Range = range;
        entity.LocalPlayerOnly = false;

        entity.AcceptInput("ShowHint", value: "");

        if (time > 0)
        {
            _Core.Scheduler.Delay(time, () =>
            {
                if (entity.IsValid)
                {
                    entity.AcceptInput("Remove", value: "");
                }
            });
        }
    }
    public CancellationTokenSource StartTimer(int seconds, Action<int> onTick, Action onFinished)
    {
        int remaining = seconds;

        CancellationTokenSource? timer = null;

        timer = _Core.Scheduler.Delay(1, () =>
        {
            remaining--;

            if (remaining > 0)
                onTick?.Invoke(remaining);
            else
            {
                onFinished?.Invoke();
                timer?.Cancel();
            }
        });

        return timer;

    }
    public void ToggleBox(bool state, string callerName = "")
    {
        var teammatesEnemies = _Core.ConVar.Find<bool>("mp_teammates_are_enemies");

        teammatesEnemies?.SetInternal(state ? true : false);
        JailbreakCore.g_IsBoxActive = state ? true : false;

        int commandValue = state ? 0 : 1;
        string boxState = state ? $" {Helper.ChatColors.Green}ON{Helper.ChatColors.Default}" : $" {Helper.ChatColors.Red}OFF{Helper.ChatColors.Default}";
        _Core.Engine.ExecuteCommand($"sv_teamid_overhead {commandValue}");

        foreach (var jbPlayer in JailbreakCore.JBPlayerManagement.GetAllPlayers())
        {
            if (!string.IsNullOrEmpty(callerName))
            {
                jbPlayer.Print(IHud.Chat, "box_toggled", null, 0, true, IPrefix.JB, callerName, boxState);
            }
            if (!string.IsNullOrEmpty(JailbreakCore.Config.Sounds.Box.Path) && state)
            {
                //jbPlayer.PlaySound(JailbreakCore.Config.Sounds.Box.Path, JailbreakCore.Config.Sounds.Box.Volume);
            }
        }
    }
    public HookResult OnBoxActive(CTakeDamageInfo info, JBPlayer attacker, JBPlayer victim)
    {
        if (JailbreakCore.g_IsBoxActive && attacker.Controller.TeamNum == victim.Controller.TeamNum && victim.Controller.TeamNum != (int)Team.T)
        {
            info.Damage = 0;
            return HookResult.Handled;
        }
        return HookResult.Continue;
    }

    private void ForceEntityInput(string designerName, string input)
    {
        var entities = _Core.EntitySystem.GetAllEntitiesByDesignerName<CBaseEntity>(designerName);
        foreach (var entity in entities)
        {
            if (entity == null || !entity.IsValid)
                continue;

            entity.AcceptInput(input, value: "");
        }
    }

    public void ToggleCells(bool value, string callerName = "")
    {
        JailbreakCore.g_AreCellsOpened = value ? true : false;
        string status = JailbreakCore.g_AreCellsOpened ? $" {Helper.ChatColors.Green}opened{Helper.ChatColors.Default}" : $" {Helper.ChatColors.Red}closed{Helper.ChatColors.Default}";

        if (!string.IsNullOrEmpty(callerName))
            PrintToChatAll("cells_toggled", true, IPrefix.JB, callerName, status);

        if (JailbreakCore.g_AreCellsOpened)
        {
            ForceEntityInput("func_door", "Open");
            ForceEntityInput("func_movelinear", "Open");
            ForceEntityInput("func_door_rotating", "Open");
            ForceEntityInput("prop_door_rotating", "Open");
            ForceEntityInput("func_breakable", "Break");
        }
        else
        {
            ForceEntityInput("func_door", "Close");
            ForceEntityInput("func_movelinear", "Close");
            ForceEntityInput("func_door_rotating", "Close");
            ForceEntityInput("prop_door_rotating", "Close");
        }
    }
    public IMenuAPI CreateMenu(string title, IMenuAPI? parent = null)
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

        var menu = _Core.MenusAPI.CreateMenu(
            configuration: config,
            keybindOverrides: keyBinds,
            parent: parent ?? null,
            optionScrollStyle: MenuOptionScrollStyle.CenterFixed,
            optionTextStyle: MenuOptionTextStyle.TruncateEnd
        );

        return menu;
    }
    public void ToggleBunnyhoop(bool state)
    {
        int value = state ? 1 : 0;
        string bhState = state ? "true" : "false";

        string isEnabled = state ?
        $" {Helper.ChatColors.Green}Enabled{Helper.ChatColors.Default}"
        : $" {Helper.ChatColors.Red}Disabled{Helper.ChatColors.Default}";

        _Core.Engine.ExecuteCommand($"sv_cheats {value}");
        _Core.Engine.ExecuteCommand($"sv_autobunnyhopping {bhState}");
        _Core.Engine.ExecuteCommand($"sv_enablebunnyhopping {bhState}");

        PrintToChatAll("bh_toggled", true, IPrefix.JB, isEnabled);
    }

    #region Entity Management

    /// <summary>
    /// Get total entity count in the server
    /// </summary>
    public int GetEntityCount()
    {
        return _Core.EntitySystem.GetAllEntities().Count();
    }

    #endregion

    #region Laser System

    /// <summary>
    /// Create a laser beam between two points
    /// </summary>
    public CEnvBeam? CreateLaser(Vector start, Vector end, float width, Color color)
    {
        var laser = _Core.EntitySystem.CreateEntity<CEnvBeam>();

        if (laser == null)
        {
            _Core.Logger.LogWarning("Failed to create laser beam entity");
            return null;
        }

        // Setup appearance
        laser.Render = color;
        laser.Width = width;

        // Set position
        MoveLaser(laser, start, end);

        // Spawn the entity
        laser.DispatchSpawn();

        return laser;
    }

    /// <summary>
    /// Move an existing laser beam to new positions
    /// </summary>
    public void MoveLaser(CEnvBeam laser, Vector start, Vector end)
    {
        if (laser == null || !laser.IsValid)
            return;

        // Teleport laser to start position
        laser.Teleport(start, new QAngle(0, 0, 0), VEC_ZERO);

        // Set end position
        laser.EndPos.X = end.X;
        laser.EndPos.Y = end.Y;
        laser.EndPos.Z = end.Z;

        // Notify state change using SwiftlyS2 method
        laser.EndPosUpdated();
    }

    /// <summary>
    /// Change laser color
    /// </summary>
    public void SetLaserColor(CEnvBeam? laser, Color color)
    {
        if (laser != null && laser.IsValid)
        {
            laser.RenderMode = RenderMode_t.kRenderTransColor;
            laser.Render = color;

            laser.RenderUpdated();
            laser.RenderModeUpdated();
        }
    }

    /// <summary>
    /// Remove a laser beam
    /// </summary>
    public void RemoveLaser(CEnvBeam? laser)
    {
        if (laser != null && laser.IsValid)
        {
            laser.AcceptInput("Kill", value: "");
        }
    }

    /// <summary>
    /// Create or update warden's laser pointer
    /// </summary>
    public void UpdateWardenLaser(JBPlayer warden, Vector start, Vector end)
    {
        if (!_wardenLasers.ContainsKey(warden) || _wardenLasers[warden] == null || !_wardenLasers[warden]!.IsValid)
        {
            // Create new laser
            var laser = CreateLaser(start, end, 2.0f, Color.FromHex("#FF0000FF")); // Red laser
            _wardenLasers[warden] = laser;
        }
        else
        {
            // Update existing laser position
            MoveLaser(_wardenLasers[warden]!, start, end);
        }
    }

    /// <summary>
    /// Remove warden's laser when they stop using it
    /// </summary>
    public void RemoveWardenLaser(JBPlayer warden)
    {
        if (_wardenLasers.TryGetValue(warden, out var laser))
        {
            RemoveLaser(laser);
            _wardenLasers.Remove(warden);
        }
    }

    /// <summary>
    /// Clean up all warden lasers
    /// </summary>
    public void CleanupAllLasers()
    {
        foreach (var laser in _wardenLasers.Values)
        {
            RemoveLaser(laser);
        }
        _wardenLasers.Clear();
    }

    #endregion

    #region Beacon System

    /// <summary>
    /// Create a circular beacon at a specific position (drawn on the ground)
    /// </summary>
    /// <param name="warden">The warden placing the beacon</param>
    /// <param name="position">Center position of the beacon</param>
    /// <param name="radius">Radius of the circle in units</param>
    /// <param name="height">Height above ground to draw the circle</param>
    /// <param name="segments">Number of segments forming the circle (more = smoother circle)</param>
    /// <param name="duration">Duration in seconds before beacon disappears</param>
    public void CreateWardenBeacon(JBPlayer warden, Vector position, float radius = 64.0f, float height = 5.0f, int segments = 32, float duration = 30.0f)
    {
        // Remove previous beacon from this warden
        RemoveWardenBeacon(warden);

        var beaconGroup = new List<CEnvBeam?>();

        // Create circle by connecting points around the circumference
        for (int i = 0; i < segments; i++)
        {
            // Current point on circle
            float angle1 = (float)(2 * Math.PI * i / segments);
            float x1 = position.X + radius * (float)Math.Cos(angle1);
            float y1 = position.Y + radius * (float)Math.Sin(angle1);

            // Next point on circle (connect to form the circle)
            float angle2 = (float)(2 * Math.PI * (i + 1) / segments);
            float x2 = position.X + radius * (float)Math.Cos(angle2);
            float y2 = position.Y + radius * (float)Math.Sin(angle2);

            // Draw beam between consecutive points
            var startPos = new Vector(x1, y1, position.Z + height);
            var endPos = new Vector(x2, y2, position.Z + height);

            var beam = CreateLaser(startPos, endPos, 2.0f, Color.FromHex("#00FF00FF")); // Green beams

            if (beam != null)
            {
                beaconGroup.Add(beam);
                _wardenBeacons.Add(beam);
            }
        }

        // Store beacons for this warden
        _wardenBeaconsByPlayer[warden] = beaconGroup;

        // Schedule removal after duration
        _Core.Scheduler.DelayBySeconds(duration, () =>
        {
            foreach (var beam in beaconGroup)
            {
                RemoveLaser(beam);
                _wardenBeacons.Remove(beam);
            }
            _wardenBeaconsByPlayer.Remove(warden);
        });
    }

    /// <summary>
    /// Remove a specific warden's beacon
    /// </summary>
    public void RemoveWardenBeacon(JBPlayer warden)
    {
        if (_wardenBeaconsByPlayer.TryGetValue(warden, out var beaconGroup))
        {
            foreach (var beam in beaconGroup)
            {
                RemoveLaser(beam);
                _wardenBeacons.Remove(beam);
            }
            _wardenBeaconsByPlayer.Remove(warden);
        }
    }

    /// <summary>
    /// Remove all warden beacons
    /// </summary>
    public void CleanupAllBeacons()
    {
        foreach (var beacon in _wardenBeacons)
        {
            RemoveLaser(beacon);
        }
        _wardenBeacons.Clear();
        _wardenBeaconsByPlayer.Clear();
    }

    #endregion
}
