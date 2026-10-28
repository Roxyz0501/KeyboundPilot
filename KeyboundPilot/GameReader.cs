using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Component.GUI;
namespace KeyboundPilot;

internal sealed unsafe class GameReader(IGameGui gui)
{
    private readonly Dictionary<string,uint> sources=new(StringComparer.Ordinal);
    private readonly FocusAcquisition focus=new();
    private readonly SeenPrompts seen=new();
    private readonly TargetMotion motion=new();
    public string FocusState {get;private set;}="Idle";
    public bool? FocusRequestAccepted {get;private set;}
    public int FocusAttempts=>focus.Attempts;
    public bool InputVisible {get;private set;}
    public bool InputFocused {get;private set;}
    public bool InputEnabled {get;private set;}
    public bool Available {get;private set;}
    public bool ContentComplete {get;private set;}
    public void ResetFocus() {focus.Reset();FocusState="Idle";FocusRequestAccepted=null;}
    public IReadOnlyList<string> Words {get;private set;}=[];
    public IReadOnlyList<KtgWord> Enemies {get;private set;}=[];
    public void Clear() {InputVisible=InputFocused=InputEnabled=Available=ContentComplete=false;Words=[];Enemies=[];sources.Clear();seen.Clear();motion.Clear();focus.Reset();FocusState="Idle";FocusRequestAccepted=null;}
    private AtkComponentTextInput* Input(out AtkUnitBase* addon)
    {
        addon=gui.GetAddonByName<AtkUnitBase>("KTGInput");
        if(addon==null || !addon->IsVisible || !Visible(addon->RootNode))return null;
        var component=addon->GetComponentByNodeId(7);
        return component!=null && (int)component->GetComponentType()==7 ? (AtkComponentTextInput*)component:null;
    }
    public bool FocusedNow() {var input=Input(out _);return input!=null && input->Enabled && input->IsActive;}
    public bool CompetingFocus()
    {
        var input=Input(out var addon);var stage=AtkStage.Instance();
        if(stage==null || stage->AtkInputManager==null || stage->RaptureAtkUnitManager==null)return true;
        // A focused HUD/addon is not an active text editor. Only protect another text input.
        return stage->AtkInputManager->IsTextInputActive && (input==null || !input->IsActive);
    }
    public bool FocusFailed=>FocusState is "Exhausted" or "MissingCollision";
    public bool EnsureFocus(long now)
    {
        var input=Input(out var addon);
        if(input==null || !input->Enabled) {FocusState="InputUnavailable";return false;}
        if(CompetingFocus()) {FocusState="OtherTextInput";return false;}
        if(input->CollisionNode==null) {FocusState="MissingCollision";return false;}
        var result=focus.Step(now,input->IsActive,()=>
        {
            // Activate the addon as well as its edit node; do not write text or invoke an answer callback.
            addon->Focus();
            FocusRequestAccepted=addon->SetFocusNode((AtkResNode*)input->CollisionNode);
        });
        FocusState=result.ToString();return result==FocusResult.Ready;
    }
    public string? EvaluatedText()
    {
        var input=Input(out _);if(input==null)return null;
        var raw=input->EvaluatedString.AsSpan();if(raw.Length>1024)return null;
        return SeString.Parse(raw).TextValue;
    }
    public void Refresh()
    {
        var result=gui.GetAddonByName<AtkUnitBase>("KTGResult");
        ContentComplete=result!=null && result->IsVisible && Visible(result->RootNode);
        var input=Input(out _);InputVisible=input!=null;InputEnabled=input!=null && input->Enabled;InputFocused=input!=null && input->IsActive;
        var wordAddon=gui.GetAddonByName<AtkUnitBase>("KTGEWord");
        Available=InputVisible && wordAddon!=null && wordAddon->IsVisible ;
        if(wordAddon==null || !wordAddon->IsVisible || !KtgArrayReader.TryRead(out var enemies)) {Available=false;Words=[];Enemies=[];seen.Clear();motion.Clear();return;}
        enemies=motion.Update(Environment.TickCount64,enemies);Enemies=enemies;
        seen.Update(enemies,PromptDisplayReader.Read(wordAddon,enemies));
        // In-flight identity is held by Plugin; rebuild candidate sources in priority order.
        sources.Clear();
        var words=new List<string>();
        foreach(var candidate in EncounterPolicy.Prioritize(enemies.Where(x=>x.Visible && x.Word.Length>0 && seen.Contains(x))))
        {sources.TryAdd(candidate.Word,candidate.EntityId);if(!words.Contains(candidate.Word))words.Add(candidate.Word);}
        Words=words;
    }
    public bool StillVisible(string word,uint entityId=0)
    {
        if(entityId==0 && !sources.TryGetValue(word,out entityId))return false;
        var addon=gui.GetAddonByName<AtkUnitBase>("KTGEWord");
        return addon!=null && addon->IsVisible && KtgArrayReader.TryRead(out var words) && words.Any(x=>x.Visible && x.EntityId==entityId && x.Word==word);
    }
    public uint Source(string word)=>sources.GetValueOrDefault(word);
    private static bool Visible(AtkResNode* node)
    {
        var depth=0;for(;node!=null && depth++<32;node=node->ParentNode)if(!node->IsVisible() || node->Color.A==0)return false;
        return depth<32;
    }
}
