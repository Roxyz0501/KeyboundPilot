using KeyboundPilot;
var checks = 0;
void Check(bool value, string what) { checks++; if (!value) throw new Exception(what); }
var samples = new List<(int, int)>();
var sent = new List<char>();
var s = new TypingSession((a,b) => { samples.Add((a,b)); return b-1; });
bool Send(char c) { sent.Add(c); return true; }
s.Start(0);
Check(s.Running && s.Status=="Waiting","start has no arming delay");
s.Tick(0, true, "ABC", false, 80, 150, Send); Check(sent.Count == 0, "first delay");
s.Tick(149, true, "ABC", true, 80, 150, Send); Check(sent.Count == 0, "upper boundary");
s.Tick(150, true, "ABC", true, 80, 150, Send); Check(sent.SequenceEqual("A"), "first character");
s.Tick(9000, true, "ABC", true, 80, 150, Send); Check(sent.SequenceEqual("AB"), "no catch-up burst");
s.Tick(9001, true, "ABC", true, 80, 150, Send); Check(sent.Count == 2, "next deadline from actual send");
s.Tick(9150, true, "ABC", true, 80, 150, Send); Check(sent.SequenceEqual("ABC"), "character order");
s.Tick(9300,true,"ABC",true,80,150,Send);Check(sent.SequenceEqual("ABC\r"),"physical Enter after complete word");
s.Tick(10000, true, "ABC", true, 80, 150, Send); Check(sent.Count == 4, "do not retype or submit stale word twice");
s.Tick(13001, true, "ABC", true, 80, 150, Send); Check(!s.Running && s.Status == "NoProgress", "stale word timeout");
Check(samples.All(x => x == (80,151)), "inclusive bounds");
s.Start(0); s.Tick(3000,false,"ABC",false,80,150,Send); Check(s.Running && s.Status=="Paused","focus loss waits without stopping");
s.Start(0); s.Tick(3000,true,"漢字",false,80,150,Send); Check(!s.Running && s.Status=="Unsupported","reject unsupported");
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
var clock=new TestClock();var writes=new List<(string Path,byte[] Bytes)>();
using(var diagnostic=new DiagnosticRecorder(Path.Combine(AppContext.BaseDirectory,"diagnostic-fixture"),clock,(path,bytes)=>writes.Add((path,bytes)))) {
 Check(diagnostic.Start(),"start diagnostic");diagnostic.Observe("{\"ready\":true}");Check(writes.Count==0,"batch diagnostic writes");
 clock.Advance(1);diagnostic.Observe("{\"ready\":true}");Check(writes.Count==0,"unchanged snapshot no write");
 clock.Advance(1);diagnostic.Tick();Check(writes.Count==1,"two second flush");
 diagnostic.Observe("{\"ready\":false}");diagnostic.Mark("block");Check(writes.Count==2,"marker flush");
 Check(writes[0].Path!=writes[1].Path,"JST date partition rollover");
 var records=diagnostic.Records;clock.Advance(5);diagnostic.Observe("{\"ready\":false}");diagnostic.Tick();Check(diagnostic.Records==records && writes.Count==2,"unchanged period zero append");
 clock.Advance(7200);diagnostic.Tick();Check(!diagnostic.Active && diagnostic.State=="DiagLimit","120 minute limit without changed snapshot");
}
Check(writes.SelectMany(x=>System.Text.Encoding.UTF8.GetString(x.Bytes).Split('\n',StringSplitOptions.RemoveEmptyEntries)).All(x=>System.Text.Json.JsonDocument.Parse(x).RootElement.GetProperty("schema").GetInt32()==1),"parseable JSON lines");
var failuresCount=0;using(var diagnostic=new DiagnosticRecorder(Path.Combine(AppContext.BaseDirectory,"diagnostic-fail"),append:(_,_)=>{failuresCount++;throw new IOException("fixture uncertain append");})){
 diagnostic.Start();diagnostic.Mark("cast");diagnostic.Tick();diagnostic.Stop();Check(failuresCount==1 && diagnostic.State=="DiagError","uncertain append not replayed");
}
using(var diagnostic=new DiagnosticRecorder(Path.Combine(AppContext.BaseDirectory,"diagnostic-limit"),append:(_,_)=>{})){
 diagnostic.Start();diagnostic.Observe(System.Text.Json.JsonSerializer.Serialize(new {value=new string('x',40000)}));Check(!diagnostic.Active && diagnostic.State=="DiagLimit","oversized record stops within cap");
}
Check(TypingPlan.Create("くえ")?.Keys=="kue","hiragana -> physical romaji");
Check(TypingPlan.Create("しんらばんぐる")?.Keys=="shin'raban'guru","nasal syllables are unambiguous");
Check(TypingPlan.Create("うぉーる")?.Keys=="uxo-ru","small kana and prolonged sound");
Check(TypingPlan.Create("ぶれいんぽっど")?.Keys=="burein'poxtsudo","geminate sequence");
Check(TypingPlan.Create("しみる～")?.Keys=="shimiru~","full-width wave -> tilde key");
Check(TypingPlan.Create("しみる〜")?.Keys=="shimiru~","wave dash -> tilde key");
Check(TypingPlan.Create("しみる~")?.Keys=="shimiru~","ASCII tilde accepted");
Check(TypingPlan.NormalizeAnswer("しみる〜")==TypingPlan.NormalizeAnswer("しみる～"),"IME wave variants compare equally");
Check(Keyboard.SupportedModifiers(0x141) && !Keyboard.SupportedModifiers(0x641),"Shift allowed, AltGr rejected");
Check(TypingPlan.Create("あ😀")==null,"unsupported character not silently omitted");
var jp=new TypingSession((a,b)=>a);sent.Clear();jp.Start(0);
jp.Tick(3000,true,"くえ",false,100,100,Send);
for(var t=3100;t<=3300;t+=100)jp.Tick(t,true,"くえ",true,100,100,Send,composing:true);
Check(new string(sent.ToArray())=="kue","romaji key sequence");
jp.Tick(3400,true,"くえ",true,100,100,Send,composing:true);Check(sent.Last()=='\r' && !jp.Submitted,"first Enter commits IME only");
jp.Tick(3600,true,"くえ",true,100,100,Send,composing:false,evaluated:"くえ");Check(jp.Submitted && sent.Count==5,"second Enter submits verified hiragana");
var wrong=new TypingSession((a,b)=>a);wrong.Start(0);wrong.Tick(3000,true,"あ",false,100,100,Send);wrong.Tick(3100,true,"あ",true,100,100,Send);
var sentBefore=sent.Count;wrong.Tick(3200,true,"あ",true,100,100,Send,evaluated:"a");Check(wrong.Status=="ImeFailed" && sent.Count==sentBefore,"unconverted Latin never submitted as Japanese");
var pause=new TypingSession((a,b)=>a);pause.Start(0);pause.Tick(3000,true,"ABC",false,100,100,Send);sentBefore=sent.Count;
pause.Tick(3100,false,"ABC",true,100,100,Send,paused:true);pause.Tick(7099,false,"ABC",true,100,100,Send,paused:true);
Check(pause.Running && sent.Count==sentBefore,"no input throughout four-second pause");
pause.Tick(7100,true,"ABC",true,100,100,Send);Check(sent.Count==sentBefore+1,"resume existing sequence without burst");
var partial=new TypingSession((a,b)=>a);partial.Start(0);partial.Tick(3000,true,"AB",false,100,100,Send);partial.Tick(3100,true,"AB",true,100,100,Send);partial.Tick(3200,true,"CD",false,100,100,Send,evaluated:"");
Check(partial.Running && partial.CurrentWord=="","cleared input after phase change abandons old word without stopping");
var policy=new EncounterPolicy();
KtgWord Enemy(uint id,string cast,int percent)=>new(id,"word",0,cast,percent,0);
policy.Update(100,[Enemy(1,"動体感知ジャマー",98)]);Check(!policy.Paused(100),"do not pause at cast start");
Check(policy.Update(200,[Enemy(1,"",0)]) && policy.PauseUntil==4200,"four seconds after inferred cast end");
Check(policy.Paused(4199) && !policy.Paused(4200),"exact four-second boundary");
policy.Update(300,[Enemy(1,"",0)]);Check(policy.PauseUntil==4200,"no repeated timer extension");
policy.Reset();policy.Update(100,[Enemy(1,"動体感知ジャマー",40)]);policy.Update(200,[Enemy(1,"",0)]);Check(!policy.Paused(200),"early interrupt is not activation");
policy.Update(300,[Enemy(1,"動体感知ジャマー",99)]);policy.Update(400,[]);Check(!policy.Paused(400),"enemy despawn is not activation");
var ordered=EncounterPolicy.Prioritize([Enemy(1,"",0),Enemy(2,"cast",40),Enemy(3,"cast",80)]).ToArray();Check(ordered.Select(x=>x.EntityId).SequenceEqual(new uint[]{3,2,1}),"cast priority then progress");
var barrier=new KtgWord(9,"support prompt",3,"",0,0,2);
ordered=EncounterPolicy.Prioritize([Enemy(1,"cast",99),new KtgWord(2,"longer ordinary enemy text",0,"",0,0),barrier]).ToArray();
Check(ordered[0].EntityId==9 && ordered[1].EntityId==1,"support-kind prompt above casting enemy, independent of text length");
var phase=new TypingSession((a,b)=>a);phase.Start(0);sent.Clear();phase.Tick(0,true,"AB",false,100,100,Send);phase.Tick(100,true,"AB",true,100,100,Send);
phase.PauseByUser();phase.Tick(200,true,"AB",true,100,100,Send);Check(phase.Running && phase.ManualPaused && sent.Count==1,"pause retains session and sends no key");
phase.Tick(5000,true,"AB",true,100,100,Send);phase.ResumeByUser();phase.Tick(5100,true,"AB",true,100,100,Send);Check(phase.Running && sent.Count==2,"play resumes paused word without reset");
phase.Tick(5200,false,null,false,100,100,Send);phase.Tick(9000,true,"CD",false,100,100,Send,evaluated:"");
Check(phase.Running && phase.CurrentWord=="","phase transition keeps automation running");
var clearing=new TypingSession((a,b)=>a);clearing.Start(0);clearing.Tick(0,true,"AB",false,100,100,Send);clearing.Tick(100,true,"AB",true,100,100,Send);
clearing.Tick(200,true,"CD",false,100,100,Send,composing:true,evaluated:"a");Check(clearing.Status=="Clearing" && sent.Last()=='\b',"clear stale partial text with physical Backspace");
clearing.Tick(300,true,"CD",false,100,100,Send,evaluated:"");Check(clearing.Running && clearing.CurrentWord=="","continue after old text clears");
// Regression: vu may be rendered as katakana by the IME; never accept unrelated text.
Check(TypingPlan.Create("ゔぁ")?.Keys=="vuxa","vu plus explicit small vowel");
Check(TypingPlan.NormalizeAnswer("ヴぁ")==TypingPlan.NormalizeAnswer("ゔぁ"),"vu script variant compares by reading");
Check(TypingPlan.NormalizeAnswer("う\u3099ぁ")==TypingPlan.NormalizeAnswer("ゔぁ"),"decomposed voiced vowel normalizes");
Check(TypingPlan.NormalizeAnswer("ぶぁ")!=TypingPlan.NormalizeAnswer("ゔぁ"),"different kana remains a mismatch");
foreach(var answer in new[]{"ゔぁ","ヴぁ","う\u3099ぁ"})
{
 var vu=new TypingSession((a,b)=>a);sent.Clear();vu.Start(0);vu.Tick(0,true,"ゔぁ",false,100,100,Send);
 for(var t=100;t<=400;t+=100)vu.Tick(t,true,"ゔぁ",true,100,100,Send,composing:true);
 vu.Tick(500,true,"ゔぁ",true,100,100,Send,composing:true);
 Check(sent.Last()==TypingSession.HiraganaKey && !vu.Submitted,"convert to hiragana before commit");
 vu.Tick(650,true,"ゔぁ",true,100,100,Send,composing:true);
 Check(sent.Last()=='\r' && sent.Count(c=>c==TypingSession.HiraganaKey)==1,"F6 once then IME Enter");
 vu.Tick(800,true,"ゔぁ",true,100,100,Send,evaluated:answer);
 Check(vu.Running && vu.Submitted && sent.Count(c=>c=='\r')==2,"verified vu variant is submitted");
}
var stale=new TypingSession((a,b)=>a);sent.Clear();stale.Start(0);stale.Tick(0,true,"AB",false,100,100,Send);stale.Tick(100,true,"AB",true,100,100,Send);
stale.Tick(200,true,"CD",false,100,100,Send,evaluated:"a");
stale.Tick(300,true,"AB",true,100,100,Send,evaluated:"a");Check(sent.SequenceEqual(new[]{'A','\b','\b'}),"target reappearance cannot cancel clearing");
stale.Tick(400,true,"CD",false,100,100,Send,evaluated:null);Check(stale.CurrentWord=="AB" && sent.Count==3,"unknown input state is not treated as empty");
stale.Tick(500,true,"CD",false,100,100,Send,evaluated:"");stale.Tick(600,true,"CD",false,100,100,Send,evaluated:"");stale.Tick(700,true,"CD",true,100,100,Send,evaluated:"");
Check(stale.Running && stale.CurrentWord=="CD" && sent.Last()=='C',"next word only after confirmed empty input");
stale.Stop("Completed");stale.Tick(900,true,"CD",true,100,100,Send);Check(!stale.Running && stale.Status=="Completed" && sent.Last()=='C',"completion cancels future input");
var seen=new SeenPrompts();var unseen=Enemy(31,"cast",99);var support=new KtgWord(32,"barrier",2,"",0,0,2);
seen.Update([unseen,support],[]);Check(!seen.Contains(unseen)&&!seen.Contains(support),"unobserved casting and support prompts remain ineligible");
seen.Update([unseen,support],[new(31,unseen.Word,false)]);Check(!seen.Contains(unseen),"hidden first appearance is not seen");
seen.Update([unseen,support],[new(31,unseen.Word,true)]);Check(seen.Contains(unseen)&&!seen.Contains(support),"first actual display admits only observed target");
seen.Update([unseen,support],[new(31,unseen.Word,false)]);Check(seen.Contains(unseen),"temporary text concealment retains known answer");
var replacement=unseen with {Word="next"};seen.Update([replacement],[]);Check(!seen.Contains(replacement)&&!seen.Contains(unseen),"new reading on same entity resets observation");
seen.Update([replacement],[new(31,unseen.Word,true)]);Check(!seen.Contains(replacement),"stale UI word cannot admit unseen replacement");
seen.Update([replacement],[new(31,replacement.Word,true)]);Check(seen.Contains(replacement),"matching replacement display admits it");
seen.Update([replacement with {Slot=8}],[]);Check(seen.Contains(replacement),"array slot reorder preserves entity observation");
var twin=replacement with {EntityId=33};seen.Update([replacement,twin],[]);Check(!seen.Contains(twin),"identical reading on another entity has independent visibility");
seen.Update([],[]);seen.Update([replacement],[]);Check(!seen.Contains(replacement),"despawn invalidates remembered reading");
seen.Update([replacement],[new(31,replacement.Word,true)]);seen.Clear();Check(!seen.Contains(replacement),"content reset removes visibility history");
seen.Update([replacement with {Visible=false}],[new(31,replacement.Word,true)]);Check(!seen.Contains(replacement),"inactive array target cannot become observed");
var delta=new DiagnosticDelta();var nums=new[]{0,1};var txt=new[]{"word"};var ui=new Dictionary<string,object>{{"input",new{enabled=true}},{"cast",new{visible=true}}};
var first=delta.Capture(nums,txt,ui);Check(first!=null && delta.Capture(nums,txt,ui)==null,"identical frame has zero record");
nums[1]=2;var changed=System.Text.Json.JsonDocument.Parse(delta.Capture(nums,txt,ui)!);
Check(!changed.RootElement.GetProperty("ui").EnumerateObject().Any(),"numeric-only change does not repeat UI");
ui["input"]=new{enabled=false};changed=System.Text.Json.JsonDocument.Parse(delta.Capture(nums,txt,ui)!);
Check(changed.RootElement.GetProperty("ui").EnumerateObject().Count()==1,"only changed UI section stored");
Check(DiagnosticRecorder.SessionLimit==64*1024*1024 && DiagnosticRecorder.TotalLimit==256*1024*1024,"expanded log capacity");
var lifecycle=new AutomationLifecycle();
Check(!lifecycle.TryAutoStart(true,true,false,true),"auto start waits for playable game");
Check(!lifecycle.TryAutoStart(true,true,true,false),"auto start does not steal another input");
Check(lifecycle.TryAutoStart(true,true,true,true),"auto start at first playable frame");
lifecycle.Start(100);
Check(lifecycle.Evaluate(101,true,false,true,false,false,true,true)==RunGate.InputUnavailable,"start with disabled input waits");
Check(lifecycle.Evaluate(3100,true,false,true,false,false,true,true)==RunGate.InputUnavailable,"disabled pre-game input waits before focus checks");
Check(lifecycle.Evaluate(3101,true,false,true,false,true,false,false)==RunGate.Run,"proceed when input becomes usable");
Check(lifecycle.Evaluate(3101,true,false,true,false,true,false,true)==RunGate.OtherTextInput,"protect another text editor");
Check(lifecycle.Evaluate(101,true,false,false,false,false,false,false)==RunGate.ForegroundLost,"foreground gate blocks key delivery");
Check(!lifecycle.TryAutoStart(true,true,true,true),"one auto start per content entry");
lifecycle.Suppress();Check(!lifecycle.TryAutoStart(true,true,true,true),"manual stop cannot auto-resume");
lifecycle.Start(5000);Check(lifecycle.Evaluate(7999,true,false,true,false,true,false,false)==RunGate.Run,"resume has no arming delay");
Check(lifecycle.Evaluate(8000,true,false,true,false,true,false,false)==RunGate.Run,"resume button reaches run");
lifecycle.TryAutoStart(false,true,false,false);Check(lifecycle.TryAutoStart(true,true,true,true),"next entry can auto start");
var focus=new FocusAcquisition();var requests=0;
Check(focus.Step(0,false,()=>requests++)==FocusResult.Requested,"first focus request");
focus.Step(250,false,()=>requests++);Check(focus.Step(500,false,()=>requests++)==FocusResult.Requested && requests==3,"third focus request still pending");
Check(focus.Step(501,false,()=>requests++)==FocusResult.Waiting,"observe third request before failure");
Check(focus.Step(749,true,()=>requests++)==FocusResult.Ready && focus.Attempts==0,"third focus request may succeed");
focus.Step(1000,false,()=>requests++);focus.Step(1250,false,()=>requests++);focus.Step(1500,false,()=>requests++);
Check(focus.Step(1750,false,()=>requests++)==FocusResult.Exhausted && requests==6,"bounded focus failure after last observation window");
Check(focus.Step(3749,false,()=>requests++)==FocusResult.Exhausted && requests==6,"focus retry cooldown has no extra requests");
Check(focus.Step(3750,false,()=>requests++)==FocusResult.Requested && requests==7,"focus reacquisition resumes after bounded cooldown");
var migrationRoot=Path.Combine(AppContext.BaseDirectory,"auto-config-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(migrationRoot);
try {
 var file=Path.Combine(migrationRoot,"config.json");File.WriteAllText(file,"{\"Version\":1,\"Language\":\"ja\",\"MinimumMs\":80,\"MaximumMs\":150}");
 using(var store=new ConfigurationStore(file)) {Check(store.Configuration.AutoStart,"old configuration inherits authorized auto start");store.Configuration.AutoStart=false;store.Update(store.Configuration);store.Flush();}
 using(var store=new ConfigurationStore(file))Check(!store.Configuration.AutoStart,"auto-start setting survives save/reload");
} finally {Directory.Delete(migrationRoot,true);}
if(args.Length>0)
{
    var numbers=new int[261];var inputEnabled=false;var replayLifecycle=new AutomationLifecycle();var started=0;var premature=false;var stopped=false;
    foreach(var line in File.ReadLines(args[0]))
    {
        using var doc=System.Text.Json.JsonDocument.Parse(line);var row=doc.RootElement;if(row.GetProperty("kind").GetString()!="snapshot")continue;
        var data=row.GetProperty("data");if(!data.TryGetProperty("numericChanges",out var changes))continue;
        foreach(var item in changes.EnumerateArray())numbers[item[0].GetInt32()]=item[1].GetInt32();
        if(data.GetProperty("ui").TryGetProperty("input",out var input))inputEnabled=input.GetProperty("enabled").GetBoolean();
        var now=(long)row.GetProperty("elapsedMs").GetDouble();
        if(replayLifecycle.TryAutoStart(true,true,inputEnabled && numbers[0]>0,true)) {started++;premature|=!inputEnabled;replayLifecycle.Start(now);}
        if(started>0) {var gate=replayLifecycle.Evaluate(now,true,false,true,false,inputEnabled,false,false);stopped|=gate is not (RunGate.Arming or RunGate.InputUnavailable or RunGate.Run);}
    }
    Check(started==1,"recording replay starts exactly once after input becomes usable");
    Check(!premature,"recording replay never starts during disabled input");
    Check(!stopped,"recording replay preserves waiting across disabled input transitions");
}
Console.WriteLine($"PASS {checks} checks; no real keyboard events sent.");
namespace KeyboundPilot { internal static class Loc { public static bool IsSupported(string? s)=>s is "ja" or "en" or "de" or "fr" or "ko" or "zh-Hans" or "zh-Hant"; } }
sealed class TestClock:TimeProvider {long ticks;public override long TimestampFrequency=>1000;public override long GetTimestamp()=>ticks;public override DateTimeOffset GetUtcNow()=>new DateTimeOffset(2026,10,28,14,59,59,TimeSpan.Zero).AddMilliseconds(ticks);public void Advance(int seconds)=>ticks+=seconds*1000;}


