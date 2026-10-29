namespace KeyboundPilot;

public enum LoopAction { None, Queue, Accept, StartTyping, Leave, ConfirmExit, ConfirmKeyboard }
public enum LoopPhase { Idle, Requesting, Waiting, Playing, Leaving, Complete, Stopped, Error }
public readonly record struct LoopSnapshot(bool LoggedIn,bool Lobby,bool InGame,bool Loading,bool OtherQueue,
    bool Queued,bool EntryReady,bool Result,bool LeaveReady,bool ExitConfirm,bool Preparing=false,bool KeyboardWarning=false);

/// <summary>One request per round and one response per confirmation; never retry uncertain mutations.</summary>
public sealed class RoundLoop
{
    public LoopPhase Phase {get;private set;}
    public bool Active=>Phase is LoopPhase.Requesting or LoopPhase.Waiting or LoopPhase.Playing or LoopPhase.Leaving;
    public bool Paused {get;private set;}
    public bool StopAfterCurrentRequested {get;private set;}
    public void RequestStopAfterCurrent() {if(Active)StopAfterCurrentRequested=true;}
    public void ToggleStopAfterCurrent() {if(Active)StopAfterCurrentRequested=!StopAfterCurrentRequested;}
    public int Total {get;private set;}
    public int Completed {get;private set;}
    public int Current=>Total==0?0:Math.Min(Total,Phase is LoopPhase.Leaving or LoopPhase.Complete?Completed:Completed+1);
    private long since,blockedAt=-1;
    private bool accepted,left,confirmed,wasPreparing,keyboardConfirmed;
    public void Start(long now,int count,bool inGame)
    {Total=Math.Clamp(count,1,9999);Completed=0;Paused=StopAfterCurrentRequested=false;blockedAt=-1;accepted=left=confirmed=wasPreparing=keyboardConfirmed=false;Set(inGame?LoopPhase.Playing:LoopPhase.Requesting,now);}
    public void Stop(bool error=false) {Phase=error?LoopPhase.Error:LoopPhase.Stopped;Paused=StopAfterCurrentRequested=false;blockedAt=-1;}
    public void Pause() {if(Active)Paused=true;}
    public void Resume() {Paused=false;}
    private void Set(LoopPhase phase,long now) {Phase=phase;since=now;}
    public LoopAction Tick(long now,LoopSnapshot state,bool blocked=false)
    {
        if(!Active)return LoopAction.None;
        if(!state.LoggedIn || state.OtherQueue) {Stop(true);return LoopAction.None;}
        if(Paused || blocked || state.Loading) {if(blockedAt<0)blockedAt=now;return LoopAction.None;}
        if(blockedAt>=0) {since+=now-blockedAt;blockedAt=-1;}
        switch(Phase)
        {
            case LoopPhase.Requesting:
                if(StopAfterCurrentRequested) {Set(LoopPhase.Complete,now);break;}
                if(!state.Lobby || state.Queued || state.InGame) {Stop(true);break;}
                accepted=left=confirmed=wasPreparing=keyboardConfirmed=false;Set(LoopPhase.Waiting,now);return LoopAction.Queue;
            case LoopPhase.Waiting:
                if(state.Preparing)wasPreparing=true;
                else if(wasPreparing){wasPreparing=false;since=now;}
                if(state.InGame) {Set(LoopPhase.Playing,now);return LoopAction.StartTyping;}
                if(state.Lobby && state.KeyboardWarning && !keyboardConfirmed){keyboardConfirmed=true;return LoopAction.ConfirmKeyboard;}
                if(state.EntryReady && state.Queued && !accepted) {accepted=true;return LoopAction.Accept;}
                if((!state.Queued && !accepted && now-since>(state.Preparing?120000:15000)) || now-since>1800000)Stop(true);
                break;
            case LoopPhase.Playing:
                if(state.Result)
                {Completed++;Set(LoopPhase.Leaving,now);if(state.LeaveReady){left=true;return LoopAction.Leave;}}
                else if(!state.InGame)Stop(true);
                break;
            case LoopPhase.Leaving:
                if(!state.InGame && state.Lobby && !state.Queued)
                {Set(Completed>=Total || StopAfterCurrentRequested?LoopPhase.Complete:LoopPhase.Requesting,now);break;}
                if(state.ExitConfirm && left && !confirmed) {confirmed=true;return LoopAction.ConfirmExit;}
                if(state.Result && state.LeaveReady && !left) {left=true;return LoopAction.Leave;}
                if(now-since>120000)Stop(true);
                break;
        }
        return LoopAction.None;
    }
}
