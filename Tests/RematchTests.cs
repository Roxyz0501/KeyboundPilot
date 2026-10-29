using KeyboundPilot;
internal static class RematchTests
{
 public static void Run(Action<bool,string> check)
 {
  var lobby=new LoopSnapshot(true,true,false,false,false,false,false,false,false,false);
  var waiting=lobby with {Queued=true,WaitingForMatch=true};
  var pop=waiting with {WaitingForMatch=false,EntryReady=true};
  var accepted=pop with {EntryReady=false};
  var loop=new RoundLoop();
  void Begin()
  {
   loop.Start(0,3,false);loop.Tick(0,lobby);loop.Tick(100,waiting);loop.Tick(200,pop);
  }
  Begin();
  check(loop.Tick(400,pop)==LoopAction.None,"same pop is not accepted twice before acknowledgement");
  check(loop.Tick(600,accepted)==LoopAction.None,"accepted pop hidden or disabled does not rearm");
  check(loop.Tick(800,pop)==LoopAction.None,"transient button readiness does not rearm");
  check(loop.Tick(1000,waiting)==LoopAction.None,"withdrawal returns to queue without resubmitting");
  check(loop.Tick(1200,pop)==LoopAction.Accept,"rematch accepted after withdrawal");
  check(loop.Tick(1400,pop)==LoopAction.None,"rematch accepted only once");
  loop.Tick(1600,waiting);
  check(loop.Tick(1800,pop)==LoopAction.Accept,"repeated withdrawal can rematch again");
  check(loop.Current==1 && loop.Completed==0,"withdrawals do not count as rounds");
  var game=lobby with {Lobby=false,InGame=true};
  check(loop.Tick(2000,game)==LoopAction.StartTyping,"rematch starts typing on actual entry");
  check(loop.Tick(2200,game)==LoopAction.None,"typing starts once");
  Begin();loop.Pause();
  check(loop.Tick(400,waiting)==LoopAction.None,"observe withdrawal while paused without action");
  check(loop.Tick(600,pop)==LoopAction.None,"do not accept while paused");
  loop.Resume();check(loop.Tick(800,pop)==LoopAction.Accept,"resume accepts rematch observed during pause");
  Begin();loop.Tick(400,waiting,blocked:true);
  check(loop.Tick(600,pop,blocked:true)==LoopAction.None,"blocked input never accepts");
  check(loop.Tick(800,pop)==LoopAction.Accept,"unblocking accepts rematch");
  Begin();loop.Tick(400,waiting with {Loading=true});
  check(loop.Tick(600,pop)==LoopAction.None,"loading queue transient does not rearm");
  Begin();loop.Tick(400,waiting with {EntryReady=true});
  check(loop.Tick(600,pop)==LoopAction.None,"stale queue state with ready pop does not rearm");
  Begin();loop.Tick(400,lobby);
  check(loop.Tick(600,pop)==LoopAction.None,"missing queue alone does not rearm");
  Begin();loop.Tick(400,waiting with {OtherQueue=true});
  check(loop.Phase==LoopPhase.Error && loop.Tick(600,pop)==LoopAction.None,"unrelated queue stops safely");
  Begin();loop.Stop();loop.Tick(400,waiting);
  check(loop.Tick(600,pop)==LoopAction.None,"manual stop cannot be resumed by rematch");
  Begin();loop.RequestStopAfterCurrent();loop.Tick(400,waiting);
  check(loop.Tick(600,pop)==LoopAction.Accept && loop.StopAfterCurrentRequested,"finish current still enters rematch");
  Begin();loop.Tick(400,waiting);loop.Tick(1800001,waiting);
  check(loop.Phase==LoopPhase.Error,"rematching does not extend the total queue timeout");
 }
}
