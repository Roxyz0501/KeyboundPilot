using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
namespace KeyboundPilot;

// Storage is cached only for the current login. Never interpret unloaded storage as empty.
internal sealed unsafe class ExchangeHoldings
{
    private readonly Dictionary<uint,uint> cabinet;
    private readonly HashSet<uint> equipment;
    private readonly Dictionary<uint,uint[]> sets;
    private Dictionary<uint,int>? dresser,armoire;
    private ulong character;
    private readonly Dictionary<uint,HoldingsCount> totals=new();
    public static bool InventoryReady
    {
        get
        {
            var inventory=InventoryManager.Instance();
            if(inventory==null || UIState.Instance()==null)return false;
            for(var i=0;i<4;i++)
            {
                var bag=inventory->GetInventoryContainer((InventoryType)i);
                if(bag==null || !bag->IsLoaded)return false;
            }
            return true;
        }
    }
    public ExchangeHoldings(IDataManager data,IReadOnlyList<ExchangeItem> items)
    {
        var ids=items.Select(i=>i.Id).ToHashSet();
        cabinet=data.GetExcelSheet<Lumina.Excel.Sheets.Cabinet>().Where(r=>ids.Contains(r.Item.RowId)).ToDictionary(r=>r.Item.RowId,r=>r.RowId);
        equipment=items.Where(i=>data.GetExcelSheet<Item>().GetRow(i.Id).EquipSlotCategory.RowId!=0).Select(i=>i.Id).ToHashSet();
        sets=new();
        foreach(var r in data.GetExcelSheet<MirageStoreSetItem>())
        {
            uint[] parts=[r.MainHand.RowId,r.OffHand.RowId,r.Head.RowId,r.Body.RowId,r.Hands.RowId,r.Legs.RowId,r.Feet.RowId,r.Earrings.RowId,r.Necklace.RowId,r.Bracelets.RowId,r.Ring.RowId];
            if(parts.Any(ids.Contains))sets[r.RowId]=parts;
        }
    }
    public void Select(ulong id)
    {if(character==id)return;character=id;dresser=null;armoire=null;totals.Clear();}
    public HoldingsCount Total(uint id)=>totals.GetValueOrDefault(id,new(-1,false));
    // Also called when the user closes storage, before zone changes clear the game's cache.
    public void CaptureStorage(ulong id)
    {
        Select(id);if(id==0)return;
        var mirage=MirageManager.Instance();
        if(mirage!=null && mirage->PrismBoxLoaded)
        {
            var counts=new Dictionary<uint,int>();
            for(var i=0;i<mirage->PrismBoxItemIds.Length;i++)
            {
                var item=mirage->PrismBoxItemIds[i]%1000000;
                if(equipment.Contains(item))counts[item]=counts.GetValueOrDefault(item)+1;
                if(sets.TryGetValue(item,out var parts))for(var slot=0;slot<parts.Length;slot++)
                    if(equipment.Contains(parts[slot]) && mirage->IsSetSlotUnlocked((uint)i,slot))counts[parts[slot]]=counts.GetValueOrDefault(parts[slot])+1;
            }
            dresser=counts;
        }
        var ui=UIState.Instance();
        if(ui!=null && ui->Cabinet.IsCabinetLoaded())
            armoire=cabinet.ToDictionary(p=>p.Key,p=>ui->Cabinet.IsItemInCabinet(p.Value)?1:0);
    }
    public void Refresh(ulong id,IReadOnlyList<ExchangeItem> items,bool captureStorage=true)
    {
        Select(id);if(captureStorage)CaptureStorage(id);totals.Clear();if(id==0)return;
        var inventory=InventoryManager.Instance();if(inventory==null)return;
        foreach(var item in items)
        {
            totals[item.Id]=HoldingsCount.Combine(inventory->GetInventoryItemCount(item.Id,checkEquipped:true,checkArmory:true),
                equipment.Contains(item.Id)?dresser?.GetValueOrDefault(item.Id):0,
                cabinet.ContainsKey(item.Id)?armoire?.GetValueOrDefault(item.Id):0);
        }
    }
}
