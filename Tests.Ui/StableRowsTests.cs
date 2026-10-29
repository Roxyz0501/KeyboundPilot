using System.Numerics;
using Dalamud.Bindings.ImGui;
using KeyboundPilot;
unsafe partial class Program
{
 static void CheckStableRows()
 {
  Vector2? anchor=null;
  foreach(var text in new[]{"","abc",new string('W',300),"first\nsecond\nthird"})
  {
   ImGui.NewFrame();ImGui.SetNextWindowPos(new(20,20));ImGui.SetNextWindowSize(new(400,420));
   ImGui.Begin("Stable rows",ImGuiWindowFlags.NoSavedSettings);
   PlaybackControls.StableText("Status: "+text);PlaybackControls.StableText("Mode: "+text);
   PlaybackControls.StableText(text);PlaybackControls.StableText(text);
   var current=ImGui.GetCursorScreenPos();
   if(anchor!=null && current!=anchor.Value)throw new Exception("State changes moved playback controls");
   anchor=current;ImGui.End();ImGui.Render();
  }
  Console.WriteLine("PASS: empty, short, long and multiline states preserve button position");
 }
}
