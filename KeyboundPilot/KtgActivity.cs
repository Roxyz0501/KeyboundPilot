using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
namespace KeyboundPilot;

internal readonly record struct KtgActivitySnapshot(bool Available,float IdleSeconds,float SameKeySeconds);

internal static unsafe class KtgActivity
{
    // Layout cross-checked against the 2026.10.16 client constructor and Update.
    // Update compares IdleSeconds with 60 and invokes duty withdrawal. After
    // repeated identical keys it can copy SameKeySeconds back to IdleSeconds.
    [StructLayout(LayoutKind.Explicit,Size=0x60)]
    internal struct InputAgent
    {
        [FieldOffset(0)] public AgentInterface Agent;
        [FieldOffset(0x28)] public AgentInterface* Hud;
        [FieldOffset(0x50)] public float IdleSeconds;
        [FieldOffset(0x54)] public float SameKeySeconds;
    }
    private static bool Resolve(out InputAgent* input,out AgentInterface* hud)
    {
        input=null;hud=null;
        var module=AgentModule.Instance();
        if(module==null)return false;
        hud=module->GetAgentByInternalId(AgentId.KTGHud);
        input=(InputAgent*)module->GetAgentByInternalId((AgentId)398);
        return Valid(input,hud);
    }
    internal static bool Valid(InputAgent* input,AgentInterface* hud) => input!=null && hud!=null
        && (AgentInterface*)input!=hud && input->Hud==hud
        && hud->UIModuleInterface!=null && input->Agent.UIModuleInterface==hud->UIModuleInterface
        && float.IsFinite(input->IdleSeconds) && input->IdleSeconds>=0
        && float.IsFinite(input->SameKeySeconds) && input->SameKeySeconds>=0;
    public static KtgActivitySnapshot Read() => Resolve(out var input,out _)
        ? new(true,input->IdleSeconds,input->SameKeySeconds) : default;
    public static bool Reset() => Resolve(out var input,out var hud) && Reset(input,hud);
    internal static bool Reset(InputAgent* input,AgentInterface* hud)
    {
        if(!Valid(input,hud))return false;
        input->IdleSeconds=0;input->SameKeySeconds=0;
        return input->IdleSeconds==0 && input->SameKeySeconds==0;
    }
}
