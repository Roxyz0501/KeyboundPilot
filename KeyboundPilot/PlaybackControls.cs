using System.Numerics;
using Dalamud.Bindings.ImGui;
namespace KeyboundPilot;

internal static class PlaybackControls
{
    // Geometry avoids depending on icon-font coverage for the transport buttons.
    public static void Draw(bool running,bool paused,Func<string,string> text,Action start,Action pause,Action stop)
    {
        Button("Start",0,running&&!paused,text(paused?"Resume":"Start"),start);
        ImGui.SameLine();Button("Pause",1,!running||paused,text("Pause"),pause);
        ImGui.SameLine();Button("Stop",2,!running,text("Stop"),stop);
    }
    private static void Button(string id,int symbol,bool disabled,string tooltip,Action action)
    {
        ImGui.BeginDisabled(disabled);
        var pressed=ImGui.Button("###Playback"+id,new Vector2(36,30));
        var p=ImGui.GetItemRectMin()+new Vector2(10,8);var draw=ImGui.GetWindowDrawList();
        var color=ImGui.GetColorU32(ImGuiCol.Text);
        if(symbol==0)draw.AddTriangleFilled(p,p+new Vector2(0,14),p+new Vector2(14,7),color);
        else if(symbol==1) {draw.AddRectFilled(p,p+new Vector2(5,14),color);draw.AddRectFilled(p+new Vector2(9,0),p+new Vector2(14,14),color);}
        else draw.AddRectFilled(p,p+new Vector2(14,14),color);
        ImGui.EndDisabled();
        if(ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))ImGui.SetTooltip(tooltip);
        if(pressed)action();
    }
}
