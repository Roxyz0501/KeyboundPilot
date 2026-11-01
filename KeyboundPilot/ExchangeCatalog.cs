using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using Lumina.Excel.Sheets;
namespace KeyboundPilot;

internal sealed unsafe class ExchangeCatalog
{
    public const uint NpcId=1058753,ShopId=1771021,CurrencyId=52322;
    public ExchangeItem[] Items {get;}=[];
    public bool Supported {get;}
    public uint CurrencyIcon {get;}
    public string CurrencyName {get;}="";
    // Construct once from installed sheets; never download a shop list during UI drawing.
    public ExchangeCatalog(IDataManager data)
    {
        var npc=data.GetExcelSheet<ENpcBase>().GetRowOrDefault(NpcId);
        var shop=data.GetExcelSheet<SpecialShop>().GetRowOrDefault(ShopId);
        if(npc==null || !npc.Value.ENpcData.Any(x=>x.RowId==ShopId) || shop==null || shop.Value.UseCurrencyType!=4)return;
        var list=new List<ExchangeItem>();
        foreach(var row in shop.Value.Item)
        {
            if(row.ReceiveItems[0].Item.RowId==0)continue;
            if(row.ReceiveItems.Skip(1).Any(r=>r.Item.RowId!=0) || row.ReceiveItems[0].ReceiveCount!=1 || row.ReceiveItems[0].ReceiveHq
                || row.ItemCosts[0].ItemCost.RowId!=CurrencyId || row.ItemCosts[0].CurrencyCost is 0 or >10000
                || row.ItemCosts.Skip(1).Any(r=>r.CurrencyCost!=0))return;
            var item=row.ReceiveItems[0].Item.Value;
            var action=item.ItemAction.Value.Action.RowId;
            // Mount, minion, emote, portrait, fashion accessory and orchestrion unlock actions.
            var learnable=action is 1322 or 853 or 2633 or 29459 or 20086 or 25183;
            if(action!=0 && action!=4647 && !learnable)return;
            var slot=item.EquipSlotCategory.Value;
            var tab=item.EquipSlotCategory.RowId==0?11u:slot.MainHand>0 || slot.OffHand>0?8u:
                slot.Ears>0 || slot.Neck>0 || slot.Wrists>0 || slot.FingerL>0 || slot.FingerR>0?10u:9u;
            list.Add(new(item.RowId,item.Name.ToString(),(int)row.ItemCosts[0].CurrencyCost,learnable,item.IsUnique,item.StackSize,item.Icon,tab));
        }
        if(list.Count is <1 or >64 || list.Select(i=>i.Id).Distinct().Count()!=list.Count)return;
        Items=list.ToArray();var currency=data.GetExcelSheet<Item>().GetRow(CurrencyId);CurrencyIcon=currency.Icon;CurrencyName=currency.Name.ToString();Supported=true;
    }
    public static int? Currency()
    {
        var manager=CurrencyManager.Instance();
        if(manager==null || !manager->HasItem(CurrencyId))return null;
        var value=manager->GetItemCount(CurrencyId);return value<=10000?(int)value:null;
    }
    public static ExchangeObservation Observe(ExchangeItem item)
    {
        var inventory=InventoryManager.Instance();var ui=UIState.Instance();
        if(inventory==null || ui==null)return new(ExchangeKnowledge.Unknown,-1,false);
        var owned=inventory->GetInventoryItemCount(item.Id);
        var knowledge=ExchangeKnowledge.NotApplicable;
        if(item.Learnable)
        {
            var row=ExdModule.GetItemRowById(item.Id);
            knowledge=row==null?ExchangeKnowledge.Unknown:ui->IsItemActionUnlocked(row) switch
            {1=>ExchangeKnowledge.Learned,2=>ExchangeKnowledge.Unlearned,_=>ExchangeKnowledge.Unknown};
        }
        var capacity=0;
        if(item.Unique || item.Learnable)capacity=inventory->GetEmptySlotsInBag()>0?1:0;
        else
        {
            foreach(var type in new[]{InventoryType.Inventory1,InventoryType.Inventory2,InventoryType.Inventory3,InventoryType.Inventory4})
            {
                var available=ExchangeCapacity.Available(inventory->GetInventoryContainer(type),item.Id,item.StackSize);
                if(available<0)return new(knowledge,owned,false,0);
                capacity+=available;
            }
        }
        return new(knowledge,owned,capacity>0,capacity);
    }
}
