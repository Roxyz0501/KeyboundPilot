using Dalamud.Bindings.ImGui;
namespace KeyboundPilot;

internal sealed class ScorePanel(ScoreHistory history) : IDisposable
{
    private string[] dates=[];
    private string date="";
    private int page,generation=-1;
    private bool loaded,more;
    private Task<(string[] Dates,ScoreRecord[] Records,bool More,string Date,int Page)>? loading;
    private ScoreRecord[] records=[];
    private bool failed;
    public IEnumerable<string> Names=>records.Select(r=>r.Character);
    public void Draw(Func<string,string> text,bool readError,Action retryReading)
    {
        Poll();
        if(!loaded || generation!=history.Generation && loading==null && !failed)Load(true);
        ImGui.TextWrapped(text("ScoreHistoryHelp"));
        if(history.Error!=null || failed || readError)
        {
            ImGui.TextWrapped(text("ScoreHistoryError"));
            if(ImGui.Button(text("Retry")+"###ScoreRetry")){history.Retry();retryReading();failed=false;Load(true);}
        }
        ImGui.BeginDisabled(loading!=null);
        if(ImGui.BeginCombo(text("ScoreDate"),date))
        {
            foreach(var item in dates)if(ImGui.Selectable(item,item==date)){date=item;page=0;Load(false);}
            ImGui.EndCombo();
        }
        if(ImGui.Button(text("ScoreRefresh")))Load(true);
        ImGui.SameLine();ImGui.BeginDisabled(page==0);
        if(ImGui.Button(text("ScorePrevious"))){page--;Load(false);}ImGui.EndDisabled();
        ImGui.SameLine();ImGui.BeginDisabled(!more);
        if(ImGui.Button(text("ScoreNext"))){page++;Load(false);}ImGui.EndDisabled();ImGui.EndDisabled();
        if(loading!=null)ImGui.TextUnformatted(text("ScoreLoading"));
        else if(!failed && records.Length==0)ImGui.TextUnformatted(text("ScoreEmpty"));
        if(records.Length>0 && ImGui.BeginTable("ScoreRecords",3,ImGuiTableFlags.RowBg|ImGuiTableFlags.BordersInnerV|ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn(text("ScoreTime"),ImGuiTableColumnFlags.WidthFixed,120);
            ImGui.TableSetupColumn(text("ScoreCharacter"),ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(text("ScoreValue"),ImGuiTableColumnFlags.WidthFixed,100);
            ImGui.TableHeadersRow();
            foreach(var record in records)
            {
                ImGui.TableNextRow();ImGui.TableNextColumn();ImGui.TextUnformatted($"{record.Ended:HH:mm:ss zzz}");
                ImGui.TableNextColumn();ImGui.TextUnformatted(record.Character);
                ImGui.TableNextColumn();ImGui.TextUnformatted(record.OwnScore?.ToString("N0")??"—");
                if(record.OwnScore==null && ImGui.IsItemHovered())ImGui.SetTooltip(text("ScoreUnavailable"));
            }
            ImGui.EndTable();
        }
    }
    private void Load(bool refreshDates)
    {
        if(loading!=null)return;
        loaded=true;generation=history.Generation;
        var selected=date;var requestedPage=page;var cachedDates=dates;
        loading=Task.Run(()=>
        {
            var list=refreshDates?history.Dates():cachedDates;
            if(!list.Contains(selected)){selected=list.FirstOrDefault()??"";requestedPage=0;}
            var result=selected.Length>0?history.ReadPage(selected,requestedPage):(Array.Empty<ScoreRecord>(),false);
            return(list,result.Item1,result.Item2,selected,requestedPage);
        });
    }
    private void Poll()
    {
        if(loading is not {IsCompleted:true})return;
        try{var result=loading.GetAwaiter().GetResult();dates=result.Dates;records=result.Records;more=result.More;date=result.Date;page=result.Page;failed=false;}
        catch{failed=true;records=[];more=false;}
        loading=null;
    }
    public void Dispose(){if(loading!=null){try{loading.GetAwaiter().GetResult();}catch{}loading=null;}}
}
