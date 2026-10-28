using System.Numerics;
using Dalamud.Plugin;
namespace KeyboundPilot;
internal sealed class ReceptionTravel(IDalamudPluginInterface pi)
{
    private CancellationTokenSource? cancellation;
    private Task<List<Vector3>>? pending;
    private bool owned;
    public void Stop()
    {
        cancellation?.Cancel();cancellation?.Dispose();cancellation=null;
        if(pending!=null)_=pending.ContinueWith(t=>{_=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
        pending=null;
        if(owned){try {pi.GetIpcSubscriber<object>("vnavmesh.Path.Stop").InvokeAction();} catch { /* Provider may already have unloaded; lifecycle cleanup must continue. */ } finally {owned=false;}}
    }
    public bool Approach(Vector3 from,Vector3 to)
    {
        if(!pi.InstalledPlugins.Any(x=>x.InternalName=="vnavmesh" && x.IsLoaded))return false;
        if(!pi.GetIpcSubscriber<bool>("vnavmesh.Nav.IsReady").InvokeFunc())return true;
        if(pending!=null)
        {
            if(!pending.IsCompleted)return true;
            var done=pending;pending=null;
            try
            {
                var points=done.GetAwaiter().GetResult();if(points.Count==0)return false;
                if(pi.GetIpcSubscriber<bool>("vnavmesh.Path.IsRunning").InvokeFunc())return false;
                owned=true;pi.GetIpcSubscriber<List<Vector3>,bool,object>("vnavmesh.Path.MoveTo").InvokeAction(points,false);
                return true;
            }
            finally {cancellation?.Dispose();cancellation=null;}
        }
        if(owned)return pi.GetIpcSubscriber<bool>("vnavmesh.Path.IsRunning").InvokeFunc();
        if(pi.GetIpcSubscriber<bool>("vnavmesh.Path.IsRunning").InvokeFunc() || pi.GetIpcSubscriber<bool>("vnavmesh.SimpleMove.PathfindInProgress").InvokeFunc())return false;
        var offset=from-to;offset.Y=0;
        var destination=offset.LengthSquared()>0.01f?to+Vector3.Normalize(offset)*1.5f:to;
        cancellation=new();pending=pi.GetIpcSubscriber<Vector3,Vector3,bool,CancellationToken,Task<List<Vector3>>>("vnavmesh.Nav.PathfindCancelable").InvokeFunc(from,destination,false,cancellation.Token);
        return pending!=null;
    }
}
