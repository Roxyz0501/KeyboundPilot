using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Command;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Roxyz0501.Dalamud.CommonUi;

namespace KeyboundPilot;

public sealed class Plugin : IDalamudPlugin
{
    private const string Command = "/keyboundpilot";
    private readonly IDalamudPluginInterface pi;
    private readonly ICommandManager commands;
    private readonly IFramework framework;
    private readonly IClientState client;
    private readonly IDataManager data;
    private readonly IPluginLog log;
    private readonly ConfigurationStore settings;
    private readonly Configuration config;
    private readonly LocalizedFonts fonts;
    private readonly GameReader reader;
    private readonly TypingSession typing = new();
    private readonly ImeInput ime = new();
    private readonly EncounterPolicy encounter = new();
    private readonly AutomationLifecycle automation = new();
    private readonly RoundLoop loop=new();
    private readonly ActivityKeeper activity=new();
    private readonly LoopGameUi loopUi;
    private readonly ICondition condition;
    private readonly IObjectTable objects;
    private bool visuallyObstructed;
    private long loopSample;
    private string controlGate="Idle";
    private string preparedWord="";
    private uint currentEntity;
    private readonly DiagnosticRecorder diagnostics;
    private readonly DiagnosticReader diagnosticReader;
    private bool diagnosticAttempted;
    private long lastDiagnosticSample;
    private readonly CommandInfo command;
    private readonly CancellationTokenRegistration shutdown, unload;
    private readonly object executionGate = new();
    private volatile bool stopping;
    private bool open, disposed;
    private long scanned;
    private uint territory;
    private bool inContent;
    private int failures;
    private bool uiCaptures;
    private ActivitySnapshot activitySnapshot;
    private long activitySample;

