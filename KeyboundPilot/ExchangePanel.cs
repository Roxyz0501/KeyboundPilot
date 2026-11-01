using Dalamud.Bindings.ImGui;
using System.Numerics;
namespace KeyboundPilot;

internal sealed class ExchangePanel
{
    private string search="";
    private ExchangePlan? lastPlan;
    private string? lastSearch;
    private IReadOnlyList<ExchangeItem>? lastItems;
    private ExchangeItem[] ordered=[];
    public void Draw(Configuration config,Func<string,string> t,Action save,IReadOnlyList<ExchangeItem> items,
        int? currency,Func<uint,int> completed,Func<uint,ExchangeObservation> observe,string status,bool busy,
        ExchangeLedger.Purchase? pending,Action<bool> resolve,bool error)
    {
        ImGui.Spacing();ImGui.PushID("Exchange");
        var enabled=config.Exchange.Enabled;
        if(ImGui.Checkbox(t("ExchangeEnabled"),ref enabled)){config.Exchange=config.Exchange with {Enabled=enabled};save();}
        ImGui.SameLine();PlaybackControls.Help(t("ExchangeHelp"));
        if(!enabled){ImGui.PopID();return;}
        ImGui.Separator();ImGui.TextUnformatted(t("ExchangeSettings"));
        ImGui.TextUnformatted(t("ExchangeThreshold"));ImGui.SameLine();ImGui.PushID("Threshold");PlaybackControls.Help(t("ExchangeThresholdHelp"));ImGui.PopID();
        var threshold=config.Exchange.Threshold;ImGui.SetNextItemWidth(Math.Min(400,ImGui.GetContentRegionAvail().X));
        if(PlaybackControls.Number("###ExchangeThreshold",ref threshold,1,10000,10,t("NumberHelp")))
        {config.Exchange=config.Exchange with {Threshold=threshold};save();}
        PlaybackControls.StableText(t("ExchangeBalance")+": "+(currency?.ToString("N0")??"—")+" MGC");
        ImGui.TextUnformatted(t("ExchangePriority"));ImGui.SameLine();ImGui.PushID("Priority");PlaybackControls.Help(t("ExchangePriorityHelp"));ImGui.PopID();
        ImGui.SameLine();
        if(ImGui.Button(t(config.Exchange.HideZero?"ExchangeShowAll":"ExchangeHideZero")))
        {config.Exchange=config.Exchange with {HideZero=!config.Exchange.HideZero};save();}
        ImGui.SetNextItemWidth(-1);ImGui.InputTextWithHint("###ExchangeSearch",t("ExchangeSearch"),ref search,128);
        if(lastPlan!=config.Exchange || lastSearch!=search || !ReferenceEquals(lastItems,items))
        {
            lastPlan=config.Exchange;lastSearch=search;lastItems=items;
            ordered=config.Exchange.Ordered(items).Where(i=>config.Exchange.Shows(i) && (search.Length==0 || i.Name.Contains(search,StringComparison.CurrentCultureIgnoreCase))).ToArray();
        }
        if(ImGui.BeginTable("ExchangeItems",6,ImGuiTableFlags.RowBg|ImGuiTableFlags.BordersInnerV|ImGuiTableFlags.ScrollY|ImGuiTableFlags.SizingStretchProp,new Vector2(0,Math.Min(260,ImGui.GetTextLineHeightWithSpacing()*10))))
        {
            ImGui.TableSetupColumn(t("ExchangePriority"),ImGuiTableColumnFlags.WidthFixed,70);
            ImGui.TableSetupColumn(t("ExchangeItem"),ImGuiTableColumnFlags.WidthStretch,1);
            ImGui.TableSetupColumn("MGC",ImGuiTableColumnFlags.WidthFixed,50);
            ImGui.TableSetupColumn(t("ExchangeQuantity"),ImGuiTableColumnFlags.WidthFixed,76);
            ImGui.TableSetupColumn(t("ExchangeCompleted"),ImGuiTableColumnFlags.WidthFixed,64);
            ImGui.TableSetupColumn(t("ExchangeKnowledge"),ImGuiTableColumnFlags.WidthFixed,90);
            ImGui.TableSetupScrollFreeze(0,1);ImGui.TableHeadersRow();
            for(var position=0;position<ordered.Length;position++)
            {
                var item=ordered[position];
                ImGui.PushID((int)item.Id);ImGui.TableNextRow();ImGui.TableNextColumn();
                ImGui.BeginDisabled(position==0);
                if(ImGui.ArrowButton("Up",ImGuiDir.Up)){config.Exchange=config.Exchange.Move(items,item.Id,-1,ordered[position-1].Id);save();}
                ImGui.EndDisabled();ImGui.SameLine(0,2);ImGui.BeginDisabled(position==ordered.Length-1);
                if(ImGui.ArrowButton("Down",ImGuiDir.Down)){config.Exchange=config.Exchange.Move(items,item.Id,1,ordered[position+1].Id);save();}
                ImGui.EndDisabled();ImGui.TableNextColumn();PlaybackControls.StableText(item.Name);
                ImGui.TableNextColumn();ImGui.TextUnformatted(item.Cost.ToString("N0"));
                ImGui.TableNextColumn();ImGui.SetNextItemWidth(-1);var quantity=config.Exchange.Quantity(item.Id,item.Learnable);
                // Learned rows intentionally stay editable: the plan is shared across characters.
                if(PlaybackControls.Number("###Quantity",ref quantity,0,item.Learnable?1:9999,1,t(item.Learnable?"ExchangeLearnableHelp":"NumberHelp")))
                {config.Exchange=config.Exchange.SetQuantity(item.Id,quantity,item.Learnable);save();}
                ImGui.TableNextColumn();ImGui.TextUnformatted(completed(item.Id).ToString("N0"));
                ImGui.TableNextColumn();var state=observe(item.Id);
                var label=state.Knowledge==ExchangeKnowledge.NotApplicable?"—":t("Exchange"+state.Knowledge);
                if(item.Learnable && state.Knowledge==ExchangeKnowledge.Unlearned && state.Owned>0)label=t("ExchangeOwned");
                PlaybackControls.StableText(label);ImGui.PopID();
            }
            ImGui.EndTable();
        }
        PlaybackControls.StableText(t("ExchangeStatus")+": "+t(status));
        if(error)ImGui.TextWrapped(t("ExchangeFailure"));
        if(pending!=null && !busy)
        {
            ImGui.TextWrapped(t("ExchangeUncertain"));
            var name=items.FirstOrDefault(x=>x.Id==pending.Item)?.Name??pending.Item.ToString();
            ImGui.TextUnformatted(name+" × "+pending.Quantity);
            if(ImGui.Button(t("ExchangeResolveYes")))resolve(true);
            ImGui.SameLine();if(ImGui.Button(t("ExchangeResolveNo")))resolve(false);
        }
        ImGui.PopID();
    }
}
