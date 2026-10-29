using KeyboundPilot;
internal static class FinishRoundTests
{
 public static void Run(Action<bool,string> check)
 {
  var lobby=new LoopSnapshot(true,true,false,false,false,false,false,false,false,false);
  var game=lobby with {Lobby=false,InGame=true};var result=game with {Result=true,LeaveReady=true};
  var loop=new RoundLoop();loop.RequestStopAfterCurrent();check(!loop.StopAfterCurrentRequested,"idle request ignored");
  loop.Start(0,5,true);loop.RequestStopAfterCurrent();loop.RequestStopAfterCurrent();
  check(loop.Active && loop.Total==5 && loop.Completed==0 && loop.StopAfterCurrentRequested,"finish request preserves playback and configured count");
  check(loop.Tick(100,game)==LoopAction.None && loop.Active,"request does not interrupt current game");
  check(loop.Tick(200,result)==LoopAction.Leave && loop.Completed==1,"finish current run still closes result");
  check(loop.Tick(300,result with {ExitConfirm=true})==LoopAction.ConfirmExit,"finish request preserves exit confirmation");
  loop.Tick(400,lobby);check(!loop.Active && loop.Phase==LoopPhase.Complete && loop.Completed==1 && loop.Current==1,"stop on return after requested run");
  check(loop.Tick(500,lobby)==LoopAction.None,"never queue another run after requested stop");
  loop.Start(0,5,false);check(!loop.StopAfterCurrentRequested,"new loop clears stop request");
  loop.RequestStopAfterCurrent();check(loop.Tick(1,lobby)==LoopAction.None && !loop.Active && loop.Completed==0,"stop before queue sends no request");
  loop.Start(0,5,false);loop.Tick(1,lobby);loop.RequestStopAfterCurrent();
  check(loop.Tick(2,lobby with {Queued=true,EntryReady=true})==LoopAction.Accept,"already requested run can enter after finish request");
  check(loop.Tick(3,game)==LoopAction.StartTyping,"already requested run still starts typing");
  loop.Pause();check(loop.Tick(4,result)==LoopAction.None && loop.StopAfterCurrentRequested,"pause retains finish request without leaving");
  loop.Resume();loop.Tick(5,result);loop.Tick(6,lobby with {Loading=true});check(loop.Active,"finish request waits for area loading");
  loop.Tick(7,lobby);check(loop.Completed==1 && !loop.Active,"queued run finishes exactly once");
  loop.Start(0,5,true);loop.Tick(1,result);loop.RequestStopAfterCurrent();loop.Tick(2,lobby);
  check(!loop.Active && loop.Completed==1,"request during result/leave prevents next queue");
  loop.Start(0,5,true);loop.RequestStopAfterCurrent();loop.Stop();check(!loop.StopAfterCurrentRequested && !loop.Active,"manual stop clears request");
 }
}
