using KeyboundPilot;
internal static class InputProfileTests
{
 public static void Run(Action<bool,string> check)
 {
  var c=new Configuration();
  check(c.DirectInput && !c.LowHpEnabled && !c.ObstructionIntervalEnabled && !c.LowHpFirst,"new profile defaults");
  c.ObstructionIntervalEnabled=true;c.LowHpEnabled=true;
  c.ObstructionSettings=new(120,180,400,600,true,2,4);c.LowHpSettings=new(20,30,100,200,true,1,1);
  check(c.ResolveInput(false,100)==c.NormalInput,"healthy uses normal");
  check(c.ResolveInput(false,49)==c.LowHpSettings,"below threshold activates all low HP settings");
  check(c.ResolveInput(false,50)==c.NormalInput,"recovery exactly at threshold restores normal");
  check(c.ResolveInput(false,100)==c.NormalInput,"full recovery restores normal");
  check(c.ResolveInput(true,49)==c.ObstructionSettings,"obstruction wins by default");
  c.LowHpFirst=true;check(c.ResolveInput(true,49)==c.LowHpSettings,"priority can be reversed");
  check(c.ResolveInput(true,50)==c.ObstructionSettings,"recovery falls back to remaining obstruction");
  check(c.ResolveInput(false,50)==c.NormalInput,"both effects ending restores normal");
  c.LowHpEnabled=false;check(c.ResolveInput(true,0)==c.ObstructionSettings,"disabled high priority cannot mask other profile");
  c.ObstructionIntervalEnabled=false;check(c.ResolveInput(true,0)==c.NormalInput,"all disabled follows normal values");
  c.LowHpEnabled=true;
  foreach(var hp in new float?[]{null,float.NaN,float.PositiveInfinity,-1,101})check(c.ResolveInput(false,hp)==c.NormalInput,"unavailable or invalid HP fails back to normal");
  c.LowHpThreshold=100;check(c.ResolveInput(false,99.9f)==c.LowHpSettings && c.ResolveInput(false,100)==c.NormalInput,"HP percentage is not rounded before comparison");
  c.LowHpThreshold=1;check(c.ResolveInput(false,0)==c.LowHpSettings && c.ResolveInput(false,1)==c.NormalInput,"threshold lower bound");
  c.LowHpThreshold=50;c.LowHpFirst=false;c.ObstructionIntervalEnabled=true;
  foreach(var background in new[]{false,true})
  foreach(var condition in new[]{(false,100f),(true,100f),(false,10f)})
  {
   c.DirectInput=background;
   var values=c.ResolveInput(condition.Item1,condition.Item2);
   var samples=new List<(int,int)>();var typoSamples=new List<(int,int)>();string text="";
   var session=new TypingSession((a,b)=>{samples.Add((a,b));return b-1;},(a,b)=>{typoSamples.Add((a,b));return a;});
   session.ConfigureMistypes(values.MistypeEnabled,values.TypoMin,values.TypoMax);session.Start(0);
   for(long now=0;now<=2500 && !session.Submitted;now++)
    session.Tick(now,true,"a",true,values.MinimumMs,values.MaximumMs,ch=>{if(ch=='\b')text=text.Length==0?"":text[..^1];else if(ch!='\r')text+=ch;return true;},evaluated:text,directInput:background,betweenWordsMs:values.WaitMin,betweenWordsMaximumMs:values.WaitMax);
   check(session.Submitted,"each profile submits in both modes");
   check(samples.Contains((values.MinimumMs,values.MaximumMs+1)),"selected input bounds reach typing scheduler");
   check(values.WaitMin==values.WaitMax || samples.Contains((values.WaitMin,values.WaitMax+1)),"selected post-submit bounds reach scheduler");
   check(values.MistypeEnabled?typoSamples.All(x=>x==(values.TypoMin,values.TypoMax+1)):typoSamples.Count==0,"selected typo bounds reach schedule");
   check(text==(values.MistypeEnabled && values.TypoMin==1?"b":"a"),"profile typo switch controls actual answer");
  }
  var folder=Path.Combine(AppContext.BaseDirectory,"profiles-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
  try
  {
   var path=Path.Combine(folder,"config.json");
   File.WriteAllText(path,"{\"Version\":1,\"DirectInput\":false,\"ObstructionIntervalEnabled\":true,\"ObstructionMinimumMs\":170,\"ObstructionMaximumMs\":260,\"BetweenWordsMs\":88,\"MistypeEnabled\":true}");
   var clock=new TestClock();
   using(var store=new ConfigurationStore(path,clock:clock))
   {
    var cfg=store.Configuration;
    check(!cfg.DirectInput,"saved foreground preference survives new default");
    check(cfg.ObstructionInput==new InputSettings(170,260,88,88,true,15,25),"legacy obstruction intervals and normal extras preserved");
    var inherited=cfg.ObstructionInput;cfg.SetWaitMaximum(120);check(cfg.ObstructionInput==inherited,"migrated dedicated extras remain independent of normal edits");cfg.SetWaitMaximum(88);
    cfg.ObstructionSettings=cfg.ObstructionInput with {WaitMin=300,WaitMax=700,TypoMin=3,TypoMax=8};
    cfg.LowHpSettings=new(35,75,180,380,true,9,12);cfg.LowHpThreshold=65;cfg.LowHpFirst=true;cfg.LowHpEnabled=true;
    store.Update(cfg);clock.Advance(59);store.Tick();check(store.SaveCount==0,"profile edits wait for autosave");
    clock.Advance(1);store.Tick();check(store.SaveCount==1 && store.LastError==null,"profile snapshot persisted");
    clock.Advance(60);store.Tick();check(store.SaveCount==1,"unchanged profiles do not write");
    var original=cfg.LowHpSettings;cfg.LowHpSettings=original with {WaitMax=400};store.Update(cfg);cfg.LowHpSettings=original;store.Update(cfg);
    check(!store.Dirty,"immutable profile values allow reverted edits to cancel writes");
   }
   using(var store=new ConfigurationStore(path))
   {
    var cfg=store.Configuration;
    check(cfg.LowHpEnabled && cfg.LowHpFirst && cfg.LowHpThreshold==65 && cfg.LowHpSettings==new InputSettings(35,75,180,380,true,9,12),"HP profile and precedence survive restart");
    check(cfg.ObstructionInput==new InputSettings(170,260,300,700,true,3,8),"obstruction complete profile survives restart");
   }
   foreach(var body in new[]{"\"LowHpThreshold\":0","\"LowHpThreshold\":101","\"LowHpSettings\":null","\"LowHpSettings\":{\"MinimumMs\":400,\"MaximumMs\":20}","\"ObstructionSettings\":{\"WaitMin\":500,\"WaitMax\":2}"})
   {
    var bad=Path.Combine(folder,Guid.NewGuid()+".json");File.WriteAllText(bad,"{\"Version\":1,"+body+"}");
    using var store=new ConfigurationStore(bad);check(store.LoadWarning!=null || store.LoadBlocked,"invalid profiles rejected");
   }
  }
  finally{Directory.Delete(folder,true);}
 }
}
