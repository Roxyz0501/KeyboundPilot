using System.Runtime.InteropServices;
namespace KeyboundPilot;
internal static class ActivityInput
{
    [StructLayout(LayoutKind.Sequential)] private struct Point {public int X,Y;}
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] private static extern bool ScreenToClient(nint window,ref Point point);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window,out uint process);
    [DllImport("user32.dll",EntryPoint="PostMessageW",SetLastError=true)] private static extern bool PostMessage(nint window,uint message,nint keys,nint position);
    public static bool NotifyWindow()
    {
        var window=Keyboard.GameWindow;
        if(window==0 || GetWindowThreadProcessId(window,out var pid)==0 || pid!=(uint)Environment.ProcessId)return false;
        // Preserve the actual position; do not activate the game or synthesize a click.
        if(!GetCursorPos(out var point) || !ScreenToClient(window,ref point))return false;
        var x=Math.Clamp(point.X,short.MinValue,short.MaxValue);
        var y=Math.Clamp(point.Y,short.MinValue,short.MaxValue);
        return PostMessage(window,0x0200,0,(nint)((y & 0xffff)<<16 | (x & 0xffff)));
    }
}
