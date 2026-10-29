namespace KeyboundPilot;

public sealed class ActivityKeeper
{
    public bool Requested {get;private set;}
    public void Begin() {Requested=true;Reset();}
    public void End() {Requested=false;Reset();}
    private long next;
    private int failures;
    public bool Failed=>failures>=3;
    public void Reset() {next=0;failures=0;}
    public void Tick(long now,bool active,Func<bool> touch)
    {
        if(!active){next=0;return;}
        if(Failed || now<next)return;
        next=now+10000;
        if(touch())failures=0;else failures++;
    }
}
