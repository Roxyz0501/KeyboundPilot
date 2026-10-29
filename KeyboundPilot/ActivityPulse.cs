namespace KeyboundPilot;

// Separate deadlines keep fast answer submission from flooding either input route.
internal sealed class ActivityPulse
{
    private long windowAt, systemAt;
    public bool WindowAccepted {get;private set;}
    public bool SystemAccepted {get;private set;}
    public void Tick(long now,Func<bool> window,Func<bool> system)
    {
        if(now>=windowAt) {windowAt=now+3000;WindowAccepted=window();}
        if(now>=systemAt) {systemAt=now+5000;SystemAccepted=system();}
    }
}
