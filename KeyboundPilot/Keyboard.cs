using System.Runtime.InteropServices;

namespace KeyboundPilot;

/// <summary>Real OS keyboard events. No game memory writes, callbacks, paste or WM_CHAR.</summary>
internal static class Keyboard
{
    [StructLayout(LayoutKind.Sequential)] private struct Input { public uint Type; public InputUnion Data; }
    [StructLayout(LayoutKind.Explicit)] private struct InputUnion
    {
        [FieldOffset(0)] public KeyboardInput Keyboard;
        [FieldOffset(0)] public MouseInput Mouse; // union must have Win32 INPUT's full size (40 bytes on x64)
    }
    [StructLayout(LayoutKind.Sequential)] private struct KeyboardInput
    { public ushort VirtualKey, Scan; public uint Flags, Time; public nuint ExtraInfo; }
    [StructLayout(LayoutKind.Sequential)] private struct MouseInput
    { public int X, Y; public uint MouseData, Flags, Time; public nuint ExtraInfo; }
    [DllImport("user32.dll", SetLastError = true)] private static extern uint SendInput(uint count, Input[] inputs, int size);
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint hwnd, out uint pid);
    [DllImport("user32.dll")] private static extern uint MapVirtualKeyEx(uint code, uint mapType, nint layout);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern short VkKeyScanEx(char c, nint layout);
    [DllImport("user32.dll")] private static extern nint GetKeyboardLayout(uint threadId);
    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int key);
    public static int InputSize => Marshal.SizeOf<Input>();
    public static bool ForegroundGame
    {
        get { GetWindowThreadProcessId(GetForegroundWindow(), out var pid); return pid == (uint)Environment.ProcessId; }
    }
    public static bool Escape => (GetAsyncKeyState(0x1B) & 0x8000) != 0;
    public static bool Modifier => new[] {0x11,0x12,0x5B,0x5C,0xA1}.Any(k=>(GetAsyncKeyState(k)&0x8000)!=0)
        || (!heldShift && (GetAsyncKeyState(0xA0)&0x8000)!=0);
    internal static bool SupportedModifiers(short mapping)=>mapping!=-1 && (mapping&0xFF00) is 0 or 0x100;

    public static bool TryMap(char c, out ushort scan)=>TryMap(c,out scan,out _);
    private static bool TryMap(char c,out ushort scan,out bool shift)
    {
        scan = 0;shift=false;
        if (!ForegroundGame) return false;
        if(c=='\b') {scan=0x0E;return true;}
        if(c==TypingSession.HiraganaKey) {scan=0x40;return true;} // F6: IME hiragana conversion
        if (c == '\r') { scan = 0x1C; return true; } // physical Enter
        if (c is < ' ' or > '~') return false;
        var thread = GetWindowThreadProcessId(GetForegroundWindow(), out _);
        var layout = GetKeyboardLayout(thread);
        // The minigame displays capitals, but physical letter keys need no Shift.
        var key = VkKeyScanEx(char.ToLowerInvariant(c), layout);
        if(!SupportedModifiers(key))return false; // Never synthesize Ctrl/Alt/AltGr.
        shift=(key&0x100)!=0;
        var mapped = MapVirtualKeyEx((uint)(key & 255), 0, layout);
        if (mapped == 0 || mapped > 255) return false;
        scan = (ushort)mapped;
        return true;
    }
    private static ushort held;
    private static bool heldShift;
    private static long releaseAt;
    private static int releaseFailures;
    public static bool Release(bool force = false)
    {
        if ((held == 0 && !heldShift) || (!force && Environment.TickCount64 < releaseAt)) return true;
        if (releaseFailures >= 3 && !force) return false;
        var releases=new List<Input>();if(held!=0)releases.Add(Make(held,true));if(heldShift)releases.Add(Make(0x2A,true));
        if(SendInput((uint)releases.Count,releases.ToArray(),Marshal.SizeOf<Input>())==releases.Count)
        {held=0;heldShift=false;releaseFailures=0;return true;}
        releaseFailures++;
        return false;
    }
    public static bool Send(char c)
    {
        if (!ForegroundGame || Modifier || Escape || !TryMap(c,out var scan,out var shift)) return false;
        if(held!=0 || heldShift)return false;
        // Keep the key down across a frame so polled keyboard consumers can observe it.
        var presses=shift?new[]{Make(0x2A,false),Make(scan,false)}:new[]{Make(scan,false)};
        var sent=SendInput((uint)presses.Length,presses,Marshal.SizeOf<Input>());
        if(sent!=presses.Length)
        {if(shift && sent>0){heldShift=true;Release(true);}return false;}
        held=scan;heldShift=shift;releaseAt=Environment.TickCount64+10;releaseFailures=0;
        return true;
    }
    private static Input Make(ushort scan, bool up) => new()
    { Type = 1, Data = new InputUnion { Keyboard = new KeyboardInput { Scan = scan, Flags = 0x0008u | (up ? 0x0002u : 0) } } };
}
