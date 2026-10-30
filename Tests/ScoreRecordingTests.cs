using KeyboundPilot;
internal static class ScoreRecordingTests
{
 public static void Run(Action<bool,string> check)
 {
  var records=new List<ScoreRecord>();var rows=new[]{new ScoreRow(1,"Me",765,true)};
  var session=new ScoreSession();var id=Guid.NewGuid();
  check(new Configuration().RecordScores && session.RecordingEnabled,"score recording defaults on");
  session.Begin("Me",id);session.Observe(0,null,true,rows,records.Add);
  session.SetRecording(false);check(session.Result.Length==0 && !session.ResultSeen && session.ReadyToLeave(100),"disable discards pending result and removes score wait");
  session.Observe(5000,null,true,rows,records.Add);check(records.Count==0 && !session.ResultSeen,"disabled session cannot observe or commit");
  session.End(records.Add);check(records.Count==0 && !session.Active,"disabled exit/logout cannot commit pending result");
  session.Begin("Me",id);session.SetRecording(true);session.Observe(6000,null,true,rows,records.Add);
  session.Observe(8000,null,true,rows,records.Add);check(records.Count==1 && records[0].Id==id,"reenabling records current game with preserved identity");
  session.SetRecording(false);session.SetRecording(true);session.Observe(9000,null,true,rows,records.Add);session.Observe(12000,null,true,rows,records.Add);session.End(records.Add);
  check(records.Count==1,"off-on on completed results never duplicates");
  session.Begin("Me");session.Observe(13000,null,true,rows,records.Add);session.SetRecording(false);session.SetRecording(true);
  session.Observe(18000,null,false,[],records.Add);session.End(records.Add);
  check(records.Count==1,"uncommitted observations do not leak across disabled interval");
  session.Begin("Me");session.SetRecording(false);session.Observe(19000,null,true,rows,records.Add);session.SetRecording(true);
  session.Observe(20000,null,true,rows,records.Add);session.Observe(22000,null,true,rows,records.Add);
  check(records.Count==2 && records[1].Id!=records[0].Id,"next game can record after reenable");
  var saved=records[0];session.Begin("Me",saved.Id,saved);session.SetRecording(false);session.SetRecording(true);
  session.Observe(30000,null,true,rows,records.Add);session.End(records.Add);check(records.Count==2,"restored recorded identity also survives toggle");
  var loop=new RoundLoop();loop.Start(0,1,true);
  var result=new LoopSnapshot(true,false,true,false,false,false,false,true,true,false);
  session.SetRecording(false);
  check(loop.Tick(0,result,exitDelayMs:3000,scoresReady:session.ReadyToLeave(0))==LoopAction.None,"disabled score recording still respects configured exit delay");
  check(loop.Tick(3000,result,exitDelayMs:3000,scoresReady:session.ReadyToLeave(3000))==LoopAction.Leave,"disabled score recording adds no extra exit wait");
  var folder=Path.Combine(AppContext.BaseDirectory,"score-toggle-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
  try
  {
   var path=Path.Combine(folder,"config.json");File.WriteAllText(path,"{\"Version\":1}");var clock=new TestClock();
   using(var store=new ConfigurationStore(path,clock:clock))
   {
    check(store.Configuration.RecordScores,"legacy settings retain recording");
    store.Configuration.RecordScores=false;store.Update(store.Configuration);clock.Advance(59);store.Tick();
    check(store.SaveCount==0,"record switch does not bypass delayed autosave");clock.Advance(1);store.Tick();
    check(store.SaveCount==1 && store.LastError==null,"record switch captured in saved snapshot");
    clock.Advance(60);store.Tick();check(store.SaveCount==1,"unchanged score switch does not rewrite");
   }
   using(var store=new ConfigurationStore(path))check(!store.Configuration.RecordScores,"off choice persists after restart");
   using(var history=new ScoreHistory(Path.Combine(folder,"history")))
   {
    history.Add(saved);history.Flush();session.SetRecording(false);session.Begin("Me");session.Observe(0,null,true,rows,history.Add);session.End(history.Add);history.Flush();
    check(history.ReadPage(saved.Ended.ToString("yyyy-MM-dd"),0).Records.Length==1,"disabling retains saved history and adds no new score");
   }
  }
  finally{Directory.Delete(folder,true);}
 }
}
