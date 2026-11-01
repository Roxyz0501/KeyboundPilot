using System.Globalization;
namespace KeyboundPilot;

public sealed record ExchangePlan(bool Enabled=false,int Threshold=9000,string Targets="",string Order="",bool HideZero=false)
{
    [System.Text.Json.Serialization.JsonIgnore] public bool Valid => Threshold is >=1 and <=10000 && Parse(Targets)!=null && ParseOrder(Order)!=null;
    private static uint[]? ParseOrder(string? value)
    {
        if(value==null || value.Length>1024)return null;
        if(value.Length==0)return [];
        var ids=new List<uint>();
        foreach(var part in value.Split(','))
        {if(!uint.TryParse(part,NumberStyles.None,CultureInfo.InvariantCulture,out var id) || id==0 || ids.Contains(id) || ids.Count>=64)return null;ids.Add(id);}
        return ids.ToArray();
    }
    public ExchangeItem[] Ordered(IEnumerable<ExchangeItem> items)
    {
        var order=ParseOrder(Order)??[];
        return items.OrderBy(i=>{var index=Array.IndexOf(order,i.Id);return index>=0?index:int.MaxValue;})
            .ThenBy(i=>i.Learnable?0:i.Unique?1:2).ToArray();
    }
    public ExchangePlan Move(IReadOnlyList<ExchangeItem> items,uint id,int direction,uint? neighbor=null)
    {
        var ordered=Ordered(items);var index=Array.FindIndex(ordered,i=>i.Id==id);
        var target=neighbor.HasValue?Array.FindIndex(ordered,i=>i.Id==neighbor):index+Math.Sign(direction);
        if(index<0 || target<0 || target>=ordered.Length || direction==0)return this;
        (ordered[index],ordered[target])=(ordered[target],ordered[index]);
        return this with {Order=string.Join(',',ordered.Select(i=>i.Id))};
    }
    public bool Shows(ExchangeItem item)=>!HideZero || Quantity(item.Id,item.Learnable)>0;
    private static SortedDictionary<uint,int>? Parse(string? text)
    {
        if(text==null || text.Length>2048)return null;
        var result=new SortedDictionary<uint,int>();
        if(text.Length==0)return result;
        foreach(var pair in text.Split(';'))
        {
            var parts=pair.Split('=');
            if(parts.Length!=2 || !uint.TryParse(parts[0],NumberStyles.None,CultureInfo.InvariantCulture,out var id) || id==0
                || !int.TryParse(parts[1],NumberStyles.None,CultureInfo.InvariantCulture,out var quantity) || quantity is <0 or >9999
                || !result.TryAdd(id,quantity) || result.Count>64)return null;
        }
        return result;
    }
    public int Quantity(uint id,bool learnable)
    {
        // UI lookup stays allocation-free; canonicalization occurs only on an actual edit.
        var remaining=Targets.AsSpan();
        while(!remaining.IsEmpty)
        {
            var end=remaining.IndexOf(';');var pair=end<0?remaining:remaining[..end];var equals=pair.IndexOf('=');
            if(equals>0 && uint.TryParse(pair[..equals],out var key) && key==id && int.TryParse(pair[(equals+1)..],out var value))
                return Math.Clamp(value,0,learnable?1:9999);
            if(end<0)break;remaining=remaining[(end+1)..];
        }
        return 0;
    }
    public ExchangePlan SetQuantity(uint id,int quantity,bool learnable)
    {
        var values=Parse(Targets)??new();var amount=Math.Clamp(quantity,0,learnable?1:9999);
        if(amount==0)values.Remove(id);else values[id]=amount;
        return this with {Targets=string.Join(';',values.Select(x=>$"{x.Key}={x.Value}"))};
    }
}

public enum ExchangeKnowledge { NotApplicable, Unlearned, Learned, Unknown }
public sealed record ExchangeItem(uint Id,string Name,int Cost,bool Learnable,bool Unique,uint StackSize);
public readonly record struct ExchangeObservation(ExchangeKnowledge Knowledge,int Owned,bool Space,int Capacity=int.MaxValue);
public static class ExchangePolicy
{
    public static int BatchSize(ExchangeItem? item,ExchangePlan plan,int currency,int completed,ExchangeObservation state)
    {
        if(item==null || item.Cost<=0 || !Eligible(item,plan.Quantity(item.Id,item.Learnable),completed,state))return 0;
        var remaining=plan.Quantity(item.Id,item.Learnable)-completed;
        var capacity=Math.Min(state.Capacity,item.Learnable || item.Unique?1:(int)Math.Min(item.StackSize,9999));
        return Math.Max(0,Math.Min(Math.Min(remaining,currency/item.Cost),capacity));
    }
    public static bool Eligible(ExchangeItem item,int requested,int completed,ExchangeObservation state)
        => requested>completed && requested>0 && state.Space && state.Capacity>0 && state.Owned>=0
           && (!item.Unique || state.Owned==0)
           && (!item.Learnable || state.Knowledge==ExchangeKnowledge.Unlearned && state.Owned==0);
    public static ExchangeItem? Next(IEnumerable<ExchangeItem> items,ExchangePlan plan,int currency,
        Func<uint,int> completed,Func<uint,ExchangeObservation> observe)
        => plan.Ordered(items).FirstOrDefault(i=>i.Cost>0 && i.Cost<=currency && Eligible(i,plan.Quantity(i.Id,i.Learnable),completed(i.Id),observe(i.Id)));
    public static bool ShouldBegin(bool requesting,bool queued,bool loading,bool stopRequested,ExchangePlan plan,int? currency,ExchangeItem? next)
        => requesting && !queued && !loading && !stopRequested && plan.Enabled && currency>=plan.Threshold && next!=null;
}
