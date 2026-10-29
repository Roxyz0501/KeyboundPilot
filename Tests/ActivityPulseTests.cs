using KeyboundPilot;
internal static class ActivityPulseTests
{
 public static void Run(Action<bool,string> check)
 {
  var pulse=new ActivityPulse();int windows=0,system=0;
  bool W(){windows++;return true;}bool S(){system++;return true;}
  pulse.Tick(0,W,S);check(windows==1 && system==1 && pulse.WindowAccepted && pulse.SystemAccepted,"both input activity routes attempted immediately");
  for(int i=1;i<3000;i++)pulse.Tick(i,W,S);
  check(windows==1 && system==1,"fast submissions do not flood input notifications");
  pulse.Tick(3000,W,S);check(windows==2 && system==1,"window activity limited to three seconds");
  pulse.Tick(5000,W,S);check(windows==2 && system==2,"OS zero-motion activity limited to five seconds");
  pulse.Tick(6000,()=>false,S);check(!pulse.WindowAccepted && pulse.SystemAccepted,"failed window notification not reported as accepted");
  pulse.Tick(9000,W,S);check(pulse.WindowAccepted,"window notification retries on next scheduled attempt");
  pulse.Tick(10000,W,()=>false);check(!pulse.SystemAccepted,"failed OS notification not masked by successful window route");
  pulse.Tick(100000,W,S);check(windows==4 && system==3,"long stalled frame has no catch-up burst");
 }
}
