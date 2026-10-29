using System.Numerics;
using Dalamud.Bindings.ImGui;
using KeyboundPilot;

unsafe partial class Program
{
 static void Preview(List<ImFontPtr> fonts,Dictionary<ulong,(nint,int,int)> textures,string path)
 {
  for(var frame=0;frame<3;frame++)
  {
   ImGui.NewFrame();ImGui.PushFont(fonts[0]);
   for(var panel=0;panel<2;panel++)
   {
    string T(string key)=>Loc.Get("ja",key);
    ImGui.SetNextWindowPos(new Vector2(20+panel*700,20));ImGui.SetNextWindowSize(new Vector2(680,510));
    ImGui.Begin("Keybound Pilot###Preview"+panel,ImGuiWindowFlags.NoResize|ImGuiWindowFlags.NoSavedSettings);
    if(ImGui.BeginTabBar("Tabs"))
    {
     if(ImGui.BeginTabItem(T("Main")))
     {
      PlaybackControls.StableText(T("Status")+": "+T(panel==0?"Waiting":"UserPaused"));PlaybackControls.StableText(T("InputMode")+": "+T(panel==0?"NormalProfile":"LowHpProfile"));PlaybackControls.StableText(panel==0?T("Current")+": じゅんぱくのていおうヴぁいす":"");PlaybackControls.StableText("");
      PlaybackControls.Draw(true,false,T,()=>{},()=>{},()=>{});ImGui.SameLine();PlaybackControls.Help(T("Guide")+"\n\n"+T("LoopHelp")+"\n\n"+T("ReceptionHelp"),30);
      ImGui.Spacing();ImGui.Separator();ImGui.Spacing();
      PlaybackControls.StableText(T("LoopStatus")+": "+string.Format(T("LoopProgress"),2,5,1)+" / "+T(panel==0?"LoopLeaving":"UserPaused")+" / "+(panel==0?string.Format(T("ExitCountdown"),12):T("ExitScoreWait")));
      PlaybackControls.StableText(panel==1?T("StopAfterCurrentPending"):"");var count=5;ImGui.SetNextItemWidth(400);PlaybackControls.Number("###LoopCount",ref count,1,9999,1,T("NumberHelp"));
      PlaybackControls.DrawLoop(false,T("LoopButton"),()=>{});PlaybackControls.DrawStopAfterCurrent(true,T,()=>{},panel==1);
      
      if(panel==1)
      {
       ImGui.Spacing();ImGui.Separator();ImGui.Spacing();ImGui.TextUnformatted(T("Words")+": 3");ImGui.TextUnformatted(T("DetectedList"));
       if(ImGui.BeginChild("DetectedWords",new Vector2(0,120),true))ImGui.TextUnformatted("じゅんぱくのていおうヴぁいす\nくえ\nぶれいんぽっど");ImGui.EndChild();
       ImGui.TextUnformatted(T("InputVisible")+": "+T("Yes"));ImGui.TextUnformatted(T("InputFocused")+": "+T("Yes"));ImGui.TextUnformatted(T("Sent")+": 268");
      }
      ImGui.EndTabItem();
     }
     foreach(var key in new[]{"ScoreHistory","Settings","Language","Support"})if(ImGui.BeginTabItem(T(key)))ImGui.EndTabItem();
     ImGui.EndTabBar();
    }
    ImGui.End();
   }
   ImGui.PopFont();ImGui.Render();
  }
  Render(ImGui.GetDrawData(),textures,Path.Combine(path,"layout.bmp"));
 }
 static void ScorePreview(List<ImFontPtr> fonts,Dictionary<ulong,(nint,int,int)> textures,string path)
 {
  var folder=Path.Combine(path,"history-fixture-"+Guid.NewGuid().ToString("N"));
  using var history=new ScoreHistory(folder);
  history.Add(new(Guid.NewGuid(),new DateTimeOffset(2026,10,29,18,30,0,TimeSpan.FromHours(9)),"Sample Player",12345,
   [new(1,"Sample Ally",16000,false),new(4,"Sample Player",12345,true)]));history.Flush();
  using var panel=new ScorePanel(history);
  for(var frame=0;frame<6;frame++)
  {
   ImGui.NewFrame();ImGui.PushFont(fonts[0]);ImGui.SetNextWindowPos(new Vector2(20,20));ImGui.SetNextWindowSize(new Vector2(680,510));
   ImGui.Begin("Keybound Pilot - スコア履歴",ImGuiWindowFlags.NoResize|ImGuiWindowFlags.NoSavedSettings);
   panel.Draw(key=>Loc.Get("ja",key),false,()=>{});ImGui.End();ImGui.PopFont();ImGui.Render();
   Thread.Sleep(30);
  }
  Render(ImGui.GetDrawData(),textures,Path.Combine(path,"scores.bmp"));
 }
 static void SettingsPreview(List<ImFontPtr> fonts,Dictionary<ulong,(nint,int,int)> textures,string path)
 {
  var configs=Enumerable.Range(0,2).Select(_=>new Configuration{DirectInput=true,AutoStart=true,ShowDebug=false,MinimumMs=70,MaximumMs=125,BetweenWordsMinimumMs=50,BetweenWordsMaximumMs=150,MistypeEnabled=true,MistypeMinimum=22,MistypeMaximum=32}).ToArray();
  void Frame()
  {
   ImGui.NewFrame();ImGui.PushFont(fonts[0]);
   for(var panel=0;panel<2;panel++)
   {
    var language=panel==0?"ja":"en";
    ImGui.SetNextWindowPos(new Vector2(20+panel*700,20));ImGui.SetNextWindowSize(new Vector2(680,840));
    ImGui.Begin("Keybound Pilot - "+Loc.Get(language,"Settings")+"###SettingsPreview"+panel,ImGuiWindowFlags.NoResize|ImGuiWindowFlags.NoSavedSettings);
    SettingsPanel.Draw(configs[panel],key=>Loc.Get(language,key),()=>{},()=>{});
    ImGui.End();
   }
   ImGui.PopFont();ImGui.Render();
  }
  void Click(float x,float y)
  {var io=ImGui.GetIO();io.AddMousePosEvent(x,y);Frame();io.AddMouseButtonEvent(0,true);Frame();io.AddMouseButtonEvent(0,false);Frame();Frame();}
  void Capture(string name){ImGui.GetIO().AddMousePosEvent(-1000,-1000);Frame();Render(ImGui.GetDrawData(),textures,Path.Combine(path,name+".bmp"));}
  Frame();Frame();Frame();Capture("settings");
  Click(110,341);Click(830,341);Capture("obstruction-disabled");
  foreach(var config in configs){config.ObstructionIntervalEnabled=true;config.ObstructionSettings=new(150,250,100,300,true,15,25);}
  Frame();Capture("obstruction-enabled");
  Click(180,341);Click(918,341);Capture("hp-disabled");
  foreach(var config in configs){config.LowHpEnabled=true;config.LowHpSettings=new(40,70,50,100,false,15,25);}
  Frame();Capture("hp-enabled");
 }
 static void CheckNumberEditing()
 {
  var io=ImGui.GetIO();var number=5;Vector2 min=default,max=default;
  void Frame()
  {
   ImGui.NewFrame();ImGui.SetNextWindowPos(new Vector2(20,20));ImGui.SetNextWindowSize(new Vector2(400,200));
   ImGui.Begin("Numeric interaction",ImGuiWindowFlags.NoSavedSettings);ImGui.SetNextItemWidth(240);
   PlaybackControls.Number("###Count",ref number,1,9999,1,"");min=ImGui.GetItemRectMin();max=ImGui.GetItemRectMax();
   ImGui.End();ImGui.Render();
  }
  Frame();Frame();var point=(min+max)/2;
  io.AddMousePosEvent(point.X,point.Y);Frame();
  io.AddMouseButtonEvent(0,true);Frame();io.AddMouseButtonEvent(0,false);Frame();
  io.AddMouseButtonEvent(0,true);Frame();io.AddMouseButtonEvent(0,false);Frame();Frame();
  if(!io.WantTextInput)throw new Exception("Double-click must enter numeric text editing");
  io.AddInputCharacter('1');io.AddInputCharacter('2');Frame();io.AddKeyEvent(ImGuiKey.Enter,true);Frame();io.AddKeyEvent(ImGuiKey.Enter,false);Frame();
  if(number!=12)throw new Exception("Double-click numeric editing must commit 12: "+number);
  for(var i=0;i<30;i++)Frame();
  io.AddMouseButtonEvent(0,true);Frame();io.AddMousePosEvent(point.X+40,point.Y);Frame();io.AddMouseButtonEvent(0,false);Frame();
  if(number<=12)throw new Exception("Dragging must increase numeric value");
  Console.WriteLine("PASS: double-click direct number entry, Enter commit, horizontal drag");
 }
}

