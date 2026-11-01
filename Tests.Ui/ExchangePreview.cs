using KeyboundPilot;
using Dalamud.Bindings.ImGui;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.Game;
unsafe partial class Program
{
    static void ExchangePreview(List<ImFontPtr> fonts,Dictionary<ulong,(nint,int,int)> textures,string path)
    {
        var items=new[]{new ExchangeItem(10,"アイテムA",430,false,true,1),new ExchangeItem(11,"アイテムB",260,true,true,1),new ExchangeItem(12,"アイテムC",910,true,true,1)};
        var panel=new ExchangePanel();var cfg=new Configuration{Exchange=new ExchangePlan(true,9000).SetQuantity(10,1,false).SetQuantity(11,1,true).SetQuantity(12,1,true)};
        var positions=new Dictionary<uint,Vector2>();
        var loopPaused=false;var pauses=0;var stops=0;var resumes=0;Vector2 loopPlay=default,loopStop=default;
        void Frame()
        {
            ImGui.NewFrame();ImGui.PushFont(fonts[0]);ImGui.SetNextWindowPos(new(20,20));ImGui.SetNextWindowSize(new(900,870));
            ImGui.Begin("Keybound Pilot",ImGuiWindowFlags.NoSavedSettings|ImGuiWindowFlags.NoResize);
            string T(string key)=>Loc.Get("ja",key);
            if(ImGui.BeginTabBar("Tabs"))
            {
                if(ImGui.BeginTabItem(T("Main")))
                {
                    PlaybackControls.StableText(T("Status")+": "+T("Waiting"));PlaybackControls.StableText(T("InputMode")+": "+T("NormalProfile"));
                    PlaybackControls.Draw(false,false,T,()=>{},()=>{},()=>{});ImGui.SameLine();PlaybackControls.Help(T("Guide"),30);
                    ImGui.Separator();PlaybackControls.StableText(T("LoopStatus")+": "+T("LoopIdle"));var count=10;ImGui.SetNextItemWidth(400);
                    PlaybackControls.Number("###Rounds",ref count,1,9999,1,T("NumberHelp"));PlaybackControls.DrawLoop(true,T(loopPaused?"Resume":"LoopButton"),()=>{loopPaused=false;resumes++;});loopPlay=ImGui.GetItemRectMin()+new Vector2(15);
                    PlaybackControls.DrawLoopPauseStop(true,loopPaused,T,()=>{loopPaused=true;pauses++;},()=>stops++);loopStop=ImGui.GetItemRectMin()+new Vector2(15);
                    PlaybackControls.DrawStopAfterCurrent(false,T,()=>{});
                    panel.Draw(cfg,T,()=>{},items,8500,_=>0,id=>new(id==12?ExchangeKnowledge.Learned:id==11?ExchangeKnowledge.Unlearned:ExchangeKnowledge.NotApplicable,0,true),"ExchangeIdle",false,null,_=>{},false,(item,size)=>
                    {positions[item.Id]=ImGui.GetCursorScreenPos();var pos=ImGui.GetCursorScreenPos();ImGui.GetWindowDrawList().AddRectFilled(pos,pos+new Vector2(size),0xff998855);ImGui.Dummy(new Vector2(size));});
                    ImGui.EndTabItem();
                }
                foreach(var key in new[]{"ScoreHistory","Settings","Language","Support"})if(ImGui.BeginTabItem(T(key)))ImGui.EndTabItem();
                ImGui.EndTabBar();
            }
            ImGui.End();ImGui.PopFont();ImGui.Render();
        }
        Frame();Frame();Frame();
        var io=ImGui.GetIO();
        void Click(Vector2 at){io.AddMousePosEvent(at.X,at.Y);Frame();io.AddMouseButtonEvent(0,true);Frame();io.AddMouseButtonEvent(0,false);Frame();}
        Click(loopStop-new Vector2(36+ImGui.GetStyle().ItemSpacing.X,0));
        if(!loopPaused || pauses!=1)throw new Exception("Loop pause button did not pause once");
        Click(loopPlay);if(loopPaused || resumes!=1)throw new Exception("Loop resume did not resume once");
        Click(loopStop);if(stops!=1)throw new Exception("Loop stop button did not stop once");
        Console.WriteLine("PASS: native loop pause, resume and stop buttons");
        var start=positions[10]+new Vector2(10,10);var target=positions[11]+new Vector2(10,3);
        io.AddMousePosEvent(start.X,start.Y);Frame();io.AddMouseButtonEvent(0,true);Frame();
        io.AddMousePosEvent(start.X+20,start.Y);Frame();io.AddMousePosEvent(target.X,target.Y);Frame();Frame();
        io.AddMouseButtonEvent(0,false);Frame();Frame();
        if(cfg.Exchange.Ordered(items)[0].Id!=10)throw new Exception("Native exchange drag did not move source before target");
        io.AddMousePosEvent(-1000,-1000);Frame();
        Console.WriteLine("PASS: native item-row drag/drop, icon slots and insertion order");
        Render(ImGui.GetDrawData(),textures,Path.Combine(path,"exchange.bmp"));
        AtkUnitBase addon=default;AtkResNode root=default;root.NodeFlags=NodeFlags.Visible;addon.RootNode=&root;addon.IsVisible=true;
        var values=stackalloc AtkValue[1400];new Span<AtkValue>(values,1400).Clear();addon.AtkValues=values;addon.AtkValuesCount=1400;
        void Set(int offset,uint value)=>values[offset]=new(){Type=AtkValueType.UInt,UInt=value};
        Set(4,3);Set(87,999);
        for(var i=0;i<3;i++){Set(1066+i,items[i].Id);Set(456+i,(uint)items[i].Cost);Set(1310+i,(uint)i);}
        if(ExchangeShopValues.Find(&addon,items[1],items,999)!=1)throw new Exception("Exchange valid catalog rejected");
        Set(4,4);Set(1069,0);Set(1313,3);
        if(ExchangeShopValues.Find(&addon,items[1],items,999)!=1)throw new Exception("Empty shop slots blocked selection");
        Set(4,2);
        if(ExchangeShopValues.Find(&addon,items[1],items,999)!=1)throw new Exception("Filtered shop blocked selection");
        if(ExchangeShopValues.Find(&addon,items[2],items,999)!=null)throw new Exception("Absent product accepted");
        Set(4,1);Set(1310,9);
        if(ExchangeShopValues.Find(&addon,items[0],items,999)!=9)throw new Exception("Visible product did not retain independent callback index 9");
        if(ExchangeShopValues.Find(&addon,items[2],items,999)!=null)throw new Exception("Nonvisible product selected");
        Set(1310,64);if(ExchangeShopValues.Find(&addon,items[2],items,999)!=null)throw new Exception("Invalid mapped index accepted");
        Set(1310,0);
        Set(4,3);
        Set(457,1);if(ExchangeShopValues.Find(&addon,items[1],items,999)!=null)throw new Exception("Exchange changed cost accepted");Set(457,260);
        Set(1067,items[0].Id);if(ExchangeShopValues.Find(&addon,items[1],items,999)!=null)throw new Exception("Duplicate exchange id accepted");Set(1067,items[1].Id);
        addon.AtkValuesCount=100;if(ExchangeShopValues.Find(&addon,items[1],items,999)!=null)throw new Exception("Truncated exchange values accepted");addon.AtkValuesCount=1400;
        Set(87,1);if(ExchangeShopValues.Find(&addon,items[1],items,999)!=null)throw new Exception("Wrong exchange currency accepted");Set(87,999);
        values[4].Type=AtkValueType.String;if(ExchangeShopValues.Find(&addon,items[1],items,999)!=null)throw new Exception("Wrong value type accepted");
        Console.WriteLine("PASS: exchange table rendering and native catalog/cost/currency/type/bounds validation");
        // Regression from a read-only capture of the live armour category: 9 rows, not the whole shop.
        uint[] liveIds=[52323,52324,52325,52326,52328,52329,52330,52331,52332];
        int[] liveCosts=[430,260,430,260,190,430,260,430,260];
        uint[] callbacks=[9,10,11,12,14,15,16,17,18];
        var liveItems=liveIds.Select((id,i)=>new ExchangeItem(id,"Reward",liveCosts[i],false,true,1)).ToArray();
        Set(4,9);Set(87,65140);
        for(var i=0;i<9;i++){Set(1066+i,liveIds[i]);Set(456+i,(uint)liveCosts[i]);Set(1310+i,callbacks[i]);}
        for(var i=0;i<9;i++)if(ExchangeShopValues.Find(&addon,liveItems[i],liveItems,65140)!=callbacks[i])throw new Exception("Live category mapping regression");
        Console.WriteLine("PASS: live 9-row armour category and independent callback IDs");
        var slots=stackalloc InventoryItem[2];new Span<InventoryItem>(slots,2).Clear();
        InventoryContainer bag=new(){Items=slots,Size=2,IsLoaded=true};slots[0].ItemId=10;slots[0].Quantity=995;
        if(ExchangeCapacity.Available(&bag,10,999)!=1003)throw new Exception("Partial stack capacity incorrect");
        slots[1].ItemId=20;slots[1].Quantity=1;
        if(ExchangeCapacity.Available(&bag,10,999)!=4)throw new Exception("Full bag partial stack rejected");
        slots[0].Flags=InventoryItem.ItemFlags.HighQuality;
        if(ExchangeCapacity.Available(&bag,10,999)!=0)throw new Exception("HQ space accepted for NQ reward");
        bag.IsLoaded=false;if(ExchangeCapacity.Available(&bag,10,999)!=-1)throw new Exception("Unloaded bag accepted");
        Console.WriteLine("PASS: batch capacity for empty slots, existing stacks, HQ separation and unloaded inventory");
    }
}
