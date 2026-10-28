namespace KeyboundPilot;

public enum RunGate { Run, Arming, InputUnavailable, OutsideContent, Escape, ForegroundLost, Modifier, UiCapture, OtherTextInput }
public sealed class AutomationLifecycle
{
    public bool Consumed {get;private set;}
    private long armUntil;
    public void Start(long now) {Consumed=true;armUntil=now+3000;}
    public void Suppress()=>Consumed=true;
    public void Reset() {Consumed=false;armUntil=0;}
    public bool TryAutoStart(bool inContent,bool enabled,bool playable,bool eligible)
    {
        if(!inContent) {Reset();return false;}
        if(Consumed || !enabled || !playable || !eligible)return false;
        Consumed=true;return true;
    }
    public RunGate Evaluate(long now,bool inContent,bool escape,bool foreground,bool modifier,bool inputReady,bool uiCapture,bool otherTextInput)
    {
        if(!inContent)return RunGate.OutsideContent;
        if(escape)return RunGate.Escape;
        if(!foreground)return RunGate.ForegroundLost;
        if(modifier)return RunGate.Modifier;
        // Start-button capture and the pre-game input state must not defeat the arming period.
        if(now<armUntil)return RunGate.Arming;
        if(!inputReady)return RunGate.InputUnavailable;
        if(uiCapture)return RunGate.UiCapture;
        if(otherTextInput)return RunGate.OtherTextInput;
        return RunGate.Run;
    }
}

public enum FocusResult { Waiting, Requested, Ready, Exhausted }
public sealed class FocusAcquisition
{
    private long observeAfter;
    public int Attempts {get;private set;}
    public void Reset() {Attempts=0;observeAfter=0;}
    public FocusResult Step(long now,bool focused,Action request)
    {
        if(focused) {Reset();return FocusResult.Ready;}
        if(now<observeAfter)return FocusResult.Waiting;
        if(Attempts>=3)return FocusResult.Exhausted;
        Attempts++;observeAfter=now+250;request();return FocusResult.Requested;
    }
}
