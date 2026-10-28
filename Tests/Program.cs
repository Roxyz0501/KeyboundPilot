using KeyboundPilot;
var checks = 0;
void Check(bool value, string what) { checks++; if (!value) throw new Exception(what); }
var samples = new List<(int, int)>();
var sent = new List<char>();
var s = new TypingSession((a,b) => { samples.Add((a,b)); return b-1; });
bool Send(char c) { sent.Add(c); return true; }
s.Start(0);
s.Tick(2999, true, "ABC", false, 80, 150, Send); Check(sent.Count == 0, "3s arming");
s.Tick(3000, true, "ABC", false, 80, 150, Send); Check(sent.Count == 0, "first delay");
s.Tick(3149, true, "ABC", true, 80, 150, Send); Check(sent.Count == 0, "upper boundary");
s.Tick(3150, true, "ABC", true, 80, 150, Send); Check(sent.SequenceEqual("A"), "first character");
s.Tick(9000, true, "ABC", true, 80, 150, Send); Check(sent.SequenceEqual("AB"), "no catch-up burst");
s.Tick(9001, true, "ABC", true, 80, 150, Send); Check(sent.Count == 2, "next deadline from actual send");
s.Tick(9150, true, "ABC", true, 80, 150, Send); Check(sent.SequenceEqual("ABC"), "character order");
s.Tick(10000, true, "ABC", true, 80, 150, Send); Check(sent.Count == 3, "do not retype stale word");
s.Tick(13001, true, "ABC", true, 80, 150, Send); Check(!s.Running && s.Status == "NoProgress", "stale word timeout");
Check(samples.All(x => x == (80,151)), "inclusive bounds");
s.Start(0); s.Tick(3000,false,"ABC",false,80,150,Send); Check(!s.Running,"focus gate");
s.Start(0); s.Tick(3000,true,"あ",false,80,150,Send); Check(!s.Running && s.Status=="Unsupported","reject unsupported");
s.Start(0); s.Tick(3000,true,"AB",false,80,150,Send); s.Tick(3150,true,"AB",true,80,150,_=>false); Check(!s.Running,"failed send never retries");
s.Start(0); s.Tick(3000,true,"AB",false,80,150,Send); var before=sent.Count;
s.Tick(3150,true,"CD",false,80,150,Send); Check(sent.Count==before && s.CurrentWord=="","target disappears before send");
var fixedSession = new TypingSession((a,b) => { Check(a==100 && b==101,"fixed random bounds"); return a; });
fixedSession.Start(0); fixedSession.Tick(3000,true,"AB",false,100,100,Send);
var fixedBefore=sent.Count;fixedSession.Tick(3099,true,"AB",true,100,100,Send);Check(sent.Count==fixedBefore,"fixed not early");
fixedSession.Tick(3100,true,"AB",true,100,100,Send);Check(sent.Count==fixedBefore+1,"fixed exactly due");
fixedSession.Stop();fixedSession.Tick(9999,true,"AB",true,100,100,Send);Check(sent.Count==fixedBefore+1,"stop cancels pending");
var c=new Configuration(); c.SetRange(99999,-1); Check(c.MinimumMs==5000 && c.MaximumMs==5000,"clamp invalid range");
c.SetRange(-50,100); Check(c.MinimumMs==20 && c.MaximumMs==100,"minimum clamp");
Check(Keyboard.InputSize==40,"Win32 x64 INPUT ABI");
var root=Path.Combine(AppContext.BaseDirectory,"KeyboundPilot-tests-"+Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try {
 var time=new TestClock(); var path=Path.Combine(root,"config.json");
 using(var store=new ConfigurationStore(path,clock:time)) {
  c=store.Configuration; c.Language="ja"; store.Update(c); store.Tick(); Check(store.SaveCount==0,"no immediate UI write");
  time.Advance(60);store.Tick(); Check(store.SaveCount==1,"60s save: "+store.LastError);
  var saved=c.MinimumMs;c.MinimumMs=90;store.Update(c);c.MinimumMs=saved;store.Update(c);time.Advance(60);store.Tick();Check(store.SaveCount==1,"reverted value no write");
  for(var i=0;i<6;i++){c.MinimumMs=81+i;store.Update(c);time.Advance(60);store.Tick();}
  Check(Directory.GetFiles(store.BackupDirectory).Length==3,"three backup generations");
  c.MaximumMs=160;store.Update(c);store.Flush();var saves=store.SaveCount;store.Flush();Check(store.SaveCount==saves,"coalesce lifecycle flush");
  Console.WriteLine($"Persistence fixture: commits={store.SaveCount}, payloadBytes={store.LogicalBytesWritten}, backupBytes={store.BackupBytesCreated}");
 }
 File.WriteAllText(path,"broken");
 using(var recovered=new ConfigurationStore(path))Check(recovered.Recovered && recovered.Configuration.Language=="ja","recover valid backup");
 var failures=0;using(var fail=new ConfigurationStore(Path.Combine(root,"fail.json"),clock:time,writePayload:(_,_)=>{failures++;throw new IOException("fixture");})){
  fail.Update(fail.Configuration);for(var i=0;i<8;i++){time.Advance(60);fail.Tick();}Check(fail.Stopped && failures==3,"bounded retry");
 }
} finally { Directory.Delete(root,true); }
Console.WriteLine($"PASS {checks} checks; no real keyboard events sent.");
namespace KeyboundPilot { internal static class Loc { public static bool IsSupported(string? s)=>s is "ja" or "en" or "de" or "fr" or "ko" or "zh-Hans" or "zh-Hant"; } }
sealed class TestClock:TimeProvider {long ticks;public override long TimestampFrequency=>1000;public override long GetTimestamp()=>ticks;public void Advance(int seconds)=>ticks+=seconds*1000;}


