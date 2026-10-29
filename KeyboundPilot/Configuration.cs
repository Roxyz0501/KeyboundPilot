namespace KeyboundPilot;
public enum InputMode { Normal, Obstruction, LowHp }
public sealed record InputSettings(int MinimumMs=80,int MaximumMs=150,int WaitMin=0,int WaitMax=0,
    bool MistypeEnabled=false,int TypoMin=15,int TypoMax=25)
{
    [System.Text.Json.Serialization.JsonIgnore] public bool Valid => MinimumMs>=20 && MaximumMs<=5000 && MinimumMs<=MaximumMs
        && WaitMin>=0 && WaitMax<=60000 && WaitMin<=WaitMax && TypoMin>=1 && TypoMax<=10004 && TypoMin<=TypoMax;
}
public sealed class Configuration
{
    public InputSettings? ObstructionSettings {get;set;}
    public InputSettings LowHpSettings {get;set;} = new();
    public bool LowHpEnabled {get;set;}
    public bool LowHpFirst {get;set;}
    public int LowHpThreshold {get;set;} = 50;
    [System.Text.Json.Serialization.JsonIgnore] public InputSettings NormalInput => new(MinimumMs,MaximumMs,WaitMin,WaitMax,MistypeEnabled,TypoMin,TypoMax);
    [System.Text.Json.Serialization.JsonIgnore] public InputSettings ObstructionInput => ObstructionSettings
        ?? NormalInput with {MinimumMs=ObstructionMinimumMs,MaximumMs=ObstructionMaximumMs};
    public InputMode ResolveMode(bool obstructed,float? hpPercent)
    {
        var low=LowHpEnabled && hpPercent is >=0 and <=100 && hpPercent<LowHpThreshold;
        if(low && LowHpFirst)return InputMode.LowHp;
        if(obstructed && ObstructionIntervalEnabled)return InputMode.Obstruction;
        return low?InputMode.LowHp:InputMode.Normal;
    }
    public InputSettings ResolveInput(bool obstructed,float? hpPercent) => ResolveMode(obstructed,hpPercent) switch
    {InputMode.Obstruction=>ObstructionInput,InputMode.LowHp=>LowHpSettings,_=>NormalInput};
    public void SetNormalInput(InputSettings value)
    {
        MinimumMs=value.MinimumMs;MaximumMs=value.MaximumMs;BetweenWordsMinimumMs=value.WaitMin;BetweenWordsMaximumMs=value.WaitMax;
        MistypeEnabled=value.MistypeEnabled;MistypeMinimum=value.TypoMin;MistypeMaximum=value.TypoMax;
    }
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
    public (int Min,int Max) InputInterval(bool obstructed)
    {var value=ResolveInput(obstructed,null);return (value.MinimumMs,value.MaximumMs);}
    public bool DirectInput { get; set; } = true;
    public bool MistypeEnabled { get; set; }
    public int MistypeInterval { get; set; } = 20;
    public bool ShowDebug { get; set; }
    public bool RecordDiagnostics { get; set; }
    public int ExitDelayMs { get; set; }
    public int? ExitDelayMinimumMs { get; set; }
    public int? ExitDelayMaximumMs { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public int ExitMin => ExitDelayMinimumMs ?? ExitDelayMs;
    [System.Text.Json.Serialization.JsonIgnore] public int ExitMax => ExitDelayMaximumMs ?? ExitDelayMs;
    public void SetExitMinimum(int value) {ExitDelayMaximumMs=ExitMax;ExitDelayMinimumMs=Math.Clamp(value,0,ExitMax);}
    public void SetExitMaximum(int value) {ExitDelayMinimumMs=ExitMin;ExitDelayMaximumMs=Math.Clamp(value,ExitMin,60000);}
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
