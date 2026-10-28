using Dalamud;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Plugin.Services;
namespace KeyboundPilot;

internal sealed class LocalizedFonts : IDisposable
{
    private IFontHandle[] fonts=[];
    private IFontHandle[] pending=[];
    private readonly IFontAtlas atlas;
    private readonly IPluginLog log;
    private readonly GlyphCoverage coverage=new();
    private string? builtText;
    private bool failed;
    public bool Available=>pending.Length==0 && fonts.Length==4 && fonts.All(x=>x.Available);
    public bool Failed=>failed;
    public LocalizedFonts(IFontAtlas atlas,IPluginLog log,IEnumerable<string> initialText)
    {
        this.atlas=atlas;this.log=log;
        Ensure(initialText.Prepend(Loc.FontText+GlyphCoverage.TypingCharacters));
    }
    // Called before any Push. Keep working handles until every replacement is ready.
    public void Ensure(IEnumerable<string> text)
    {
        var changed=coverage.Include(text);
        if(failed)return;
        if(pending.Length>0)
        {
            var error=pending.Select(x=>x.LoadException).FirstOrDefault(x=>x!=null);
            if(error!=null) {failed=true;log.Error(error,"Keybound Pilot UI font build failed");foreach(var f in pending)f.Dispose();pending=[];return;}
            if(pending.All(x=>x.Available))
            {
                var previous=fonts;fonts=pending;pending=[];
                foreach(var f in previous)f.Dispose();
            }
            else return;
        }
        if(!changed && builtText!=null && coverage.Count==builtText.Length)return;
        var fontText=coverage.Text;if(fontText==builtText)return;
        var ranges=fontText.ToGlyphRange();var replacement=new List<IFontHandle>();
        using var batch=atlas.SuppressAutoRebuild();
        try
        {
            for(var face=0;face<4;face++)
            {
                var fontFace=face;
                replacement.Add(atlas.NewDelegateFontHandle(step=>step.OnPreBuild(tk=>
                    tk.Font=tk.AddDalamudAssetFont(DalamudAsset.NotoSansCjkRegular,
                        new SafeFontConfig{SizePx=17,FontNo=fontFace,GlyphRanges=ranges}))));
            }
        }
        catch {foreach(var f in replacement)f.Dispose();throw;}
        pending=replacement.ToArray();builtText=fontText;
    }
    private static int Face(string? code)=>code switch {"zh-Hant"=>1,"zh-Hans"=>2,"ko"=>3,_=>0};
    public IDisposable? Push(string? code)=>Available?fonts[Face(code)].Push():null;
    public void Dispose() {foreach(var f in pending)f.Dispose();foreach(var f in fonts)f.Dispose();pending=[];fonts=[];}
}
