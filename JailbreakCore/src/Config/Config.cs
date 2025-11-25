namespace JailbreakCore;

public class JailbreakConfig
{
    public WardenConfig Warden { get; set; } = new();
    public PrisonerConfig Prisoner { get; set; } = new();
    public ColorsConfig Colors { get; set; } = new();
    public SoundsConfig Sounds { get; set; } = new();
    public ModelsConfig Models { get; set; } = new();
    public SpecialDayConfig SpecialDay { get; set; } = new();
    public Bunnyhoop_Config Bunnyhoop { get; set; } = new();
    public LastRequestConfig LastRequest { get; set; } = new();
}
public class WardenConfig
{
    public bool ShowMenuOnSet { get; set; } = true;
    public WardenCommands Commands { get; set; } = new();
}
public class SoundsConfig
{
    public WardenKilled_Sound WardenKilled { get; set; } = new();
    public WardenTake_Sound WardenTake { get; set; } = new();
    public WardenRemoved_Sound WardenRemoved { get; set; } = new();
    public Rebel_Sound Rebel { get; set; } = new();
    public Box_Sound Box { get; set; } = new();
    public Countdown5_Sound Countdown5 { get; set; } = new();
    public Countdown10_Sound Countdown10 { get; set; } = new();
}
public class WardenCommands
{
    public List<string> TakeWarden { get; set; } = ["w", "warden"];
    public List<string> GiveUpWarden { get; set; } = ["uw", "unwarden"];
    public List<string> ToggleBox { get; set; } = ["box"];
    public List<string> WardenMenu { get; set; } = ["wmenu"];
    public List<string> SDMenu { get; set; } = ["sd"];
}
public class PrisonerConfig
{
    public PrisonerCommands Commands { get; set; } = new();
    public int SurrenderTriesXRound { get; set; } = 2;
    public int HealTriesXRound { get; set; } = 2;
    public int PrisonerMuteDuration { get; set; } = 30;
    public bool UnmutePrisonerOnRoundEnd { get; set; } = true;


}
public class PrisonerCommands
{
    public List<string> Surrender { get; set; } = ["surrender"];
    public List<string> LRMenu { get; set; } = ["lr", "lastrequest"];
    public List<string> HealRequest { get; set; } = ["h", "heal"];
}
public class ModelsConfig
{
    public string Warden { get; set; } = string.Empty;
    public string Guardian { get; set; } = string.Empty;
    public string Prisoner { get; set; } = string.Empty;
}
public class ColorsConfig
{
    public List<int> RebelColor { get; set; } = [255, 0, 0];
    public List<int> FreedayColor { get; set; } = [0, 255, 0];
    public List<int> WardenColor { get; set; } = [0, 0, 255];
}
public class SpecialDayConfig
{
    public int CooldownInRounds { get; set; } = 3;
}
public class WardenTake_Sound
{
    public string Path { get; set; } = "warden_take.mp3";
    public float Volume { get; set; } = 0.5f;
}
public class WardenRemoved_Sound
{
    public string Path { get; set; } = "warden_removed.mp3";
    public float Volume { get; set; } = 0.5f;
}
public class WardenKilled_Sound
{
    public string Path { get; set; } = "warden_killed.mp3";
    public float Volume { get; set; } = 0.5f;
}
public class Rebel_Sound
{
    public string Path { get; set; } = "rebel.mp3";
    public float Volume { get; set; } = 0.5f;
}
public class Box_Sound
{
    public string Path { get; set; } = "box.mp3";
    public float Volume { get; set; } = 0.5f;
}
public class Countdown5_Sound
{
    public string Path { get; set; } = "countdown5.mp3";
    public float Volume { get; set; } = 0.5f;
}
public class Countdown10_Sound
{
    public string Path { get; set; } = "countdown10.mp3";
    public float Volume { get; set; } = 0.5f;
}
public class Bunnyhoop_Config
{
    public bool Enable { get; set; } = true;
    public int RoundStartCooldown { get; set; } = 30;
    public bool EnableOnLastRequest { get; set; } = true;
    public bool EnableOnSpecialDay { get; set; } = true;
}
public class LastRequestConfig
{
    public int PrepCountdownSeconds { get; set; } = 15;
    public bool ShowHtmlCountdown { get; set; } = true;
    public bool EnableLinkLaser { get; set; } = true;
    public bool EnablePlayerBeacons { get; set; } = true;
}
