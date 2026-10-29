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
  var draws=new List<(int,int)>();var randomLoop=new RoundLoop((min,max)=>{draws.Add((min,max));return max-1;});
  randomLoop.Start(0,3,true);randomLoop.Tick(0,game,exitDelayMs:1000,exitMaximumMs:3000);
  check(draws.Count==0,"no random draw before readiness");
  randomLoop.Tick(100,result,exitDelayMs:1000,scoresReady:false,exitMaximumMs:3000);
  check(draws.SequenceEqual(new[]{(1000,3001)}),"draw includes both requested endpoints");
  for(var n=200;n<3100;n+=100)randomLoop.Tick(n,result,exitDelayMs:1000,exitMaximumMs:3000);
  check(draws.Count==1,"polling and score wait do not redraw deadline");
  check(randomLoop.Tick(3100,result,exitDelayMs:1000,exitMaximumMs:3000)==LoopAction.Leave,"drawn maximum governs actual exit");
  randomLoop.Tick(3200,result,exitDelayMs:2000,exitMaximumMs:4000);check(draws.Count==1,"no redraw after dispatch");
  randomLoop.Start(4000,3,true);randomLoop.Tick(4000,result,exitDelayMs:1000,exitMaximumMs:3000);check(draws.Count==2,"new run draws independently");
  randomLoop.Pause();randomLoop.Tick(4500,result,exitDelayMs:1000,exitMaximumMs:3000);randomLoop.Resume();randomLoop.Tick(4600,result,exitDelayMs:1000,exitMaximumMs:3000);
  check(draws.Count==2,"pause and resume preserve sampled wait");
  randomLoop.Tick(4700,result,exitDelayMs:1000,exitMaximumMs:60000);check(draws.Count==3 && draws[^1]==(1000,60001),"range edit redraws once and supports maximum endpoint");
  randomLoop.Tick(4800,result,exitDelayMs:1000,exitMaximumMs:60000);check(draws.Count==3,"unchanged edited range is stable");
  randomLoop.Tick(4900,result with {LeaveReady=false},exitDelayMs:1000,exitMaximumMs:60000);randomLoop.Tick(5000,result,exitDelayMs:1000,exitMaximumMs:60000);
  check(draws.Count==4,"readiness reacquisition draws a new wait");
  randomLoop.Start(0,1,true);randomLoop.Tick(0,result,exitDelayMs:2000,exitMaximumMs:2000);check(draws.Count==4,"equal endpoints use fixed wait without RNG");
  check(randomLoop.Tick(2000,result,exitDelayMs:2000,exitMaximumMs:2000)==LoopAction.Leave,"equal endpoints exit at exact fixed time");
  var minimumLoop=new RoundLoop((min,max)=>min);minimumLoop.Start(0,1,true);check(minimumLoop.Tick(0,result,exitDelayMs:0,exitMaximumMs:60000)==LoopAction.Leave,"sampled zero minimum may exit immediately");
  var migrated=System.Text.Json.JsonSerializer.Deserialize<Configuration>("{\"ExitDelayMs\":12300}")!;
  check(migrated.ExitMin==12300 && migrated.ExitMax==12300,"fixed setting migrates to equal endpoints");
  migrated.SetExitMaximum(5000);check(migrated.ExitMin==12300 && migrated.ExitMax==12300,"upper endpoint cannot lower existing minimum");
  migrated.SetExitMinimum(5000);migrated.SetExitMaximum(60000);check(migrated.ExitMin==5000 && migrated.ExitMax==60000,"upper endpoint changes without moving minimum");
  migrated.SetExitMinimum(70000);check(migrated.ExitMin==60000 && migrated.ExitMax==60000,"lower endpoint cannot move upper");
  migrated.SetExitMinimum(-5);check(migrated.ExitMin==0 && migrated.ExitMax==60000,"lower endpoint clamps independently");
  var countdownDraws=0;var countdown=new RoundLoop((min,max)=>{countdownDraws++;return 2500;});
  check(countdown.ExitWaitSeconds(0)==null,"idle has no exit countdown");
  countdown.Start(0,3,true);check(countdown.ExitWaitSeconds(0)==null,"playing without readiness has no countdown");
  countdown.Tick(100,result,exitDelayMs:1000,exitMaximumMs:3000);
  check(countdown.ExitWaitSeconds(100)==3 && countdown.ExitWaitSeconds(600)==2,"countdown rounds remaining milliseconds up to seconds");
  check(countdown.ExitWaitSeconds(2599)==1 && countdown.ExitWaitSeconds(2600)==0,"countdown exact expiry boundary");
  for(var n=0;n<100;n++)countdown.ExitWaitSeconds(200);
  check(countdownDraws==1,"drawing countdown never redraws random wait");
  countdown.Pause();countdown.Tick(3000,result,exitDelayMs:1000,exitMaximumMs:3000);
  check(countdown.ExitWaitSeconds(3000)==0 && countdown.Paused,"paused countdown follows existing elapsed-time semantics without exit");
  countdown.Resume();countdown.Tick(3001,result,exitDelayMs:1000,scoresReady:false,exitMaximumMs:3000);
  check(countdown.ExitWaitSeconds(3001)==0 && countdown.WaitingForScores,"expired configured wait identifies remaining score wait");
  countdown.Tick(3100,result,exitDelayMs:1000,scoresReady:true,exitMaximumMs:3000);
  check(countdown.ExitWaitSeconds(3100)==null,"countdown disappears after exit dispatch");
  countdown.Start(0,3,true);countdown.Tick(100,result,exitDelayMs:5000);countdown.Tick(200,result with {LeaveReady=false},exitDelayMs:5000);
  check(countdown.ExitWaitSeconds(200)==null,"readiness loss hides countdown");
  countdown.Tick(300,result,exitDelayMs:5000);check(countdown.ExitWaitSeconds(300)==5,"new readiness updates countdown origin");
  countdown.Tick(400,result,exitDelayMs:8000);check(countdown.ExitWaitSeconds(400)==8,"edited range is reflected in remaining time");
  countdown.Stop();check(countdown.ExitWaitSeconds(400)==null,"manual stop clears countdown");
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
   using(var store=new ConfigurationStore(path))
   {
    check(store.Configuration.ExitDelayMs==12300 && store.Configuration.ExitMin==12300 && store.Configuration.ExitMax==12300,"exit delay restored on reload");
    store.Configuration.SetExitMinimum(7000);store.Configuration.SetExitMaximum(18000);store.Update(store.Configuration);
   }
   using(var store=new ConfigurationStore(path))check(store.Configuration.ExitMin==7000 && store.Configuration.ExitMax==18000,"both exit endpoints survive lifecycle flush and reload");
  }
  finally {Directory.Delete(root,true);}
 }
}
