namespace KeyboundPilot;

public enum RunGate { Run, Arming, InputUnavailable, OutsideContent, Escape, ForegroundLost, Modifier, UiCapture, OtherTextInput }
public sealed class AutomationLifecycle
{
    public bool Consumed {get;private set;}
    public void Start(long now) {Consumed=true;}
    public void Suppress()=>Consumed=true;
    public void Reset() {Consumed=false;}
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
        if(!inputReady)return RunGate.InputUnavailable;
        if(uiCapture)return RunGate.UiCapture;
        if(otherTextInput)return RunGate.OtherTextInput;
        return RunGate.Run;
    }
}

public enum FocusResult { Waiting, Requested, Ready, Exhausted }
public sealed class FocusAcquisition
{
    private long observeAfter,retryAfter;
    public int Attempts {get;private set;}
    public void Reset() {Attempts=0;observeAfter=retryAfter=0;}
    public FocusResult Step(long now,bool focused,Action request)
    {
        if(focused) {Reset();return FocusResult.Ready;}
        if(now<observeAfter)return FocusResult.Waiting;
        if(Attempts>=3)
        {
            if(retryAfter==0)retryAfter=now+2000;
            if(now<retryAfter)return FocusResult.Exhausted;
            Reset();
        }
        Attempts++;observeAfter=now+250;request();return FocusResult.Requested;
    }
}
