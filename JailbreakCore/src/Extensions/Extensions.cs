using System;
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
    private readonly Dictionary<Guid, PlayerLinkLaserEffect> _playerLinkLasers = new();
    private readonly Dictionary<Guid, PlayerBeaconEffect> _playerBeaconAnimations = new();
    private static readonly Vector ANGLE_ZERO = new Vector(0, 0, 0);
    private static readonly Vector VEC_ZERO = new Vector(0, 0, 0);

    private sealed class PlayerLinkLaserEffect
    {
        public PlayerLinkLaserEffect(JBPlayer playerA, JBPlayer playerB, Color color, float width, float heightOffset, TimeSpan? lifespan)
        {
            PlayerA = playerA;
            PlayerB = playerB;
            Color = color;
            Width = width;
            HeightOffset = heightOffset;
            ExpireAtUtc = lifespan.HasValue ? DateTimeOffset.UtcNow.Add(lifespan.Value) : null;
        }

        public JBPlayer PlayerA { get; }
        public JBPlayer PlayerB { get; }
        public Color Color { get; }
        public float Width { get; }
        public float HeightOffset { get; }
        public DateTimeOffset? ExpireAtUtc { get; }
        public CEnvBeam? Beam { get; set; }
    }

    private sealed class PlayerBeaconEffect
    {
        public PlayerBeaconEffect(JBPlayer player, List<CEnvBeam?> segments, Color color, float radius, float radiusStep, float heightOffset, float durationSeconds, float width, TimeSpan stepInterval, bool loop)
        {
            Player = player;
            Segments = segments;
            Color = color;
            Radius = radius;
            ResetRadius = radius;
            RadiusStep = radiusStep;
            HeightOffset = heightOffset;
            DurationSeconds = durationSeconds;
            Width = width;
            StepInterval = stepInterval;
            Loop = loop;
            NextStepAtUtc = DateTimeOffset.UtcNow;
            AngleStep = segments.Count > 0 ? (float)(2 * Math.PI / segments.Count) : 0f;
        }

        public JBPlayer Player { get; }
        public List<CEnvBeam?> Segments { get; }
        public Color Color { get; }
        public float Radius { get; set; }
        public float ResetRadius { get; }
        public float RadiusStep { get; }
        public float HeightOffset { get; }
        public float DurationSeconds { get; }
        public float ElapsedSeconds { get; set; }
        public float Width { get; }
        public TimeSpan StepInterval { get; }
        public DateTimeOffset NextStepAtUtc { get; set; }
        public bool Loop { get; }
        public float AngleStep { get; }
    }
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
                jbPlayer.PlaySound(JailbreakCore.Config.Sounds.Box.Path, JailbreakCore.Config.Sounds.Box.Volume);
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
    
    /// <summary>
    /// new critical menu that cannot be accidentally exited (v1.0.3 featt)
    /// </summary>
    public IMenuAPI CreateCriticalMenu(string title, IMenuAPI? parent = null)
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
            DisableExit = false, // allow Tab key to exit menu
        };

        var keyBinds = new MenuKeybindOverrides
        {
            Select = KeyBind.E,
            Move = KeyBind.S,
            MoveBack = KeyBind.W,
            Exit = KeyBind.Tab // still bound but disabled by DisableExit
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
    
    /// <summary>
    /// Check if warden has reasonable proximity to target player for enhanced targeting
    /// Note: Simplified implementation until SwiftlyS2 1.0.3
    /// </summary>
    public bool WardenHasLineOfSight(IJBPlayer warden, IJBPlayer target)
    {
        if (warden?.Pawn == null || target?.Pawn == null) return false;
        
        try
        {
            var wardenPos = warden.Pawn.AbsOrigin;
            var targetPos = target.Pawn.AbsOrigin;
            
            if (!wardenPos.HasValue || !targetPos.HasValue) return false;
            
            // Calculate distance between warden and target
            var dx = wardenPos.Value.X - targetPos.Value.X;
            var dy = wardenPos.Value.Y - targetPos.Value.Y;
            var dz = wardenPos.Value.Z - targetPos.Value.Z;
            var distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            
            // Return true if within reasonable interaction distance (e.g., 1000 units)
            return distance <= 1000.0;
        }
        catch
        {
            return false;
        }
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

        StopAllPlayerLinkLasers();
    }

    /// <summary>
    /// Starts a persistent laser that links two players together. The beam is updated every tick.
    /// </summary>
    /// <param name="playerA">First player.</param>
    /// <param name="playerB">Second player.</param>
    /// <param name="colorOverride">Optional custom color.</param>
    /// <param name="width">Beam width.</param>
    /// <param name="heightOffset">Height offset from the player's origin (defaults to approximate eye level).</param>
    /// <param name="durationSeconds">Positive value to auto-expire after the provided time; zero/negative keeps it alive until removed.</param>
    public Guid StartPlayerLinkLaser(JBPlayer playerA, JBPlayer playerB, Color? colorOverride = null, float width = 2.0f, float heightOffset = 64.0f, float durationSeconds = 0f)
    {
        ArgumentNullException.ThrowIfNull(playerA);
        ArgumentNullException.ThrowIfNull(playerB);

        if (playerA == playerB)
            throw new ArgumentException("Players must be different.", nameof(playerB));

        if (!IsPlayerRenderable(playerA) || !IsPlayerRenderable(playerB))
            throw new InvalidOperationException("Both players must be valid and alive to start a link laser.");

        TimeSpan? lifespan = durationSeconds > 0 ? TimeSpan.FromSeconds(durationSeconds) : null;
        var effect = new PlayerLinkLaserEffect(playerA, playerB, colorOverride ?? Color.FromHex("#FFFFFFFF"), Math.Max(0.1f, width), heightOffset, lifespan);

        var id = Guid.NewGuid();
        _playerLinkLasers[id] = effect;

        if (!UpdatePlayerLinkLaser(effect))
        {
            StopPlayerLinkLaser(id);
            throw new InvalidOperationException("Failed to create link laser entity.");
        }

        return id;
    }

    /// <summary>
    /// Stops an active player link laser.
    /// </summary>
    public void StopPlayerLinkLaser(Guid effectId)
    {
        if (_playerLinkLasers.TryGetValue(effectId, out var effect))
        {
            RemoveLaser(effect.Beam);
            _playerLinkLasers.Remove(effectId);
        }
    }

    /// <summary>
    /// Removes every active link laser.
    /// </summary>
    public void StopAllPlayerLinkLasers()
    {
        foreach (var effect in _playerLinkLasers.Values)
        {
            RemoveLaser(effect.Beam);
        }

        _playerLinkLasers.Clear();
    }

    internal void TickDynamicEffects()
    {
        TickPlayerLinkLasers();
        TickBeaconAnimations();
    }

    private void TickPlayerLinkLasers()
    {
        if (_playerLinkLasers.Count == 0)
            return;

        List<Guid>? staleEffects = null;

        foreach (var entry in _playerLinkLasers)
        {
            if (!UpdatePlayerLinkLaser(entry.Value))
            {
                staleEffects ??= new List<Guid>();
                staleEffects.Add(entry.Key);
            }
        }

        if (staleEffects == null)
            return;

        foreach (var effectId in staleEffects)
        {
            StopPlayerLinkLaser(effectId);
        }
    }

    private bool UpdatePlayerLinkLaser(PlayerLinkLaserEffect effect)
    {
        if (!IsPlayerRenderable(effect.PlayerA) || !IsPlayerRenderable(effect.PlayerB))
            return false;

        if (effect.ExpireAtUtc.HasValue && DateTimeOffset.UtcNow >= effect.ExpireAtUtc.Value)
            return false;

        var start = GetPlayerPosition(effect.PlayerA, effect.HeightOffset);
        var end = GetPlayerPosition(effect.PlayerB, effect.HeightOffset);

        if (!start.HasValue || !end.HasValue)
            return false;

        if (effect.Beam == null || !effect.Beam.IsValid)
        {
            effect.Beam = CreateLaser(start.Value, end.Value, effect.Width, effect.Color);
            return effect.Beam != null;
        }

        MoveLaser(effect.Beam, start.Value, end.Value);
        return true;
    }

    #endregion

    #region Beacon System

    /// <summary>
    /// Spawns an animated circular beacon that follows the provided player.
    /// </summary>
    /// <param name="player">Target player.</param>
    /// <param name="colorOverride">Optional custom color for the beacon segments.</param>
    /// <param name="segments">Number of laser segments to approximate the circle.</param>
    /// <param name="startRadius">Initial radius of the beacon.</param>
    /// <param name="radiusStep">How much the radius increases every update step.</param>
    /// <param name="durationSeconds">Lifetime of the animation. Zero or negative values make it run until removed.</param>
    /// <param name="heightOffset">Height offset above the player's origin.</param>
    /// <param name="stepIntervalSeconds">How often to move the beacon outward.</param>
    /// <param name="width">Beam width for each segment.</param>
    /// <param name="loop">If true, the beacon resets to the starting radius when it reaches the duration limit.</param>
    public Guid CreateBeaconAnimationOnPlayer(JBPlayer player, Color? colorOverride = null, int segments = 20, float startRadius = 20.0f,
        float radiusStep = 10.0f, float durationSeconds = 0.9f, float heightOffset = 5.0f, float stepIntervalSeconds = 0.1f, float width = 2.0f, bool loop = false)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (segments < 3)
            throw new ArgumentOutOfRangeException(nameof(segments), "Beacon animations require at least three segments.");

        if (!IsPlayerRenderable(player))
            throw new InvalidOperationException("Player must be valid and alive to draw a beacon.");

        var center = GetPlayerPosition(player);
        if (!center.HasValue)
            throw new InvalidOperationException("Player does not have a valid origin yet.");

        var color = colorOverride ?? GetDefaultBeaconColor(player);
        var beams = new List<CEnvBeam?>(segments);
        float angle = 0f;
        float angleStep = (float)(2 * Math.PI / segments);

        for (int i = 0; i < segments; i++)
        {
            var start = AngleOnCircle(center.Value, angle, startRadius, heightOffset);
            angle += angleStep;
            var end = AngleOnCircle(center.Value, angle, startRadius, heightOffset);

            beams.Add(CreateLaser(start, end, width, color));
        }

        var effect = new PlayerBeaconEffect(
            player,
            beams,
            color,
            startRadius,
            radiusStep,
            heightOffset,
            durationSeconds,
            width,
            TimeSpan.FromSeconds(Math.Max(stepIntervalSeconds, 0.05f)),
            loop
        );

        var id = Guid.NewGuid();
        _playerBeaconAnimations[id] = effect;

        return id;
    }

    /// <summary>
    /// Removes a running beacon animation from a player.
    /// </summary>
    public void StopPlayerBeacon(Guid beaconId)
    {
        if (_playerBeaconAnimations.TryGetValue(beaconId, out var effect))
        {
            foreach (var beam in effect.Segments)
            {
                RemoveLaser(beam);
            }

            _playerBeaconAnimations.Remove(beaconId);
        }
    }

    /// <summary>
    /// Stops all custom beacon animations.
    /// </summary>
    public void StopAllPlayerBeacons()
    {
        foreach (var effect in _playerBeaconAnimations.Values)
        {
            foreach (var beam in effect.Segments)
            {
                RemoveLaser(beam);
            }
        }

        _playerBeaconAnimations.Clear();
    }

    private void TickBeaconAnimations()
    {
        if (_playerBeaconAnimations.Count == 0)
            return;

        List<Guid>? staleEffects = null;

        foreach (var entry in _playerBeaconAnimations)
        {
            if (!UpdateBeaconAnimation(entry.Value))
            {
                staleEffects ??= new List<Guid>();
                staleEffects.Add(entry.Key);
            }
        }

        if (staleEffects == null)
            return;

        foreach (var effectId in staleEffects)
        {
            StopPlayerBeacon(effectId);
        }
    }

    private bool UpdateBeaconAnimation(PlayerBeaconEffect effect)
    {
        if (!IsPlayerRenderable(effect.Player))
            return false;

        if (effect.AngleStep <= 0f)
            return false;

        var now = DateTimeOffset.UtcNow;
        if (now < effect.NextStepAtUtc)
            return true;

        effect.NextStepAtUtc = now.Add(effect.StepInterval);

        var center = GetPlayerPosition(effect.Player);
        if (!center.HasValue)
            return false;

        float angle = 0f;
        for (int i = 0; i < effect.Segments.Count; i++)
        {
            var start = AngleOnCircle(center.Value, angle, effect.Radius, effect.HeightOffset);
            angle += effect.AngleStep;
            var end = AngleOnCircle(center.Value, angle, effect.Radius, effect.HeightOffset);

            var beam = effect.Segments[i];
            if (beam == null || !beam.IsValid)
            {
                beam = CreateLaser(start, end, effect.Width, effect.Color);
                effect.Segments[i] = beam;
            }
            else
            {
                MoveLaser(beam, start, end);
            }
        }

        effect.Radius += effect.RadiusStep;
        effect.ElapsedSeconds += (float)effect.StepInterval.TotalSeconds;

        if (effect.DurationSeconds > 0f && effect.ElapsedSeconds >= effect.DurationSeconds)
        {
            if (effect.Loop)
            {
                effect.Radius = effect.ResetRadius;
                effect.ElapsedSeconds = 0f;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

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

        StopAllPlayerBeacons();
    }

    #endregion

    #region Helper Methods

    private static bool IsPlayerRenderable(JBPlayer? player)
    {
        return player != null &&
            player.IsValid &&
            player.Controller != null &&
            player.Controller.PawnIsAlive &&
            player.PlayerPawn != null &&
            player.PlayerPawn.IsValid;
    }

    private static Vector? GetPlayerPosition(JBPlayer player, float heightOffset = 0f)
    {
        var absOrigin = player.PlayerPawn.AbsOrigin;
        if (!absOrigin.HasValue)
            return null;

        return new Vector(absOrigin.Value.X, absOrigin.Value.Y, absOrigin.Value.Z + heightOffset);
    }

    private static Vector AngleOnCircle(Vector center, float angle, float radius, float heightOffset)
    {
        var x = center.X + radius * (float)Math.Cos(angle);
        var y = center.Y + radius * (float)Math.Sin(angle);
        return new Vector(x, y, center.Z + heightOffset);
    }

    private static Color GetDefaultBeaconColor(JBPlayer player)
    {
        return player.Controller.TeamNum switch
        {
            (int)Team.T => Color.FromHex("#FF0000FF"),
            (int)Team.CT => Color.FromHex("#0000FFFF"),
            _ => Color.FromHex("#FFFFFFFF")
        };
    }

    public IPlayer? ResolvePlayerFromHandle(CHandle<CEntityInstance> handle)
    {
        if (!handle.IsValid)
            return null;

        var entity = handle.Value;
        if (entity == null)
            return null;

        foreach (var player in _Core.PlayerManager.GetAllPlayers())
        {
            try
            {
                if (player.PlayerPawn?.Address == entity.Address ||
                    player.Controller?.Address == entity.Address)
                    return player;
            }
            catch (NullReferenceException)
            {
                // Player state not fully initialized, skip
                continue;
            }
        }

        return null;
    }

    #endregion
}
