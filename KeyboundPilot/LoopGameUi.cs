using Dalamud.Plugin.Services;
using Dalamud.Plugin;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
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
    private bool preparingQueue,interacted;
    private readonly IObjectTable objects;
    private readonly ReceptionTravel travel;
    public void SuspendTravel()=>travel.Stop();
    private long queueStepAt;
    public void CancelQueuePreparation() {preparingQueue=false;interacted=false;travel.Stop();}
    public bool Supported {get;}
    public LoopGameUi(IGameGui gui,IDataManager data,IObjectTable objects,IDalamudPluginInterface pi)
    {
        this.gui=gui;this.objects=objects;travel=new ReceptionTravel(pi);
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
            MatchingConfirmation(pop),Visible(result),Ready(button),MatchingExit(gui.GetAddonByName<AddonSelectYesno>("SelectYesno")),preparingQueue);
    }
    public bool Execute(LoopAction action,uint territory)
    {
        if(!Supported)return false;
        if(action==LoopAction.Queue)
        {
            var finder=ContentsFinder.Instance();
            if(territory!=LobbyTerritory || finder==null || finder->QueueInfo.QueueState!=ContentsFinderQueueState.None)return false;
            preparingQueue=true;interacted=false;queueStepAt=0;
            return true;
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
    // Only the identified reception NPC and its dedicated entry dialog are used.
    public bool AdvanceQueue(long now,uint territory)
    {
        if(!preparingQueue || now<queueStepAt)return true;
        queueStepAt=now+500;
        if(territory!=LobbyTerritory)return false;
        var finder=ContentsFinder.Instance();if(finder==null)return false;
        if(finder->QueueInfo.QueueState!=ContentsFinderQueueState.None){CancelQueuePreparation();return true;}
        var player=objects.LocalPlayer;if(player==null)return false;
        var npc=objects.Where(x=>x.BaseId==1058751 && x.IsTargetable)
            .OrderBy(x=>Vector3.DistanceSquared(x.Position,player.Position)).FirstOrDefault();
        if(npc==null)return true; // Bounded by preparation timeout; never choose a different NPC.
        if(Vector3.Distance(player.Position,npc.Position)>3.0f)
        {
            if(interacted)return false;
            return travel.Approach(player.Position,npc.Position);
        }
        travel.Stop();
        var dialog=gui.GetAddonByName<AtkUnitBase>("KTGEnterDialogue");
        if(interacted && Visible(dialog))
        {
            var button=(AtkComponentButton*)dialog->GetComponentByNodeId(9);
            if(!Ready(button))return true;
            preparingQueue=false; // One join attempt, including uncertain outcomes.
            return Click(dialog,button);
        }
        if(!interacted)
        {
            // Never advance an unrelated conversation or selection window.
            if(Visible(gui.GetAddonByName<AtkUnitBase>("Talk")) || Visible(gui.GetAddonByName<AtkUnitBase>("SelectString")) || Visible(gui.GetAddonByName<AtkUnitBase>("SelectYesno")))return false;
            var target=TargetSystem.Instance();if(target==null)return false;
            interacted=true;
            target->InteractWithObject((GameObject*)npc.Address,true);
        }
        return true;
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
