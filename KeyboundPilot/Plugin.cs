using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Command;
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
    private readonly DiagnosticRecorder diagnostics;
    private readonly DiagnosticReader diagnosticReader;
    private bool diagnosticRequested=true, diagnosticAttempted;
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

    public Plugin(IDalamudPluginInterface pi, ICommandManager commands, IFramework framework,
        IClientState client, IDataManager data, IGameGui gui, IPluginLog log, IGameLifecycle lifecycle)
    {
        this.pi = pi; this.commands = commands; this.framework = framework;
        this.client = client; this.data = data; this.log = log;
        Loc.CheckCompleteness();
        settings = new ConfigurationStore(pi.ConfigFile.FullName, ex => { log.Error(ex, "Configuration saving stopped"); open = true; },
            onFinalFailure: ex => { log.Error(ex, "Final configuration flush failed"); open = true; });
        config = settings.Configuration;
        if (!Loc.IsSupported(config.Language))
        {
            config.Language = Loc.FromGame(client.ClientLanguage) ?? Loc.Normalize(pi.UiLanguage) ?? "en";
            settings.Update(config);
        }
        fonts = new LocalizedFonts(pi.UiBuilder.FontAtlas, []);
        reader = new GameReader(gui);
        diagnostics = new DiagnosticRecorder(Path.Combine(pi.GetPluginConfigDirectory(),"diagnostics"));
        diagnosticReader = new DiagnosticReader(gui);
        open = settings.Recovered || settings.LoadWarning != null;
        command = new CommandInfo((_, args) =>
        {
            if(args.Trim().Equals("log start",StringComparison.OrdinalIgnoreCase)) { StartDiagnostics(); return; }
            if(args.Trim().Equals("log stop",StringComparison.OrdinalIgnoreCase)) { diagnosticRequested=false;diagnostics.Stop();return; }
            if(args.Trim().Equals("mark block",StringComparison.OrdinalIgnoreCase)) { diagnostics.Mark("block");return; }
            if(args.Trim().Equals("mark cast",StringComparison.OrdinalIgnoreCase)) { diagnostics.Mark("cast");return; }
            if (args.Trim().Equals("stop", StringComparison.OrdinalIgnoreCase)) { typing.Stop(); Keyboard.Release(true); }
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
    private string T(string key) => Loc.Get(config.Language, key);
    private void Open() => open = true;
    private void Logout(int type, int code) { lock (executionGate) { typing.Stop(); Keyboard.Release(true); reader.Clear(); diagnostics.Stop();diagnosticReader.Reset();diagnosticAttempted=false;settings.Flush(); } }
    private void Stop() { lock (executionGate) { stopping = true; typing.Stop(); Keyboard.Release(true);diagnostics.Dispose(); settings.Dispose(); } }
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
    private void UpdateCore()
    {
        if (stopping) return;
        settings.Tick();
        try { UpdateDiagnostics(); } catch(Exception ex) { diagnostics.Stop();diagnosticAttempted=true;log.Error(ex,"KTG diagnostic sampling stopped"); }
        if (!Keyboard.Release()) { typing.Stop("SendFailed"); return; }
        if (!typing.Running) return;
        try
        {
            var now = Environment.TickCount64;
            if (Keyboard.Escape || Keyboard.Modifier || !Keyboard.ForegroundGame) { typing.Stop("FocusLost"); Keyboard.Release(true); return; }
            if (now - scanned >= 50)
            {
                reader.Refresh(); scanned = now;
            }
            var ready = IsContent() && reader.InputFocused && !uiCaptures;
            // Recheck foreground/focus at the actual send boundary; cached word lists are not sufficient.
            typing.Tick(now, ready, reader.Words.FirstOrDefault(), reader.StillVisible(typing.CurrentWord),
                config.MinimumMs, config.MaximumMs, c => !stopping && IsContent() && !uiCaptures && reader.FocusedNow() && reader.StillVisible(typing.CurrentWord) && Keyboard.Send(c));
            failures = 0;
        }
        catch (Exception ex)
        {
            typing.Stop("Fault"); open = true;
            if (++failures <= 3) log.Error(ex, "Keybound read stopped; restart is manual");
        }
    }
    private void StartDiagnostics()
    {
        diagnostics.Stop();diagnosticReader.Reset();diagnosticRequested=true;
        diagnosticAttempted=IsContent();
        if(diagnosticAttempted)diagnostics.Start();
    }
    private void UpdateDiagnostics()
    {
        if(!IsContent()) { diagnostics.Stop();diagnosticAttempted=false;return; }
        if(!diagnosticRequested)return;
        if(!diagnosticAttempted) { diagnosticAttempted=true;diagnosticReader.Reset();diagnostics.Start(); }
        var now=Environment.TickCount64;
        if(diagnostics.Active && now-lastDiagnosticSample>=100)
        {
            lastDiagnosticSample=now;
            var sample=diagnosticReader.Capture(typing);
            if(sample!=null)diagnostics.Observe(sample);
        }
        diagnostics.Tick();
    }
    private void DrawDiagnostics()
    {
        ImGui.TextWrapped(T("DiagHelp"));
        ImGui.TextUnformatted(T(diagnostics.State));
        ImGui.TextWrapped(diagnostics.DirectoryPath);
        if(ImGui.Button(T("DiagStart")))StartDiagnostics();
        ImGui.SameLine();
        if(ImGui.Button(T("DiagStop"))) {diagnosticRequested=false;diagnostics.Stop();}
        if(ImGui.Button(T("DiagCopy")))ImGui.SetClipboardText(diagnostics.DirectoryPath);
        if(ImGui.Button(T("DiagBlock")))diagnostics.Mark("block");
        ImGui.SameLine();
        if(ImGui.Button(T("DiagCast")))diagnostics.Mark("cast");
        ImGui.TextUnformatted(T("DiagRecords")+": "+diagnostics.Records+" / "+diagnostics.BytesWritten+" B");
    }
    private void Draw()
    {
        uiCaptures = ImGui.GetIO().WantCaptureKeyboard || ImGui.GetIO().WantTextInput;
        if (stopping || !open) return;
        using var font = fonts.Push(config.Language);
        ImGui.SetNextWindowSize(new Vector2(650, 440), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Keybound Pilot###KeyboundPilot", ref open))
        {
            ImGui.TextWrapped(T("Preview"));
            if (ImGui.BeginTabBar("Tabs"))
            {
                if (ImGui.BeginTabItem(T("Main") + "###Main"))
                {
                    ImGui.TextUnformatted(T("Status") + ": " + T(typing.Status));
                    ImGui.TextUnformatted(T("Words") + ": " + reader.Words.Count);
                    ImGui.TextUnformatted(T("InputVisible") + ": " + T(reader.InputVisible ? "Yes" : "No"));
                    ImGui.TextUnformatted(T("InputFocused") + ": " + T(reader.InputFocused ? "Yes" : "No"));
                    ImGui.TextUnformatted(T("Sent") + ": " + typing.Sent);
                    ImGui.TextWrapped(T("Current") + ": " + typing.CurrentWord);
                    if (typing.Running)
                    {
                        if (ImGui.Button(T("Stop") + "###Stop")) { typing.Stop(); Keyboard.Release(true); }
                    }
                    else if (ImGui.Button(T("Start") + "###Start"))
                    {
                        if (failures < 3) { scanned = 0; reader.Clear(); typing.Start(Environment.TickCount64); }
                        else typing.Stop("Fault");
                    }
                    ImGui.SameLine();
                    if (ImGui.Button(T("Refresh") + "###Inspect"))
                    {
                        typing.Stop(); Keyboard.Release(true);
                        try { reader.Refresh(); } catch (Exception ex) { typing.Stop("Fault"); log.Error(ex, "Manual word inspection failed"); }
                    }
                    ImGui.TextWrapped(T("Guide"));
                    foreach (var word in reader.Words) ImGui.TextUnformatted(word);
                    ImGui.EndTabItem();
                }
                if(ImGui.BeginTabItem(T("Diagnostics")+"###Diagnostics")) {DrawDiagnostics();ImGui.EndTabItem();}
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
        var min = config.MinimumMs; var max = config.MaximumMs;
        ImGui.SetNextItemWidth(200);
        var changed = ImGui.InputInt(T("Min") + "###Minimum", ref min, 5, 20);
        if (changed) { min = Math.Clamp(min, 20, 5000); max = Math.Max(min, max); }
        ImGui.SetNextItemWidth(200);
        var maxChanged = ImGui.InputInt(T("Max") + "###Maximum", ref max, 5, 20);
        if (maxChanged) { max = Math.Clamp(max, 20, 5000); min = Math.Min(min, max); }
        if (changed || maxChanged) { config.SetRange(min, max); settings.Update(config); typing.Stop(); }
        ImGui.TextWrapped(T("RangeHelp"));
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

