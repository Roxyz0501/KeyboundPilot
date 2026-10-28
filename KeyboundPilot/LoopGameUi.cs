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
    private bool preparingQueue,selectionRequested;
    private long queueStepAt;
    public void CancelQueuePreparation() {preparingQueue=false;selectionRequested=false;}
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
            var agent=AgentContentsFinder.Instance();
            if(agent==null || agent->SelectedContent.Count>0)return false;
            preparingQueue=true;selectionRequested=false;queueStepAt=Environment.TickCount64+500;
            agent->OpenRegularDuty(DutyId);return true;
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
    // Follow the normal Duty Finder UI flow; never submit a raw QueueDuties request.
    public bool AdvanceQueue(long now,uint territory)
    {
        if(!preparingQueue || now<queueStepAt)return true;
        queueStepAt=now+500;
        if(territory!=LobbyTerritory)return false;
        var finder=ContentsFinder.Instance();var agent=AgentContentsFinder.Instance();
        if(finder==null || agent==null)return false;
        if(finder->QueueInfo.QueueState!=ContentsFinderQueueState.None){CancelQueuePreparation();return true;}
        var addon=gui.GetAddonByName<AddonContentsFinder>("ContentsFinder");
        if(!Visible((AtkUnitBase*)addon))return true;
        if(!Own(agent->SelectedDuty))return false;
        if(agent->SelectedContent.Count>1)return false;
        if(agent->SelectedContent.Count==1)
        {
            if(!Own(agent->SelectedContent[0]))return false;
            if(Text(addon->SelectedDutyTextNode[0].Value).Trim()!=dutyName)return true;
            if(!Ready(addon->JoinButton))return true;
            preparingQueue=false; // Latch before dispatch: never repeat an uncertain join.
            return Callback((AtkUnitBase*)addon,12,0);
        }
        if(selectionRequested)return true;
        var list=addon->DutyList;
        if(list==null || list->Items.Count==0)return true;
        var selected=list->SelectedItemIndex;
        if(selected<0 || selected>=list->Items.Count || list->Items.Count>2048)return false;
        var item=list->Items[selected].Value;
        if(item==null || item->Renderer==null || Text(item->Renderer->GetTextNodeById(5)).Trim()!=dutyName)return false;
        var leaf=0;
        for(var i=0;i<=selected;i++)
        {
            var entry=list->Items[i].Value;if(entry==null)return false;
            if(entry->Type is TreeListItemType.None or TreeListItemType.LastItemInGroup)leaf++;
        }
        if(leaf==0)return false;
        selectionRequested=true;
        return Callback((AtkUnitBase*)addon,3,leaf);
    }
    private static bool Callback(AtkUnitBase* addon,int operation,int argument)
    {
        AtkValue* values=stackalloc AtkValue[2];values[0]=default;values[1]=default;
        values[0].SetInt(operation);values[1].SetInt(argument);
        addon->FireCallback(2,values,true);return true;
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
