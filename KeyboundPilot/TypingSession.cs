using System.Text;
namespace KeyboundPilot;

/// <summary>At most one physical key per tick. IME confirmation and game submission are separate steps.</summary>
public sealed class TypingSession
{
    public const char HiraganaKey='\u0006'; // Internal key command, mapped to physical F6.
    private readonly Func<int,int,int> random;
    private long due,pausedAt=-1,commitAt,nextWordAt;
    private TypingPlan? plan;
    private int index,clearKeys;
    private bool submitted,committing,hiraganaRequested,clearing;
    public string AnswerCheck {get;private set;}="None";
    public bool Running {get;private set;}
    public bool ManualPaused {get;private set;}
    public string Status {get;private set;}="Stopped";
    public string CurrentWord=>plan?.Expected ?? "";
    public bool Japanese=>plan?.Japanese ?? false;
    public bool NeedsPreparation=>plan!=null && index==0 && !submitted;
    public bool Submitted=>submitted;
    public int Position=>index;
    public long Sent {get;private set;}
    public TypingSession(Func<int,int,int>? sample=null)=>random=sample??Random.Shared.Next;
    public void Start(long now) {Stop();Running=true;Status="Waiting";due=now;Sent=0;}
    public void Stop(string reason="Stopped") {Running=false;ManualPaused=false;Status=reason;plan=null;index=clearKeys=0;submitted=committing=hiraganaRequested=clearing=false;pausedAt=-1;nextWordAt=0;}
    public void PauseByUser() {if(Running){ManualPaused=true;Status="UserPaused";}}
    public void ResumeByUser() {if(Running){ManualPaused=false;Status="Waiting";}}
    public void Tick(long now,bool ready,string? candidate,bool currentStillVisible,int min,int max,Func<char,bool> send,
        bool paused=false,bool composing=false,string? evaluated=null,int betweenWordsMs=0)
    {
        if(!Running)return;
        if(paused || !ready || ManualPaused) {if(pausedAt<0)pausedAt=now;Status=ManualPaused?"UserPaused":"Paused";return;}
        if(pausedAt>=0) {var delay=now-pausedAt;due+=delay;commitAt+=delay;pausedAt=-1;}
        if(now<due)return;
        if(plan!=null && (!currentStillVisible || clearing))
        {
            if(index>0 && !submitted && (composing || evaluated==null || evaluated.Length>0))
            {
                clearing=true;
                if(evaluated==null && !composing) {Status="Clearing";return;}
                if(clearKeys>=384) {Stop("NoProgress");return;}
                if(!send('\b')) {Stop("SendFailed");return;}
                clearKeys++;due=now+max;Status="Clearing";return;
            }
            plan=null;index=clearKeys=0;submitted=committing=hiraganaRequested=clearing=false;due=Math.Max(now,nextWordAt);Status=now<nextWordAt?"BetweenWords":"Waiting";return;
        }
        if(plan!=null && index>=plan.Keys.Length)
        {
            if(!submitted)
            {
                if(plan.Japanese && composing)
                {
                    if(committing) {if(now-commitAt>2000)Stop("ImeFailed");return;}
                    if(!hiraganaRequested && plan.Expected.Contains('ゔ'))
                    {
                        if(!send(HiraganaKey)) {Stop("SendFailed");return;}
                        hiraganaRequested=true;due=now+Math.Max(max,150);Status="Confirming";return;
                    }
                    if(!send('\r')) {Stop("SendFailed");return;}
                    committing=true;commitAt=now;due=now+Math.Max(max,150);Status="Confirming";return;
                }
                if(plan.Japanese)
                {
                    AnswerCheck=evaluated==null?"Unavailable":TypingPlan.NormalizeAnswer(evaluated)!=TypingPlan.NormalizeAnswer(plan.Expected)?"DifferentText":evaluated==plan.Expected?"Exact":"Equivalent";
                    if(AnswerCheck is "Unavailable" or "DifferentText") {Stop("ImeFailed");return;}
                }
                if(!send('\r')) {Stop("SendFailed");return;}
                submitted=true;nextWordAt=now+Math.Clamp(betweenWordsMs,0,60000);due=now+max;Status="Waiting";return;
            }
            Status="Waiting";if(now-due>3000)Stop("NoProgress");return;
        }
        if(plan==null)
        {
            if(string.IsNullOrEmpty(candidate)) {Status="Waiting";due=now+100;return;}
            plan=TypingPlan.Create(candidate);
            if(plan==null) {Stop("Unsupported");return;}
            index=0;submitted=committing=hiraganaRequested=clearing=false;AnswerCheck="None";due=now+random(min,max+1);Status="Typing";return;
        }
        if(!send(plan.Keys[index])) {Stop("SendFailed");return;}
        index++;Sent++;Status="Typing";due=now+random(min,max+1);
    }
}
