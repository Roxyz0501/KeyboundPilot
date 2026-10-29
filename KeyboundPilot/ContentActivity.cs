using FFXIVClientStructs.FFXIV.Client.UI.Misc;
namespace KeyboundPilot;

internal readonly record struct ActivitySnapshot(bool Available,ushort Territory,float Content,float Input,float Afk,float Limit,int Status,float Controller,float Mouse,KtgActivitySnapshot Ktg);
internal static unsafe class ContentActivity
{
    public static bool DedicatedSucceeded {get;private set;}
    public static long LastAttempt {get;private set;}
    public static bool LastSucceeded {get;private set;}
    public static long ResetCount {get;private set;}
    public static ActivitySnapshot Read()
    {
        var t=InputTimerModule.Instance();
        return t==null?default:new(true,t->TerritoryTypeId,t->ContentInputTimer,t->InputTimer,t->AfkTimer,t->InstanceContentAfkTimeLimit,t->Status,ActivityTimers.Maximum(t->ControllerInputTimers),ActivityTimers.Maximum(t->MouseInputTimers),t->TerritoryTypeId==LoopGameUi.GameTerritory?KtgActivity.Read():default);
    }
    public static bool Touch()
    {
        LastAttempt=Environment.TickCount64;LastSucceeded=DedicatedSucceeded=false;
        var timers=InputTimerModule.Instance();
        if(timers==null || timers->TerritoryTypeId!=LoopGameUi.GameTerritory)return false;
        DedicatedSucceeded=KtgActivity.Reset();
        LastSucceeded=ActivityTimers.Reset(timers) && DedicatedSucceeded;
        if(LastSucceeded)ResetCount++;
        return LastSucceeded;
    }
}
