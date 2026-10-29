using KeyboundPilot;
internal static class ExitDelayTests
{
 public static void Run(Action<bool,string> check)
 {
  var game=new LoopSnapshot(true,false,true,false,false,false,false,false,false,false);
  var result=game with {Result=true,LeaveReady=true};var loop=new RoundLoop();
  loop.Start(0,2,true);
  check(loop.Tick(100,game,exitDelayMs:3000)==LoopAction.None,"gameplay never starts exit timer");
  check(loop.Tick(10000,result with {LeaveReady=false},exitDelayMs:3000)==LoopAction.None,"disabled exit button does not start wait");
  check(loop.Tick(20000,result,exitDelayMs:3000)==LoopAction.None,"wait starts on actual exit readiness");
  check(loop.Tick(22999,result,exitDelayMs:3000)==LoopAction.None,"no early exit");
  check(loop.Tick(23000,result,exitDelayMs:3000)==LoopAction.Leave,"exit at readiness plus selected wait");
  check(loop.Tick(24000,result,exitDelayMs:3000)==LoopAction.None,"exit dispatched once");
  check(loop.Tick(24001,result with {ExitConfirm=true},exitDelayMs:3000)==LoopAction.ConfirmExit,"confirmation is not delayed again");
  loop.Start(0,2,true);loop.Tick(100,result,exitDelayMs:3000,scoresReady:false);
  check(loop.Tick(3100,result,exitDelayMs:3000,scoresReady:false)==LoopAction.None,"score wait may outlast configured wait");
  check(loop.Tick(4000,result,exitDelayMs:3000,scoresReady:true)==LoopAction.Leave,"score and configured wait overlap rather than add");
  loop.Start(0,2,true);loop.Tick(100,result,exitDelayMs:5000,scoresReady:false);
  check(loop.Tick(3000,result,exitDelayMs:5000,scoresReady:true)==LoopAction.None,"score completion does not bypass longer exit wait");
  check(loop.Tick(5100,result,exitDelayMs:5000)==LoopAction.Leave,"longer exit wait completes normally");
  loop.Start(0,2,true);check(loop.Tick(1,result,exitDelayMs:0)==LoopAction.Leave,"zero preserves immediate exit when scores ready");
  loop.Start(0,2,true);loop.Tick(100,result,exitDelayMs:2000);loop.Pause();
  check(loop.Tick(5000,result,exitDelayMs:2000)==LoopAction.None,"never exit while paused");loop.Resume();
  check(loop.Tick(5001,result,exitDelayMs:2000)==LoopAction.Leave,"resume honors elapsed time since ready");
  loop.Start(0,2,true);loop.Tick(100,result,exitDelayMs:2000);loop.Tick(500,result with {LeaveReady=false},exitDelayMs:2000);
  loop.Tick(1000,result,exitDelayMs:2000);check(loop.Tick(2999,result,exitDelayMs:2000)==LoopAction.None,"readiness loss restarts wait");
  check(loop.Tick(3000,result,exitDelayMs:2000)==LoopAction.Leave,"readiness recovery gets full wait");
  loop.Start(0,2,true);loop.Tick(100,result,exitDelayMs:60000);loop.ToggleStopAfterCurrent();
  check(loop.Tick(60099,result,exitDelayMs:60000)==LoopAction.None,"finish-current request preserves maximum delay");
  check(loop.Tick(60100,result,exitDelayMs:60000)==LoopAction.Leave,"maximum wait exits before existing timeout");
  loop.Start(0,2,true);loop.Tick(100,result,exitDelayMs:5000);check(loop.Tick(1100,result,exitDelayMs:1000)==LoopAction.Leave,"edited delay applies immediately to original readiness time");
  loop.Start(0,2,true);loop.Tick(100,result,exitDelayMs:2000);loop.Stop();check(loop.Tick(10000,result,exitDelayMs:2000)==LoopAction.None,"manual stop cancels pending exit");
  var root=Path.Combine(AppContext.BaseDirectory,"exit-delay-"+Guid.NewGuid());Directory.CreateDirectory(root);
  try
  {
   var path=Path.Combine(root,"config.json");File.WriteAllText(path,"{\"Version\":1,\"Language\":\"en\"}");var clock=new TestClock();
   using(var store=new ConfigurationStore(path,clock:clock))
   {
    check(store.Configuration.ExitDelayMs==0,"old configuration defaults to no additional exit delay");
    store.Configuration.ExitDelayMs=12300;store.Update(store.Configuration);clock.Advance(59);store.Tick();check(store.SaveCount==0,"exit setting uses delayed autosave");
    clock.Advance(1);store.Tick();check(store.SaveCount==1,"exit delay included in snapshot validation and save");
    clock.Advance(60);store.Tick();check(store.SaveCount==1,"unchanged delay does not write again");
   }
   using(var store=new ConfigurationStore(path))check(store.Configuration.ExitDelayMs==12300,"exit delay restored on reload");
  }
  finally {Directory.Delete(root,true);}
 }
}
