using System.Globalization;

namespace KeyboundPilot;

public sealed record ScoreRow(int Slot,string Name,int Score,bool IsSelf);
public sealed record ScoreRecord(Guid Id,DateTimeOffset Ended,string Character,int? OwnScore,ScoreRow[] Players);

public static class ScoreNumbers
{
    // Read only a complete integer. Hidden placeholders, bonuses and arbitrary text are not scores.
    public static int? Parse(string? text)
    {
        if(string.IsNullOrWhiteSpace(text) || text.Length>24)return null;
        var groups=text.Trim().Split([',','.','\u00a0','\u202f',' ']);
        if(groups.Length>1 && (groups[0].Length is <1 or >3 || groups.Skip(1).Any(x=>x.Length!=3)))return null;
        var value=string.Concat(groups);
        return value.Length>0 && value.All(c=>c is >= '0' and <= '9') &&
            int.TryParse(value,NumberStyles.None,CultureInfo.InvariantCulture,out var score)?score:null;
    }
}

public sealed class ScoreSession
{
    private bool active,recorded;
    private Guid id;
    private long resultSince=-1,changedAt;
    private string character="";
    public int? LiveScore {get;private set;}
    public ScoreRow[] Result {get;private set;}=[];
    public bool RecordingEnabled {get;private set;}=true;
    public void SetRecording(bool enabled)
    {
        if(RecordingEnabled==enabled)return;
        RecordingEnabled=enabled;
        // Drop uncommitted observations, but retain this game's identity and recorded flag.
        // Toggling off/on on the same results screen must not create another result.
        Result=[];LiveScore=null;resultSince=-1;changedAt=0;
    }
    public bool ResultSeen=>resultSince>=0;
    public bool ReadyToLeave(long now)=>!RecordingEnabled || !ResultSeen || recorded || now-resultSince>=5000;
    public void Begin(string name,Guid? sessionId=null,ScoreRecord? saved=null)
    {
        active=true;recorded=saved!=null;id=sessionId??Guid.NewGuid();character=name;resultSince=saved==null?-1:0;changedAt=0;LiveScore=null;Result=saved?.Players??[];
    }
    public bool Active=>active;
    public void Observe(long now,int? live,bool resultVisible,ScoreRow[] rows,Action<ScoreRecord> save)
    {
        if(!active || !RecordingEnabled)return;
        LiveScore=live;
        if(!resultVisible)return;
        if(resultSince<0){resultSince=now;changedAt=now;}
        if(recorded)return;
        var observed=Result.Concat(rows).GroupBy(x=>x.Slot).Select(x=>x.Last()).OrderBy(x=>x.Slot).ToArray();
        if(!Result.SequenceEqual(observed)) {Result=observed;changedAt=now;}
        // Allow the result animation to reveal its rows before recording and automatic exit.
        if((Result.Length>0 && now-resultSince>=2000 && now-changedAt>=750) || now-resultSince>=5000)Commit(save);
    }
    public void End(Action<ScoreRecord> save)
    {
        if(RecordingEnabled && active && ResultSeen && !recorded)Commit(save);
        active=false;LiveScore=null;
    }
    private void Commit(Action<ScoreRecord> save)
    {
        var self=Result.FirstOrDefault(x=>x.IsSelf);
        save(new(id,DateTimeOffset.Now,character,self?.Score,Result.Where(x=>x.IsSelf).ToArray()));recorded=true;
    }
}
