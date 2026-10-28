using System.Text;
namespace KeyboundPilot;

/// <summary>At most one physical key per tick. IME confirmation and game submission are separate steps.</summary>
public sealed class TypingSession
{
    private readonly Func<int,int,int> random;
    private long due,pausedAt=-1,commitAt;
    private TypingPlan? plan;
    private int index;
    private bool submitted,committing;
    public bool Running {get;private set;}
    public string Status {get;private set;}="Stopped";
    public string CurrentWord=>plan?.Expected ?? "";
    public bool Japanese=>plan?.Japanese ?? false;
    public bool NeedsPreparation=>plan!=null && index==0 && !submitted;
    public bool Submitted=>submitted;
    public int Position=>index;
    public long Sent {get;private set;}
    public TypingSession(Func<int,int,int>? sample=null)=>random=sample??Random.Shared.Next;
    public void Start(long now) {Stop();Running=true;Status="Arming";due=now+3000;Sent=0;}
    public void Stop(string reason="Stopped") {Running=false;Status=reason;plan=null;index=0;submitted=committing=false;pausedAt=-1;}
    public void Tick(long now,bool ready,string? candidate,bool currentStillVisible,int min,int max,Func<char,bool> send,
        bool paused=false,bool composing=false,string? evaluated=null)
    {
        if(!Running)return;
        if(paused) {if(pausedAt<0)pausedAt=now;Status="Paused";return;}
        if(pausedAt>=0) {var delay=now-pausedAt;due+=delay;commitAt+=delay;pausedAt=-1;}
        if(now<due)return;
        if(!ready) {Stop("FocusLost");return;}
        if(plan!=null && !currentStillVisible)
        {
            if(index>0 && !submitted) {Stop("TargetChanged");return;}
            plan=null;index=0;submitted=committing=false;due=now+max;Status="Waiting";return;
        }
        if(plan!=null && index>=plan.Keys.Length)
        {
            if(!submitted)
            {
                if(plan.Japanese && composing)
                {
                    if(committing) {if(now-commitAt>2000)Stop("ImeFailed");return;}
                    if(!send('\r')) {Stop("SendFailed");return;}
                    committing=true;commitAt=now;due=now+Math.Max(max,150);Status="Confirming";return;
                }
                if(plan.Japanese && (evaluated==null || TypingPlan.NormalizeAnswer(evaluated)!=TypingPlan.NormalizeAnswer(plan.Expected)))
                {Stop("ImeFailed");return;}
                if(!send('\r')) {Stop("SendFailed");return;}
                submitted=true;due=now+max;Status="Waiting";return;
            }
            Status="Waiting";if(now-due>3000)Stop("NoProgress");return;
        }
        if(plan==null)
        {
            if(string.IsNullOrEmpty(candidate)) {Status="Waiting";due=now+100;return;}
            plan=TypingPlan.Create(candidate);
            if(plan==null) {Stop("Unsupported");return;}
            index=0;submitted=committing=false;due=now+random(min,max+1);Status="Typing";return;
        }
        if(!send(plan.Keys[index])) {Stop("SendFailed");return;}
        index++;Sent++;Status="Typing";due=now+random(min,max+1);
    }
}
