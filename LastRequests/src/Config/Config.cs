namespace LastRequests;

public class LRConfig
{
    public KnifeFight_LR KnifeFight { get; set; } = new();
}
public class KnifeFight_LR
{
    public bool Enable { get; set; } = true;
    public float Gravity { get; set; } = 0.5f;
    public float Speed { get; set; } = 5.0f;
    public int OneShotHealth { get; set; } = 1;
}
