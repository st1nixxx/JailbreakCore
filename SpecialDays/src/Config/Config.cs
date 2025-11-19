namespace SpecialDays;

public class DaysConfig
{
    public FreeForAllConfig FFA { get; set; } = new();
    public TeleportConfig Teleport { get; set; } = new();
    public NoScopeConfig NoScope { get; set; } = new();
    public OneInTheChamberConfig OneInTheChamber { get; set; } = new();
    public HeadshotOnlyConfig HeadshotOnly { get; set; } = new();
    public HideAndSeekConfig HideAndSeek { get; set; } = new();
    public KnifeFightConfig KnifeFight { get; set; } = new();
    public FreezeTagConfig FreezeTag { get; set; } = new();
}
public class FreeForAllConfig
{
    public bool Enable { get; set; } = true;
    public int DelayCooldwon { get; set; } = 15;
}
public class TeleportConfig
{
    public bool Enable { get; set; } = true;
    public int DelayCooldown { get; set; } = 15;
}
public class NoScopeConfig
{
    public bool Enable { get; set; } = true;
    public int DelayCooldown { get; set; } = 15;
    public float Gravity { get; set; } = 0.6f;
    public float Speed { get; set; } = 1.3f;
}
public class OneInTheChamberConfig
{
    public bool Enable { get; set; } = true;
    public int DelayCooldown { get; set; } = 15;
}
public class HeadshotOnlyConfig
{
    public bool Enable { get; set; } = true;
    public int DelayCooldown { get; set; } = 15;
    public string Weapon { get; set; } = "weapon_ak47";
}
public class HideAndSeekConfig
{
    public bool Enable { get; set; } = true;
    public int DelayCooldown { get; set; } = 45;
}
public class KnifeFightConfig
{
    public bool Enable { get; set; } = true;
    public int DelayCooldown { get; set; } = 15;
    public float Gravity { get; set; } = 0.6f; // if day type is knife (gravity)
    public float Speed { get; set; } = 2.3f; // if day type is knife (speed)
}
public class FreezeTagConfig
{
    public bool Enable { get; set; } = true;
    public int DelayCooldown { get; set; } = 15;
    public float UnfreezeRadius { get; set; } = 100.0f;
    public float UnfreezeTime { get; set; } = 3.0f; 
}
