using System.Text.Json;

namespace KeyboundPilot;

/// <summary>One immutable file per result, date-partitioned by the recorded local UTC offset.</summary>
public sealed class ScoreHistory : IDisposable
{
    private readonly string directory;
    public sealed record SessionMarker(Guid Id,string Character,DateTimeOffset Started,ScoreRecord? Result);
    private sealed record Work(ScoreRecord? Record,SessionMarker? Marker,bool Clear=false);
    private SessionMarker? resume;
    private readonly object gate=new();
    private readonly Queue<Work> pending=new();
    private Task worker=Task.CompletedTask;
    private bool disposed,working;
    private int generation;
    private string? error;
    public string? Error=>Volatile.Read(ref error);
    public int Generation=>Volatile.Read(ref generation);
    public ScoreHistory(string directory)
    {
        this.directory=directory;
        var path=Path.Combine(directory,"session.state");
        try
        {
            if(File.Exists(path) && new FileInfo(path).Length<=65536)
                resume=JsonSerializer.Deserialize<SessionMarker>(File.ReadAllBytes(path));
            if(resume!=null && (resume.Id==Guid.Empty || string.IsNullOrWhiteSpace(resume.Character) || resume.Character.Length>128 ||
                resume.Result!=null && (!Valid(resume.Result) || resume.Result.Id!=resume.Id || resume.Result.Character!=resume.Character)))
                throw new JsonException("Invalid score session journal");
            if(resume is {Result:not null})Add(resume.Result);
        }
        catch(Exception ex) when(ex is IOException or UnauthorizedAccessException or JsonException){resume=null;error=ex.Message;}
    }
    public SessionMarker BeginSession(string character)
    {
        lock(gate)
        {
            var marker=resume is not null && resume.Id!=Guid.Empty && resume.Character==character
                ?resume:new SessionMarker(Guid.NewGuid(),character,DateTimeOffset.Now,null);
            resume=null;Enqueue(new(null,marker));return marker;
        }
    }
    public void EndSession(){lock(gate){resume=null;Enqueue(new(null,null,true));}}
    private void Enqueue(Work work)
    {
        if(disposed)throw new ObjectDisposedException(nameof(ScoreHistory));
        if(pending.Count>=32)throw new IOException("Score history queue is full; retry saving.");
        pending.Enqueue(work);StartWorker();
    }
    public void Add(ScoreRecord record)
    {
        lock(gate)
        {
            if(pending.Any(x=>x.Record?.Id==record.Id))return;
            Enqueue(new(record,null));
        }
    }
    public void Retry() {lock(gate){if(disposed)return;error=null;StartWorker();}}
    private void StartWorker()
    {
        if(working || Error!=null || pending.Count==0)return;
        working=true;worker=Task.Run(Drain);
    }
    private void Drain()
    {
        while(true)
        {
                Work work;
                lock(gate){if(!pending.TryPeek(out work!)){working=false;return;}}
                Exception? failure=null;
                for(var attempt=0;attempt<3;attempt++)
                {
                    try
                    {
                        if(work.Record is {} record)Write(record);
                        else if(work.Marker is {} marker)WriteMarker(marker);
                        else if(work.Clear)
                        {var path=Path.Combine(directory,"session.state");if(File.Exists(path))File.Delete(path);}
                        failure=null;break;
                    }
                    catch(Exception ex) when(ex is IOException or UnauthorizedAccessException or JsonException)
                    {failure=ex;if(attempt<2)Thread.Sleep(250);}
                }
                lock(gate)
                {
                    if(failure!=null){Volatile.Write(ref error,failure.Message);working=false;return;}
                    pending.Dequeue();if(work.Record!=null)Interlocked.Increment(ref generation);
                }
        }
    }
    private void Write(ScoreRecord record)
    {
        var date=record.Ended.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture);
        var folder=Path.Combine(directory,date);Directory.CreateDirectory(folder);
        var path=Path.Combine(folder,record.Ended.ToString("HHmmssfffffff")+"-"+record.Id.ToString("N")+".json");
        var bytes=JsonSerializer.SerializeToUtf8Bytes(record);
        if(File.Exists(path))
        {
            if(!File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes))throw new IOException("Existing score record differs.");
            return;
        }
        // Durable journal makes a reload during the result screen idempotent.
        WriteMarker(new(record.Id,record.Character,record.Ended,record));
        var temp=path+".tmp";
        using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None))
        {stream.Write(bytes);stream.Flush(true);}
        File.Move(temp,path,false);
    }
    private void WriteMarker(SessionMarker marker)
    {
        Directory.CreateDirectory(directory);
        var path=Path.Combine(directory,"session.state");var bytes=JsonSerializer.SerializeToUtf8Bytes(marker);
        if(File.Exists(path) && File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes))return;
        using(var stream=new FileStream(path+".tmp",FileMode.Create,FileAccess.Write,FileShare.None))
        {stream.Write(bytes);stream.Flush(true);}
        File.Move(path+".tmp",path,true);
    }
    public void Flush() {Task task;lock(gate)task=worker;task.GetAwaiter().GetResult();}
    public void Dispose() {lock(gate){if(disposed)return;disposed=true;}Flush();}
    public string[] Dates()=>Directory.Exists(directory)?Directory.EnumerateDirectories(directory)
        .Select(Path.GetFileName).Where(x=>DateOnly.TryParseExact(x,"yyyy-MM-dd",out _)).Cast<string>()
        .OrderDescending(StringComparer.Ordinal).ToArray():[];
    public (ScoreRecord[] Records,bool More) ReadPage(string date,int page)
    {
        if(!DateOnly.TryParseExact(date,"yyyy-MM-dd",out _) || page<0)throw new ArgumentException("Invalid history page");
        var folder=Path.Combine(directory,date);if(!Directory.Exists(folder))return([],false);
        var paths=Directory.EnumerateFiles(folder,"*.json").OrderDescending(StringComparer.Ordinal)
            .Skip(checked(page*50)).Take(51).ToArray();
        var records=new List<ScoreRecord>(50);
        foreach(var path in paths.Take(50))
        {
            if(new FileInfo(path).Length>65536)throw new IOException("Score record exceeds size limit");
            var record=JsonSerializer.Deserialize<ScoreRecord>(File.ReadAllBytes(path));
            if(!Valid(record))
                throw new JsonException("Invalid score record");
            records.Add(record!);
        }
        return(records.ToArray(),paths.Length>50);
    }
    private static bool Valid(ScoreRecord? record)=>record!=null && record.Id!=Guid.Empty && record.Players!=null && record.Players.Length<=4 &&
        !string.IsNullOrWhiteSpace(record.Character) && record.Character.Length<=128 && record.OwnScore is not <0 &&
        record.Players.All(x=>x!=null && x.Score>=0 && x.Slot is >=1 and <=4 && !string.IsNullOrWhiteSpace(x.Name) && x.Name.Length<=128);
}
