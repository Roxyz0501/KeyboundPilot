using KeyboundPilot;

static class MistypeTests
{
 public static void Run(Action<bool,string> check)
 {
  var ranges=new List<(int,int)>();var schedule=new MistypeSchedule((a,b)=>{ranges.Add((a,b));return a;});
  check(!schedule.Due && schedule.Remaining==0,"mistypes default disabled");
  schedule.Configure(true,20);check(ranges.Last()==(15,26) && schedule.Remaining==15,"20 draws inclusive 15 through 25");
  for(var i=0;i<14;i++)schedule.Submitted();check(schedule.Due,"15th answer is due");
  var calls=ranges.Count;schedule.Configure(true,20);check(ranges.Count==calls && schedule.Due,"unchanged settings do not reset count");
  schedule.Submitted();check(ranges.Count==calls+1 && schedule.Remaining==15,"actual submission draws next interval");
  schedule.Configure(true,1);check(ranges.Last()==(1,7),"small interval is at least one");
  schedule.Configure(true,9999);check(ranges.Last()==(9994,10005),"upper setting retains full plus five range");
  var upper=new MistypeSchedule((a,b)=>b-1);upper.Configure(true,20);check(upper.Remaining==25,"upper endpoint selectable");
  schedule.Configure(false,20);schedule.Submitted();check(schedule.Remaining==0 && !schedule.Due,"disabled counter never advances");
  foreach(var word in new[]{"あ","くえ","ヴ", "ABC","~","～","レッドXIII"})
  {
   var original=TypingPlan.Create(word);if(original==null)continue;
   var wrong=MistypeSchedule.WithMistake(original);
   check(wrong!=null && wrong.Expected!=original.Expected && wrong.Expected.Length==original.Expected.Length,"one-character valid typo for "+word);
  }
  var sent=new List<char>();var picks=0;var s=new TypingSession((a,b)=>a,(a,b)=>picks++==0?a:b-1);
  bool Send(char c){sent.Add(c);return true;}
  s.Start(0);s.ConfigureMistypes(true,6);
  s.Tick(0,true,"あ",false,100,100,Send);check(s.CurrentWord=="あ","mistyped plan retains target identity");
  s.Tick(100,true,"あ",true,100,100,Send,composing:true);check(sent.SequenceEqual("i"),"physically type changed kana");
  s.Tick(200,true,"あ",true,100,100,Send,composing:true);check(!s.Submitted && picks==1,"IME commit does not count as submission");
  s.Tick(350,true,"あ",true,100,100,Send,evaluated:"い");check(s.Submitted && picks==2 && sent.Count(c=>c=='\r')==2,"verified wrong answer is actually submitted");
  s.Tick(500,true,"あ",true,100,100,Send,paused:true,evaluated:"い");check(s.Submitted,"pause prevents cleanup input");
  s.Tick(600,true,"あ",true,100,100,Send,evaluated:"い");
  s.Tick(700,true,"あ",true,100,100,Send,evaluated:"い");check(s.Recovering && sent.Last()=='\b',"rejected answer clears without no-progress stop");
  s.Tick(800,true,"あ",true,100,100,Send,evaluated:"");
  s.Tick(900,true,"あ",true,100,100,Send,evaluated:"");
  s.Tick(1000,true,"あ",true,100,100,Send,evaluated:"");check(s.Running && sent.Last()=='a',"next attempt returns to ordinary typing");
  var cancel=new TypingSession((a,b)=>a,(a,b)=>a);cancel.Start(0);cancel.ConfigureMistypes(true,6);
  cancel.Tick(0,true,"あ",false,100,100,Send);cancel.Tick(100,true,"あ",true,100,100,Send,composing:true);
  var enters=sent.Count(c=>c=='\r');cancel.ConfigureMistypes(false,6);cancel.Tick(200,true,"あ",true,100,100,Send,composing:true,evaluated:"い");
  check(cancel.Recovering && sent.Count(c=>c=='\r')==enters,"turning off clears an in-flight typo before submission");
  var abandonedPicks=0;var abandoned=new TypingSession((a,b)=>a,(a,b)=>{abandonedPicks++;return a;});abandoned.Start(0);abandoned.ConfigureMistypes(true,6);
  abandoned.Tick(0,true,"あ",false,100,100,Send);abandoned.Tick(100,true,"くえ",false,100,100,Send,evaluated:"");
  check(abandonedPicks==1 && abandoned.CurrentWord=="","abandoned target does not consume interval");
  var fail=new TypingSession((a,b)=>a,(a,b)=>a);fail.Start(0);fail.ConfigureMistypes(true,6);fail.Tick(0,true,"あ",false,100,100,Send);fail.Tick(100,true,"あ",true,100,100,Send);
  fail.Tick(200,true,"あ",true,100,100,_=>false,evaluated:"い");check(!fail.Running && fail.Status=="SendFailed","failed typo submit stops instead of retrying Enter");

  var red=TypingPlan.Create("レッドXIII")!;check(red.ExactCase && red.Expected=="レッドXIII","red exact spelling plan");
  check(TypingPlan.Create("レッドⅩⅢ")?.Expected=="レッドXIII","roman numeral label normalized for red");
  var typing=new TypingSession((a,b)=>a);sent.Clear();typing.Start(0);typing.Tick(0,true,"レッドXIII",false,100,100,Send);
  for(var t=100;t<=900;t+=100)typing.Tick(t,true,"レッドXIII",true,100,100,Send,composing:true);
  check(sent.Last()==TypingSession.KatakanaKey && typing.Japanese,"F7 converts prefix to katakana");
  typing.Tick(1050,true,"レッドXIII",true,100,100,Send,composing:true);check(sent.Last()=='\r' && !typing.Submitted,"prefix confirmation is not answer submission");
  var before=sent.Count;typing.Tick(1200,true,"レッドXIII",true,100,100,Send,composing:true);check(sent.Count==before,"wait for prefix composition to finish");
  typing.Tick(1300,true,"レッドXIII",true,100,100,Send,evaluated:"レッド");check(sent.Last()==TypingSession.LatinModeKey && !typing.Japanese,"switch to Latin mode only after prefix commit");
  for(var t=1450;t<=1750;t+=100)typing.Tick(t,true,"レッドXIII",true,100,100,Send,evaluated:"レッド");
  check(new string(sent.TakeLast(4).ToArray())=="XIII" && typing.ExactCase,"suffix requests exact uppercase XIII");
  typing.Tick(1850,true,"レッドXIII",true,100,100,Send,evaluated:"レッドXIII");check(typing.Submitted,"exact mixed-script answer submitted");
  var mismatch=new TypingSession((a,b)=>a);mismatch.Start(0);mismatch.Tick(0,true,"レッドXIII",false,100,100,Send);
  for(var t=200;t<=3200;t+=200)mismatch.Tick(t,true,"レッドXIII",true,100,100,Send,composing:t<=2000,evaluated:t<3200?"レッド":"レッドＸＩＩＩ");
  check(!mismatch.Submitted && mismatch.Recovering,"fullwidth suffix cannot pass strict red verification");
  var noComposition=new TypingSession((a,b)=>a);sent.Clear();noComposition.Start(0);noComposition.Tick(0,true,"レッドXIII",false,100,100,Send);
  for(var t=100;t<=900;t+=100)noComposition.Tick(t,true,"レッドXIII",true,100,100,Send,evaluated:"rextsudo");
  check(noComposition.Recovering && !sent.Contains('\r'),"missing red composition never sends a partial answer Enter");

  var root=Path.Combine(AppContext.BaseDirectory,"mistype-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
  try
  {
   var file=Path.Combine(root,"config.json");File.WriteAllText(file,"{\"Version\":1,\"MinimumMs\":80,\"MaximumMs\":150}");
   using(var store=new ConfigurationStore(file))
   {check(!store.Configuration.MistypeEnabled && store.Configuration.MistypeInterval==20,"old settings migrate mistypes off with interval 20");store.Configuration.MistypeEnabled=true;store.Configuration.MistypeInterval=40;store.Update(store.Configuration);check(store.SaveCount==0,"mistype UI changes do not write immediately");store.Flush();}
   using(var store=new ConfigurationStore(file))check(store.Configuration.MistypeEnabled && store.Configuration.MistypeInterval==40,"mistype options survive reload");
  }
  finally{Directory.Delete(root,true);}
 }
}
