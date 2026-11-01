using System.Text.Json;
namespace KeyboundPilot;

/// <summary>Small character checkpoint and date-partitioned transaction receipts. No IO on unchanged UI frames.</summary>
public sealed class ExchangeLedger(string directory) : IDisposable
{
    public sealed record Purchase(Guid Id,uint Item,int CurrencyBefore,int OwnedBefore,int Cost,DateTimeOffset Created,int Quantity=1);
    public sealed record State(ulong Character,Dictionary<uint,int> Completed,Purchase? Pending);
    private sealed record Receipt(ulong Character,Purchase Purchase,bool Purchased);
    private Task<State>? work;
    private ulong character;
    private bool disposed;
    public State? Current {get;private set;}
    public string? Error {get;private set;}
    public bool Busy=>work!=null;
    public bool Ready=>!Busy && Error==null && Current!=null;
    public int Completed(uint item)=>Current?.Completed.GetValueOrDefault(item)??0;
    public long Writes {get;private set;}
    public void Select(ulong id)
    {
        Poll();if(id==character)return;
        Flush();character=id;Current=null;Error=null;
        if(id==0 || disposed)return;
        work=Task.Run(()=>Load(id));
    }
    public void Poll()
    {
        if(work is not {IsCompleted:true})return;
        try {Current=work.GetAwaiter().GetResult();}catch(Exception ex){Error=ex.Message;}
        finally{work=null;}
    }
    public bool Reserve(uint item,int currency,int owned,int cost,int quantity=1)
    {
        Poll();if(disposed || !Ready || Current!.Pending!=null || item==0 || cost<=0 || quantity is <1 or >9999 || (long)cost*quantity>currency || owned<0)return false;
        var state=Current with {Pending=new(Guid.NewGuid(),item,currency,owned,checked(cost*quantity),DateTimeOffset.Now,quantity)};
        work=Task.Run(()=>{Save(state);return state;});return true;
    }
    public bool Resolve(bool purchased)
    {
        Poll();if(disposed || !Ready || Current!.Pending is not {} purchase)return false;
        var original=Current;
        work=Task.Run(()=>
        {
            var receipt=new Receipt(original.Character,purchase,purchased);
            var path=ReceiptPath(purchase);Write(path,JsonSerializer.SerializeToUtf8Bytes(receipt),false);
            var state=Apply(original,purchased);Save(state);return state;
        });return true;
    }
    private State Apply(State state,bool purchased)
    {
        var counts=new Dictionary<uint,int>(state.Completed);
        if(purchased && state.Pending is {} pending)counts[pending.Item]=checked(counts.GetValueOrDefault(pending.Item)+pending.Quantity);
        return state with {Completed=counts,Pending=null};
    }
    private State Load(ulong id)
    {
        var path=StatePath(id);
        if(!File.Exists(path))return new(id,new(),null);
        var state=Read<State>(path);
        if(state.Character!=id || state.Completed==null || state.Completed.Count>64 || state.Completed.Any(x=>x.Key==0 || x.Value<0)
            || state.Pending is {} p && (p.Id==Guid.Empty || p.Item==0 || p.Cost<=0 || p.Quantity is <1 or >9999 || p.CurrencyBefore<p.Cost || p.OwnedBefore<0))
            throw new InvalidDataException("Invalid exchange checkpoint");
        if(state.Pending is {} pending && File.Exists(ReceiptPath(pending)))
        {
            var receipt=Read<Receipt>(ReceiptPath(pending));
            if(receipt.Character!=id || receipt.Purchase!=pending)throw new InvalidDataException("Exchange receipt mismatch");
            state=Apply(state,receipt.Purchased);Save(state);
        }
        return state;
    }
    private static T Read<T>(string path)
    {
        if(new FileInfo(path).Length>32768)throw new InvalidDataException("Exchange record too large");
        return JsonSerializer.Deserialize<T>(File.ReadAllBytes(path))??throw new InvalidDataException("Invalid exchange record");
    }
    private string StatePath(ulong id)=>Path.Combine(directory,id.ToString("X16"),"progress.json");
    private string ReceiptPath(Purchase purchase)=>Path.Combine(directory,character.ToString("X16"),purchase.Created.ToString("yyyy-MM-dd"),purchase.Id.ToString("N")+".json");
    private void Save(State state)=>Write(StatePath(state.Character),JsonSerializer.SerializeToUtf8Bytes(state),true);
    private void Write(string path,byte[] bytes,bool replace)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        // Transaction operations are idempotent locally; never retry a game purchase.
        for(var attempt=0;;attempt++)
        {
            var temp=path+".tmp";
            try
            {
                if(!replace && File.Exists(path))
                {if(!File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes))throw new InvalidDataException("Receipt conflict");return;}
                using(var file=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){file.Write(bytes);file.Flush(true);}
                File.Move(temp,path,true);Writes++;return;
            }
            catch(IOException) when(attempt<2){Thread.Sleep(100);}
            finally {if(File.Exists(temp))File.Delete(temp);}
        }
    }
    public void Flush(){if(work==null)return;try{work.GetAwaiter().GetResult();}catch{ }Poll();}
    public void Dispose(){if(disposed)return;disposed=true;Flush();}
}
