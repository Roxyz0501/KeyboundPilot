using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Component.GUI;
namespace KeyboundPilot;

internal sealed record KtgWord(uint EntityId, string Word, int Slot);
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
            var word=Read(s->StringArray[3*slot+1]) ?? Read(s->StringArray[3*slot]);
            if(word!=null)result.Add(new(id,word,slot));
        }
        words=result;return true;
    }
    private static string? Read(byte* ptr)
    {
        if(ptr==null)return null;
        var len=0;while(len<512 && ptr[len]!=0)len++;
        if(len==512)return null;
        var text=SeString.Parse(new ReadOnlySpan<byte>(ptr,len)).TextValue.Trim();
        return text.Length is >0 and <=96 && text.All(c=>char.IsAsciiLetterOrDigit(c) || c is ' ' or '-' or '\'') ? text : null;
    }
}
