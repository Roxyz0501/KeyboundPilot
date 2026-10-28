using FFXIVClientStructs.FFXIV.Client.UI.Misc;
namespace KeyboundPilot;

internal static unsafe class ContentActivity
{
    public static bool Touch()
    {
        var timers=InputTimerModule.Instance();
        if(timers==null || timers->TerritoryTypeId!=LoopGameUi.GameTerritory)return false;
        // Refresh client activity only; never alter timeout limits, configuration, or send a gameplay action.
        timers->ContentInputTimer=0;timers->InputTimer=0;
        if(timers->AfkTimer>0)timers->AfkTimer=0;
        return true;
    }
}