    public Plugin(IDalamudPluginInterface pi, ICommandManager commands, IFramework framework,
        IClientState client, IDataManager data, IGameGui gui, IPluginLog log, IGameLifecycle lifecycle, ICondition condition, IObjectTable objects)
    {
        this.pi = pi; this.commands = commands; this.framework = framework;
        this.client = client; this.data = data; this.log = log;this.condition=condition;this.objects=objects;loopUi=new LoopGameUi(gui,data,objects,pi);
        Loc.CheckCompleteness();
        settings = new ConfigurationStore(pi.ConfigFile.FullName, ex => { log.Error(ex, "Configuration saving stopped"); open = true; },
            onFinalFailure: ex => { log.Error(ex, "Final configuration flush failed"); open = true; });
        config = settings.Configuration;
        if(config.RecordDiagnostics){config.RecordDiagnostics=false;settings.Update(config);}
        if (!Loc.IsSupported(config.Language))
        {
            config.Language = Loc.FromGame(client.ClientLanguage) ?? Loc.Normalize(pi.UiLanguage) ?? "en";
            settings.Update(config);
        }
        reader = new GameReader(gui);
        diagnostics = new DiagnosticRecorder(Path.Combine(pi.GetPluginConfigDirectory(),"diagnostics"));
        diagnosticReader = new DiagnosticReader(gui);
        fonts = new LocalizedFonts(pi.UiBuilder.FontAtlas,log,[diagnostics.DirectoryPath]);
        open = settings.Recovered || settings.LoadWarning != null;
        command = new CommandInfo((_, args) =>
        {
            if (args.Trim().Equals("stop", StringComparison.OrdinalIgnoreCase)) StopByUser();
            else open = true;
        }) { HelpMessage = T("CommandHelp") };
        commands.AddHandler(Command, command);
        framework.Update += Update;
        pi.UiBuilder.Draw += Draw;
        pi.UiBuilder.OpenMainUi += Open;
        pi.UiBuilder.OpenConfigUi += Open;
        client.Logout += Logout;
        shutdown = lifecycle.GameShuttingDownToken.Register(Stop);
        unload = lifecycle.DalamudUnloadingToken.Register(Stop);
    }
    private bool UiBlocked => Keyboard.ForegroundGame && uiCaptures;
    private bool Loading => condition[ConditionFlag.BetweenAreas] || condition[ConditionFlag.BetweenAreas51];
    private bool InputAllowed => config.DirectInput || Keyboard.ForegroundGame;
    private bool EscapePressed => Keyboard.ForegroundGame && Keyboard.Escape;
    private bool ModifierPressed => Keyboard.ForegroundGame && Keyboard.Modifier;
    private string T(string key) => Loc.Get(config.Language, key);
    private void Open() => open = true;
    private void Logout(int type, int code) { lock (executionGate) { activity.End();loop.Stop();loopUi.CancelQueuePreparation();typing.Stop(); Keyboard.Release(true); ime.Restore(); encounter.Reset(); automation.Reset(); reader.Clear(); diagnostics.Stop();diagnosticReader.Reset();diagnosticAttempted=false;settings.Flush(); } }
    private void Stop() { lock (executionGate) { stopping = true;activity.End();loop.Stop();loopUi.CancelQueuePreparation(); typing.Stop(); Keyboard.Release(true);ime.Restore();diagnostics.Dispose(); settings.Dispose(); } }
    private bool IsContent()
    {
        if (!client.IsLoggedIn) return false;
        if (territory != client.TerritoryType || territory == 0)
        {
            territory = client.TerritoryType;
            inContent = data.GetExcelSheet<TerritoryType>().GetRowOrDefault(territory)?.TerritoryIntendedUse.RowId == 65;
        }
        return inContent;
    }
    private void Update(IFramework _)
    {
        lock (executionGate) UpdateCore();
    }
    private void StartTyping(long now,string source)
    {
        activity.Begin();automation.Start(now);activity.Reset();failures=0;scanned=0;reader.ResetFocus();
        if(typing.Running && typing.ManualPaused)typing.ResumeByUser();
        else {preparedWord="";currentEntity=0;typing.Start(now);}
        controlGate="Starting";
        diagnostics.Event("typing_start",new {source});
    }
    private void StopByUser()
    {
        activity.End();loop.Stop();loopUi.CancelQueuePreparation();activity.Reset();automation.Suppress();typing.Stop();controlGate="ManualStop";Keyboard.Release(true);ime.Restore();
    }
    private void UpdateCore()
    {
        if(stopping)return;
        settings.Tick();
        try
        {
            var now=Environment.TickCount64;
            if(open && config.ShowDebug && now-activitySample>=250){activitySnapshot=ContentActivity.Read();activitySample=now;}
            typing.ConfigureMistypes(config.MistypeEnabled,config.TypoMin,config.TypoMax);
            if(activity.Requested && EscapePressed){StopByUser();return;}
            var inGame=IsContent();
            if(!inGame)activity.End();
            activity.Tick(now,inGame && !Loading && !reader.ContentComplete && activity.Requested && !typing.ManualPaused && !loop.Paused,ContentActivity.Touch);
            if(!Keyboard.Release()) {typing.Stop("SendFailed");controlGate="KeyReleaseFailed";if(loop.Active)loop.Stop(true);return;}
            UpdateLoop(now);

            if(!inGame) {automation.Reset();reader.Clear();encounter.Reset();if(typing.Running){typing.Stop("Completed");controlGate="OutsideContent";}return;}
            var needsRead=inGame && (typing.Running || loop.Active || activity.Requested || (open && config.ShowDebug) || (config.AutoStart && !automation.Consumed));
            if(needsRead && now-scanned>=50) {reader.Refresh(config.ExcludeUnseenPrompts);visuallyObstructed=objects.LocalPlayer?.StatusList.Any(s=>VisualObstruction.IsStatus(s.StatusId)) == true;scanned=now;}
            if(reader.ContentComplete) {activity.End();automation.Suppress();typing.Stop("Completed");controlGate="ContentComplete";Keyboard.Release(true);return;}
            if(encounter.Update(now,reader.Enemies))diagnostics.Event("input_prohibition_cast_end",new {waitMs=4000,source="cast_disappeared_after_90_percent"});
            if(!typing.Running)
            {
                ime.Restore();preparedWord="";currentEntity=0;
                if(inGame && EscapePressed)automation.Suppress();
                if(loop.Active || !config.AutoStart || automation.Consumed)return;
                var playable=!Loading && reader.Available && reader.InputEnabled && reader.Words.Count>0;
                if(automation.TryAutoStart(inGame,config.AutoStart,playable,
                    !UiBlocked && InputAllowed && !ModifierPressed && !EscapePressed && !reader.CompetingFocus()))
                    StartTyping(now,"game_started");
                else return;
            }
            var gate=automation.Evaluate(now,inGame,EscapePressed,InputAllowed,ModifierPressed,
                !Loading && reader.InputEnabled && reader.Available,UiBlocked,reader.CompetingFocus());
            controlGate=gate.ToString();
            if(gate==RunGate.Escape) {StopByUser();return;}
            if(gate!=RunGate.Run)
            {
                reader.ResetFocus();
                Keyboard.Release(true);
                typing.Tick(now,true,null,false,config.MinimumMs,config.MaximumMs,_=>false,paused:true);return;
            }
            var paused=encounter.Paused(now) || typing.ManualPaused || (config.DirectInput && ime.Inspect(true).Composing);
            var focused=!typing.NeedsInputFocus || (!paused && reader.EnsureFocus(now));
            if(typing.CurrentWord.Length==0) {preparedWord="";currentEntity=0;}
            if(!config.DirectInput && !paused && focused && !typing.Recovering && typing.NeedsPreparation && preparedWord!=typing.CurrentWord)
            {
                if(!string.IsNullOrEmpty(reader.EvaluatedText())) {controlGate="ExistingInput";typing.RecoverIme(now);return;}
                if(!ime.Prepare(typing.Japanese)) {controlGate="ImePreparation";typing.RecoverIme(now);return;}
                preparedWord=typing.CurrentWord;controlGate="ImeSettling";return;
            }
            if(config.DirectInput && focused && !paused && typing.NeedsPreparation && !string.IsNullOrEmpty(reader.EvaluatedText()))
            {typing.RecoverIme(now);return;}
            var imeState=config.DirectInput?default:ime.Inspect();
            if(!config.DirectInput && !paused && focused && !typing.Recovering && typing.Japanese && !typing.Submitted && preparedWord.Length>0 && (!imeState.Available || !imeState.Open || (imeState.Conversion&3)!=1))
            {controlGate="ImeState";typing.RecoverIme(now);return;}
            var previousWord=typing.CurrentWord;
            controlGate=typing.ManualPaused?"ManualPause":paused?"JammerWait":!focused?"FocusWait":"Typing";
            var interval=config.InputInterval(visuallyObstructed);
            typing.Tick(now,true,reader.Words.FirstOrDefault(),reader.StillVisible(typing.CurrentWord,currentEntity),
                interval.Min,interval.Max,c=>!stopping && !Loading && IsContent() && !UiBlocked && !encounter.Paused(Environment.TickCount64)
                    && reader.FocusedNow() && (c=='\b' || reader.StillVisible(typing.CurrentWord,currentEntity)) && (config.DirectInput?reader.SendDirect(c):(c==TypingSession.LatinModeKey?ime.Prepare(false):Keyboard.Send(c,typing.ExactCase))),
                paused:paused || !focused,composing:imeState.Composing,evaluated:reader.EvaluatedText(),betweenWordsMs:config.WaitMin,directInput:config.DirectInput,betweenWordsMaximumMs:config.WaitMax,onSubmitted:()=>activity.OnSubmission(Environment.TickCount64,ContentActivity.Touch));
            if(previousWord.Length==0 && typing.CurrentWord.Length>0)currentEntity=reader.Source(typing.CurrentWord);
            failures=0;
        }
        catch(Exception ex)
        {
            controlGate="Exception";if(loop.Active)loop.Stop(true);loopUi.CancelQueuePreparation();typing.Stop("Fault");open=true;
            if(++failures<=3)log.Error(ex,"Keybound read stopped; restart is manual");
        }
        finally {if(!typing.Running){ime.Restore();if(loop.Active && loop.Phase==LoopPhase.Playing && typing.Status!="Completed")loop.Stop(true);}}
    }
    private void StartLoop()
    {
        if(!loopUi.Supported || !client.IsLoggedIn || client.TerritoryType is not (LoopGameUi.LobbyTerritory or LoopGameUi.GameTerritory))return;
        var now=Environment.TickCount64;
        StopByUser();loop.Start(now,config.LoopCount,IsContent());loopSample=0;
        if(IsContent())StartTyping(now,"loop_current_game");
    }
    private void UpdateLoop(long now)
    {
        if(!loop.Active){loopUi.CancelQueuePreparation();return;}
        if(Keyboard.ForegroundGame && Keyboard.Escape){StopByUser();return;}
        if(now-loopSample<200)return;loopSample=now;
        var loading=condition[ConditionFlag.BetweenAreas] || condition[ConditionFlag.BetweenAreas51];
        if(loop.Paused || loading || !InputAllowed || ModifierPressed || UiBlocked)loopUi.SuspendTravel();
        var snapshot=loopUi.Read(client.IsLoggedIn,client.TerritoryType,loading);
        var blocked=!InputAllowed || ModifierPressed || UiBlocked;
        if(loop.Phase==LoopPhase.Waiting && !loop.Paused && !blocked && !loading && snapshot.LoggedIn && !snapshot.OtherQueue && !snapshot.InGame && !snapshot.KeyboardWarning
            && !loopUi.AdvanceQueue(now,client.TerritoryType))loop.Stop(true);
        var action=loop.Tick(now,snapshot,blocked);
        if(action==LoopAction.StartTyping)StartTyping(now,"loop_entry");
        else if(action!=LoopAction.None && !loopUi.Execute(action,client.TerritoryType))loop.Stop(true);
        if(loop.Phase is LoopPhase.Error or LoopPhase.Complete){loopUi.CancelQueuePreparation();automation.Suppress();typing.Stop(loop.Phase==LoopPhase.Complete?"Completed":"Fault");Keyboard.Release(true);}
    }
    private void StartDiagnostics()
    {
        SetDiagnostics(true);diagnostics.Stop();diagnosticReader.Reset();
        diagnosticAttempted=IsContent();
        if(diagnosticAttempted)diagnostics.Start();
    }
    private void SetDiagnostics(bool enabled)
    {
        if(config.RecordDiagnostics!=enabled){config.RecordDiagnostics=enabled;settings.Update(config);}
        diagnosticAttempted=false;diagnosticReader.Reset();
        if(!enabled)diagnostics.Stop();
    }
    private void UpdateDiagnostics()
    {
        if(!config.RecordDiagnostics)return;
        if(!IsContent()) { diagnostics.Stop();diagnosticAttempted=false;return; }
        if(!diagnosticAttempted) { diagnosticAttempted=true;diagnosticReader.Reset();diagnostics.Start(); }
        var now=Environment.TickCount64;
        if(diagnostics.Active && now-lastDiagnosticSample>=100)
        {
            lastDiagnosticSample=now;
            var sample=diagnosticReader.Capture(typing,ime.Inspect(),encounter.Paused(now),
                new {gate=controlGate,activityFailed=activity.Failed,loopPhase=loop.Phase.ToString(),loop.Current,loop.Total,loop.Completed,currentEntity,betweenWordsMs=config.BetweenWordsMs,reader.FocusState,reader.FocusAttempts,reader.FocusRequestAccepted,uiCaptures,autoStart=config.AutoStart,autoConsumed=automation.Consumed});
            if(sample!=null)diagnostics.Observe(sample);
        }
        diagnostics.Tick();
    }
    private void DrawDiagnostics()
    {
        ImGui.TextWrapped(T("DiagHelp"));
        ImGui.TextUnformatted(T(config.RecordDiagnostics?diagnostics.State:"DiagDisabled"));
        ImGui.TextWrapped(diagnostics.DirectoryPath);
        if(ImGui.Button(T("DiagStart")))StartDiagnostics();
        ImGui.SameLine();
        if(ImGui.Button(T("DiagStop"))) {SetDiagnostics(false);}
        if(ImGui.Button(T("DiagCopy")))ImGui.SetClipboardText(diagnostics.DirectoryPath);
        if(ImGui.Button(T("DiagBlock")))diagnostics.Mark("block");
        ImGui.SameLine();
        if(ImGui.Button(T("DiagCast")))diagnostics.Mark("cast");
        ImGui.TextUnformatted(T("DiagRecords")+": "+diagnostics.Records+" / "+diagnostics.BytesWritten+" B");
    }
    private void Draw()
    {
        uiCaptures = ImGui.GetIO().WantTextInput || ImGui.IsAnyItemActive();
        if (stopping || !open) return;
        fonts.Ensure(reader.Words.Append(typing.CurrentWord));
        using var font = fonts.Push(config.Language);
        ImGui.SetNextWindowSize(new Vector2(650, 440), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Keybound Pilot###KeyboundPilot", ref open))
        {
            if(!fonts.Available)
            {
                ImGui.TextWrapped(Loc.Get("en",fonts.Failed?"FontFailed":"FontLoading"));
                ImGui.End();return;
            }
            if (ImGui.BeginTabBar("Tabs"))
            {
                if (ImGui.BeginTabItem(T("Main") + "###Main"))
                {
                    ImGui.TextUnformatted(T("Status") + ": " + T(typing.Status));
                    if(activity.Failed)ImGui.TextWrapped(T("ActivityUnavailable"));
                    if(activity.Requested && !typing.Running)ImGui.TextWrapped(T("ActivityHeld"));
                    if(typing.Running && !typing.ManualPaused && typing.CurrentWord.Length>0)
                        ImGui.TextWrapped(T("Current") + ": " + typing.CurrentWord);
                    PlaybackControls.Draw(typing.Running||loop.Active||activity.Requested,loop.Active?loop.Paused:typing.ManualPaused || (activity.Requested && !typing.Running),T,
                        ()=>{if(loop.Active){activity.Begin();loop.Resume();typing.ResumeByUser();reader.ResetFocus();}else StartTyping(Environment.TickCount64,"button");},
                        ()=>{activity.End();loop.Pause();typing.PauseByUser();controlGate="ManualPause";Keyboard.Release(true);},StopByUser);
                    ImGui.SameLine();
                    PlaybackControls.Help(T("Guide")+"\n\n"+T("LoopHelp")+"\n\n"+T("ReceptionHelp"));
                    ImGui.Spacing();ImGui.Separator();ImGui.Spacing();
                    ImGui.TextWrapped(T("LoopStatus")+": "+(loop.Total>0
                        ?string.Format(T("LoopProgress"),loop.Current,loop.Total,loop.Completed)+" / "+T(loop.Paused?"UserPaused":loop.Phase==LoopPhase.Complete && loop.StopAfterCurrentRequested?"LoopStopped":"Loop"+loop.Phase)
                        :T("LoopIdle")));
                    if(loop.Active && loop.StopAfterCurrentRequested)ImGui.TextWrapped(T("StopAfterCurrentPending"));
                    ImGui.BeginDisabled(loop.Active);
                    var count=config.LoopCount;ImGui.SetNextItemWidth(Math.Min(400,ImGui.GetContentRegionAvail().X));
                    if(PlaybackControls.Number("###LoopCount",ref count,1,9999,1,T("NumberHelp")))
                    {config.LoopCount=count;settings.Update(config);}
                    ImGui.EndDisabled();
                    PlaybackControls.DrawLoop(!loop.Active && loopUi.Supported && client.IsLoggedIn && client.TerritoryType is LoopGameUi.LobbyTerritory or LoopGameUi.GameTerritory,T("LoopButton"),StartLoop);
                    PlaybackControls.DrawStopAfterCurrent(loop.Active && !loop.StopAfterCurrentRequested,T,loop.RequestStopAfterCurrent);
                    if(config.ShowDebug)
                    {
                        ImGui.Spacing();ImGui.Separator();ImGui.Spacing();
                        ImGui.TextUnformatted(T("Words")+": "+reader.Words.Count);
                        ImGui.TextUnformatted(T("DetectedList"));
                        if(ImGui.BeginChild("DetectedWords",new Vector2(0,120),true))
                            foreach(var word in reader.Words)ImGui.TextUnformatted(word);
                        ImGui.EndChild();
                        ImGui.TextUnformatted(T("InputVisible")+": "+T(reader.InputVisible?"Yes":"No"));
                        ImGui.TextUnformatted(T("InputFocused")+": "+T(reader.InputFocused?"Yes":"No"));
                        ImGui.TextUnformatted(T("Sent")+": "+typing.Sent);
                        ImGui.TextWrapped(activitySnapshot.Available?string.Format(T("ActivityTimers"),activitySnapshot.Content,activitySnapshot.Input,activitySnapshot.Afk,activitySnapshot.Limit):T("ActivityNoData"));
                        if(activitySnapshot.Available)ImGui.TextWrapped(string.Format(T("ActivityDevices"),activitySnapshot.Controller,activitySnapshot.Mouse));
                        ImGui.TextWrapped(activitySnapshot.Ktg.Available?string.Format(T("KtgActivity"),activitySnapshot.Ktg.IdleSeconds,activitySnapshot.Ktg.SameKeySeconds,ContentActivity.LastAttempt==0?"—":T(ContentActivity.DedicatedSucceeded?"Yes":"No")):T("KtgActivityUnavailable"));
                        ImGui.TextWrapped(string.Format(T("ActivityReset"),ContentActivity.LastAttempt==0?"—":T(ContentActivity.LastSucceeded?"Yes":"No"),ContentActivity.ResetCount,ContentActivity.LastAttempt==0?"—":((Environment.TickCount64-ContentActivity.LastAttempt)/1000d).ToString("F1"),activitySnapshot.Territory,activitySnapshot.Status));
                    }
                    ImGui.EndTabItem();
                }
                StandardPluginTabs.DrawStandardTrailingTabs(T("Settings"), "Settings", DrawSettings,
                    T("Language"), DrawLanguage, T("Support"), () =>
                    StandardPluginTabs.DrawSupportContent(T("Support"), T("SupportText"), T("Recipient"), T("Donate")));
                ImGui.EndTabBar();
            }
        }
        ImGui.End();
    }
    private void DrawSettings()
    {
        SettingsPanel.Draw(config,T,()=>settings.Update(config),StopByUser);
        if (settings.Recovered) ImGui.TextWrapped(T("Recovered"));
        if (settings.LoadWarning != null || settings.LastError != null || settings.Stopped)
        {
            ImGui.TextWrapped(T("Persistence"));
            if (ImGui.Button(T("Retry"))) settings.Retry();
        }
    }
    private void DrawLanguage()
    {
        var code = StandardPluginTabs.DrawLanguageSelector(T("Language"), T("LanguageHelp"), config.Language ?? "en");
        if (code == null) return;
        config.Language = code; settings.Update(config); command.HelpMessage = T("CommandHelp");
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true; Stop();
        framework.Update -= Update; pi.UiBuilder.Draw -= Draw;
        pi.UiBuilder.OpenMainUi -= Open; pi.UiBuilder.OpenConfigUi -= Open;
        client.Logout -= Logout; commands.RemoveHandler(Command);
        shutdown.Dispose(); unload.Dispose(); settings.Dispose(); fonts.Dispose(); reader.Clear();
    }
}

