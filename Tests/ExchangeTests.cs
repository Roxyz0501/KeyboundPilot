using KeyboundPilot;
using System.Text.Json;
static class ExchangeTests
{
    public static void Run(Action<bool,string> check)
    {
        void C(bool value,string text)=>check(value,"exchange: "+text);
        var item=new ExchangeItem(10,"Item",100,true,true,1);
        var box=new ExchangeItem(11,"Box",200,false,false,999);
        var unlearned=new ExchangeObservation(ExchangeKnowledge.Unlearned,0,true);
        var normal=new ExchangeObservation(ExchangeKnowledge.NotApplicable,0,true);
        var plan=new ExchangePlan(true,100).SetQuantity(10,99,true).SetQuantity(11,3,false);
        C(!new ExchangePlan().Enabled,"defaults off");C(plan.Quantity(10,true)==1,"learnable capped to one");
        C(plan.SetQuantity(10,-1,true).Quantity(10,true)==0,"learnable capped to zero");
        C(plan.SetQuantity(11,999999,false).Quantity(11,false)==9999,"ordinary upper bound");
        C(plan.SetQuantity(10,1,true)==plan,"value equality for unchanged settings");
        C(plan.Valid && !new ExchangePlan(Targets:"10=1;10=2").Valid,"reject duplicate ids");
        var equipment=new ExchangeItem(12,"Equipment",100,false,true,1);
        var mixed=new[]{box,equipment,item};
        C(plan.Ordered(mixed).Select(i=>i.Id).SequenceEqual(new uint[]{10,12,11}),"default unlockable equipment coffer priority");
        var reordered=plan.Move(mixed,11,-1).Move(mixed,11,-1);
        C(reordered.Ordered(mixed)[0]==box,"custom priority can move coffer first");
        C(ExchangePolicy.Next(mixed,reordered,300,_=>0,id=>id==10?unlearned:normal)==box,"purchase policy uses custom order");
        C(reordered.Move(mixed,11,-1)==reordered,"cannot move above first");
        C(!new ExchangePlan(Order:"10,10").Valid && !new ExchangePlan(Order:"../").Valid,"invalid order rejected");
        var hidden=plan with {HideZero=true};
        C(!hidden.Shows(equipment) && hidden.Shows(item) && hidden.Shows(box),"hide only requested zero");
        C((hidden with {HideZero=false}).Shows(equipment),"show all restores zero items");
        C(hidden.Targets==plan.Targets && hidden.Order==plan.Order,"filter retains quantities and priorities");
        C(hidden.Move(mixed,11,-1,10).Ordered(mixed).Select(i=>i.Id).SequenceEqual(new uint[]{11,12,10}),"filtered movement swaps visible neighbors");
        C(plan.Insert(mixed,11,10,false).Ordered(mixed).Select(i=>i.Id).SequenceEqual(new uint[]{11,10,12}),"drag inserts before target without swapping others");
        C(plan.Insert(mixed,10,11,true).Ordered(mixed).Select(i=>i.Id).SequenceEqual(new uint[]{12,11,10}),"drag inserts after target");
        C(plan.Insert(mixed,10,10,true)==plan,"self drop leaves plan unchanged");
        C(plan.Insert(mixed,999,10,true)==plan && plan.Insert(mixed,10,999,true)==plan,"unknown drag ids rejected");
        C(hidden.Insert(mixed,11,10,false).Targets==hidden.Targets && hidden.Insert(mixed,11,10,false).HideZero,"filtered drag preserves quantities and filter");
        C(plan.Insert(mixed,11,10,false).SetQuantity(10,1,true).Ordered(mixed)[0].Id==11,"quantity edits retain dropped priority");
        C((normal with {TotalOwned=7}).Owned==0 && (normal with {TotalOwned=7}).DisplayOwned==7,"receipt inventory independent of displayed storage");
        C(ExchangePolicy.Next([box],plan,1000,_=>0,_=>normal with {TotalOwned=999})==box,"displayed holdings do not exclude targets");
        C(JsonSerializer.Deserialize<ExchangePlan>("{\"ExcludeOverTarget\":true}")==new ExchangePlan(),"removed legacy exclusion ignored");
        C(ExchangePolicy.Eligible(equipment,1,0,normal with {TotalOwned=1}),"stored unique gear not confused with carried inventory");
        C(ExchangePolicy.BatchSize(box,plan,1000,0,normal)==3,"batch remaining target");
        C(ExchangePolicy.BatchSize(box,plan,400,0,normal)==2,"batch currency limit");
        C(ExchangePolicy.BatchSize(box,plan,1000,2,normal)==1,"batch subtracts prior progress");
        C(ExchangePolicy.BatchSize(box,plan,1000,0,normal with {Capacity=2})==2,"batch capacity limit");
        C(ExchangePolicy.BatchSize(box,plan,1000,0,normal with {Capacity=0})==0,"batch no capacity");
        C(ExchangePolicy.BatchSize(item,plan,1000,0,unlearned)==1,"learnable batch remains one");
        C(ExchangePolicy.BatchSize(item,plan,1000,0,unlearned with {Knowledge=ExchangeKnowledge.Learned})==0,"learned batch excluded");
        C(ExchangePolicy.BatchSize(box,plan,199,0,normal)==0,"batch cannot afford one");
        C(!new ExchangePlan(Threshold:0).Valid && !new ExchangePlan(Threshold:10001).Valid,"threshold bounds");
        C(!new ExchangePlan(Targets:"../=1").Valid && !new ExchangePlan(Targets:"10=-1").Valid,"invalid targets rejected");
        C(new ExchangePlan(Targets:"10=99").Quantity(10,true)==1,"external oversized learned value clamped");
        C(ExchangePolicy.Eligible(item,1,0,unlearned),"unlearned eligible");
        C(!ExchangePolicy.Eligible(item,1,0,unlearned with {Knowledge=ExchangeKnowledge.Learned}),"learned excluded");
        C(plan.Quantity(10,true)==1,"learned exclusion never edits shared plan");
        C(!ExchangePolicy.Eligible(item,1,0,unlearned with {Knowledge=ExchangeKnowledge.Unknown}),"unknown excluded");
        C(!ExchangePolicy.Eligible(item,1,0,unlearned with {Owned=1}),"unused owned unlock excluded");
        C(!ExchangePolicy.Eligible(item,1,1,unlearned),"completed target excluded");
        C(!ExchangePolicy.Eligible(box,1,0,normal with {Space=false}),"full bags excluded");
        C(!ExchangePolicy.Eligible(box,1,0,normal with {Owned=-1}),"unavailable inventory excluded");
        C(ExchangePolicy.Next([item,box],plan,100,_=>0,id=>id==10?unlearned:normal)==item,"exact price affordable");
        C(ExchangePolicy.Next([item,box],plan,99,_=>0,_=>unlearned)==null,"insufficient currency");
        C(ExchangePolicy.Next([item,box],plan,200,_=>0,id=>id==10?unlearned with {Knowledge=ExchangeKnowledge.Learned}:normal)==box,"skip learned and continue");
        C(ExchangePolicy.ShouldBegin(true,false,false,false,plan,100,item),"threshold equal triggers");
        C(ExchangePolicy.ShouldBegin(true,false,false,false,plan,101,item),"above threshold triggers");
        C(!ExchangePolicy.ShouldBegin(true,false,false,false,plan,99,item),"below threshold waits");
        C(!ExchangePolicy.ShouldBegin(true,true,false,false,plan,101,item),"queued cannot exchange");
        C(!ExchangePolicy.ShouldBegin(true,false,true,false,plan,101,item),"loading cannot exchange");
        C(!ExchangePolicy.ShouldBegin(true,false,false,true,plan,101,item),"stop request prevents new visit");
        C(!ExchangePolicy.ShouldBegin(true,false,false,false,plan,null,item),"unknown currency cannot trigger");
        var lobby=new LoopSnapshot(true,true,false,false,false,false,false,false,false,false);
        var loop=new RoundLoop();loop.Start(0,2,false);
        C(loop.Tick(0,lobby,exchangeDue:true)==LoopAction.Exchange && loop.Phase==LoopPhase.Exchanging,"exchange before queue");
        C(loop.Tick(1,lobby)==LoopAction.None && loop.Completed==0,"exchange adds no rounds");
        loop.Pause();C(loop.Tick(2,lobby,exchangeFinished:true)==LoopAction.None && loop.Phase==LoopPhase.Exchanging,"pause retains exchange phase");
        loop.Resume();loop.Tick(3,lobby,exchangeFinished:true);C(loop.Phase==LoopPhase.Requesting,"exchange returns to queue phase");
        C(loop.Tick(4,lobby)==LoopAction.Queue,"queue once after exchange");
        loop.Start(0,1,true);loop.Tick(1,lobby with {Lobby=false,InGame=true,Result=true,LeaveReady=true});
        C(loop.Tick(2,lobby,exchangeDue:true)==LoopAction.Exchange && loop.Completed==1,"last round can exchange");
        loop.Tick(3,lobby,exchangeFinished:true);C(loop.Phase==LoopPhase.Complete,"last exchange completes loop");
        loop.Start(0,2,true);loop.RequestStopAfterCurrent();loop.Tick(1,lobby with {Lobby=false,InGame=true,Result=true});loop.Tick(2,lobby,exchangeDue:true);
        C(loop.Phase==LoopPhase.Complete,"scheduled stop skips new visit");
        var directory=Path.Combine(AppContext.BaseDirectory,"Keybound-exchange-"+Guid.NewGuid().ToString("N"));
        using(var ledger=new ExchangeLedger(directory))
        {
            ledger.Select(1);ledger.Flush();C(ledger.Ready && ledger.Completed(10)==0,"initial character ready");
            var writes=ledger.Writes;for(var i=0;i<100;i++){ledger.Select(1);ledger.Poll();}
            C(ledger.Writes==writes,"unchanged frames write nothing");
            C(ledger.Reserve(10,100,0,100),"reserve before callback");ledger.Flush();
            C(ledger.Current!.Pending!=null && !ledger.Reserve(10,100,0,100),"single pending purchase: "+ledger.Error);
            C(ledger.MarkDispatchStarted(),"persist dispatch boundary");ledger.Flush();
            using(var recovered=new ExchangeLedger(directory)){recovered.Select(1);recovered.Flush();C(recovered.Current?.Pending!=null,"uncertain purchase retained across reload");}
            C(ledger.Resolve(true),"commit acknowledgment");ledger.Flush();
            C(ledger.Completed(10)==1 && ledger.Current!.Pending==null,"progress committed");
            C(!ledger.Resolve(true),"no duplicate commit");
            ledger.Select(2);ledger.Flush();C(ledger.Completed(10)==0,"other character separate progress");
            C(plan.Quantity(10,true)==1,"other character retains target");
            ledger.Reserve(10,100,0,100);ledger.Flush();ledger.Resolve(false);ledger.Flush();C(ledger.Completed(10)==0,"manual non-purchase clears without increment");
            ledger.Select(1);ledger.Flush();C(ledger.Completed(10)==1,"returning character retains count");
            C(Directory.GetFiles(directory,"*.json",SearchOption.AllDirectories).Length==4,"two checkpoints and two dated receipts");
        }
        using(var ledger=new ExchangeLedger(Path.Combine(directory,"flow")))
        {
            ledger.Select(3);ledger.Flush();var session=new ExchangeSession();session.Start(0);
            var balance=100;var owned=0;var selects=0;var confirms=0;var dialog=false;var closes=0;
            void Tick(long now,bool enabled=true,bool blocked=false,ExchangeItem? candidate=null)=>session.Tick(now,blocked,enabled,ledger,candidate??item,balance,_=>unlearned with {Owned=owned},()=>true,()=>true,
                _=>{selects++;return true;},_=>{confirms++;return true;},()=>dialog,()=>{closes++;return true;});
            Tick(0);Tick(1);ledger.Flush();Tick(2);C(selects==0,"no callback before dispatch marker is durable");ledger.Flush();Tick(2);C(selects==1 && ledger.Current!.Pending!=null,"dispatch after durable reservation");
            Tick(3);Tick(4);C(selects==1 && confirms==0,"no repeat while awaiting dialog");
            dialog=true;Tick(5);Tick(6);C(confirms==1,"single confirmation");
            balance=0;owned=1;dialog=false;Tick(7);ledger.Flush();Tick(8);C(ledger.Completed(10)==1,"matching inventory and currency acknowledges");
            session.Tick(9,false,true,ledger,null,balance,_=>normal,()=>true,()=>true,_=>false,_=>false,()=>false,()=>true);
            Tick(10);C(session.Step==ExchangeStep.Done,"no remaining targets finishes visit");
            session.Start(20);Tick(20,false);C(session.Step==ExchangeStep.Done,"disable before purchase closes without dispatch");
            C(selects==1 && confirms==1,"disabled visit sends no purchase");
            session.Start(30);Tick(30);balance=100;owned=0;Tick(31);ledger.Flush();Tick(32);ledger.Flush();Tick(32);
            Tick(16000);C(session.Step==ExchangeStep.Failed && ledger.Current!.Pending!=null,"unacknowledged purchase stops with journal");
            var prior=selects;Tick(17000);C(selects==prior,"failed flow never retries purchase");
        }
        using(var ledger=new ExchangeLedger(Path.Combine(directory,"dialog-ready")))
        {
            ledger.Select(11);ledger.Flush();var session=new ExchangeSession();session.Start(0);var ready=false;var clicks=0;
            void Tick(long now)=>session.Tick(now,false,true,ledger,item,100,_=>unlearned,()=>true,()=>true,_=>true,_=>{if(!ready)return false;clicks++;return true;},()=>true,()=>true);
            Tick(0);Tick(1);ledger.Flush();Tick(2);ledger.Flush();Tick(3);Tick(4);
            C(session.Step==ExchangeStep.Confirm && clicks==0,"visible incomplete confirmation waits without a click");
            ready=true;Tick(5);Tick(6);C(session.Step==ExchangeStep.Acknowledge && clicks==1,"ready confirmation dispatches exactly once");
            Tick(16006);C(session.Step==ExchangeStep.Failed && clicks==1,"missing acknowledgment never repeats confirmation");
        }
        var cfgPath=Path.Combine(directory,"config.json");
        using(var store=new ConfigurationStore(cfgPath))
        {store.Configuration.Exchange=plan;store.Update(store.Configuration);store.Flush();C(store.SaveCount==1,"exchange settings persisted once");}
        using(var store=new ConfigurationStore(cfgPath))
        {C(store.Configuration.Exchange==plan,"exchange settings survive reload");store.Update(store.Configuration);store.Flush();C(store.SaveCount==0,"unchanged exchange plan not rewritten");}
        C(JsonSerializer.Deserialize<Configuration>("{\"Version\":1}")!.Exchange==new ExchangePlan(),"legacy defaults off");
        using(var ledger=new ExchangeLedger(Path.Combine(directory,"cancel")))
        {
            ledger.Select(4);ledger.Flush();var session=new ExchangeSession();var sent=0;
            void Tick(long now,bool enabled=true,bool blocked=false,int currency=100,int owned=0)=>session.Tick(now,blocked,enabled,ledger,item,currency,
                _=>unlearned with {Owned=owned},()=>true,()=>true,_=>{sent++;return true;},_=>{sent++;return true;},()=>false,()=>true);
            session.Start(0);Tick(0);Tick(1);ledger.Flush();Tick(2,false);ledger.Flush();Tick(3,false);Tick(4,false);
            C(sent==0 && ledger.Current!.Pending==null && ledger.Completed(10)==0,"off during durable reservation cancels before dispatch");
            session.Start(10);Tick(10);Tick(11);ledger.Flush();Tick(12,true,true);Tick(50000,true,true);
            C(sent==0 && session.Active,"paused reservation sends nothing and retains session");
            Tick(50001);ledger.Flush();Tick(50001);C(sent==1,"resume sends reservation once");
            Tick(50002,currency:0);C(ledger.Current!.Pending!=null,"currency change alone cannot acknowledge");
            Tick(50003,owned:1);C(ledger.Current!.Pending!=null,"inventory change alone cannot acknowledge");
            Tick(50004,currency:0,owned:1);ledger.Flush();C(ledger.Completed(10)==1,"both changes commit exactly one");
            Tick(50005,currency:0,owned:1);C(ledger.Completed(10)==1,"later poll cannot duplicate result");
            session.Stop();C(!session.Active && session.Step==ExchangeStep.Idle,"stop clears automation only");
        }
        using(var ledger=new ExchangeLedger(Path.Combine(directory,"rejected-selection")))
        {
            ledger.Select(7);ledger.Flush();var session=new ExchangeSession();session.Start(0);
            void Tick(long now)=>session.Tick(now,false,true,ledger,item,100,_=>unlearned,()=>true,()=>true,_=>false,_=>throw new Exception("Unexpected confirmation"),()=>false,()=>true);
            Tick(0);Tick(1);ledger.Flush();Tick(2);ledger.Flush();Tick(2);ledger.Flush();
            C(session.Step==ExchangeStep.Commit && session.Deferred && ledger.Current!.Pending==null && ledger.Completed(10)==0,"rejected selection clears unsubmitted reservation");
            Tick(3);Tick(4);C(session.Step==ExchangeStep.Done,"unsubmitted rejection closes visit to resume loop");
        }
        var badDir=Path.Combine(directory,"corrupt");Directory.CreateDirectory(Path.Combine(badDir,"0000000000000005"));
        File.WriteAllText(Path.Combine(badDir,"0000000000000005","progress.json"),"invalid");
        using(var bad=new ExchangeLedger(badDir))
        {bad.Select(5);bad.Flush();C(bad.Error!=null && !bad.Ready && !bad.Reserve(10,100,0,100),"corrupt progress never silently resets purchase counts");}
        using(var ledger=new ExchangeLedger(Path.Combine(directory,"batch")))
        {
            ledger.Select(6);ledger.Flush();var session=new ExchangeSession();session.Start(0);var selects=0;var confirms=0;
            void Tick(long now,int currency=1000,int owned=0,bool dialog=false)=>session.Tick(now,false,true,ledger,box,currency,
                _=>normal with {Owned=owned},()=>true,()=>true,_=>{selects++;return ledger.Current!.Pending!.Quantity==3;},
                _=>{confirms++;return ledger.Current!.Pending!.Quantity==3;},()=>dialog,()=>true,3);
            Tick(0);Tick(1);ledger.Flush();C(ledger.Current!.Pending is {Quantity:3,Cost:600},"batch journal captures quantity and total cost");
            Tick(2);ledger.Flush();Tick(2);Tick(3,dialog:true);C(selects==1 && confirms==1,"three units use one selection and confirmation");
            Tick(4,400,1);C(ledger.Current!.Pending!=null,"partial batch inventory does not commit");
            Tick(5,400,3);ledger.Flush();C(ledger.Completed(11)==3,"batch progress adds all units");
            C(!ledger.Reserve(11,100,3,200,3),"unaffordable reservation rejected");
        }
        using(var ledger=new ExchangeLedger(Path.Combine(directory,"unsubmitted")))
        {
            ledger.Select(8);ledger.Flush();ledger.Reserve(10,100,0,100);ledger.Flush();
        }
        using(var ledger=new ExchangeLedger(Path.Combine(directory,"unsubmitted")))
        {
            ledger.Select(8);ledger.Flush();C(ledger.Ready && ledger.Current!.Pending==null && ledger.Completed(10)==0,"unsubmitted reservation automatically recovered on reload");
            ledger.Reserve(10,100,0,100);ledger.Flush();C(ledger.RecoverUnsubmitted(),"stopped unsubmitted reservation recovered");ledger.Flush();
            C(!ledger.RecoverUnsubmitted(),"unchanged state does not repeat recovery writes");
        }
        using(var ledger=new ExchangeLedger(Path.Combine(directory,"category")))
        {
            ledger.Select(9);ledger.Flush();var session=new ExchangeSession();session.Start(0);var ready=false;var sends=0;
            void Tick(long now)=>session.Tick(now,false,true,ledger,item,100,_=>unlearned,()=>true,()=>true,_=>{sends++;return true;},_=>false,()=>false,()=>true,1,_=>ready);
            Tick(0);Tick(1);C(session.Step==ExchangeStep.Reserve && ledger.Current!.Pending==null,"category refresh precedes purchase reservation");
            ready=true;Tick(2);ledger.Flush();Tick(3);ledger.Flush();Tick(4);C(sends==1,"ready category selects once");
        }
        using(var ledger=new ExchangeLedger(Path.Combine(directory,"category-timeout")))
        {
            ledger.Select(10);ledger.Flush();var session=new ExchangeSession();session.Start(0);
            void Tick(long now)=>session.Tick(now,false,true,ledger,item,100,_=>unlearned,()=>true,()=>true,_=>throw new Exception("No purchase during category wait"),_=>false,()=>false,()=>true,1,_=>false);
            Tick(0);Tick(1);Tick(16001);C(session.Deferred && session.Step==ExchangeStep.Done && ledger.Current!.Pending==null,"category timeout resumes loop without a purchase");
        }
        var legacy=JsonSerializer.Deserialize<ExchangeLedger.Purchase>("{\"Id\":\"00000000-0000-0000-0000-000000000001\",\"Item\":10,\"CurrencyBefore\":100,\"OwnedBefore\":0,\"Cost\":100}");
        C(legacy!.DispatchStarted==null,"legacy pending is unknown, never assumed unsubmitted");
        Console.WriteLine("Exchange fixture: bounded 20-item catalog, 0 external calls, unchanged polling 0 writes; per purchase reservation + dispatch marker + receipt + checkpoint.");
    }
}
