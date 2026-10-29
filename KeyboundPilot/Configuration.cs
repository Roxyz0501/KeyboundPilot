namespace KeyboundPilot;
public sealed class Configuration
{
    public int? BetweenWordsMinimumMs { get; set; }
    public int? BetweenWordsMaximumMs { get; set; }
    public int? MistypeMinimum { get; set; }
    public int? MistypeMaximum { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public int WaitMin => BetweenWordsMinimumMs ?? BetweenWordsMs;
    [System.Text.Json.Serialization.JsonIgnore] public int WaitMax => BetweenWordsMaximumMs ?? BetweenWordsMs;
    [System.Text.Json.Serialization.JsonIgnore] public int TypoMin => MistypeMinimum ?? Math.Max(1,MistypeInterval-5);
    [System.Text.Json.Serialization.JsonIgnore] public int TypoMax => MistypeMaximum ?? MistypeInterval+5;
    public void SetWaitMinimum(int value) {BetweenWordsMaximumMs=WaitMax;BetweenWordsMinimumMs=Math.Clamp(value,0,WaitMax);}
    public void SetWaitMaximum(int value) {BetweenWordsMinimumMs=WaitMin;BetweenWordsMaximumMs=Math.Clamp(value,WaitMin,60000);}
    public void SetTypoMinimum(int value) {MistypeMaximum=TypoMax;MistypeMinimum=Math.Clamp(value,1,TypoMax);}
    public void SetTypoMaximum(int value) {MistypeMinimum=TypoMin;MistypeMaximum=Math.Clamp(value,TypoMin,10004);}
    public bool ExcludeUnseenPrompts { get; set; } = true;
    public bool ObstructionIntervalEnabled { get; set; }
    public int ObstructionMinimumMs { get; set; } = 150;
    public int ObstructionMaximumMs { get; set; } = 250;
    public void SetObstructionMinimum(int value) => ObstructionMinimumMs = Math.Clamp(value,20,ObstructionMaximumMs);
    public void SetObstructionMaximum(int value) => ObstructionMaximumMs = Math.Clamp(value,ObstructionMinimumMs,5000);
    public (int Min,int Max) InputInterval(bool obstructed) => ObstructionIntervalEnabled && obstructed
        ? (ObstructionMinimumMs,ObstructionMaximumMs) : (MinimumMs,MaximumMs);
    public bool DirectInput { get; set; }
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
