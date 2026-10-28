namespace KeyboundPilot;

public sealed class TargetMotion
{
    private readonly Dictionary<uint,(int X,int Y,long Moved)> positions=[];
    public void Clear()=>positions.Clear();
    public IReadOnlyList<KtgWord> Update(long now,IReadOnlyList<KtgWord> words)
    {
        foreach(var id in positions.Keys.ToArray())if(!words.Any(x=>x.Visible && x.EntityId==id))positions.Remove(id);
        return words.Select(word=>
        {
            if(!word.Visible || word.ScreenY<=0)return word with {Moving=false};
            var moved=positions.TryGetValue(word.EntityId,out var prior)?prior.Moved:long.MinValue;
            if(positions.ContainsKey(word.EntityId) && (Math.Abs(word.ScreenX-prior.X)>=2 || Math.Abs(word.ScreenY-prior.Y)>=2))moved=now;
            // Retain the baseline until a meaningful change, so slow sub-pixel movement accumulates.
            if(!positions.ContainsKey(word.EntityId) || moved==now)positions[word.EntityId]=(word.ScreenX,word.ScreenY,moved);
            return word with {Moving=moved!=long.MinValue && now-moved<=2000};
        }).ToArray();
    }
}
