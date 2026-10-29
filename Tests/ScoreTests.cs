using KeyboundPilot;
internal static class ScoreTests
{
    public static void Run(Action<bool,string> check)
    {
        var lobby=new LoopSnapshot(true,true,false,false,false,false,false,false,false,false);
        var game=lobby with {Lobby=false,InGame=true};var result=game with {Result=true,LeaveReady=true};
        var loop=new RoundLoop();loop.ToggleStopAfterCurrent();check(!loop.StopAfterCurrentRequested,"idle toggle ignored");
        loop.Start(0,3,true);loop.ToggleStopAfterCurrent();loop.ToggleStopAfterCurrent();
        check(!loop.StopAfterCurrentRequested && loop.Active,"second click cancels reservation");
        loop.Tick(1,result);loop.Tick(2,lobby);check(loop.Tick(3,lobby)==LoopAction.Queue,"cancelled reservation allows next queue");
        loop.Start(0,3,true);loop.ToggleStopAfterCurrent();loop.Pause();loop.ToggleStopAfterCurrent();
        check(loop.Paused && !loop.StopAfterCurrentRequested,"cancel during pause keeps pause");
        loop.Resume();loop.Tick(1,result);loop.ToggleStopAfterCurrent();loop.ToggleStopAfterCurrent();loop.Tick(2,lobby);
        check(loop.Phase==LoopPhase.Requesting,"cancel during leaving resumes loop");
        loop.Start(0,3,true);loop.ToggleStopAfterCurrent();loop.ToggleStopAfterCurrent();loop.ToggleStopAfterCurrent();
        loop.Tick(1,result);loop.Tick(2,lobby);check(loop.Phase==LoopPhase.Complete,"third click re-arms stop");
        loop.ToggleStopAfterCurrent();check(loop.StopAfterCurrentRequested && !loop.Active,"completed run cannot be rearmed by toggle");
        foreach(var text in new[]{"12,345","12.345","12 345","12\u202f345","12\u00a0345"})check(ScoreNumbers.Parse(text)==12345,"localized integer score "+text);
        foreach(var text in new[]{"—","???","12.5","1,23","+100","-2","x2","2147483648","1,,234",""})check(ScoreNumbers.Parse(text)==null,"reject unavailable/non-integer score "+text);
        check(ScoreNumbers.Parse("0")==0,"legitimate zero is retained");
        var records=new List<ScoreRecord>();var session=new ScoreSession();
        var rows=new[]{new ScoreRow(1,"Friend",999,false),new ScoreRow(4,"Me",456,true)};
        session.Begin("Me");session.Observe(0,123,false,[],records.Add);
        check(session.LiveScore==123 && records.Count==0,"live observation does not write history");
        session.Observe(100,null,true,[],records.Add);check(!session.ReadyToLeave(100),"wait for result animation before automatic leave");
        session.Observe(1000,null,true,rows[..1],records.Add);
        session.Observe(2000,null,true,rows,records.Add);check(records.Count==0,"wait for newly revealed self score");
        session.Observe(2800,null,true,rows,records.Add);
        check(records.Count==1 && records[0].OwnScore==456 && records[0].Players.Length==1 && records[0].Players[0].Name=="Me","record only self and exclude other participants");
        check(session.ReadyToLeave(2800),"recorded result permits loop exit");
        session.Observe(4000,null,true,rows,records.Add);session.End(records.Add);session.End(records.Add);
        check(records.Count==1,"repeated result polls and exit record exactly once");
        session.Begin("Me");session.Observe(0,null,true,rows,records.Add);session.End(records.Add);
        check(records.Count==2 && records[0].Id!=records[1].Id,"identical scores in two games are distinct records");
        session.Begin("Me");session.Observe(0,444,false,[],records.Add);session.End(records.Add);
        check(records.Count==2,"forced exit without results is not a final score");
        session.Begin("Me");session.Observe(0,444,true,[],records.Add);session.Observe(5000,444,true,[],records.Add);
        check(records.Count==3 && records[2].OwnScore==null && records[2].Players.Length==0,"missing final score is never replaced with live score or zero");
        check(session.ReadyToLeave(5000),"unavailable results cannot block loop indefinitely");
        session.End(records.Add);session.Begin("Me");check(!session.ResultSeen && session.Result.Length==0 && session.LiveScore==null,"new game clears previous result");
        var folder=Path.Combine(AppContext.BaseDirectory,"KeyboundScoreTests-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var time=new DateTimeOffset(2026,10,29,23,59,59,TimeSpan.FromHours(9));
            var record=new ScoreRecord(Guid.NewGuid(),time,"Me",456,rows);
            using(var history=new ScoreHistory(folder))
            {
                history.Flush();check(Directory.GetFiles(folder,"*",SearchOption.AllDirectories).Length==0,"idle flush performs zero writes");
                history.Add(record);history.Flush();if(history.Error!=null)throw new Exception(history.Error);var path=Directory.GetFiles(folder,"*.json",SearchOption.AllDirectories).Single();
                var bytes=File.ReadAllBytes(path);var modified=File.GetLastWriteTimeUtc(path);
                history.Add(record);history.Flush();
                check(File.GetLastWriteTimeUtc(path)==modified && File.ReadAllBytes(path).SequenceEqual(bytes),"idempotent same-event retry performs zero replacement writes");
                for(var i=0;i<55;i++){history.Add(record with {Id=Guid.NewGuid(),Ended=time.AddSeconds(1+i)});history.Flush();}
                check(history.Dates().SequenceEqual(new[]{"2026-10-30","2026-10-29"}),"dates follow recorded offset across midnight");
                var first=history.ReadPage("2026-10-30",0);var second=history.ReadPage("2026-10-30",1);
                check(first.Records.Length==50 && first.More && second.Records.Length==5 && !second.More,"bounded 50-result pages cover all results");
                var before=Directory.GetFiles(folder,"*.json",SearchOption.AllDirectories).Select(File.GetLastWriteTimeUtc).ToArray();
                history.ReadPage("2026-10-29",0);history.Dates();history.Flush();
                check(before.SequenceEqual(Directory.GetFiles(folder,"*.json",SearchOption.AllDirectories).Select(File.GetLastWriteTimeUtc)),"viewing and unchanged flush cause zero writes");
                try{history.ReadPage("../escape",0);check(false,"reject traversal");}catch(ArgumentException){check(true,"reject traversal");}
            }
            using(var reopened=new ScoreHistory(folder))
            {
                check(reopened.ReadPage("2026-10-29",0).Records.Single().Id==record.Id,"restart reads persistent records");
                reopened.Add(record);reopened.Flush();check(Directory.GetFiles(folder,"*.json",SearchOption.AllDirectories).Length==56,"restart duplicate event remains a single file");
                var corrupt=Path.Combine(folder,"2026-10-28");Directory.CreateDirectory(corrupt);File.WriteAllText(Path.Combine(corrupt,"bad.json"),"broken");
                try{reopened.ReadPage("2026-10-28",0);check(false,"corruption reported");}catch(System.Text.Json.JsonException){check(true,"corruption reported without deleting data");}
            }
            var blocked=Path.Combine(folder,"blocked");File.WriteAllText(blocked,"x");
            using(var history=new ScoreHistory(blocked))
            {
                history.Add(record);history.Flush();check(history.Error!=null && history.Generation==0,"failed saves stop after bounded attempts retaining pending record");
                File.Delete(blocked);history.Retry();history.Flush();check(history.Error==null && history.ReadPage("2026-10-29",0).Records.Length==1,"explicit retry saves pending result after recovery");
            }
            var disposed=new ScoreHistory(Path.Combine(folder,"dispose"));disposed.Add(record);disposed.Dispose();
            check(Directory.GetFiles(Path.Combine(folder,"dispose"),"*.json",SearchOption.AllDirectories).Length==1,"dispose flushes queued result");
            try{disposed.Add(record);check(false,"no writes after dispose");}catch(ObjectDisposedException){check(true,"no writes after dispose");}
            var resumeFolder=Path.Combine(folder,"resume");Guid sessionId;
            using(var history=new ScoreHistory(resumeFolder)){sessionId=history.BeginSession("Me").Id;history.Flush();}
            using(var history=new ScoreHistory(resumeFolder))
            {
                var marker=history.BeginSession("Me");check(marker.Id==sessionId && marker.Result==null,"reload mid-game keeps durable session identity");
                history.Add(record with {Id=sessionId});history.Flush();
            }
            using(var history=new ScoreHistory(resumeFolder))
            {
                var marker=history.BeginSession("Me");var restored=new ScoreSession();restored.Begin("Me",marker.Id,marker.Result);
                var repeated=0;restored.Observe(9999,null,true,rows,_=>repeated++);restored.End(_=>repeated++);history.Flush();
                check(marker.Result!=null && repeated==0 && history.ReadPage("2026-10-29",0).Records.Length==1,"reload on results replays journal idempotently without duplicate result");
                history.EndSession();history.Flush();
            }
            using(var history=new ScoreHistory(resumeFolder)){check(history.BeginSession("Me").Id!=sessionId,"new game after exit gets a new session identity");}
        }
        finally{Directory.Delete(folder,true);}
    }
}
