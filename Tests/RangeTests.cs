using KeyboundPilot;
internal static class RangeTests
{
 public static void Run(Action<bool,string> check)
 {
  var c=new Configuration{BetweenWordsMs=50,MistypeInterval=27};
  check(c.WaitMin==50 && c.WaitMax==50 && c.TypoMin==22 && c.TypoMax==32,"legacy ranges preserve fixed wait and plus/minus five");
  c.SetWaitMaximum(20);check(c.WaitMin==50 && c.WaitMax==50,"wait maximum cannot move minimum");
  c.SetWaitMaximum(100);c.SetWaitMinimum(200);check(c.WaitMin==100 && c.WaitMax==100,"wait minimum cannot move maximum");
  c.SetWaitMinimum(-1);c.SetWaitMaximum(70000);check(c.WaitMin==0 && c.WaitMax==60000,"wait limits clamped");
  c.SetTypoMaximum(1);check(c.TypoMin==22 && c.TypoMax==22,"typo maximum cannot move minimum");
  c.SetTypoMaximum(30);c.SetTypoMinimum(100);check(c.TypoMin==30 && c.TypoMax==30,"typo minimum cannot move maximum");
  c.SetTypoMinimum(0);c.SetTypoMaximum(20000);check(c.TypoMin==1 && c.TypoMax==10004,"typo bounds preserve legacy largest interval");
  var draws=new List<(int,int)>();var m=new MistypeSchedule((a,b)=>{draws.Add((a,b));return b-1;});
  m.Configure(true,2,4);check(m.Remaining==4 && draws.Last()==(2,5),"explicit typo range includes upper endpoint without extra plus/minus");
  m.Configure(true,2,4);check(draws.Count==1,"unchanged range does not resample");
  for(int i=0;i<4;i++)m.Submitted();check(draws.Count==2 && m.Remaining==4,"typo resampled after actual chosen answer count");
  m.Configure(true,7,7);check(m.Remaining==7,"equal typo bounds give fixed count");
  foreach(var background in new[]{false,true})
  {
   var waits=new List<(int,int)>();var t=new TypingSession((a,b)=>{waits.Add((a,b));return b-1;});t.Start(0);var text="";
   bool Send(char ch){if(ch!='\r')text+=ch;return true;}
   void Tick(long now,bool visible=true)=>t.Tick(now,true,"a",visible,20,20,Send,evaluated:text,betweenWordsMs:100,directInput:background,betweenWordsMaximumMs:300);
   Tick(0);Tick(20);Tick(40);check(t.Submitted && waits.Contains((100,301)),"post-submit sample inclusive for both transports");
   var count=waits.Count;Tick(60);check(waits.Count==count,"submission wait never resampled while waiting");
   Tick(80,false);text="";Tick(339);check(t.CurrentWord=="","upper sampled wait blocks next target");Tick(340);check(t.CurrentWord=="a","next target starts at sampled deadline");
  }
  foreach(var background in new[]{false,true})
  {
   var t=new TypingSession((a,b)=>a);t.Start(0);string text="";int sends=0;
   bool Send(char c){if(c=='\r'){sends++;text="";}else text+=c;return true;}
   void Tick(long n)=>t.Tick(n,true,"a",true,20,20,Send,evaluated:text,betweenWordsMs:500,directInput:background);
   Tick(0);Tick(20);Tick(40);Tick(60);Tick(539);
   check(t.Running && t.CurrentWord=="" && sends==1,"cleared submitted field advances even with unchanged target, respecting delay");
   Tick(540);Tick(560);Tick(580);check(sends==2 && t.Running,"same surviving target can receive another answer in both modes");
  }
  var unconfirmed=new TypingSession((a,b)=>a);unconfirmed.Start(0);
  unconfirmed.Tick(0,true,"a",true,20,20,_=>true);unconfirmed.Tick(20,true,"a",true,20,20,_=>true);unconfirmed.Tick(40,true,"a",true,20,20,_=>true);
  unconfirmed.Tick(4000,true,"a",true,20,20,_=>throw new Exception("unexpected retry"),evaluated:"a");
  check(!unconfirmed.Running && unconfirmed.Status=="NoProgress","uncertain uncleared answer is not retried");
  foreach(var background in new[]{false,true})
  {
   var t=new TypingSession((a,b)=>a);t.Start(0);int reset=0;
   void Tick(long n,bool composing=false,bool paused=false)=>t.Tick(n,true,"あ",true,20,20,_=>true,paused:paused,composing:composing,evaluated:"あ",directInput:background,onSubmitted:()=>reset++);
   Tick(0);Tick(20);Tick(40,true);check(reset==0,"typing and IME confirmation never reset submission activity");
   Tick(190,false,true);check(reset==0,"paused submission does not reset activity");Tick(290);Tick(390);
   check(reset==1 && t.Submitted,"successful answer resets activity once in both modes");
  }
  var protection=new ActivityKeeper();var resetCalls=0;bool Reset(){resetCalls++;return true;}
  protection.OnSubmission(0,Reset);check(resetCalls==0,"stopped session does not reset on callback");
  protection.Begin();for(int n=0;n<3;n++)protection.Tick(n*10000,true,()=>false);
  protection.OnSubmission(30000,Reset);check(resetCalls==1 && !protection.Failed,"successful answer retries failed periodic protection");
  protection.Tick(39999,true,Reset);check(resetCalls==1,"submission reset postpones redundant periodic write");protection.Tick(40000,true,Reset);check(resetCalls==2,"periodic protection continues after submission");
  var dir=Path.Combine(AppContext.BaseDirectory,"ranges-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);var file=Path.Combine(dir,"config.json");
  try
  {
   File.WriteAllText(file,"{\"Version\":1,\"BetweenWordsMs\":50,\"MistypeInterval\":27}");
   using(var store=new ConfigurationStore(file))
   {var cfg=store.Configuration;check(cfg.WaitMin==50 && cfg.WaitMax==50 && cfg.TypoMin==22 && cfg.TypoMax==32,"old JSON migrates unchanged behavior");cfg.SetWaitMaximum(500);cfg.SetTypoMinimum(25);store.Update(cfg);check(store.SaveCount==0,"range UI changes do not write immediately");store.Flush();}
   using(var store=new ConfigurationStore(file))check(store.Configuration.WaitMax==500 && store.Configuration.WaitMin==50 && store.Configuration.TypoMin==25 && store.Configuration.TypoMax==32,"ranges survive serialization and reload");
  }
  finally{Directory.Delete(dir,true);}
 }
}
