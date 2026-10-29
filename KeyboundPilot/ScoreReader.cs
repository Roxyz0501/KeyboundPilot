using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace KeyboundPilot;

internal sealed unsafe class ScoreReader(IGameGui gui)
{
    // Node IDs verified against installed KTGResult ULD. No game state is written.
    public (bool ResultVisible,ScoreRow[] Rows) Read(string character)
        =>Read(gui.GetAddonByName<AtkUnitBase>("KTGResult"),character);
    internal static (bool ResultVisible,ScoreRow[] Rows) Read(AtkUnitBase* result,string character)
    {
        if(!Ready(result))return(false,[]);
        var rows=new List<ScoreRow>(4);
        for(uint node=30;node<=33;node++)
        {
            var component=Component(result,node);
            if(component==null || !Visible((AtkResNode*)component->OwnerNode))continue;
            var name=Text(component,9);
            if(string.IsNullOrWhiteSpace(character) || name!=character)continue;
            var score=ScoreNumbers.Parse(Text(component,11));
            if(string.IsNullOrWhiteSpace(name) || score==null)continue;
            rows.Add(new((int)node-29,name,score.Value,name==character));
        }
        return(true,rows.ToArray());
    }
    private static bool Ready(AtkUnitBase* addon)=>addon!=null && addon->IsReady && addon->IsVisible && Visible(addon->RootNode);
    private static AtkResNode* Node(AtkUldManager* manager,uint id)
    {
        if(manager->NodeList==null || manager->NodeListCount>1024)return null;
        for(var i=0;i<manager->NodeListCount;i++)
        {var node=manager->NodeList[i];if(node!=null && node->NodeId==id)return node;}
        return null;
    }
    private static AtkComponentBase* Component(AtkUnitBase* addon,uint id)
    {
        var node=Node(&addon->UldManager,id);
        return node!=null && (ushort)node->Type>=1000?((AtkComponentNode*)node)->Component:null;
    }
    private static string? Text(AtkComponentBase* component,uint id)
    {
        var node=Node(&component->UldManager,id);
        if(node==null || node->Type!=NodeType.Text || !Visible(node))return null;
        var native=&((AtkTextNode*)node)->NodeText;
        if((byte*)native->StringPtr==null || native->BufUsed is <=1 or >513 || native->BufSize<native->BufUsed)return null;
        var text=native->AsSpan();
        return text.Length is >0 and <=512?SeString.Parse(text).TextValue.Trim():null;
    }
    private static bool Visible(AtkResNode* node)
    {
        if(node==null)return false;
        var depth=0;for(;node!=null && depth++<32;node=node->ParentNode)
            if((node->NodeFlags&NodeFlags.Visible)==0 || node->Color.A==0)return false;
        return depth<32;
    }
}
