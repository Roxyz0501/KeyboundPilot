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
    public static bool Modifier => new[] { 0x10, 0x11, 0x12, 0x5B, 0x5C }.Any(k => (GetAsyncKeyState(k) & 0x8000) != 0);

    public static bool TryMap(char c, out ushort scan)
    {
        scan = 0;
        if (!ForegroundGame) return false;
        if (c == '\r') { scan = 0x1C; return true; } // physical Enter
        if (c is < ' ' or > '~') return false;
        var thread = GetWindowThreadProcessId(GetForegroundWindow(), out _);
        var layout = GetKeyboardLayout(thread);
        // The minigame displays capitals, but physical letter keys need no Shift.
        var key = VkKeyScanEx(char.ToLowerInvariant(c), layout);
        if (key == -1 || (key & 0xFF00) != 0) return false; // don't synthesize modifiers or guess layout
        var mapped = MapVirtualKeyEx((uint)(key & 255), 0, layout);
        if (mapped == 0 || mapped > 255) return false;
        scan = (ushort)mapped;
        return true;
    }
    private static ushort held;
    private static long releaseAt;
    private static int releaseFailures;
    public static bool Release(bool force = false)
    {
        if (held == 0 || (!force && Environment.TickCount64 < releaseAt)) return true;
        if (releaseFailures >= 3 && !force) return false;
        if (SendInput(1, new[] { Make(held, true) }, Marshal.SizeOf<Input>()) == 1)
        { held = 0; releaseFailures = 0; return true; }
        releaseFailures++;
        return false;
    }
    public static bool Send(char c)
    {
        if (!ForegroundGame || Modifier || Escape || !TryMap(c, out var scan)) return false;
        if (held != 0) return false;
        // Keep the key down across a frame so polled keyboard consumers can observe it.
        var sent = SendInput(1, new[] { Make(scan, false) }, Marshal.SizeOf<Input>());
        if (sent != 1) return false;
        held = scan; releaseAt = Environment.TickCount64 + 10; releaseFailures = 0;
        return true;
    }
    private static Input Make(ushort scan, bool up) => new()
    { Type = 1, Data = new InputUnion { Keyboard = new KeyboardInput { Scan = scan, Flags = 0x0008u | (up ? 0x0002u : 0) } } };
}
