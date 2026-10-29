using FFXIVClientStructs.FFXIV.Client.UI.Misc;
namespace KeyboundPilot;

internal readonly record struct ActivitySnapshot(bool Available,ushort Territory,float Content,float Input,float Afk,float Limit,int Status,float Controller,float Mouse);
internal static unsafe class ContentActivity
{
    private static readonly ActivityPulse pulse=new();
    public static bool WindowAccepted=>pulse.WindowAccepted;
    public static bool SystemAccepted=>pulse.SystemAccepted;
    public static long LastAttempt {get;private set;}
    public static bool LastSucceeded {get;private set;}
    public static long ResetCount {get;private set;}
    public static ActivitySnapshot Read()
    {
        var t=InputTimerModule.Instance();
        return t==null?default:new(true,t->TerritoryTypeId,t->ContentInputTimer,t->InputTimer,t->AfkTimer,t->InstanceContentAfkTimeLimit,t->Status,ActivityTimers.Maximum(t->ControllerInputTimers),ActivityTimers.Maximum(t->MouseInputTimers));
    }
    public static bool Touch()
    {
        LastAttempt=Environment.TickCount64;LastSucceeded=false;
        var timers=InputTimerModule.Instance();
        if(timers==null || timers->TerritoryTypeId!=LoopGameUi.GameTerritory)return false;
        pulse.Tick(LastAttempt,ActivityInput.NotifyWindow,Keyboard.PulseActivity);
        LastSucceeded=ActivityTimers.Reset(timers);
        if(LastSucceeded)ResetCount++;
        return LastSucceeded && pulse.WindowAccepted && pulse.SystemAccepted;
    }
}
