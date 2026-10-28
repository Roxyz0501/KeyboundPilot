using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace KeyboundPilot;

internal sealed unsafe class GameReader(IGameGui gui)
{
    public const int MaxWords = 64;
    private Dictionary<string, uint> sources = new(StringComparer.Ordinal);
    public bool InputVisible { get; private set; }
    public bool InputFocused { get; private set; }
    public IReadOnlyList<string> Words { get; private set; } = [];
    public void Clear() { InputVisible = false; InputFocused = false; Words = []; sources.Clear(); }
    public bool FocusedNow()
    {
        var input = gui.GetAddonByName<AtkUnitBase>("KTGInput");
        if (input == null || !input->IsVisible || !Visible(input->RootNode)) return false;
        var component = input->GetComponentByNodeId(7);
        return component != null && (int)component->GetComponentType() == 7
            && ((AtkComponentTextInput*)component)->Enabled
            && ((AtkComponentTextInput*)component)->IsActive;
    }
    public void Refresh()
    {
        InputFocused = FocusedNow();
        var input = gui.GetAddonByName<AtkUnitBase>("KTGInput");
        InputVisible = input != null && input->IsVisible && Visible(input->RootNode);
        var words = new List<string>();
        var nextSources = new Dictionary<string,uint>(StringComparer.Ordinal);
        var wordAddon=gui.GetAddonByName<AtkUnitBase>("KTGEWord");
        if(InputVisible && wordAddon!=null && wordAddon->IsVisible && KtgArrayReader.TryRead(out var candidates))
            foreach(var candidate in candidates)
                if(nextSources.TryAdd(candidate.Word,candidate.EntityId))words.Add(candidate.Word);
        Words=words;sources=nextSources;
    }
    public bool StillVisible(string word)
    {
        if(!sources.TryGetValue(word,out var id))return false;
        var addon=gui.GetAddonByName<AtkUnitBase>("KTGEWord");
        return addon!=null && addon->IsVisible && KtgArrayReader.TryRead(out var words)
            && words.Any(x=>x.EntityId==id && x.Word==word);
    }
    private static bool Visible(AtkResNode* node)
    {
        var depth = 0;
        for (; node != null && depth++ < 32; node = node->ParentNode)
            if (!node->IsVisible() || node->Color.A == 0) return false;
        return depth < 32;
    }
}
