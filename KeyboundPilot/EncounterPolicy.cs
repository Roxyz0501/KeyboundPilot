namespace KeyboundPilot;

public sealed record KtgWord(uint EntityId,string Word,int Slot,string Cast,int Progress,int Special,int Kind=0,bool Visible=true,int ScreenX=0,int ScreenY=0,bool Moving=false);

/// <summary>Recognizes the Japanese jammer and cursed gaze. Cast end is a sampled inference, not a server action event.</summary>
public sealed class EncounterPolicy
{
    private readonly Dictionary<uint,(string Cast,int Progress)> blockingProgress=[];
    public long PauseUntil { get; private set; }
    public bool Update(long now,IReadOnlyList<KtgWord> enemies)
    {
        var triggered=false;
        foreach(var prior in blockingProgress.ToArray())
        {
            var enemy=enemies.FirstOrDefault(x=>x.EntityId==prior.Key);
            if(enemy?.Cast==prior.Value.Cast)continue;
            // A vanished enemy or an early interruption is not an observed activation.
            if(enemy!=null && prior.Value.Progress>=90) {PauseUntil=Math.Max(PauseUntil,now+4000);triggered=true;}
            blockingProgress.Remove(prior.Key);
        }
        foreach(var enemy in enemies)
            if(enemy.Cast is "動体感知ジャマー" or "呪いの視線")blockingProgress[enemy.EntityId]=(enemy.Cast,enemy.Progress);
        return triggered;
    }
    public bool Paused(long now)=>now<PauseUntil;
    public void Reset() {blockingProgress.Clear();PauseUntil=0;}
    public static IEnumerable<KtgWord> Prioritize(IEnumerable<KtgWord> enemies)
    {
        var current=enemies.ToArray();
        var closest=current.Where(x=>x.Visible && x.Kind!=2 && x.Cast.Length==0 && x.Moving && x.ScreenY>0)
            .OrderByDescending(x=>x.ScreenY).ThenBy(x=>x.Slot).FirstOrDefault();
        return current.OrderByDescending(x=>x.Kind==2).ThenByDescending(x=>x.Cast.Length>0)
            .ThenByDescending(x=>x.Cast.Length>0?x.Progress:0).ThenByDescending(x=>x==closest).ThenBy(x=>x.Slot);
    }
}
