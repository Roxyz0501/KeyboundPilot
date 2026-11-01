using System.Globalization;
using System.Text.RegularExpressions;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Component.GUI;
namespace KeyboundPilot;

internal static unsafe class ExchangeConfirmation
{
    // Item-bearing SelectYesno captured from the actual currency exchange confirmation.
    // The total price validates the requested quantity without guessing undocumented scalar fields.
    public static bool Matches(AtkUnitBase* dialog,ExchangeItem item,int quantity,string currencyName)
    {
        if(dialog==null || !dialog->IsVisible || dialog->AtkValues==null || dialog->AtkValuesCount!=22
            || quantity<1 || quantity>item.StackSize || (item.Learnable || item.Unique) && quantity!=1
            || item.Cost<=0 || string.IsNullOrWhiteSpace(currencyName))return false;
        if(ExchangeShopValues.Number(dialog,12)!=1 || ExchangeShopValues.Number(dialog,14)!=item.Id
            || ExchangeShopValues.Number(dialog,13)!=item.Icon || Read(dialog,15)!=item.Name)return false;
        var prompt=Read(dialog,0);
        if(!prompt.Contains(currencyName,StringComparison.Ordinal))return false;
        var prices=Regex.Matches(prompt.Replace(currencyName,"",StringComparison.Ordinal),@"[0-9]+(?:[,.' \u00a0\u202f][0-9]{3})*");
        if(prices.Count!=1)return false;
        var digits=new string(prices[0].Value.Where(char.IsAsciiDigit).ToArray());
        return long.TryParse(digits,NumberStyles.None,CultureInfo.InvariantCulture,out var price) && price==(long)item.Cost*quantity;
    }

    private static string Read(AtkUnitBase* dialog,int index)
    {
        var value=dialog->AtkValues[index];
        if(value.Type is not (AtkValueType.String or AtkValueType.ManagedString or AtkValueType.ConstString) || value.String.Value==null)return "";
        var length=0;while(length<2048 && value.String.Value[length]!=0)length++;
        return length==2048?"":SeString.Parse(new ReadOnlySpan<byte>(value.String.Value,length)).TextValue;
    }
}
