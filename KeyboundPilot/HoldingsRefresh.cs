namespace KeyboundPilot;

// Coalesce inventory notifications; unchanged frames never scan inventories or storage.
public sealed class HoldingsRefresh(IEnumerable<uint> items)
{
    private readonly HashSet<uint> tracked=items.ToHashSet();
    private bool pending=true;
    private long refreshed=-250;
    public bool Pending=>pending;
    public bool Tracks(uint container,uint item)=>tracked.Contains(item) &&
        (container<=3 || container==1000 || container is >=3200 and <=3209 || container is 3300 or 3400 or 3500);
    public void Invalidate()=>pending=true;
    public bool Due(long now,bool ready)=>pending && ready && now-refreshed>=250;
    public void Complete(long now){pending=false;refreshed=now;}
    public void Reset(){pending=true;refreshed=-250;}
}
