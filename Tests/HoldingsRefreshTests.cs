using KeyboundPilot;
static class HoldingsRefreshTests
{
    public static void Run(Action<bool,string> check)
    {
        var refresh=new HoldingsRefresh([52261,52323]);
        check(!refresh.Due(0,false) && refresh.Due(0,true),"holdings: initial refresh waits for loaded inventory");
        refresh.Complete(0);
        check(!refresh.Pending && !refresh.Due(10000,true),"holdings: unchanged frames require no inventory reads");
        foreach(uint container in new uint[]{0,1,2,3,1000,3200,3201,3202,3203,3205,3206,3207,3208,3209,3300,3400,3500})
            check(refresh.Tracks(container,52323),"holdings: bags, equipped and armoury tracked");
        check(!refresh.Tracks(10000,52323) && !refresh.Tracks(4000,52323) && !refresh.Tracks(0,1),"holdings: unrelated containers/items ignored");
        for(var i=0;i<100;i++)refresh.Invalidate();
        check(!refresh.Due(249,true) && refresh.Due(250,true),"holdings: inventory burst coalesces within 250ms");
        refresh.Complete(250);
        check(!refresh.Due(500,true),"holdings: one completed refresh consumes whole burst");
        refresh.Invalidate();
        check(!refresh.Due(1000,false) && refresh.Pending,"holdings: pending update retained during loading");
        check(refresh.Due(1001,true),"holdings: update resumes when inventory loads");
        refresh.Complete(1001);refresh.Reset();
        check(refresh.Due(1001,true),"holdings: character change triggers initial refresh");
    }
}
