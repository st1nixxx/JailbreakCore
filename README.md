<div align="center">
  <img src="https://pan.samyyc.dev/s/VYmMXE" />
  <h2><strong>JailbreakCore</strong></h2>
  <h3>A comprehensive Jailbreak framework for Counter-Strike 2 with Swiftly</h3>
</div>

<p align="center">
  <img src="https://img.shields.io/badge/build-passing-brightgreen" alt="Build Status">
  <img src="https://img.shields.io/github/downloads/T3Marius/JailbreakCore/total" alt="Downloads">
  <img src="https://img.shields.io/github/stars/T3Marius/JailbreakCore?style=flat&logo=github" alt="Stars">
  <img src="https://img.shields.io/github/license/T3Marius/JailbreakCore" alt="License">
</p>

---

## Overview

JailbreakCore is a modular jailbreak plugin system built for CS2 with Swiftly. It provides developers with a clean API to extend functionality through custom Last Requests and Special Days, while offering a complete out-of-the-box jailbreak experience.

The core plugin handles all the heavy lifting - role management, warden mechanics, prisoner systems, and event handling - so you can focus on creating unique gameplay experiences.

---

## Core Features

### Player Role System
- **Warden Management** - Automatic assignment, manual takeover via commands, and configurable warden menu
- **Role Tracking** - Guardian, Prisoner, Rebel, Freeday roles with automatic state management
- **Color System** - Visual indicators for rebels (red), freedays (green), and warden (blue)
- **Model Override** - Custom player models for each role

### Warden Tools
- **Cell Control** - Toggle cell doors open/closed
- **Boxing System** - Enable/disable damage restriction for boxing events
- **Freeday Management** - Grant freedays to specific prisoners through an interactive menu
- **Color Marking** - Mark prisoners with custom colors via menu selection
- **Warden Commands** - `/w`, `/uw`, `/box`, `/wmenu`

### Prisoner Systems
- **Surrender System** - Rebels can surrender with limited attempts per round (`/surrender`)
- **Heal Request** - Request healing from the warden (configurable tries per round via `/heal`)
- **Voice Control** - Automatic prisoner muting with configurable duration
- **Rebel Detection** - Automatic rebel status when attacking guards

### Last Request System
The plugin provides a complete LR framework:
- Start LR when conditions are met (typically 1v1 scenarios)
- Preparation timer with HTML notifications
- Weapon selection for gun-based LRs
- Winner/loser tracking
- Automatic warden removal during LR
- **Included LR**: Knife Fight (via separate plugin)

### Special Day System
Full special day support with cooldown management:
- **Free For All** - Everyone against everyone
- **Hide and Seek** - CTs hunt invisible Ts
- **Headshot Only** - Only headshots deal damage
- **One in the Chamber** - One bullet, one kill
- **No Scope** - Scoped weapons disabled
- **Teleport Day** - Random teleportation chaos
- **Knife Fight** - Melee only (with speed/gravity variants)
- Cooldown system between special days (configurable)

### Additional Features
- **Bunnyhop Control** - Toggle bhop on/off with cooldown system
- **Audio Integration** - Sound effects for warden events, rebels, and boxing
- **Interactive Menus** - Full menu system for warden, special days, and last requests
- **Translation Support** - Multi-language support via localization files
- **Damage Hooks** - Native damage hook system for custom gameplay modifications

---

## Developer API

The `IJailbreakApi` interface exposes everything you need:

```csharp
public interface IJailbreakApi
{
    ILastRequestService LastRequest { get; }
    ISpecialDayService SpecialDay { get; }
    IPlayerService Players { get; }
    IHookService Hooks { get; }
    IUtilityService Utilities { get; }
}
```

### Creating Last Requests

Register your own LR implementations:

```csharp
public class MyCustomLR : ILastRequest
{
    public string Name => "My LR";
    public IJBPlayer? Prisoner { get; set; }
    public IJBPlayer? Guardian { get; set; }

    public void Start(IJBPlayer guardian, IJBPlayer prisoner)
    {
        // Your LR logic here
    }

    public void End(IJBPlayer? winner, IJBPlayer? loser)
    {
        // Cleanup
    }
}

// Register it
Api.LastRequest.Register(new MyCustomLR());
```

### Creating Special Days

Same clean interface for special days:

```csharp
public class MyCustomDay : ISpecialDay
{
    public string Name => "My Special Day";
    public string Description => "Something fun happens";

    public void Start()
    {
        // Setup your special day
    }

    public void End()
    {
        // Cleanup
    }
}

// Register it
Api.SpecialDay.Register(new MyCustomDay());
```

### IJBPlayer Interface

Work with enhanced player objects:

```csharp
var jbPlayer = Api.Players.GetOrCreate(player);

jbPlayer.SetWarden(true);
jbPlayer.SetRebel(true);
jbPlayer.SetFreeday(true);
jbPlayer.StripWeapons(keepKnife: true);
jbPlayer.Print(IHud.Chat, "custom_message_key", args: param1, param2);
```

### Damage Hooks

Subscribe to damage events:

```csharp
Api.Hooks.SubscribeTakeDamage((context) =>
{
    var attacker = context.Attacker;
    var victim = context.Victim;
    var damage = context.Info.Damage;

    // Modify damage
    context.Info.Damage *= 2.0f;

    return HookResult.Continue; // or HookResult.Handled to block damage
});
```

---

## Project Structure

```
JailbreakCore/           # Core plugin
├── src/
│   ├── Api/             # IJailbreakApi implementation
│   ├── Commands/        # All game commands
│   ├── Config/          # Configuration models
│   ├── Extensions/      # Helper methods
│   ├── Hooks/           # Native damage hooks
│   ├── JBPlayer/        # Enhanced player management
│   ├── LastRequest/     # LR system core
│   ├── SpecialDay/      # SD system core
│   └── Menus/           # Interactive menus
└── resources/
    ├── gamedata/        # Signatures, offsets, patches
    └── translations/    # Language files

JailbreakApi/            # Shared interfaces
├── IJailbreakApi.cs
├── IJBPlayer.cs
├── ILastRequest.cs
└── ISpecialDay.cs

LastRequests/            # Default LR implementations
└── src/Library/

SpecialDays/             # Default SD implementations
└── src/Library/
```

---

## Installation

1. Build the solution or download the latest release
2. Place `JailbreakCore` folder in your Swiftly plugins directory
3. Place `LastRequests` and `SpecialDays` folders (optional, but recommended)
4. Configure via `config.toml` in each plugin folder
5. Restart server

---

## Configuration

The plugin is highly configurable. Check `config.toml` for:
- Warden settings and commands
- Prisoner settings (surrender tries, heal tries, mute duration)
- Color configurations
- Audio paths and volumes
- Special day cooldowns
- Bunnyhop settings
- Custom models

---

## Roadmap

### Planned Features

#### Warden Tools
- [x] Warden laser pointer - Hold E to show a laser where he looks.
- [x] Ping beacon for warden - Let warden place markers on the map
- [ ] Expand warden menu - More tools and options for better control

#### Last Request Improvements
- [ ] Last request beacons - Mark LR participants with glowing beacons
- [ ] More last requests - Shotgun wars, mag-4-mag, race, and more

Feel free to contribute or suggest features!

---

## Credits

**Author:** T3Marius
**Built for:** Swiftly (CS2)
**License:** Check repository license

If you use this in your server, a star on the repo would be appreciated!
