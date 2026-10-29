using KeyboundPilot;
internal static class BackgroundTests
{
    public static void Run(Action<bool,string> check)
    {
        foreach(var word in new[]{"くえ","じゅんぱくのていおうゔぁいす","レッドXIII","あ～〜~","ABC 123"})
        {
            var plan=TypingPlan.Create(word)!;
            var steps=BackgroundInput.Steps(plan);
            check(new string(steps.Where(c=>c>=32).ToArray())==plan.Expected,"background preserves expected Unicode: "+word);
            check(steps.Count(c=>c>=32)==plan.Expected.Length,"background emits one final character at a time");
            check(!steps.Contains('\r') && !steps.Contains(TypingSession.LatinModeKey),"background has no IME or prefix Enter dispatch");
        }
        var kana=BackgroundInput.Steps(TypingPlan.Create("くえ")!);
        check(kana.Length==4 && kana[0]==TypingSession.InternalWaitKey,"ku-e uses three romaji steps plus commit");
        var red=BackgroundInput.Steps(TypingPlan.Create("レッドXIII")!);
        check(red.Count(c=>c==TypingSession.InternalCommitKey)==4,"Red XIII preserves conversion, prefix, mode and final confirmation waits");
        var session=new TypingSession((a,b)=>a);session.Start(0);
        string text="";var events=new List<(long,char)>();long time=0;
        bool Send(char c){events.Add((time,c));if(c=='\b')text=text.Length==0?"":text[..^1];else if(c!='\r')text+=c;return true;}
        void Tick(long now,bool visible=true,bool paused=false){time=now;session.Tick(now,true,"くえ",visible,80,80,Send,paused:paused,evaluated:text,directInput:true,betweenWordsMs:500);}
        Tick(0);Tick(80);check(events.Count==0,"romaji prefix is internal wait only");
        Tick(160);check(text=="く","kana emitted after both romaji intervals");
        Tick(240);check(text=="くえ","converted text preserved");Tick(320);Tick(469);
        check(!events.Any(e=>e.Item2=='\r'),"simulated IME commit waits 150 ms");
        Tick(470,paused:true);check(!session.Submitted,"prohibition blocks background submit");Tick(570);Tick(670);
        check(session.Submitted && events.Count(e=>e.Item2=='\r')==1,"one final answer submit after pause");
        text="";Tick(750,false);Tick(1000);check(session.CurrentWord=="","post-submit delay preserved");Tick(1170);check(session.CurrentWord=="くえ","post-submit resumes at deadline");
        session.Start(0);text="";events.Clear();Tick(0);Tick(80);Tick(160);Tick(240,false);
        check(text=="" && events.Last().Item2=='\b',"vanished target clears converted characters");
        session.Start(0);text="";events.Clear();Tick(0);Tick(80);Tick(160);Tick(240);text="wrong";Tick(320);Tick(470);
        check(session.Recovering && !session.Submitted,"background verifies exact text before submit");
        var typo=new TypingSession((a,b)=>a,(a,b)=>1);typo.ConfigureMistypes(true,1);typo.Start(0);string wrong="";int enters=0;
        for(long now=0;now<2000 && enters==0;now+=100)
            typo.Tick(now,true,"くえ",true,80,80,c=>{if(c=='\r')enters++;else wrong+=c;return true;},evaluated:wrong,directInput:true);
        check(enters==1 && wrong=="あえ" && typo.CurrentWord=="くえ","background typo changes answer but keeps target identity");
        var path=Path.Combine(AppContext.BaseDirectory,"keybound-background-"+Guid.NewGuid()+".json");
        try
        {
            using(var store=new ConfigurationStore(path))
            {check(!store.Configuration.DirectInput,"background defaults off");store.Configuration.DirectInput=true;store.Update(store.Configuration);check(!File.Exists(path),"background toggle does not write immediately");store.Flush();check(store.LastError==null,"background save: "+store.LastError);}
            using(var store=new ConfigurationStore(path))check(store.Configuration.DirectInput,"background persists across restart");
        }
        finally{if(File.Exists(path))File.Delete(path);}
    }
}

