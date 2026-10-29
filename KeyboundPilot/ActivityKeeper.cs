namespace KeyboundPilot;

public sealed class ActivityKeeper
{
    public bool Requested {get;private set;}
    public void Begin() {Requested=true;Reset();}
    public void End() {Requested=false;Reset();}
    private long next;
    private int failures;
    public bool Failed=>failures>=3;
    public void OnSubmission(long now,Func<bool> touch)
    {
        // A real answer is a new retry opportunity, even after periodic attempts failed.
        if(!Requested)return;
        if(touch())failures=0;else failures++;
        next=now+3000;
    }
    public void Reset() {next=0;failures=0;}
    public void Tick(long now,bool active,Func<bool> touch)
    {
        if(!active){next=0;return;}
        if(Failed || now<next)return;
        next=now+3000;
        if(touch())failures=0;else failures++;
    }
}
