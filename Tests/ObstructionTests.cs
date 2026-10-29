using KeyboundPilot;
using System.Text.Json;
internal static class ObstructionTests
{
 public static void Run(Action<bool,string> check)
 {
  var c=JsonSerializer.Deserialize<Configuration>("{\"Version\":1,\"MinimumMs\":70,\"MaximumMs\":125}")!;
  check(c.ExcludeUnseenPrompts && !c.ObstructionIntervalEnabled,"old settings preserve unseen filtering and normal timing");
  check(c.InputInterval(true)==(70,125),"disabled override uses normal timing during obstruction");
  c.ObstructionIntervalEnabled=true;
  check(c.InputInterval(false)==(70,125) && c.InputInterval(true)==(150,250),"override applies only during effect");
  c.SetObstructionMaximum(20);check(c.ObstructionMinimumMs==150 && c.ObstructionMaximumMs==150,"maximum edit never moves minimum");
  c.SetObstructionMaximum(250);c.SetObstructionMinimum(500);check(c.ObstructionMinimumMs==250 && c.ObstructionMaximumMs==250,"minimum edit never moves maximum");
  c.SetObstructionMinimum(-1);c.SetObstructionMaximum(9000);check(c.ObstructionMinimumMs==20 && c.ObstructionMaximumMs==5000,"obstruction interval bounds");
  check(VisualObstruction.IsStatus(5348) && VisualObstruction.IsStatus(5472) && !VisualObstruction.IsStatus(5347) && !VisualObstruction.IsStatus(0),"visual effects distinguished from input prohibition and no effect");
  var seen=new SeenPrompts();var word=new KtgWord(1,"abc",0,"",0,0);
  seen.Update([word],[new(1,"abc",false)]);
  check(!seen.Eligible(word,true) && seen.Eligible(word,false),"unseen exclusion switch");
  check(!seen.Eligible(word with {Visible=false},false) && !seen.Eligible(word with {Word=""},false),"bypass never selects missing or empty target");
  seen.Update([word],[new(1,"abc",true)]);seen.Update([word],[new(1,"abc",false)]);
  check(seen.Eligible(word,true),"observed prompt remains eligible during next hidden segment");
  seen.Update([word with {Word="def"}],[]);check(!seen.Eligible(word with {Word="def"},true),"changed prompt must be observed again");
  foreach(var background in new[]{false,true})
  {
   c.ObstructionMinimumMs=150;c.ObstructionMaximumMs=250;
   var samples=new List<(int,int)>();var t=new TypingSession((a,b)=>{samples.Add((a,b));return b-1;});t.Start(0);
   void Tick(long now,bool effect){var range=c.InputInterval(effect);t.Tick(now,true,"abcd",true,range.Min,range.Max,_=>true,directInput:background);}
   Tick(0,true);Tick(249,true);check(t.Sent==0,"obstruction upper delay blocks early key in both modes");
   Tick(250,true);Tick(500,true);check(t.Sent==2 && samples.All(x=>x==(150,251)),"visible segments retain obstruction interval in both modes");
   Tick(750,false);check(samples.Last()==(70,126),"effect removal restores normal interval mid-word in both modes");
  }
  var directory=Path.Combine(AppContext.BaseDirectory,"KeyboundPilot-obstruction-"+Guid.NewGuid());Directory.CreateDirectory(directory);
  try
  {
   var path=Path.Combine(directory,"config.json");var clock=new TestClock();
   using(var store=new ConfigurationStore(path,clock:clock))
   {
    var config=store.Configuration;config.ExcludeUnseenPrompts=false;config.ObstructionIntervalEnabled=true;config.ObstructionMinimumMs=180;config.ObstructionMaximumMs=360;
    store.Update(config);clock.Advance(59);store.Tick();check(store.SaveCount==0,"new settings do not bypass delayed autosave");
    clock.Advance(1);store.Tick();check(store.SaveCount==1,"new settings included in changed snapshot: "+store.LastError);
    clock.Advance(60);store.Tick();check(store.SaveCount==1,"unchanged obstruction settings do not write");
   }
   using(var store=new ConfigurationStore(path))check(!store.Configuration.ExcludeUnseenPrompts && store.Configuration.ObstructionIntervalEnabled && store.Configuration.InputInterval(true)==(180,360),"all obstruction settings survive reload");
  }
  finally {Directory.Delete(directory,true);}
 }
}


