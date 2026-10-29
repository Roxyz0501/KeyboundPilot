using FFXIVClientStructs.FFXIV.Client.UI.Misc;
namespace KeyboundPilot;

internal readonly record struct ActivitySnapshot(bool Available,ushort Territory,float Content,float Input,float Afk,float Limit,int Status);
internal static unsafe class ContentActivity
{
    public static long LastAttempt {get;private set;}
    public static bool LastSucceeded {get;private set;}
    public static long ResetCount {get;private set;}
    public static ActivitySnapshot Read()
    {
        var t=InputTimerModule.Instance();
        return t==null?default:new(true,t->TerritoryTypeId,t->ContentInputTimer,t->InputTimer,t->AfkTimer,t->InstanceContentAfkTimeLimit,t->Status);
    }
    public static bool Touch()
    {
        LastAttempt=Environment.TickCount64;LastSucceeded=false;
        var timers=InputTimerModule.Instance();
        if(timers==null || timers->TerritoryTypeId!=LoopGameUi.GameTerritory)return false;
        timers->ContentInputTimer=0;timers->InputTimer=0;
        if(timers->AfkTimer>0)timers->AfkTimer=0;
        LastSucceeded=timers->ContentInputTimer==0 && timers->InputTimer==0 && timers->AfkTimer<=0;
        if(LastSucceeded)ResetCount++;
        return LastSucceeded;
    }
}
