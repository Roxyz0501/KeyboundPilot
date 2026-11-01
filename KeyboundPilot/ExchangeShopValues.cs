using FFXIVClientStructs.FFXIV.Component.GUI;
namespace KeyboundPilot;
internal static unsafe class ExchangeShopValues
{
    private static bool Visible(AtkUnitBase* addon)=>addon!=null && addon->IsVisible && addon->RootNode!=null && (addon->RootNode->NodeFlags & NodeFlags.Visible)!=0;
    internal static uint? Number(AtkUnitBase* addon,int offset)
    {
        if(addon==null || addon->AtkValues==null || offset<0 || offset>=addon->AtkValuesCount)return null;
        var value=addon->AtkValues[offset];
        return value.Type switch {AtkValueType.UInt=>value.UInt,AtkValueType.Int when value.Int>=0=>(uint)value.Int,_=>null};
    }
    internal static uint? Find(AtkUnitBase* addon,ExchangeItem item,IReadOnlyList<ExchangeItem> catalog,uint currencyIcon)
    {
        if(!Visible(addon) || Number(addon,87)!=currencyIcon || Number(addon,4) is not {} count || count is 0 or >64)return null;
        uint? found=null;var ids=new HashSet<uint>();
        for(var i=0;i<count;i++)
        {
            var index=Number(addon,1310+i);
            if(index==null || index>=64)return null;
            // Visible rows map to original shop entries; owned-item filters leave holes.
            var id=Number(addon,1066+(int)index.Value);
            if(id==0)continue; // Empty list slots are not products.
            var price=Number(addon,456+(int)index.Value);
            if(id==null || price==null || index==null || index>=64 || !ids.Add(id.Value))return null;
            var expected=catalog.FirstOrDefault(x=>x.Id==id);
            if(expected==null || expected.Cost!=price)return null;
            if(id==item.Id)found=index;
        }
        return found;
    }
}
