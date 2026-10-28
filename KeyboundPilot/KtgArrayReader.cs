using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Component.GUI;
namespace KeyboundPilot;

internal static unsafe class KtgArrayReader
{
    public const int Slots=20;
    public static bool TryRead(out IReadOnlyList<KtgWord> words)
    {
        words=[];
        var stage=AtkStage.Instance();if(stage==null || stage->AtkArrayDataHolder==null)return false;
        var h=stage->AtkArrayDataHolder;
        if(h->NumberArrayCount<=161 || h->StringArrayCount<=128 || h->NumberArrays==null || h->StringArrays==null)return false;
        var n=h->NumberArrays[161];var s=h->StringArrays[128];
        if(n==null || s==null || n->IntArray==null || s->StringArray==null || n->Size<261 || n->Size>1024 || s->Size<60 || s->Size>256 || n->IntArray[0]<0 || n->IntArray[0]>Slots)return false;
        var result=new List<KtgWord>();
        for(var slot=0;slot<n->IntArray[0];slot++)
        {
            var id=unchecked((uint)n->IntArray[1+13*slot]);
            if(id is 0 or 0xE0000000)continue;
            var label=ReadText(s->StringArray[3*slot]);
            var word=TypingPlan.IsRedXiii(label)?"レッドXIII":Read(s->StringArray[3*slot+1]) ?? Read(s->StringArray[3*slot]);
            result.Add(new(id,word ?? "",slot,ReadText(s->StringArray[3*slot+2]),n->IntArray[1+13*slot+7],n->IntArray[1+13*slot+11],n->IntArray[1+13*slot+10],n->IntArray[1+13*slot+4]!=0,n->IntArray[1+13*slot+1],n->IntArray[1+13*slot+2]));
        }
        words=result;return true;
    }
    private static string ReadText(byte* ptr)
    {
        if(ptr==null)return "";var length=0;while(length<512 && ptr[length]!=0)length++;
        return length==512?"":SeString.Parse(new ReadOnlySpan<byte>(ptr,length)).TextValue.Trim();
    }
    private static string? Read(byte* ptr)
    {
        if(ptr==null)return null;
        var len=0;while(len<512 && ptr[len]!=0)len++;
        if(len==512)return null;
        var text=SeString.Parse(new ReadOnlySpan<byte>(ptr,len)).TextValue.Trim();
        return TypingPlan.Create(text)!=null ? text : null;
    }
}
