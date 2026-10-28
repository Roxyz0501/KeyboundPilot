using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Component.GUI;
namespace KeyboundPilot;

internal sealed unsafe class GameReader(IGameGui gui)
{
    private readonly Dictionary<string,uint> sources=new(StringComparer.Ordinal);
    private long nextFocus;
    private int focusAttempts;
    public bool InputVisible {get;private set;}
    public bool InputFocused {get;private set;}
    public bool InputEnabled {get;private set;}
    public bool Available {get;private set;}
    public IReadOnlyList<string> Words {get;private set;}=[];
    public IReadOnlyList<KtgWord> Enemies {get;private set;}=[];
    public void Clear() {InputVisible=InputFocused=InputEnabled=Available=false;Words=[];Enemies=[];sources.Clear();focusAttempts=0;nextFocus=0;}
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
        var other=stage->RaptureAtkUnitManager->FocusedAddon;
        return (stage->AtkInputManager->IsTextInputActive && (input==null || !input->IsActive))
            || (other!=null && other!=addon && other->IsVisible);
    }
    public bool FocusFailed=>focusAttempts>=3;
    public bool EnsureFocus(long now)
    {
        var input=Input(out var addon);if(input==null || !input->Enabled || CompetingFocus())return false;
        if(input->IsActive) {focusAttempts=0;return true;}
        if(now<nextFocus || focusAttempts>=3)return false;
        var stage=AtkStage.Instance();if(stage==null || stage->AtkInputManager==null || input->CollisionNode==null)return false;
        nextFocus=now+250;focusAttempts++;
        // Focus only. Text and game callbacks are never injected.
        stage->AtkInputManager->SetFocus((AtkResNode*)input->CollisionNode,addon,0);
        return false; // allow a framework frame before inspecting the result
    }
    public string? EvaluatedText()
    {
        var input=Input(out _);if(input==null)return null;
        var raw=input->EvaluatedString.AsSpan();if(raw.Length>1024)return null;
        return SeString.Parse(raw).TextValue;
    }
    public void Refresh()
    {
        var input=Input(out _);InputVisible=input!=null;InputEnabled=input!=null && input->Enabled;InputFocused=input!=null && input->IsActive;
        var wordAddon=gui.GetAddonByName<AtkUnitBase>("KTGEWord");
        Available=InputVisible && wordAddon!=null && wordAddon->IsVisible ;
        if(!Available || !KtgArrayReader.TryRead(out var enemies)) {Available=false;Words=[];Enemies=[];return;}
        Enemies=enemies;
        // Keep the entity identity of an in-flight word, even if array slots reorder.
        foreach(var entry in sources.ToArray())if(!enemies.Any(x=>x.EntityId==entry.Value && x.Word==entry.Key))sources.Remove(entry.Key);
        var words=new List<string>();
        foreach(var candidate in EncounterPolicy.Prioritize(enemies).Where(x=>x.Word.Length>0))
        {sources.TryAdd(candidate.Word,candidate.EntityId);if(!words.Contains(candidate.Word))words.Add(candidate.Word);}
        Words=words;
    }
    public bool StillVisible(string word,uint entityId=0)
    {
        if(entityId==0 && !sources.TryGetValue(word,out entityId))return false;
        var addon=gui.GetAddonByName<AtkUnitBase>("KTGEWord");
        return addon!=null && addon->IsVisible && KtgArrayReader.TryRead(out var words) && words.Any(x=>x.EntityId==entityId && x.Word==word);
    }
    public uint Source(string word)=>sources.GetValueOrDefault(word);
    private static bool Visible(AtkResNode* node)
    {
        var depth=0;for(;node!=null && depth++<32;node=node->ParentNode)if(!node->IsVisible() || node->Color.A==0)return false;
        return depth<32;
    }
}
