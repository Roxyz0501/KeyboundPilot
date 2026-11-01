namespace KeyboundPilot;

public enum ExchangeStep { Idle, Travel, Reserve, Select, Confirm, Acknowledge, Commit, Close, Done, Failed }
public sealed class ExchangeSession
{
    public ExchangeStep Step {get;private set;}
    public ExchangeItem? Item {get;private set;}
    public bool Deferred {get;private set;}
    private long since,pausedAt=-1;
    private bool confirmed;
    public bool Active=>Step is not (ExchangeStep.Idle or ExchangeStep.Done or ExchangeStep.Failed);
    public void Start(long now){Step=ExchangeStep.Travel;Item=null;since=now;pausedAt=-1;confirmed=false;Deferred=false;}
    public void Stop(){Step=ExchangeStep.Idle;Item=null;pausedAt=-1;Deferred=false;}
    public void Fail(){Step=ExchangeStep.Failed;}
    private void Move(ExchangeStep step,long now){Step=step;since=now;}
    // IO completion and receipts are supplied by the ledger. Every game mutation is dispatched once.
    public void Tick(long now,bool blocked,bool enabled,ExchangeLedger ledger,ExchangeItem? next,int? currency,
        Func<uint,ExchangeObservation> observe,Func<bool> approach,Func<bool> shopReady,Func<ExchangeItem,bool> select,
        Func<ExchangeItem,bool> confirm,Func<bool> dialogOpen,Func<bool> close,int quantity=1,Func<ExchangeItem,bool>? prepare=null)
    {
        ledger.Poll();if(!Active)return;
        if(ledger.Error!=null){Fail();return;}
        if(blocked){if(pausedAt<0)pausedAt=now;return;}
        if(pausedAt>=0){since+=now-pausedAt;pausedAt=-1;}
        if(now-since>(Step==ExchangeStep.Travel?120000:15000))
        {
            if(Step is ExchangeStep.Travel or ExchangeStep.Reserve && ledger.Ready && ledger.Current!.Pending==null){Deferred=true;Move(ExchangeStep.Close,now);}
            else {Fail();return;}
        }
        // Off cancels future purchases; an already submitted purchase still needs its receipt.
        if(!enabled && Step is ExchangeStep.Travel or ExchangeStep.Reserve){Move(ExchangeStep.Close,now);}
        if(Step==ExchangeStep.Travel)
        {
            if(!approach()){Fail();return;}
            if(shopReady())Move(ExchangeStep.Reserve,now);
        }
        else if(Step==ExchangeStep.Reserve)
        {
            if(!ledger.Ready)return;
            if(ledger.Current!.Pending!=null){Fail();return;}
            if(next==null){Move(ExchangeStep.Close,now);return;}
            if(currency==null || !shopReady())return;
            if(prepare!=null && !prepare(next))return;
            Item=next;confirmed=false;
            if(!ledger.Reserve(next.Id,currency.Value,observe(next.Id).Owned,next.Cost,quantity)){Fail();return;}
            Move(ExchangeStep.Select,now);
        }
        else if(Step==ExchangeStep.Select)
        {
            if(!ledger.Ready)return;
            // Recheck the live plan and character state after the durable reservation.
            if(!enabled || next?.Id!=Item?.Id || quantity!=ledger.Current!.Pending?.Quantity || currency!=ledger.Current!.Pending?.CurrencyBefore)
            {ledger.Resolve(false);Move(ExchangeStep.Commit,now);return;}
            if(ledger.Current!.Pending is {DispatchStarted:false})
            {if(!ledger.MarkDispatchStarted())Fail();return;}
            Move(ExchangeStep.Confirm,now);
            if(Item==null || !select(Item))
            {if(!ledger.Resolve(false)){Fail();return;}Deferred=true;Move(ExchangeStep.Commit,now);}
        }
        else if(Step is ExchangeStep.Confirm or ExchangeStep.Acknowledge)
        {
            var pending=ledger.Current?.Pending;
            if(pending==null || Item==null){Fail();return;}
            var state=observe(pending.Item);
            if(currency<=pending.CurrencyBefore-pending.Cost && state.Owned==pending.OwnedBefore+pending.Quantity)
            {if(!ledger.Resolve(true)){Fail();return;}Move(ExchangeStep.Commit,now);return;}
            if(!enabled){Fail();return;}
            if(!confirmed && dialogOpen())
            {
                // Server currency and inventory updates can arrive on different frames.
                if(currency!=pending.CurrencyBefore || state.Owned!=pending.OwnedBefore)return;
                if(next?.Id!=Item.Id || quantity!=pending.Quantity){Fail();return;}
                // A visible dialog may still be populating its item values or enabling its button.
                // Retry validation only; once the click is dispatched, never send it again.
                if(confirm(Item)){confirmed=true;Move(ExchangeStep.Acknowledge,now);}
            }
        }
        else if(Step==ExchangeStep.Commit)
        {
            if(!ledger.Ready)return;
            if(ledger.Current!.Pending!=null){Fail();return;}
            if(!dialogOpen())Move(enabled && !Deferred?ExchangeStep.Reserve:ExchangeStep.Close,now);
        }
        else if(Step==ExchangeStep.Close)
        {if(!close()){Fail();return;}Move(ExchangeStep.Done,now);}
    }
}
