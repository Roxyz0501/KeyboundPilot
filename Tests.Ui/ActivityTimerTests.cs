using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using KeyboundPilot;
unsafe partial class Program
{
 static void CheckActivityTimers()
 {
  InputTimerModule value=default;
  value.ContentInputTimer=120;value.InputTimer=80;value.AfkTimer=-1;
  value.TerritoryTypeId=1359;value.Status=3;value.InstanceContentAfkTimeLimit=600;
  foreach(ref var device in value.ControllerInputTimers){device.Timer=90;device.HistoryIndex=2;device.TotalHistoryTime=7;}
  foreach(ref var device in value.MouseInputTimers){device.Timer=85;device.HistoryIndex=3;device.TotalHistoryTime=8;}
  if(!ActivityTimers.Reset(&value) || ActivityTimers.Maximum(value.ControllerInputTimers)!=0 || ActivityTimers.Maximum(value.MouseInputTimers)!=0 || value.AfkTimer!=-1)throw new Exception("Device activity reset failed");
  if(value.TerritoryTypeId!=1359 || value.Status!=3 || value.InstanceContentAfkTimeLimit!=600 || value.ControllerInputTimers[0].HistoryIndex!=2 || value.MouseInputTimers[0].TotalHistoryTime!=8)throw new Exception("Activity reset modified unrelated fields");
  value.AfkTimer=50;if(!ActivityTimers.Reset(&value) || value.AfkTimer!=0 || ActivityTimers.Reset(null))throw new Exception("AFK timer/null handling failed");
  Console.WriteLine("PASS: native timer fixture resets all 16 controller / 5 mouse timers; preserves limits, status, history and negative AFK state; no game or OS calls");
 }
}
