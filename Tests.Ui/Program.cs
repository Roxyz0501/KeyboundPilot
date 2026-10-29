using System.Runtime.InteropServices;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ManagedFontAtlas;
using KeyboundPilot;
unsafe partial class Program
{
 static void Main(string[] args)
 {
  NativeLibrary.SetDllImportResolver(typeof(ImGui).Assembly,(name,_,_)=>name.Contains("cimgui")?NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory,"cimgui.dll")):0);
  ImGui.CreateContext();var io=ImGui.GetIO();io.IniFilename=null;io.DisplaySize=new Vector2(1440,900);io.DeltaTime=1f/60;
  var coverage=new GlyphCoverage();coverage.Include([Loc.FontText,GlyphCoverage.TypingCharacters]);
  var synthetic="くえ ぶれいんぽっど ～ 〜 記録場所/霧の谷";
  if(!coverage.Include([synthetic]))throw new Exception("Dynamic additions must extend coverage");
  if(coverage.Include([synthetic]))throw new Exception("Repeated text must not rebuild coverage");
  var text=coverage.Text;var ranges=text.ToGlyphRange();var pin=GCHandle.Alloc(ranges,GCHandleType.Pinned);
  var fonts=new List<ImFontPtr>();
  for(var i=0;i<4;i++) {var raw=new SafeFontConfig{SizePx=17,FontNo=i}.Raw;fonts.Add(io.Fonts.AddFontFromFileTTF(args[0],17,raw,(ushort*)pin.AddrOfPinnedObject()));}
  io.Fonts.Build();pin.Free();
  for(var i=0;i<fonts.Count;i++) {
   var missing=text.Where(c=>!char.IsControl(c)).Distinct().Where(c=>fonts[i].FindGlyphNoFallback(c)==null).ToArray();
   if(missing.Length>0)throw new Exception($"Face {i} missing {new string(missing)}");
   Console.WriteLine($"Face {i}: all {text.Length} requested characters present");
  }
  var textures=new Dictionary<ulong,(nint,int,int)>();
  for(int t=0;t<io.Fonts.Textures.Size;t++) {byte* pixels;int w=0,h=0,bpp=0;io.Fonts.GetTexDataAsRGBA32(t,&pixels,&w,&h,&bpp);io.Fonts.SetTexID(t,new ImTextureID((ulong)t+1));textures[(ulong)t+1]=((nint)pixels,w,h);}
  for(var frame=0;frame<3;frame++)
  {
   ImGui.NewFrame();ImGui.SetNextWindowPos(new Vector2(20,20));ImGui.SetNextWindowSize(new Vector2(1400,860));
   ImGui.Begin("Keybound Pilot - font verification",ImGuiWindowFlags.NoResize|ImGuiWindowFlags.NoSavedSettings);
   foreach(var code in Loc.Codes)
   {
    var face=code switch {"zh-Hant"=>1,"zh-Hans"=>2,"ko"=>3,_=>0};ImGui.PushFont(fonts[face]);ImGui.PushID(code);
    ImGui.TextUnformatted(code+" | "+Loc.Get(code,"AutoStart"));
    PlaybackControls.Draw(false,false,key=>Loc.Get(code,key),()=>{},()=>{},()=>{});ImGui.SameLine();PlaybackControls.DrawLoop(true,Loc.Get(code,"LoopButton"),()=>{});ImGui.SameLine();ImGui.Button(Loc.Get(code,"Support"));
    ImGui.TextUnformatted(string.Format(Loc.Get(code,"LoopProgress"),1,3,0)+" | "+Loc.Get(code,"ShowDebug"));ImGui.TextUnformatted(synthetic);ImGui.Separator();ImGui.PopID();ImGui.PopFont();
   }
   ImGui.End();ImGui.Render();
  }
  Directory.CreateDirectory(args[1]);Render(ImGui.GetDrawData(),textures,Path.Combine(args[1],"fonts.bmp"));
  Console.WriteLine("PASS: 4 font faces, dynamic glyph additions/coalescing, 7-language native ImGui rendering");
  Preview(fonts,textures,args[1]);SettingsPreview(fonts,textures,args[1]);CheckNumberEditing();
  ImGui.DestroyContext();
 }
}

