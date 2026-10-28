namespace KeyboundPilot;
public sealed class Configuration
{
    public int Version { get; set; } = 1;
    public string? Language { get; set; }
    public int MinimumMs { get; set; } = 80;
    public int MaximumMs { get; set; } = 150;
    public void SetRange(int min, int max)
    {
        MinimumMs = Math.Clamp(min, 20, 5000);
        MaximumMs = Math.Clamp(max, MinimumMs, 5000);
    }
}
