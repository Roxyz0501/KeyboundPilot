using Dalamud.Plugin.Services;
using Dalamud.Plugin;
using Dalamud.Game.Text.SeStringHandling;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI;
namespace KeyboundPilot;

/// <summary>Owned NPC conversation only; validate the live catalog before any single-item callback.</summary>
internal sealed unsafe class ExchangeShop(IGameGui gui,IObjectTable objects,IDalamudPluginInterface pi,ExchangeCatalog catalog)
{
    private readonly ReceptionTravel travel=new(pi);
    private bool interacted;
    private nint ownedShop;
    private uint preparedItem;
    public void Suspend()=>travel.Stop();
    public void Reset(){travel.Stop();interacted=false;ownedShop=0;preparedItem=0;}
    internal static bool Visible(AtkUnitBase* addon)=>addon!=null && addon->IsVisible && addon->RootNode!=null && addon->RootNode->IsVisible();
    private AtkUnitBase* Shop=>gui.GetAddonByName<AtkUnitBase>("ShopExchangeCurrency");
    public bool IsOpen=>Visible(Shop);
    public bool DialogOpen=>Visible(gui.GetAddonByName<AtkUnitBase>("ShopExchangeCurrencyDialog")) || Visible(gui.GetAddonByName<AtkUnitBase>("SelectYesno"));
    public bool Approach()
    {
        var player=objects.LocalPlayer;if(player==null)return false;
        if(interacted)
        {
            if(Visible(Shop))ownedShop=(nint)Shop;
            return true;
        }
        if(IsOpen || DialogOpen || Visible(gui.GetAddonByName<AtkUnitBase>("Talk")) || Visible(gui.GetAddonByName<AtkUnitBase>("SelectString")))return false;
        var npc=objects.Where(x=>x.BaseId==ExchangeCatalog.NpcId && x.IsTargetable).OrderBy(x=>Vector3.DistanceSquared(x.Position,player.Position)).FirstOrDefault();
        if(npc==null)return true;
        if(Vector3.Distance(player.Position,npc.Position)>3)return travel.Approach(player.Position,npc.Position);
        travel.Stop();var target=TargetSystem.Instance();if(target==null)return false;
        interacted=true;target->InteractWithObject((GameObject*)npc.Address,true);return true;
    }
    public bool Select(ExchangeItem item,int quantity)
    {
        var addon=Shop;
        if(quantity<1 || quantity>item.StackSize || (item.Learnable || item.Unique) && quantity!=1 || !interacted || ownedShop!=(nint)addon || DialogOpen || ExchangeShopValues.Find(addon,item,catalog.Items,catalog.CurrencyIcon) is not {} index)return false;
        var values=stackalloc AtkValue[3];values[0]=new(){Type=AtkValueType.Int,Int=0};values[1]=new(){Type=AtkValueType.UInt,UInt=index};values[2]=new(){Type=AtkValueType.Int,Int=quantity};
        addon->FireCallback(3,values,true);return true;
    }
    public bool Prepare(ExchangeItem item)
    {
        if(!Matches() || DialogOpen)return false;
        if(ExchangeShopValues.Find(Shop,item,catalog.Items,catalog.CurrencyIcon)!=null)return true;
        if(preparedItem==item.Id || item.ShopTab is <8 or >11)return false;
        var button=(AtkComponentRadioButton*)Shop->GetComponentByNodeId(item.ShopTab);
        if(button==null || button->GetComponentType()!=ComponentType.RadioButton || button->IsSelected)return false;
        preparedItem=item.Id;
        LoopGameUi.Click(Shop,(AtkComponentButton*)button,ComponentType.RadioButton);
        return false; // Wait for the refreshed item/price list before reserving currency.
    }
    public bool Matches()=>interacted && ownedShop==(nint)Shop && catalog.Items.Any(item=>ExchangeShopValues.Find(Shop,item,catalog.Items,catalog.CurrencyIcon)!=null);
    public string Describe()
    {
        var shop=Shop;
        return $"visible={Visible(shop)}, owned={interacted && ownedShop==(nint)shop}, values={(shop==null?0:shop->AtkValuesCount)}, rows={ExchangeShopValues.Number(shop,4)}, currencyIcon={ExchangeShopValues.Number(shop,87)}/{catalog.CurrencyIcon}, firstItem={ExchangeShopValues.Number(shop,1066)}, firstPrice={ExchangeShopValues.Number(shop,456)}, firstIndex={ExchangeShopValues.Number(shop,1310)}, dialog={DialogOpen}";
    }
    private static string Text(AtkTextNode* node)=>node==null || node->NodeText.Length>2048?"":SeString.Parse(node->NodeText.AsSpan()).TextValue;
    public bool Confirm(ExchangeItem item,int expectedQuantity)
    {
        if(!Matches())return false;
        var yesno=gui.GetAddonByName<AddonSelectYesno>("SelectYesno");
        if(Visible((AtkUnitBase*)yesno))
            return ExchangeConfirmation.Matches((AtkUnitBase*)yesno,item,expectedQuantity,catalog.CurrencyName)
                && LoopGameUi.Click((AtkUnitBase*)yesno,yesno->YesButton);
        var dialog=gui.GetAddonByName<AtkUnitBase>("ShopExchangeCurrencyDialog");
        if(!Visible(dialog))return false;
        // The selected item must be present in the dialog's actual text values, with the exact reserved quantity.
        var matched=false;
        if(dialog->AtkValues==null || dialog->AtkValuesCount>256)return false;
        for(var i=0;i<dialog->AtkValuesCount;i++)
        {
            var value=dialog->AtkValues[i];
            if(value.Type is AtkValueType.String or AtkValueType.ManagedString && value.String.Value!=null)
            {
                var length=0;while(length<2048 && value.String.Value[length]!=0)length++;
                if(length<2048 && SeString.Parse(new ReadOnlySpan<byte>(value.String.Value,length)).TextValue==item.Name)matched=true;
            }
        }
        if(!matched)return false;
        var quantity=(AtkComponentNumericInput*)dialog->GetComponentByNodeId(13);
        if(expectedQuantity<1 || quantity==null || quantity->GetComponentType()!=ComponentType.NumericInput || quantity->Value!=expectedQuantity)return false;
        return LoopGameUi.Click(dialog,(AtkComponentButton*)dialog->GetComponentByNodeId(17));
    }
    public bool Close()
    {
        travel.Stop();
        if(!Visible(Shop))return true;
        if(ownedShop!=(nint)Shop || DialogOpen)return false;
        Shop->Close(true);return true;
    }
}
