using System.Runtime.InteropServices;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Component.GUI;
namespace KeyboundPilot;

internal static unsafe class PromptDisplayReader
{
    // Read-only KTGEWord display entries, independently inspected for the current client.
    // Never assume number-array slot order equals the recycled display-component order.
    [StructLayout(LayoutKind.Explicit,Size=0xC8)]
    private struct Entry
    {
        [FieldOffset(0)] public AtkComponentNode* Root;
        [FieldOffset(0xA8)] public uint EntityId;
    }
    public static IReadOnlyList<PromptDisplay> Read(AtkUnitBase* addon,IReadOnlyList<KtgWord> current)
    {
        var result=new List<PromptDisplay>();
        if(addon==null || !addon->IsVisible || !Visible(addon->RootNode))return result;
        var manager=&addon->UldManager;
        if(manager->NodeList==null || manager->NodeListCount>512)return result;
        var entries=(Entry*)((byte*)addon+0x238);
        for(var i=0;i<20;i++)
        {
            var entry=entries[i];
            var word=current.FirstOrDefault(x=>x.EntityId==entry.EntityId && x.Visible && x.Word.Length>0);
            if(word==null || entry.Root==null)continue;
            var known=false;
            for(var j=0;j<manager->NodeListCount;j++)
                if(manager->NodeList[j]==(AtkResNode*)entry.Root && (int)manager->NodeList[j]->Type>=1000) {known=true;break;}
            if(!known || entry.Root->Component==null)continue;
            var component=entry.Root->Component;
            var label=component->GetTextNodeById(20);var reading=component->GetTextNodeById(22);
            // Verify both the entity and reading: UI can lag an array text replacement by a frame.
            var readingText=Text(reading);
            var matches=readingText==word.Word || (readingText.Length==0 && Text(label)==word.Word)
                || (word.Word=="レッドXIII" && TypingPlan.IsRedXiii(Text(label)));
            var visible=matches && Visible((AtkResNode*)entry.Root)
                && (Visible((AtkResNode*)label) || Visible((AtkResNode*)reading));
            result.Add(new(entry.EntityId,word.Word,visible));
        }
        return result;
    }
    private static string Text(AtkTextNode* node)
    {
        if(node==null)return "";var text=node->NodeText.AsSpan();
        return text.Length<=512?SeString.Parse(text).TextValue.Trim():"";
    }
    private static bool Visible(AtkResNode* node)
    {
        if(node==null)return false;
        var depth=0;
        for(;node!=null && depth++<32;node=node->ParentNode)
            if(!node->IsVisible() || node->Color.A==0 || node->ScaleX==0 || node->ScaleY==0)return false;
        return depth<32;
    }
}
