namespace KeyboundPilot;

public readonly record struct PromptDisplay(uint EntityId,string Word,bool TextVisible);

/// <summary>Only retain observations for current entity/reading pairs (at most 20 slots).</summary>
public sealed class SeenPrompts
{
    private readonly Dictionary<uint,string> seen=[];
    public void Clear()=>seen.Clear();
    public void Update(IReadOnlyList<KtgWord> current,IReadOnlyList<PromptDisplay> display)
    {
        foreach(var pair in seen.ToArray())
            if(!current.Any(x=>x.Visible && x.EntityId==pair.Key && x.Word==pair.Value))seen.Remove(pair.Key);
        foreach(var observation in display)
            if(observation.TextVisible && current.Any(x=>x.Visible && x.EntityId==observation.EntityId && x.Word==observation.Word))
                seen[observation.EntityId]=observation.Word;
    }
    public bool Eligible(KtgWord word,bool excludeUnseen) => word.Visible && word.Word.Length>0 && (!excludeUnseen || Contains(word));
    public bool Contains(KtgWord word)=>seen.TryGetValue(word.EntityId,out var text) && text==word.Word;
}
