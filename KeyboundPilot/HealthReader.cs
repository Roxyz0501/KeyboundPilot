using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
namespace KeyboundPilot;

internal static unsafe class HealthReader
{
    // Installed KTGHudGaugeHp ULD: node 2 is the allied HP gauge (range 0..100).
    public static float? Read(IGameGui gui)
    {
        var addon=gui.GetAddonByName<AtkUnitBase>("KTGHudGaugeHp");
        if(addon==null || !addon->IsReady || !addon->IsVisible || !Visible(addon->RootNode))return null;
        var manager=&addon->UldManager;
        if(manager->NodeList==null || manager->NodeListCount>64)return null;
        for(var i=0;i<manager->NodeListCount;i++)
        {
            var node=manager->NodeList[i];
            if(node==null || node->NodeId!=2 || (ushort)node->Type<1000 || !Visible(node))continue;
            var component=((AtkComponentNode*)node)->Component;
            if(component==null || component->OwnerNode!=(AtkComponentNode*)node || (int)component->GetComponentType()!=5)return null;
            return ReadGauge((AtkComponentGaugeBar*)component);
        }
        return null;
    }
    internal static float? ReadGauge(AtkComponentGaugeBar* gauge)
    {
        if(gauge==null || !Visible((AtkResNode*)gauge->OwnerNode))return null;
        var min=gauge->MinValue;var max=gauge->MaxValue;var value=gauge->Values[0].ValueInt;
        if(min!=0 || max<=0 || value<min || value>max)return null;
        return (float)(100.0*value/max);
    }
    private static bool Visible(AtkResNode* node)
    {
        if(node==null)return false;
        for(var depth=0;depth<32;depth++)
        {
            if((node->NodeFlags&NodeFlags.Visible)==0 || node->Color.A==0)return false;
            node=node->ParentNode;if(node==null)return true;
        }
        return false;
    }
}
