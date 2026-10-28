namespace KeyboundPilot;
public sealed class Configuration
{
    public bool MistypeEnabled { get; set; }
    public int MistypeInterval { get; set; } = 20;
    public bool ShowDebug { get; set; }
    public bool RecordDiagnostics { get; set; }
    public int LoopCount { get; set; } = 1;
    public bool AutoStart { get; set; } = true;
    public int Version { get; set; } = 1;
    public string? Language { get; set; }
    public int MinimumMs { get; set; } = 80;
    public int MaximumMs { get; set; } = 150;
    public int BetweenWordsMs { get; set; }
    public void SetMinimum(int value) => MinimumMs = Math.Clamp(value, 20, MaximumMs);
    public void SetMaximum(int value) => MaximumMs = Math.Clamp(value, MinimumMs, 5000);
    public void SetRange(int min, int max)
    {
        MinimumMs = Math.Clamp(min, 20, 5000);
        MaximumMs = Math.Clamp(max, MinimumMs, 5000);
    }
}
