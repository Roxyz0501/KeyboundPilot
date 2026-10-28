using System.Text.Json;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Game.Text.SeStringHandling;
namespace KeyboundPilot;

/// <summary>Read only the dedicated KTG arrays; unknown values stay unnamed until correlated with play logs.</summary>
internal sealed unsafe class DiagnosticReader(IGameGui gui)
{
    private readonly DiagnosticDelta delta=new();
    private string? unavailable;
    public void Reset() {delta.Reset();unavailable=null;}
    public string? Capture(TypingSession typing,ImeState ime,bool paused,object control)
    {
        var stage=AtkStage.Instance();
        if(stage==null || stage->AtkArrayDataHolder==null) return Unavailable("stage_missing");
        var holder=stage->AtkArrayDataHolder;
        // This index was confirmed in both installed KTG EWord and EnemyCastBar update routines.
        if(holder->NumberArrayCount<=161 || holder->StringArrayCount<=128 || holder->NumberArrays==null || holder->StringArrays==null) return Unavailable("array_capacity_mismatch");
        var n=holder->NumberArrays[161];var s=holder->StringArrays[128];
        if(n==null || s==null || n->IntArray==null || s->StringArray==null || n->Size<261 || n->Size>1024 || s->Size<60 || s->Size>256) return Unavailable("array_layout_mismatch");
        if(n->IntArray[0]<0 || n->IntArray[0]>20) return Unavailable("enemy_count_invalid");
        unavailable=null;
        var numbers=new ReadOnlySpan<int>(n->IntArray,261).ToArray();
        var strings=new string[60];
        for(var i=0;i<60;i++) strings[i]=ReadText(s->StringArray[i]);
        var input=gui.GetAddonByName<AtkUnitBase>("KTGInput");
        var inputState=new {visible=input!=null && input->IsVisible,enabled=false,focused=false,inputLength=0};
        if(input!=null)
        {
            var component=input->GetComponentByNodeId(7);
            if(component!=null && (int)component->GetComponentType()==7)
            {
                var text=(AtkComponentTextInput*)component;
                inputState=new {visible=input->IsVisible,enabled=text->Enabled,focused=text->IsActive,inputLength=Math.Clamp(text->RawString.Length,0,1024)};
            }
        }
        var ui=new Dictionary<string,object> {
            ["control"]=control,["input"]=inputState,["word"]=Probe("KTGEWord"),["cast"]=Probe("KTGEnemyCastBar"),["hp"]=Probe("KTGHudGaugeHp"),
            ["typing"]=new {typing.Running,typing.ManualPaused,typing.Status,typing.Position,typing.Sent,typing.AnswerCheck,paused},["ime"]=ime,
            ["inputHints"]=new {label=ReadNode(input,3),hint=ReadNode(input,6)}
        };
        if(KtgArrayReader.TryRead(out var prompts))
            ui["promptDisplay"]=PromptDisplayReader.Read(gui.GetAddonByName<AtkUnitBase>("KTGEWord"),prompts)
                .Select(x=>new {x.EntityId,x.TextVisible}).ToArray();
        return delta.Capture(numbers,strings,ui);
    }
    private static string ReadNode(AtkUnitBase* addon,uint id)
    {
        if(addon==null)return "";var node=addon->GetTextNodeById(id);
        return node==null?"":ReadText(node->NodeText.StringPtr);
    }
    private string? Unavailable(string reason)
    {
        if(unavailable==reason)return null;
        unavailable=reason;delta.Reset();
        return JsonSerializer.Serialize(new {available=false,reason});
    }
    private object Probe(string name)
    {
        var addon=gui.GetAddonByName<AtkUnitBase>(name);
        if(addon==null)return new {exists=false};
        var components=new List<object>();
        var manager=&addon->UldManager;
        if(manager->NodeList!=null && manager->NodeListCount<=512)
        for(var i=0;i<manager->NodeListCount && components.Count<20;i++)
        {
            var node=manager->NodeList[i];
            if(node==null || (int)node->Type<1000)continue;
            // Only node visibility/geometry, never arbitrary player-facing text or raw pointers.
            components.Add(new {ordinal=components.Count,nodeId=node->NodeId,visible=node->IsVisible(),alpha=node->Color.A});
        }
        return new {exists=true,visible=addon->IsVisible,components};
    }
    private static string ReadText(byte* ptr)
    {
        if(ptr==null)return "";
        var len=0;while(len<192 && ptr[len]!=0)len++;
        if(len==192)return "[truncated]";
        return SeString.Parse(new ReadOnlySpan<byte>(ptr,len)).TextValue;
    }
}
