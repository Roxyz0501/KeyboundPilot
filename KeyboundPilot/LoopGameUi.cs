using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Enums;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
namespace KeyboundPilot;

internal sealed unsafe class LoopGameUi
{
    // Verified from installed TerritoryType/ContentFinderCondition data, fail closed on changed mappings.
    public const uint LobbyTerritory=1360,GameTerritory=1359,DutyId=1095;
    private readonly IGameGui gui;
    private readonly string dutyName,exitPrompt;
    public bool Supported {get;}
    public LoopGameUi(IGameGui gui,IDataManager data)
    {
        this.gui=gui;
        var duty=data.GetExcelSheet<ContentFinderCondition>().GetRowOrDefault(DutyId);
        var lobby=data.GetExcelSheet<TerritoryType>().GetRowOrDefault(LobbyTerritory);
        Supported=duty?.TerritoryType.RowId==GameTerritory && lobby?.Name.ToString()=="w1eo";
        dutyName=duty?.Name.ToString()??"";
        exitPrompt=data.GetExcelSheet<Addon>().GetRowOrDefault(17854)?.Text.ToString()??"";
    }
    private static bool Visible(AtkUnitBase* addon)=>addon!=null && addon->IsVisible && addon->RootNode!=null && addon->RootNode->IsVisible();
    private static bool Ready(AtkComponentButton* button)=>button!=null && button->OwnerNode!=null && button->IsEnabled && button->OwnerNode->IsVisible();
    private static string Text(AtkTextNode* node)=>node==null || node->NodeText.Length>2048?"":SeString.Parse(node->NodeText.AsSpan()).TextValue;
    private static string Normalize(string text)=>string.Concat(text.Where(c=>!char.IsWhiteSpace(c)));
    private bool MatchingConfirmation(AddonContentsFinderConfirm* confirm)=>Visible((AtkUnitBase*)confirm)
        && Text(confirm->AtkTextNode230).Trim()==dutyName && Ready(confirm->CommenceButton);
    private bool MatchingExit(AddonSelectYesno* confirm)=>Visible((AtkUnitBase*)confirm) && dutyName.Length>0 && exitPrompt.Length>0
        && Text(confirm->PromptText).Contains(dutyName,StringComparison.Ordinal)
        && Normalize(Text(confirm->PromptText).Replace(dutyName,"",StringComparison.Ordinal))==Normalize(exitPrompt)
        && Ready(confirm->YesButton);
    private static bool Own(ContentsId entry)=>entry.ContentType==ContentsType.Regular && entry.Id==DutyId;
    public LoopSnapshot Read(bool loggedIn,uint territory,bool loading)
    {
        var finder=ContentsFinder.Instance();var queued=false;var other=false;
        if(finder!=null)
        {
            var q=&finder->QueueInfo;
            queued=q->QueueState is ContentsFinderQueueState.Pending or ContentsFinderQueueState.Queued or ContentsFinderQueueState.Ready or ContentsFinderQueueState.Accepted;
            if(queued)
            {
                var matching=Own(q->PoppedQueueEntry);var unrelated=false;
                for(var i=0;i<5;i++){var e=q->QueuedEntries[i];if(e.ContentType==ContentsType.None)continue;matching|=Own(e);unrelated|=!Own(e);}
                other=unrelated || !matching;
            }
            if(q->QueueState==ContentsFinderQueueState.InContent && territory!=GameTerritory)queued=true;
        }
        var result=gui.GetAddonByName<AtkUnitBase>("KTGResult");
        var button=Visible(result)?(AtkComponentButton*)result->GetComponentByNodeId(48):null;
        var pop=gui.GetAddonByName<AddonContentsFinderConfirm>("ContentsFinderConfirm");
        return new(loggedIn,territory==LobbyTerritory,territory==GameTerritory,loading,other,queued,
            MatchingConfirmation(pop),Visible(result),Ready(button),MatchingExit(gui.GetAddonByName<AddonSelectYesno>("SelectYesno")));
    }
    public bool Execute(LoopAction action,uint territory)
    {
        if(!Supported)return false;
        if(action==LoopAction.Queue)
        {
            var finder=ContentsFinder.Instance();
            if(territory!=LobbyTerritory || finder==null || finder->QueueInfo.QueueState!=ContentsFinderQueueState.None)return false;
            uint id=DutyId;finder->QueueInfo.QueueDuties(&id,1);return true;
        }
        if(action==LoopAction.Accept)
        {
            var addon=gui.GetAddonByName<AddonContentsFinderConfirm>("ContentsFinderConfirm");
            return MatchingConfirmation(addon) && Click((AtkUnitBase*)addon,addon->CommenceButton);
        }
        if(action==LoopAction.Leave)
        {
            var addon=gui.GetAddonByName<AtkUnitBase>("KTGResult");
            return territory==GameTerritory && Visible(addon) && Click(addon,(AtkComponentButton*)addon->GetComponentByNodeId(48));
        }
        if(action==LoopAction.ConfirmExit)
        {
            var addon=gui.GetAddonByName<AddonSelectYesno>("SelectYesno");
            return territory==GameTerritory && MatchingExit(addon) && Click((AtkUnitBase*)addon,addon->YesButton);
        }
        return false;
    }
    private static bool Click(AtkUnitBase* addon,AtkComponentButton* button)
    {
        if(!Visible(addon) || !Ready(button) || (int)button->GetComponentType()!=1)return false;
        var evt=button->OwnerNode->AtkEventManager.Event;
        for(var i=0;evt!=null && i<64;i++,evt=evt->NextEvent)
        {
            if(evt->State.EventType!=AtkEventType.ButtonClick || evt->Listener!=(AtkEventListener*)addon)continue;
            var copy=*evt;copy.NextEvent=null;copy.State.ReturnFlags=0;copy.State.StateFlags=0;
            AtkEventData data=default;
            evt->Listener->ReceiveEvent(AtkEventType.ButtonClick,unchecked((int)evt->Param),&copy,&data);return true;
        }
        return false;
    }
}
