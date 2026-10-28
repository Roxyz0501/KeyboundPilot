namespace KeyboundPilot;

public sealed record KtgWord(uint EntityId,string Word,int Slot,string Cast,int Progress,int Special);

/// <summary>Only the observed Japanese jammer is recognized. Cast end is a sampled inference, not a server action event.</summary>
public sealed class EncounterPolicy
{
    private readonly Dictionary<uint,int> jammerProgress=[];
    public long PauseUntil { get; private set; }
    public bool Update(long now,IReadOnlyList<KtgWord> enemies)
    {
        var triggered=false;
        foreach(var prior in jammerProgress.ToArray())
        {
            var enemy=enemies.FirstOrDefault(x=>x.EntityId==prior.Key);
            if(enemy?.Cast=="動体感知ジャマー")continue;
            // A vanished enemy or an early interruption is not an observed activation.
            if(enemy!=null && prior.Value>=90) {PauseUntil=Math.Max(PauseUntil,now+4000);triggered=true;}
            jammerProgress.Remove(prior.Key);
        }
        foreach(var enemy in enemies)
            if(enemy.Cast=="動体感知ジャマー")jammerProgress[enemy.EntityId]=enemy.Progress;
        return triggered;
    }
    public bool Paused(long now)=>now<PauseUntil;
    public void Reset() {jammerProgress.Clear();PauseUntil=0;}
    public static IEnumerable<KtgWord> Prioritize(IEnumerable<KtgWord> enemies)=>enemies
        .OrderByDescending(x=>x.Cast.Length>0).ThenByDescending(x=>x.Cast.Length>0?x.Progress:0).ThenBy(x=>x.Slot);
}
