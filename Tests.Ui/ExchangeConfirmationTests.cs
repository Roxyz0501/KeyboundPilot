using KeyboundPilot;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
unsafe partial class Program
{
    static void CheckExchangeConfirmation()
    {
        var allocated=new List<nint>();
        try
        {
            var dialog=stackalloc AtkUnitBase[1];*dialog=default;dialog->IsVisible=true;
            var values=stackalloc AtkValue[22];new Span<AtkValue>(values,22).Clear();dialog->AtkValues=values;dialog->AtkValuesCount=22;
            void Num(int i,uint v)=>values[i]=new(){Type=AtkValueType.UInt,UInt=v};
            void Text(int i,string v){var p=Marshal.StringToCoTaskMemUTF8(v);allocated.Add(p);values[i]=new(){Type=AtkValueType.String};values[i].String.Value=(byte*)p;}
            var item=new ExchangeItem(52261,"ガードスコーピオン認証鍵",430,true,true,1,26014);
            const string currency="マンダヴィル・ゴールドソーサークレジット";
            Num(12,1);Num(13,26014);Num(14,52261);Text(15,item.Name);
            Text(0,"以下のアイテムを\n"+currency+"×430と交換します。");
            void Check(bool expected,string label,int quantity=1){if(ExchangeConfirmation.Matches(dialog,item,quantity,currency)!=expected)throw new Exception(label);}
            Check(true,"Captured item-bearing Yes/No was rejected");
            Num(14,52262);Check(false,"Wrong item accepted");Num(14,52261);
            Num(13,1);Check(false,"Wrong icon accepted");Num(13,26014);
            Text(15,"Other reward");Check(false,"Wrong item name accepted");Text(15,item.Name);
            Text(0,"Other currency×430");Check(false,"Wrong currency accepted");
            Text(0,currency+"×431");Check(false,"Wrong total accepted");
            Text(0,currency+"×430 (1)");Check(false,"Ambiguous numbers accepted");
            Text(0,currency+"×430");Check(false,"Learnable batch accepted",2);
            dialog->AtkValuesCount=12;Check(false,"Ordinary Yes/No accepted");dialog->AtkValuesCount=22;
            values[14].Type=AtkValueType.String;Check(false,"Invalid item ID type accepted");Num(14,52261);
            item=item with {Learnable=false,Unique=false,StackSize=999};
            foreach(var total in new[]{"1,290","1.290","1 290","1\u202f290","1'290","1290"}){Text(0,currency+"×"+total);Check(true,"Batch/localized price rejected",3);}
            Check(false,"Wrong batch quantity accepted",2);
            Text(0,currency+"×999999999999999999999999");Check(false,"Overflow accepted",3);
            Text(0,new string('x',2048));Check(false,"Unbounded text accepted",3);
            dialog->IsVisible=false;Check(false,"Hidden dialog accepted");
            Console.WriteLine("PASS: captured exchange Yes/No, item/icon/name/currency/quantity/total validation, batch formatting, unrelated dialogs and malformed data");
        }
        finally{foreach(var p in allocated)Marshal.FreeCoTaskMem(p);}
    }
}
