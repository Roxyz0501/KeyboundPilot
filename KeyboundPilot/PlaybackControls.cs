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
    public static void DrawLoop(bool enabled,string tooltip,Action action)
    {
        ImGui.BeginDisabled(!enabled);var pressed=ImGui.Button("    "+tooltip+"###LoopStart",new Vector2(ImGui.CalcTextSize(tooltip).X+42,30));
        var p=ImGui.GetItemRectMin();var draw=ImGui.GetWindowDrawList();var color=ImGui.GetColorU32(ImGuiCol.Text);
        draw.AddTriangleFilled(p+new Vector2(9,8),p+new Vector2(9,22),p+new Vector2(21,15),color);
        ImGui.EndDisabled();if(ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))ImGui.SetTooltip(tooltip);
        if(pressed)action();
    }
    public static void DrawStopAfterCurrent(bool enabled,Func<string,string> text,Action action)
    {
        ImGui.SameLine();ImGui.BeginDisabled(!enabled);
        var pressed=ImGui.Button(text("StopAfterCurrent")+"###LoopStopAfterCurrent",new Vector2(0,30));
        ImGui.EndDisabled();
        if(ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))ImGui.SetTooltip(text("StopAfterCurrentHelp"));
        if(pressed)action();
    }
    public static bool Number(string id,ref int value,int min,int max,float speed,string tooltip)
    {
        // Native DragInt supports double-click and Ctrl-click direct text editing.
        var changed=ImGui.DragInt(id,ref value,speed,min,max,"%d",ImGuiSliderFlags.AlwaysClamp);
        if(changed)value=Math.Clamp(value,min,max);
        if(ImGui.IsItemHovered())ImGui.SetTooltip(tooltip);
        return changed;
    }
    public static void Help(string text)
    {
        ImGui.InvisibleButton("###PlaybackHelp",new Vector2(24,30));
        var center=ImGui.GetItemRectMin()+new Vector2(12,15);var draw=ImGui.GetWindowDrawList();
        var color=ImGui.GetColorU32(ImGuiCol.TextDisabled);
        draw.AddCircleFilled(center,8,color);
        var ink=ImGui.GetColorU32(ImGuiCol.WindowBg);
        draw.AddCircleFilled(center+new Vector2(0,-4),1,ink);
        draw.AddLine(center+new Vector2(0,-1),center+new Vector2(0,5),ink,2);
        if(ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();ImGui.PushTextWrapPos(ImGui.GetFontSize()*28);
            ImGui.TextUnformatted(text);ImGui.PopTextWrapPos();ImGui.EndTooltip();
        }
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
