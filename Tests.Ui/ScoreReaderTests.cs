using FFXIVClientStructs.FFXIV.Component.GUI;
using KeyboundPilot;
unsafe partial class Program
{
    static void CheckScoreReader()
    {
        void Check(bool value,string message){if(!value)throw new Exception(message);}
        AtkResNode root=default;root.NodeFlags=NodeFlags.Visible;root.Color.A=255;
        AtkUnitBase addon=default;addon.RootNode=&root;addon.IsVisible=true;addon.Flags1A1=1;
        AtkComponentBase component=default;AtkComponentNode owner=default;
        owner.NodeId=30;owner.Type=(NodeType)1005;owner.NodeFlags=NodeFlags.Visible;owner.Color.A=255;owner.ParentNode=&root;
        owner.Component=&component;component.OwnerNode=&owner;
        AtkTextNode name=default,score=default;
        name.NodeId=9;score.NodeId=11;name.Type=score.Type=NodeType.Text;
        name.NodeFlags=score.NodeFlags=NodeFlags.Visible;name.Color.A=score.Color.A=255;
        name.ParentNode=score.ParentNode=(AtkResNode*)&owner;
        var nameBytes=stackalloc byte[]{77,101,0};var scoreBytes=stackalloc byte[]{49,50,44,51,52,53,0};
        name.NodeText.StringPtr=nameBytes;name.NodeText.BufSize=name.NodeText.BufUsed=3;
        score.NodeText.StringPtr=scoreBytes;score.NodeText.BufSize=score.NodeText.BufUsed=7;
        AtkResNode** addonNodes=stackalloc AtkResNode*[1];addonNodes[0]=(AtkResNode*)&owner;
        AtkResNode** textNodes=stackalloc AtkResNode*[2];textNodes[0]=(AtkResNode*)&name;textNodes[1]=(AtkResNode*)&score;
        addon.UldManager.NodeList=addonNodes;addon.UldManager.NodeListCount=1;
        component.UldManager.NodeList=textNodes;component.UldManager.NodeListCount=2;
        var sample=ScoreReader.Read(null,&addon,"Me");
        Check(sample.ResultVisible && sample.Rows.Length==1 && sample.Rows[0].Score==12345 && sample.Rows[0].IsSelf,"result names and score decoded");
        Check(ScoreReader.Read(null,&addon,"Someone Else").Rows[0].IsSelf==false,"unknown/abbreviated identity not guessed");
        score.Color.A=0;Check(ScoreReader.Read(null,&addon,"Me").Rows.Length==0,"hidden score text excluded");score.Color.A=255;
        owner.NodeFlags=0;Check(ScoreReader.Read(null,&addon,"Me").Rows.Length==0,"hidden row excluded");owner.NodeFlags=NodeFlags.Visible;
        score.NodeText.BufUsed=1024;Check(ScoreReader.Read(null,&addon,"Me").Rows.Length==0,"oversized text rejected");score.NodeText.BufUsed=7;
        component.UldManager.NodeListCount=1025;Check(ScoreReader.Read(null,&addon,"Me").Rows.Length==0,"invalid node array bounded");component.UldManager.NodeListCount=2;
        owner.Type=NodeType.Text;Check(ScoreReader.Read(null,&addon,"Me").Rows.Length==0,"non-component node not cast");owner.Type=(NodeType)1005;
        owner.NodeId=2;score.NodeId=5;
        sample=ScoreReader.Read(&addon,null,"Me");Check(sample.Live==12345 && !sample.ResultVisible,"live score uses HUD component 2 text 5");
        addon.IsVisible=false;
        sample=ScoreReader.Read(&addon,&addon,"Me");Check(sample.Live==null && !sample.ResultVisible && sample.Rows.Length==0,"hidden addons supply no scores");
        Console.WriteLine("PASS: native score node fixture, hidden rows, bounded arrays/text, self identity and live HUD mapping");
    }
}
