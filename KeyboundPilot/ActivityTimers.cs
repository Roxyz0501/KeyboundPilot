using FFXIVClientStructs.FFXIV.Client.UI.Misc;
namespace KeyboundPilot;
internal static unsafe class ActivityTimers
{
    public static bool Reset(InputTimerModule* timers)
    {
        if(timers==null)return false;
        timers->ContentInputTimer=timers->InputTimer=0;
        if(timers->AfkTimer>0)timers->AfkTimer=0;
        foreach(ref var device in timers->ControllerInputTimers)device.Timer=0;
        foreach(ref var device in timers->MouseInputTimers)device.Timer=0;
        return timers->ContentInputTimer==0 && timers->InputTimer==0 && timers->AfkTimer<=0
            && Maximum(timers->ControllerInputTimers)==0 && Maximum(timers->MouseInputTimers)==0;
    }
    public static float Maximum(Span<InputTimerModule.InputTimerData> devices)
    {
        var maximum=0f;foreach(ref var device in devices)maximum=Math.Max(maximum,device.Timer);return maximum;
    }
}
