using Dalamud.Bindings.ImGui;
namespace KeyboundPilot;
internal static class SettingsPanel
{
    public static void Draw(Configuration config,Func<string,string> t,Action save,Action stop)
    {
        void Help(string key) {ImGui.SameLine();ImGui.PushID(key);PlaybackControls.Help(t(key));ImGui.PopID();}
        void Title(string key,string help) {ImGui.AlignTextToFramePadding();ImGui.TextUnformatted(t(key));Help(help);}
        void Number(string label,string id,int value,int min,int max,float step,Action<int> set)
        {ImGui.SetNextItemWidth(200);if(PlaybackControls.Number(t(label)+"###"+id,ref value,min,max,step,t("NumberHelp"))){set(value);save();}}
        var direct=config.DirectInput;
        if(ImGui.Checkbox(t("DirectInput")+"###DirectInput",ref direct)){stop();config.DirectInput=direct;save();}
        Help("DirectInputHelp");
        var auto=config.AutoStart;
        if(ImGui.Checkbox(t("AutoStart")+"###AutoStart",ref auto)){config.AutoStart=auto;save();}
        Help("AutoStartHelp");
        var debug=config.ShowDebug;
        if(ImGui.Checkbox(t("ShowDebug")+"###ShowDebug",ref debug)){config.ShowDebug=debug;save();}
        Help("DebugHelp");
        ImGui.Spacing();
        if(!ImGui.CollapsingHeader(t("InputAdjustment")+"###InputAdjustment",ImGuiTreeNodeFlags.DefaultOpen))return;
        Title("InputInterval","RangeHelp");
        Number("MinimumMs","Minimum",config.MinimumMs,20,5000,5,config.SetMinimum);
        Number("MaximumMs","Maximum",config.MaximumMs,20,5000,5,config.SetMaximum);
        ImGui.Spacing();Title("SubmissionWait","BetweenWordsHelp");
        Number("MinimumMs","WaitMinimum",config.WaitMin,0,60000,50,config.SetWaitMinimum);
        Number("MaximumMs","WaitMaximum",config.WaitMax,0,60000,50,config.SetWaitMaximum);
        ImGui.Spacing();var mistype=config.MistypeEnabled;
        if(ImGui.Checkbox(t("MistypeEnabled")+"###MistypeEnabled",ref mistype)){config.MistypeEnabled=mistype;save();}
        Help("MistypeHelp");
        Title("TypoCandidates","MistypeRangeHelp");
        ImGui.BeginDisabled(!config.MistypeEnabled);
        Number("MinimumCount","TypoMinimum",config.TypoMin,1,10004,1,config.SetTypoMinimum);
        Number("MaximumCount","TypoMaximum",config.TypoMax,1,10004,1,config.SetTypoMaximum);
        ImGui.EndDisabled();
    }
}
