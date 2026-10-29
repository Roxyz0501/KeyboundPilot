using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KeyboundPilot;
unsafe partial class Program
{
 static void CheckKtgActivity()
 {
  AgentInterface hud=default;hud.UIModuleInterface=(UIModuleInterface*)1;
  KtgActivity.InputAgent value=default;value.Agent.UIModuleInterface=hud.UIModuleInterface;value.Hud=&hud;
  value.IdleSeconds=59.9f;value.SameKeySeconds=59.8f;
  var memory=new Span<byte>(&value,sizeof(KtgActivity.InputAgent));var before=memory.ToArray();
  if(sizeof(KtgActivity.InputAgent)!=0x60 || Marshal.OffsetOf<KtgActivity.InputAgent>(nameof(KtgActivity.InputAgent.IdleSeconds)).ToInt32()!=0x50)throw new Exception("Input agent layout changed");
  if(!KtgActivity.Reset(&value,&hud) || value.IdleSeconds!=0 || value.SameKeySeconds!=0)throw new Exception("Both dedicated timers must reset");
  for(int i=0;i<memory.Length;i++)if((i<0x50 || i>=0x58) && memory[i]!=before[i])throw new Exception("Dedicated reset modified unrelated agent fields");
  if(KtgActivity.Reset(null,&hud) || KtgActivity.Reset(&value,null))throw new Exception("Missing input/HUD must fail closed");
  value.Hud=null;value.IdleSeconds=31;value.SameKeySeconds=32;
  if(KtgActivity.Reset(&value,&hud) || value.IdleSeconds!=31 || value.SameKeySeconds!=32)throw new Exception("Mismatched HUD must not write");
  value.Hud=&hud;value.Agent.UIModuleInterface=(UIModuleInterface*)2;
  if(KtgActivity.Reset(&value,&hud) || value.IdleSeconds!=31)throw new Exception("Different UI owners must not write");
  value.Agent.UIModuleInterface=hud.UIModuleInterface;
  foreach(var invalid in new[]{float.NaN,float.PositiveInfinity,-1f})
  {
   value.IdleSeconds=invalid;if(KtgActivity.Reset(&value,&hud))throw new Exception("Invalid idle timer accepted");
   value.IdleSeconds=31;value.SameKeySeconds=invalid;if(KtgActivity.Reset(&value,&hud) || value.IdleSeconds!=31)throw new Exception("Invalid key timer accepted");
  }
  // Replay the observed independent counter and repeated-key restore for three minutes.
  // General timer resets alone cannot affect these counters; reproduce the old failure.
  value.IdleSeconds=value.SameKeySeconds=0;bool oldWithdraw=false;
  for(int i=0;i<1800;i++){value.IdleSeconds+=0.1f;value.SameKeySeconds+=0.1f;if(i%10==0)value.IdleSeconds=value.SameKeySeconds;if(value.IdleSeconds>=60){oldWithdraw=true;break;}}
  if(!oldWithdraw)throw new Exception("Fixture must reproduce independent timeout");
  value.IdleSeconds=value.SameKeySeconds=0;
  var input=&value;var owner=&hud;var keeper=new ActivityKeeper();keeper.Begin();var touches=0;
  bool Touch(){touches++;return KtgActivity.Reset(input,owner);}
  for(int i=0;i<1800;i++)
  {
   keeper.Tick(i*100,keeper.Requested,Touch);
   if(i%173==0)keeper.OnSubmission(i*100,Touch);
   value.IdleSeconds+=0.1f;value.SameKeySeconds+=0.1f;
   if(i%10==0)value.IdleSeconds=value.SameKeySeconds;
   if(value.IdleSeconds>=60)throw new Exception("Dedicated reset must survive absent submissions and repeated identical keys");
  }
  keeper.End();var count=touches;keeper.Tick(200000,keeper.Requested,Touch);keeper.OnSubmission(200000,Touch);
  if(touches!=count || keeper.Failed)throw new Exception("Stopped activity must not touch dedicated timers");
  Console.WriteLine("PASS: dedicated 60-second timeout replay, both counters reset, stopped session, owner/layout/null/invalid-value guards, unrelated bytes preserved; no game calls");
 }
}
