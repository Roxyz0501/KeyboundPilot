using System.Text;
using System.Text.Json;
namespace KeyboundPilot;

/// <summary>Changed-only, JST date-partitioned, bounded local diagnostics. No automatic replay of uncertain writes.</summary>
public sealed class DiagnosticRecorder : IDisposable
{
    public const long SessionLimit = 64 * 1024 * 1024;
    public const long TotalLimit = 256 * 1024 * 1024;
    public const int BufferLimit = 64 * 1024;
    private readonly object gate = new();
    private readonly string root;
    private readonly TimeProvider clock;
    private readonly Action<string, byte[]> append;
    private readonly MemoryStream buffer = new();
    private string? previous;
    private string session = "";
    private string? bufferedDate;
    private long started, flushed, totalAtStart, sequence;
    private bool disposed;
    public bool Active { get; private set; }
    public string State { get; private set; } = "DiagIdle";
    public long BytesWritten { get; private set; }
    public long Records { get; private set; }
    public string DirectoryPath => root;
    public DiagnosticRecorder(string root, TimeProvider? clock = null, Action<string, byte[]>? append = null)
    {
        this.root = Path.GetFullPath(root); this.clock = clock ?? TimeProvider.System;
        this.append = append ?? Append;
    }
    public bool Start()
    {
        lock(gate)
        {
            if(disposed || Active) return Active;
            try
            {
                var files = Directory.Exists(root) ? Directory.EnumerateFiles(root,"*.jsonl",SearchOption.AllDirectories).Take(1001).ToArray() : [];
                if(files.Length > 1000) { State="DiagLimit";return false; }
                totalAtStart=files.Sum(x=>new FileInfo(x).Length);
                if(totalAtStart >= TotalLimit) { State="DiagLimit"; return false; }
                session=Guid.NewGuid().ToString("N"); started=flushed=clock.GetTimestamp();
                BytesWritten=Records=sequence=0;previous=null;buffer.SetLength(0);bufferedDate=null;
                Active=true;State="DiagRecording";
                Add("start",new { schema=1, plugin="KeyboundPilot",version="1.0.8.0",timeZone="Asia/Tokyo", samplingMs=100,
                    numbers="KTG number array 161: count then 20 records of 13 integers; meanings are provisional", strings="KTG string array 128: 20 triples, game words/reading/cast names",privacy="No chat, player names, raw keys or input text" });
                return Active;
            }
            catch { State="DiagError";Active=false;return false; }
        }
    }
    public void Observe(string snapshot)
    {
        lock(gate)
        {
            if(!Active || disposed) return;
            if(clock.GetElapsedTime(started)>=TimeSpan.FromMinutes(120)) { StopCore("DiagLimit");return; }
            if(snapshot!=previous) { previous=snapshot; Add("snapshot",JsonSerializer.Deserialize<JsonElement>(snapshot)); }
            if(Active && clock.GetElapsedTime(flushed)>=TimeSpan.FromSeconds(2)) FlushCore();
        }
    }
    public void Event(string name,object details) {lock(gate) {if(Active)Add("event",new {name,details});}}
    public void Mark(string kind)
    {
        lock(gate) { if(Active && kind is "block" or "cast") { Add("marker",new {kind}); FlushCore(); } }
    }
    public void Stop() { lock(gate) { if(Active) StopCore("DiagIdle"); } }
    public void Tick() { lock(gate) { if(Active && clock.GetElapsedTime(started)>=TimeSpan.FromMinutes(120))StopCore("DiagLimit");else if(Active && clock.GetElapsedTime(flushed)>=TimeSpan.FromSeconds(2)) FlushCore(); } }
    private void Add(string kind, object value)
    {
        if(!Active) return;
        var utc=clock.GetUtcNow();var day=utc.ToOffset(TimeSpan.FromHours(9)).ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture);
        var bytes=JsonSerializer.SerializeToUtf8Bytes(new { schema=1,session,sequence=++sequence,utc=utc.ToString("O"),elapsedMs=clock.GetElapsedTime(started).TotalMilliseconds,kind,data=value });
        if(bytes.Length>32*1024) { StopCore("DiagLimit");return; }
        if(bufferedDate!=null && bufferedDate!=day) FlushCore();
        if(!Active) return;
        if(buffer.Length+bytes.Length+1>BufferLimit) FlushCore();
        if(!Active) return;
        if(BytesWritten+buffer.Length+bytes.Length+1>SessionLimit || totalAtStart+BytesWritten+buffer.Length+bytes.Length+1>TotalLimit)
        { StopCore("DiagLimit");return; }
        bufferedDate=day;buffer.Write(bytes);buffer.WriteByte(10);Records++;
    }
    private void StopCore(string state) { FlushCore();Active=false;if(State!="DiagError")State=state;previous=null; }
    private void FlushCore()
    {
        if(buffer.Length==0) {flushed=clock.GetTimestamp();return;}
        try
        {
            var bytes=buffer.ToArray();
            append(Path.Combine(root,bufferedDate!,"ktg-"+session+".jsonl"),bytes);
            BytesWritten+=bytes.Length;buffer.SetLength(0);flushed=clock.GetTimestamp();
        }
        catch
        {
            // An append may have partially succeeded. Do not retry/replay it into the same file.
            Active=false;State="DiagError";buffer.SetLength(0);previous=null;
        }
    }
    private static void Append(string path, byte[] bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var stream=new FileStream(path,FileMode.Append,FileAccess.Write,FileShare.Read);
        stream.Write(bytes);stream.Flush(true);
    }
    public void Dispose() {lock(gate) {if(disposed)return;if(Active)StopCore("DiagIdle");disposed=true;buffer.Dispose();}}
}
