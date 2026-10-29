using System.Runtime.InteropServices;
namespace KeyboundPilot;

internal readonly record struct ImeState(bool Available,bool Open,uint Conversion,bool Composing);
/// <summary>Operate the game window's IME on its framework thread; never set or paste input text.</summary>
internal sealed class ImeInput
{
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("imm32.dll")] private static extern nint ImmGetContext(nint hwnd);
    [DllImport("imm32.dll")] private static extern bool ImmReleaseContext(nint hwnd,nint context);
    [DllImport("imm32.dll")] private static extern bool ImmGetOpenStatus(nint context);
    [DllImport("imm32.dll")] private static extern bool ImmSetOpenStatus(nint context,bool open);
    [DllImport("imm32.dll")] private static extern bool ImmGetConversionStatus(nint context,out uint conversion,out uint sentence);
    [DllImport("imm32.dll")] private static extern bool ImmSetConversionStatus(nint context,uint conversion,uint sentence);
    [DllImport("imm32.dll",EntryPoint="ImmGetCompositionStringW")] private static extern int CompositionLength(nint context,uint index,nint buffer,uint size);
    private nint window,savedContext;
    private bool originalOpen,owned;
    private uint originalConversion,originalSentence;
    public ImeState Inspect(bool background=false)
    {
        if(!background && !Keyboard.ForegroundGame)return default;
        var hwnd=background?Keyboard.GameWindow:GetForegroundWindow();if(hwnd==0)return default;var context=ImmGetContext(hwnd);if(context==0)return default;
        try {var valid=ImmGetConversionStatus(context,out var conversion,out _);var length=CompositionLength(context,8,0,0);
            return new(valid && length>=-1,ImmGetOpenStatus(context),conversion,length>0);}
        finally {ImmReleaseContext(hwnd,context);}
    }
    public bool Prepare(bool japanese)
    {
        if(!Keyboard.ForegroundGame)return false;
        var hwnd=GetForegroundWindow();var context=ImmGetContext(hwnd);if(context==0)return !japanese;
        try
        {
            var length=CompositionLength(context,8,0,0);
            if(!ImmGetConversionStatus(context,out var conversion,out var sentence) || length>0 || length< -1)return false;
            if(!owned) {window=hwnd;savedContext=context;originalOpen=ImmGetOpenStatus(context);originalConversion=conversion;originalSentence=sentence;owned=true;}
            if(window!=hwnd || savedContext!=context)return false;
            if(ImmGetOpenStatus(context)!=japanese && !ImmSetOpenStatus(context,japanese))return false;
            var desired=(conversion|0x09u)&~0x02u; // NATIVE | FULLSHAPE, clear KATAKANA; preserve the IME's ROMAN bit.
            if(japanese && desired!=conversion && !ImmSetConversionStatus(context,desired,sentence))return false;
            return true;
        }
        finally {ImmReleaseContext(hwnd,context);}
    }
    public void Restore()
    {
        if(!owned)return;owned=false;
        var context=ImmGetContext(window);if(context==0)return;
        try
        {
            // Do not commit/cancel remaining composition as a cleanup side effect.
            if(context==savedContext && CompositionLength(context,8,0,0) is 0 or -1)
            {ImmSetConversionStatus(context,originalConversion,originalSentence);ImmSetOpenStatus(context,originalOpen);}
        }
        finally {ImmReleaseContext(window,context);window=savedContext=0;}
    }
}
