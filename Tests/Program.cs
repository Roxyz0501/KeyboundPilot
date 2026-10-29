using KeyboundPilot;
var checks = 0;
void Check(bool value, string what) { checks++; if (!value) throw new Exception(what); }
ObstructionTests.Run(Check);FinishRoundTests.Run(Check);
ScoreTests.Run(Check);ExitDelayTests.Run(Check);
RematchTests.Run(Check);InputProfileTests.Run(Check);
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
var independent=new Configuration();
independent.SetMaximum(40);Check(independent.MinimumMs==80 && independent.MaximumMs==80,"maximum cannot pull minimum down");
independent.SetMaximum(300);Check(independent.MinimumMs==80 && independent.MaximumMs==300,"raising maximum preserves minimum");
independent.SetMinimum(400);Check(independent.MinimumMs==300 && independent.MaximumMs==300,"minimum cannot push maximum up");
independent.SetMinimum(20);Check(independent.MinimumMs==20 && independent.MaximumMs==300,"lowering minimum preserves maximum");
independent.SetMaximum(99999);Check(independent.MinimumMs==20 && independent.MaximumMs==5000,"maximum global bound preserves minimum");
independent.SetMinimum(-1);Check(independent.MinimumMs==20 && independent.MaximumMs==5000,"minimum global bound preserves maximum");
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
var sentBefore=sent.Count;wrong.Tick(3200,true,"あ",true,100,100,Send,evaluated:"a");Check(wrong.Recovering && wrong.Running && sent.Count==sentBefore,"unconverted Latin never submitted as Japanese");
wrong.Tick(3300,true,"あ",true,100,100,Send,evaluated:"a");Check(sent.Last()=='\b' && wrong.Recovering,"IME mismatch clears physical input");
var clearedCount=sent.Count;wrong.Tick(3400,true,"あ",true,100,100,Send,evaluated:"");Check(!wrong.Recovering && wrong.Running && wrong.CurrentWord=="" && sent.Count==clearedCount,"empty confirmation before recovery restart");
wrong.Tick(3500,true,"あ",true,100,100,Send,evaluated:"");Check(wrong.CurrentWord=="あ" && wrong.NeedsPreparation,"recovery prepares fresh IME input");

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
var motion=new TargetMotion();var mover=Enemy(45,"",0) with {ScreenX=500,ScreenY=600};
Check(!motion.Update(0,[mover])[0].Moving,"first observation does not invent movement");
Check(!motion.Update(50,[mover with {ScreenY=601}])[0].Moving,"one pixel jitter ignored");
Check(motion.Update(100,[mover with {ScreenY=602}])[0].Moving,"slow movement accumulates from baseline");
Check(!motion.Update(2101,[mover with {ScreenY=602}])[0].Moving,"stationary target stops moving classification");
motion.Update(2200,[]);Check(!motion.Update(2300,[mover])[0].Moving,"despawn drops motion history");
var near=mover with {EntityId=46,Slot=7,ScreenY=800,Moving=true};var far=mover with {EntityId=47,Slot=1,ScreenY=650,Moving=true};var still=mover with {EntityId=48,Slot=0,ScreenY=900};
ordered=EncounterPolicy.Prioritize([still,far,near,barrier,Enemy(49,"cast",80)]).ToArray();
Check(ordered.Select(x=>x.EntityId).SequenceEqual(new uint[]{9,49,46,48,47}),"barrier then cast then closest moving then other slots");
Check(EncounterPolicy.Prioritize([near with {Visible=false},far]).First().EntityId==47,"inactive target cannot receive closest priority");
var gap=new TypingSession((a,b)=>a);sent.Clear();gap.Start(0);gap.Tick(0,true,"A",false,100,100,Send);gap.Tick(100,true,"A",true,100,100,Send);
gap.Tick(200,true,"A",true,100,100,Send,betweenWordsMs:1000);gap.Tick(300,true,"B",false,100,100,Send,evaluated:"");
Check(gap.Status=="BetweenWords" && sent.SequenceEqual("A\r"),"post-submit delay starts at game Enter");
gap.Tick(1199,true,"B",false,100,100,Send);Check(gap.CurrentWord=="" && sent.Count==2,"next answer not selected early");
gap.Tick(1200,true,"B",false,100,100,Send);gap.Tick(1300,true,"B",true,100,100,Send);Check(gap.CurrentWord=="B" && sent.Last()=='B',"next answer begins after deadline plus per-key interval");
gap.Stop();gap.Start(1400);gap.Tick(1400,true,"C",false,100,100,Send);Check(gap.CurrentWord=="C","manual start discards old post-submit deadline");
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
var gazePolicy=new EncounterPolicy();
gazePolicy.Update(100,[Enemy(1,"呪いの視線",98)]);Check(!gazePolicy.Paused(100),"gaze does not stop during cast");
Check(gazePolicy.Update(200,[Enemy(1,"",0)]) && gazePolicy.Paused(4199) && !gazePolicy.Paused(4200),"gaze end pauses exactly four seconds");
gazePolicy.Reset();gazePolicy.Update(100,[Enemy(1,"呪いの視線",40)]);Check(!gazePolicy.Update(200,[Enemy(1,"",0)]) && !gazePolicy.Paused(200),"gaze interruption does not trigger");
gazePolicy.Update(300,[Enemy(1,"呪いの視線",99)]);Check(!gazePolicy.Update(400,[]) && !gazePolicy.Paused(400),"gaze despawn does not trigger");
gazePolicy.Update(500,[Enemy(1,"呪いの視線",100)]);Check(gazePolicy.Update(600,[Enemy(1,"動体感知ジャマー",1)]),"consecutive different blocking casts detect first end");
gazePolicy.Update(700,[Enemy(1,"動体感知ジャマー",99)]);gazePolicy.Update(800,[Enemy(1,"",0)]);Check(gazePolicy.PauseUntil==4800,"second blocking cast extends pause");
gazePolicy.Reset();Check(!gazePolicy.Paused(800),"reset clears gaze history and pause");
var migrationRoot=Path.Combine(AppContext.BaseDirectory,"auto-config-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(migrationRoot);
try {
 var file=Path.Combine(migrationRoot,"config.json");File.WriteAllText(file,"{\"Version\":1,\"Language\":\"ja\",\"MinimumMs\":80,\"MaximumMs\":150}");
 using(var store=new ConfigurationStore(file)) {Check(store.Configuration.AutoStart,"old configuration inherits authorized auto start");Check(store.Configuration.BetweenWordsMs==0,"old configuration defaults to zero answer delay");store.Configuration.BetweenWordsMs=1750;store.Configuration.AutoStart=false;store.Update(store.Configuration);store.Flush();}
 using(var store=new ConfigurationStore(file)){Check(!store.Configuration.AutoStart,"auto-start setting survives save/reload");Check(store.Configuration.BetweenWordsMs==1750,"answer delay survives save/reload");}
} finally {Directory.Delete(migrationRoot,true);}

var lobby=new LoopSnapshot(true,true,false,false,false,false,false,false,false,false);
var playing=lobby with {Lobby=false,InGame=true};
var result=playing with {Result=true,LeaveReady=true};
var rounds=new RoundLoop();rounds.Start(0,2,false);
Check(rounds.Current==1 && rounds.Total==2,"loop starts at run one");
Check(rounds.Tick(0,lobby)==LoopAction.Queue,"loop requests once");
Check(rounds.Tick(200,lobby)==LoopAction.None,"uncertain request not repeated");
var popped=lobby with {Queued=true,EntryReady=true};
Check(rounds.Tick(400,popped)==LoopAction.Accept,"matching pop accepted");
Check(rounds.Tick(600,popped)==LoopAction.None,"pop accepted at most once");
Check(rounds.Tick(800,playing)==LoopAction.StartTyping,"entry starts typing");
Check(rounds.Tick(1000,playing)==LoopAction.None,"phase changes preserve playing");
Check(rounds.Tick(1200,result)==LoopAction.Leave && rounds.Completed==1,"result counts once and leaves");
Check(rounds.Tick(1400,result)==LoopAction.None && rounds.Completed==1,"persistent result does not recount or reclick");
Check(rounds.Tick(1600,result with {ExitConfirm=true})==LoopAction.ConfirmExit,"result exit confirmation once");
Check(rounds.Tick(1800,result with {ExitConfirm=true})==LoopAction.None,"persistent confirmation not repeated");
rounds.Tick(2000,lobby with {Queued=true});Check(rounds.Phase==LoopPhase.Leaving,"wait for departed queue state to clear");
rounds.Tick(2200,lobby);Check(rounds.Phase==LoopPhase.Requesting && rounds.Current==2,"next round prepared after return");
Check(rounds.Tick(2400,lobby)==LoopAction.Queue,"second request issued after return");
rounds.Tick(2600,playing);rounds.Tick(2800,result);rounds.Tick(3000,lobby);
Check(!rounds.Active && rounds.Phase==LoopPhase.Complete && rounds.Completed==2 && rounds.Current==2,"configured two runs completed");
Check(rounds.Tick(3200,lobby)==LoopAction.None,"no extra queue after completion");
rounds.Start(0,1,true);Check(rounds.Phase==LoopPhase.Playing,"loop can include current game");
Check(rounds.Tick(1,result with {LeaveReady=false})==LoopAction.None && rounds.Completed==1,"count result while exit button unavailable");
Check(rounds.Tick(2,result)==LoopAction.Leave,"leave once result button ready");
rounds.Start(0,1,false);rounds.Pause();Check(rounds.Tick(1,lobby)==LoopAction.None,"paused loop never queues");
rounds.Resume();Check(rounds.Tick(100000,lobby)==LoopAction.Queue,"resume queues exactly once");
rounds.Pause();rounds.Tick(100001,lobby);rounds.Resume();rounds.Tick(200000,lobby);
Check(rounds.Active,"manual pause freezes request timeout");
rounds.Tick(215001,lobby);Check(rounds.Phase==LoopPhase.Error,"missing queue times out");
rounds.Start(0,1,false);rounds.Tick(0,lobby);rounds.Tick(100,lobby with {Loading=true});rounds.Tick(100000,lobby);
Check(rounds.Active,"loading freezes timeout");
rounds.Start(0,1,false);Check(rounds.Tick(0,lobby with {OtherQueue=true})==LoopAction.None && !rounds.Active,"unrelated queue blocks all actions");
rounds.Start(0,1,false);rounds.Tick(0,lobby with {Queued=true});Check(rounds.Phase==LoopPhase.Error,"do not take over preexisting queue");
rounds.Start(0,1,true);rounds.Tick(0,lobby);Check(rounds.Phase==LoopPhase.Error && rounds.Completed==0,"early departure is not completion");
rounds.Start(0,1,false);rounds.Tick(0,lobby with {LoggedIn=false});Check(!rounds.Active,"logout stops loop");
rounds.Start(0,1,false);rounds.Stop();Check(rounds.Tick(0,lobby)==LoopAction.None,"stop cancels pending loop work");
rounds.Start(0,1,false);Check(rounds.Tick(0,lobby,true)==LoopAction.None,"foreground or UI capture blocks queue");
rounds.Start(0,1,true);rounds.Tick(1,result);rounds.Tick(120002,result);Check(rounds.Phase==LoopPhase.Error,"departure timeout stops without duplicate exit");
rounds.Start(0,1,false);rounds.Tick(0,lobby);rounds.Stop();Check(rounds.Tick(100,popped)==LoopAction.None,"cancel during UI preparation prevents later accept");
rounds.Start(0,1,false);rounds.Tick(0,lobby);rounds.Pause();Check(rounds.Tick(100,popped)==LoopAction.None,"pause during UI preparation blocks pop action");
rounds.Resume();Check(rounds.Tick(200,popped with {OtherQueue=true})==LoopAction.None && rounds.Phase==LoopPhase.Error,"unrelated queue after UI preparation stops loop");
rounds.Start(0,1,false);rounds.Tick(0,lobby);rounds.Tick(16000,lobby with {Preparing=true});Check(rounds.Active,"reception travel can exceed normal queue timeout");
rounds.Tick(120001,lobby with {Preparing=true});Check(rounds.Phase==LoopPhase.Error,"missing receptionist or path has bounded preparation wait");
rounds.Start(0,1,false);rounds.Tick(0,lobby);rounds.Tick(10000,lobby with {Preparing=true});rounds.Tick(16000,lobby);Check(rounds.Active,"queue response timeout starts after reception preparation");
rounds.Tick(31001,lobby);Check(rounds.Phase==LoopPhase.Error,"finished reception request without queue is not retried");
var warning=lobby with {KeyboardWarning=true};
rounds.Start(0,2,false);rounds.Tick(0,lobby);Check(rounds.Tick(100,warning)==LoopAction.ConfirmKeyboard,"keyboard confirmation accepted during own loop wait");
Check(rounds.Tick(200,warning)==LoopAction.None,"persistent keyboard warning clicked once");
rounds.Start(0,1,false);rounds.Tick(0,lobby);rounds.Pause();Check(rounds.Tick(100,warning)==LoopAction.None,"paused loop never confirms warning");
rounds.Resume();Check(rounds.Tick(200,warning,true)==LoopAction.None,"foreground gate blocks warning confirmation");
rounds.Stop();Check(rounds.Tick(300,warning)==LoopAction.None,"stopped loop never confirms warning");
rounds.Start(0,1,false);rounds.Tick(0,lobby);Check(rounds.Tick(100,warning with {OtherQueue=true})==LoopAction.None,"unrelated queue blocks warning");
rounds.Start(0,1,true);Check(rounds.Tick(100,playing with {KeyboardWarning=true})==LoopAction.None,"in-game loop never confirms entry warning");
var keeper=new ActivityKeeper();var touches=0;bool Touch(){touches++;return true;}
keeper.Tick(0,false,Touch);Check(touches==0,"inactive content has no activity writes");
keeper.Tick(0,true,Touch);keeper.Tick(2999,true,Touch);Check(touches==1,"activity write limited to once per three seconds");
keeper.Tick(3000,true,Touch);Check(touches==2,"active content refreshes on schedule");
keeper.Tick(20000,false,Touch);Check(touches==2,"paused or stopped typing never refreshes activity");
keeper.Tick(20001,true,Touch);Check(touches==3,"resumed typing refreshes activity");
keeper.Reset();var attempts=0;for(var i=0;i<10;i++)keeper.Tick(i*10000,true,()=>{attempts++;return false;});
Check(attempts==3 && keeper.Failed,"activity failures bounded to three attempts");
keeper.Reset();keeper.Tick(0,true,Touch);Check(!keeper.Failed,"explicit start permits activity retry");
keeper.Begin();Check(keeper.Requested,"play starts activity session independently of typing status");
var heldTouches=0;keeper.Tick(0,keeper.Requested,()=>{heldTouches++;return true;});keeper.Tick(10000,keeper.Requested,()=>{heldTouches++;return true;});
Check(heldTouches==2 && keeper.Requested,"activity session continues across a typing failure until explicit end");
keeper.End();Check(!keeper.Requested,"manual pause stop or exit ends activity session");
keeper.Begin();Check(keeper.Requested && !keeper.Failed,"resume restarts protection and retry budget");keeper.End();
var logOffRoot=Path.Combine(AppContext.BaseDirectory,"logs-off-"+Guid.NewGuid().ToString("N"));
using(var inactiveLog=new DiagnosticRecorder(logOffRoot)){
 inactiveLog.Observe("{}");inactiveLog.Event("test",new{});inactiveLog.Mark("cast");inactiveLog.Tick();inactiveLog.Stop();
 Check(inactiveLog.Records==0 && inactiveLog.BytesWritten==0 && !Directory.Exists(logOffRoot),"disabled recorder creates no files or directories");
}
var newConfigRoot=Path.Combine(AppContext.BaseDirectory,"loop-config-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(newConfigRoot);
try {
 var file=Path.Combine(newConfigRoot,"config.json");File.WriteAllText(file,"{\"Language\":\"ja\",\"MinimumMs\":80,\"MaximumMs\":150}");
 using(var store=new ConfigurationStore(file)) {
  Check(!store.Configuration.RecordDiagnostics && store.Configuration.LoopCount==1,"old config migrates logging off and one run");
  Check(!store.Configuration.ShowDebug,"old config defaults debug display off");
  store.Configuration.ShowDebug=true;store.Configuration.RecordDiagnostics=true;store.Configuration.LoopCount=12;store.Update(store.Configuration);store.Flush();
 }
 using(var store=new ConfigurationStore(file)) {
  Check(store.Configuration.ShowDebug,"debug display survives save/reload");
  Check(store.Configuration.RecordDiagnostics && store.Configuration.LoopCount==12,"loop and log settings survive save/reload");
  store.Configuration.ShowDebug=false;store.Configuration.RecordDiagnostics=false;store.Update(store.Configuration);store.Flush();
 }
 using(var store=new ConfigurationStore(file)){Check(!store.Configuration.RecordDiagnostics,"disabled logging survives restart");Check(!store.Configuration.ShowDebug,"disabled debug display survives restart");}
} finally {Directory.Delete(newConfigRoot,true);}

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
MistypeTests.Run(Check);
BackgroundTests.Run(Check);RangeTests.Run(Check);
Console.WriteLine($"PASS {checks} checks; no real keyboard events sent.");
namespace KeyboundPilot { internal static class Loc { public static bool IsSupported(string? s)=>s is "ja" or "en" or "de" or "fr" or "ko" or "zh-Hans" or "zh-Hant"; } }
sealed class TestClock:TimeProvider {long ticks;public override long TimestampFrequency=>1000;public override long GetTimestamp()=>ticks;public override DateTimeOffset GetUtcNow()=>new DateTimeOffset(2026,10,28,14,59,59,TimeSpan.Zero).AddMilliseconds(ticks);public void Advance(int seconds)=>ticks+=seconds*1000;}



