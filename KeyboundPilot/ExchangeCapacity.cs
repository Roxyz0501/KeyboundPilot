using FFXIVClientStructs.FFXIV.Client.Game;
namespace KeyboundPilot;
internal static unsafe class ExchangeCapacity
{
    public static int Available(InventoryContainer* bag,uint item,uint stackSize)
    {
        if(bag==null || !bag->IsLoaded || bag->Items==null || bag->Size is <1 or >35 || stackSize is <1 or >9999)return -1;
        var total=0;
        for(var i=0;i<bag->Size;i++)
        {
            var slot=bag->Items[i];if(slot.IsSymbolic)continue;
            if(slot.ItemId==0)total+=(int)stackSize;
            else if(slot.ItemId==item && slot.Flags==InventoryItem.ItemFlags.None && slot.Quantity>=0 && slot.Quantity<=stackSize)
                total+=(int)stackSize-slot.Quantity;
        }
        return total;
    }
}
