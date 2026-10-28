using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace KeyboundPilot;

internal sealed unsafe class GameReader(IGameGui gui)
{
    public const int MaxWords = 64;
    private Dictionary<string, int> sources = new(StringComparer.Ordinal);
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
        var nextSources = new Dictionary<string, int>(StringComparer.Ordinal);
        if (InputVisible)
        {
            for (var i = 1; i <= MaxWords; i++)
            {
                var addon = gui.GetAddonByName<AtkUnitBase>("KTGEWord", i);
                if (addon == null || !addon->IsVisible || !Visible(addon->RootNode)) continue;
                var component = addon->GetComponentByNodeId(2);
                if (component == null) continue;
                // Installed KTG EWord ULD: 22 is the small reading; 20 is the main word.
                var word = Read(component->GetTextNodeById(22)) ?? Read(component->GetTextNodeById(20));
                if (word != null && nextSources.TryAdd(word, i)) words.Add(word);
            }
        }
        Words = words; sources = nextSources;
    }
    public bool StillVisible(string word)
    {
        if (!sources.TryGetValue(word, out var index)) return false;
        var addon = gui.GetAddonByName<AtkUnitBase>("KTGEWord", index);
        if (addon == null || !addon->IsVisible || !Visible(addon->RootNode)) return false;
        var component = addon->GetComponentByNodeId(2);
        return component != null && (Read(component->GetTextNodeById(22)) ?? Read(component->GetTextNodeById(20))) == word;
    }
    private static string? Read(AtkTextNode* node)
    {
        if (node == null || !Visible((AtkResNode*)node) || node->NodeText.Length is <= 0 or > 1024) return null;
        var text = SeString.Parse(node->NodeText.AsSpan()).TextValue.Trim();
        return text.Length is > 0 and <= 96 && text.All(c => char.IsAsciiLetterOrDigit(c) || c is ' ' or '-' or '\'') ? text : null;
    }
    private static bool Visible(AtkResNode* node)
    {
        var depth = 0;
        for (; node != null && depth++ < 32; node = node->ParentNode)
            if (!node->IsVisible() || node->Color.A == 0) return false;
        return depth < 32;
    }
}
