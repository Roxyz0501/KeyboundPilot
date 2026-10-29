using System.Text;
namespace KeyboundPilot;

/// <summary>At most one physical key per tick. IME confirmation and game submission are separate steps.</summary>
public sealed class TypingSession
{
    public const char InternalWaitKey='\0'; // One simulated romaji key interval; never dispatched.
    public const char InternalCommitKey='\u0001'; // Simulated IME conversion/commit.
    public const char HiraganaKey='\u0006'; // Internal key command, mapped to physical F6.
    public const char KatakanaKey='\u0007'; // Physical F7, before committing the katakana prefix.
    public const char LatinModeKey='\u000f'; // Change IME mode only after the prefix is committed.
    private readonly Func<int,int,int> random;
    private readonly MistypeSchedule mistakes;
    private string targetWord="";
    private bool plannedMistake, mistakeSubmitted, cancelMistake;
    private long due,pausedAt=-1,commitAt,nextWordAt;
    private TypingPlan? plan;
    private int index,clearKeys;
    private bool submitted,committing,hiraganaRequested,clearing;
    public string AnswerCheck {get;private set;}="None";
    public bool Running {get;private set;}
    public bool ManualPaused {get;private set;}
    public string Status {get;private set;}="Stopped";
    public string CurrentWord=>plan==null?"":targetWord;
    public bool Japanese=>plan!=null && plan.Japanese && (plan.LatinModeIndex<0 || index<=plan.LatinModeIndex);
    public bool ExactCase=>plan?.ExactCase ?? false;
    public bool NeedsPreparation=>plan!=null && index==0 && !submitted;
    public bool Submitted=>submitted;
    public bool NeedsInputFocus=>!submitted || mistakeSubmitted;
    public bool Recovering=>clearing;
    public void RecoverIme(long now) { if(!Running)return; clearing=true;submitted=false;index=Math.Max(1,index);due=now;Status="Clearing"; }
    public int Position=>index;
    public long Sent {get;private set;}
    public TypingSession(Func<int,int,int>? sample=null,Func<int,int,int>? mistakeSample=null)
    {random=sample??Random.Shared.Next;mistakes=new MistypeSchedule(mistakeSample);}
    public void ConfigureMistypes(bool enabled,int interval)
    {
        if(mistakes.Configure(enabled,interval) && plannedMistake && !submitted)cancelMistake=true;
    }
    public void ConfigureMistypes(bool enabled,int minimum,int maximum)
    {
        if(mistakes.Configure(enabled,minimum,maximum) && plannedMistake && !submitted)cancelMistake=true;
    }
    public void Start(long now) {Stop();mistakes.Reset();Running=true;Status="Waiting";due=now;Sent=0;}
    public void Stop(string reason="Stopped") {Running=false;ManualPaused=false;Status=reason;plan=null;targetWord="";plannedMistake=mistakeSubmitted=cancelMistake=false;index=clearKeys=0;submitted=committing=hiraganaRequested=clearing=false;pausedAt=-1;nextWordAt=0;}
    public void PauseByUser() {if(Running){ManualPaused=true;Status="UserPaused";}}
    public void ResumeByUser() {if(Running){ManualPaused=false;Status="Waiting";}}
    public void Tick(long now,bool ready,string? candidate,bool currentStillVisible,int min,int max,Func<char,bool> send,
        bool paused=false,bool composing=false,string? evaluated=null,int betweenWordsMs=0,bool directInput=false,int? betweenWordsMaximumMs=null,Action? onSubmitted=null)
    {
        if(!Running)return;
        if(cancelMistake){cancelMistake=false;RecoverIme(now);}
        if(paused || !ready || ManualPaused) {if(pausedAt<0)pausedAt=now;Status=ManualPaused?"UserPaused":"Paused";return;}
        if(pausedAt>=0) {var delay=now-pausedAt;due+=delay;commitAt+=delay;pausedAt=-1;}
        if(now<due)return;
        if(mistakeSubmitted)
        {
            // A rejected answer can leave the same target and input text present.
            // Clear through the normal verified path instead of hitting NoProgress.
            mistakeSubmitted=false;submitted=false;clearing=true;
        }
        if(clearing || (plan!=null && !currentStillVisible))
        {
            if(index>0 && !submitted && (composing || evaluated==null || evaluated.Length>0))
            {
                clearing=true;
                if(evaluated==null && !composing) {Status="Clearing";return;}
                if(clearKeys>=384) {Stop("NoProgress");return;}
                if(!send('\b')) {Stop("SendFailed");return;}
                clearKeys++;due=now+max;Status="Clearing";return;
            }
            plan=null;targetWord="";plannedMistake=false;index=clearKeys=0;submitted=committing=hiraganaRequested=clearing=false;due=Math.Max(now,nextWordAt);Status=now<nextWordAt?"BetweenWords":"Waiting";return;
        }
        if(plan!=null && index>=plan.Keys.Length)
        {
            if(!submitted)
            {
                if(plan.Japanese && composing)
                {
                    if(committing) {if(now-commitAt>2000)RecoverIme(now);return;}
                    if(!directInput && !hiraganaRequested && plan.Expected.Contains('ゔ'))
                    {
                        if(!send(HiraganaKey)) {Stop("SendFailed");return;}
                        hiraganaRequested=true;due=now+Math.Max(max,150);Status="Confirming";return;
                    }
                    if(!send('\r')) {Stop("SendFailed");return;}
                    committing=true;commitAt=now;due=now+Math.Max(max,150);Status="Confirming";return;
                }
                if(plan.Japanese)
                {
                    AnswerCheck=evaluated==null?"Unavailable":(plan.ExactCase?evaluated!=plan.Expected:TypingPlan.NormalizeAnswer(evaluated)!=TypingPlan.NormalizeAnswer(plan.Expected))?"DifferentText":evaluated==plan.Expected?"Exact":"Equivalent";
                    if(AnswerCheck is "Unavailable" or "DifferentText") {RecoverIme(now);return;}
                }
                if(!send('\r')) {Stop("SendFailed");return;}
                submitted=true;onSubmitted?.Invoke();mistakeSubmitted=plannedMistake;mistakes.Submitted();var waitMin=Math.Clamp(betweenWordsMs,0,60000);var waitMax=Math.Clamp(betweenWordsMaximumMs??waitMin,waitMin,60000);nextWordAt=now+(waitMin==waitMax?waitMin:random(waitMin,waitMax+1));due=now+Math.Max(max,plannedMistake?150:0);Status="Waiting";return;
            }
            // A consumed answer may leave the same enemy/prompt visible (for example multiple hits).
            // A cleared field is observable progress; do not require target disappearance as well.
            if(!composing && evaluated=="")
            {
                plan=null;targetWord="";plannedMistake=false;index=clearKeys=0;
                submitted=committing=hiraganaRequested=clearing=false;
                due=Math.Max(now,nextWordAt);Status=now<nextWordAt?"BetweenWords":"Waiting";return;
            }
            Status="Waiting";if(now-due>3000)Stop("NoProgress");return;
        }
        if(plan==null)
        {
            if(string.IsNullOrEmpty(candidate)) {Status="Waiting";due=now+100;return;}
            plan=TypingPlan.Create(candidate);
            if(plan==null) {Stop("Unsupported");return;}
            targetWord=candidate.Trim();plannedMistake=mistakes.Due;
            if(plannedMistake)plan=MistypeSchedule.WithMistake(plan)??plan;
            if(directInput)plan=plan with {Keys=BackgroundInput.Steps(plan),Japanese=true,ExactCase=true};
            index=0;submitted=committing=hiraganaRequested=clearing=false;AnswerCheck="None";due=now+random(min,max+1);Status="Typing";return;
        }
        var key=plan.Keys[index];
        if(!directInput && plan.ExactCase && key is KatakanaKey or '\r' && !composing)
        {RecoverIme(now);return;}
        if(key==LatinModeKey && composing)
        {if(now-commitAt>2000)RecoverIme(now);else Status="Confirming";return;}
        if(key==LatinModeKey && evaluated!=plan.Expected[..^4])
        {RecoverIme(now);return;}
        if(key is not (InternalWaitKey or InternalCommitKey) && !send(key)) {Stop("SendFailed");return;}
        if(key=='\r')commitAt=now;
        index++;if(key>=32)Sent++;Status="Typing";due=now+(key==InternalCommitKey?Math.Max(max,150):Math.Max(random(min,max+1),key is KatakanaKey or '\r' or LatinModeKey?150:0));
    }
}
