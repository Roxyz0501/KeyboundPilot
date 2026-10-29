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
        void Profile(InputSettings value,Action<InputSettings> set)
        {
            void Change(InputSettings next){value=next;set(next);}
            Title("InputInterval","RangeHelp");
            Number("MinimumMs","Minimum",value.MinimumMs,20,value.MaximumMs,5,n=>Change(value with {MinimumMs=n}));
            Number("MaximumMs","Maximum",value.MaximumMs,value.MinimumMs,5000,5,n=>Change(value with {MaximumMs=n}));
            ImGui.Spacing();Title("SubmissionWait","BetweenWordsHelp");
            Number("MinimumMs","WaitMinimum",value.WaitMin,0,value.WaitMax,50,n=>Change(value with {WaitMin=n}));
            Number("MaximumMs","WaitMaximum",value.WaitMax,value.WaitMin,60000,50,n=>Change(value with {WaitMax=n}));
            ImGui.Spacing();var mistype=value.MistypeEnabled;
            if(ImGui.Checkbox(t("MistypeEnabled")+"###MistypeEnabled",ref mistype)){Change(value with {MistypeEnabled=mistype});save();}
            Help("MistypeHelp");Title("TypoCandidates","MistypeRangeHelp");
            ImGui.BeginDisabled(!value.MistypeEnabled);
            Number("MinimumCount","TypoMinimum",value.TypoMin,1,value.TypoMax,1,n=>Change(value with {TypoMin=n}));
            Number("MaximumCount","TypoMaximum",value.TypoMax,value.TypoMin,10004,1,n=>Change(value with {TypoMax=n}));
            ImGui.EndDisabled();
        }
        var direct=config.DirectInput;
        if(ImGui.Checkbox(t("DirectInput")+"###DirectInput",ref direct)){stop();config.DirectInput=direct;save();}
        Help("DirectInputHelp");
        var auto=config.AutoStart;
        if(ImGui.Checkbox(t("AutoStart")+"###AutoStart",ref auto)){config.AutoStart=auto;save();}
        Help("AutoStartHelp");
        var debug=config.ShowDebug;
        if(ImGui.Checkbox(t("ShowDebug")+"###ShowDebug",ref debug)){config.ShowDebug=debug;save();}
        Help("DebugHelp");ImGui.Spacing();
        if(ImGui.CollapsingHeader(t("LoopButton")+"###LoopSettings",ImGuiTreeNodeFlags.DefaultOpen))
        {
            Title("ExitDelay","ExitDelayHelp");
            Number("MinimumMs","ExitMinimum",config.ExitMin,0,60000,100,config.SetExitMinimum);
            Number("MaximumMs","ExitMaximum",config.ExitMax,0,60000,100,config.SetExitMaximum);
        }
        ImGui.Spacing();
        if(!ImGui.CollapsingHeader(t("InputAdjustment")+"###InputAdjustment",ImGuiTreeNodeFlags.DefaultOpen))return;
        Title("InputPriority","InputPriorityHelp");
        ImGui.SetNextItemWidth(360);
        var priority=config.LowHpFirst?1:0;
        if(ImGui.Combo("###InputPriority",ref priority,new[]{t("ObstructionFirst"),t("LowHpFirst")},2)){config.LowHpFirst=priority==1;save();}
        if(!ImGui.BeginTabBar("InputProfiles"))return;
        if(ImGui.BeginTabItem(t("NormalProfile")+"###NormalProfile"))
        {
            ImGui.PushID("NormalProfile");Profile(config.NormalInput,config.SetNormalInput);ImGui.PopID();ImGui.EndTabItem();
        }
        if(ImGui.BeginTabItem(t("ObstructionProfile")+"###ObstructionProfile"))
        {
            ImGui.PushID("ObstructionProfile");
            var exclude=config.ExcludeUnseenPrompts;
            if(ImGui.Checkbox(t("ExcludeUnseen")+"###ExcludeUnseen",ref exclude)){config.ExcludeUnseenPrompts=exclude;save();}
            Help("ExcludeUnseenHelp");ImGui.Spacing();
            var custom=config.ObstructionIntervalEnabled;
            if(ImGui.Checkbox(t("ObstructionInterval")+"###ObstructionInterval",ref custom))
            {if(custom)config.ObstructionSettings??=config.ObstructionInput;config.ObstructionIntervalEnabled=custom;save();}
            Help("ObstructionIntervalHelp");
            ImGui.BeginDisabled(!custom);
            Profile(custom?config.ObstructionInput:config.NormalInput,value=>config.ObstructionSettings=value);
            ImGui.EndDisabled();ImGui.PopID();ImGui.EndTabItem();
        }
        if(ImGui.BeginTabItem(t("LowHpProfile")+"###LowHpProfile"))
        {
            ImGui.PushID("LowHpProfile");
            var custom=config.LowHpEnabled;
            if(ImGui.Checkbox(t("LowHpEnabled")+"###LowHpEnabled",ref custom)){config.LowHpEnabled=custom;save();}
            Help("LowHpHelp");
            ImGui.BeginDisabled(!custom);
            Number("HpThreshold","HpThreshold",config.LowHpThreshold,1,100,1,n=>config.LowHpThreshold=n);
            ImGui.Spacing();Profile(custom?config.LowHpSettings:config.NormalInput,value=>config.LowHpSettings=value);
            ImGui.EndDisabled();ImGui.PopID();ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
    }
}
