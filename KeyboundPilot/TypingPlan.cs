using System.Text;
namespace KeyboundPilot;

public sealed record TypingPlan(string Expected, string Keys, bool Japanese)
{
    public bool ExactCase {get;init;}
    public int LatinModeIndex => Keys.IndexOf(TypingSession.LatinModeKey);
    public static bool IsRedXiii(string text)=>text.Trim().Normalize(NormalizationForm.FormKC).Equals("レッドXIII",StringComparison.OrdinalIgnoreCase)
        || text.Trim().Normalize(NormalizationForm.FormKC).Equals("れっどXIII",StringComparison.OrdinalIgnoreCase);
    private static readonly Dictionary<char,string> Kana = BuildKana();
    private static Dictionary<char,string> BuildKana()
    {
        var result=new Dictionary<char,string>();
        void Add(string chars,string syllables) { var parts=syllables.Split(' ');for(var i=0;i<chars.Length;i++)result.Add(chars[i],parts[i]); }
        Add("あいうえお","a i u e o");Add("かきくけこ","ka ki ku ke ko");Add("がぎぐげご","ga gi gu ge go");
        Add("さしすせそ","sa shi su se so");Add("ざじずぜぞ","za ji zu ze zo");Add("たちつてと","ta chi tsu te to");Add("だぢづでど","da di du de do");
        Add("なにぬねの","na ni nu ne no");Add("はひふへほ","ha hi fu he ho");Add("ばびぶべぼ","ba bi bu be bo");Add("ぱぴぷぺぽ","pa pi pu pe po");
        Add("まみむめも","ma mi mu me mo");Add("やゆよ","ya yu yo");Add("らりるれろ","ra ri ru re ro");Add("わをん","wa wo n'");
        // Explicit small-kana sequences avoid ambiguity in contractions, geminates and foreign syllables.
        Add("ぁぃぅぇぉゃゅょっゎゔ","xa xi xu xe xo xya xyu xyo xtsu xwa vu");Add("ー","-");Add("～〜","~ ~");
        return result;
    }
    // Some IMEs keep vu as katakana even in hiragana mode; the reading is identical.
    public static string NormalizeAnswer(string text)=>text.Replace('〜','～').Normalize(NormalizationForm.FormKC).Replace('ヴ','ゔ');
    public static TypingPlan? Create(string text)
    {
        text=text.Trim();if(text.Length is 0 or >96)return null;
        if(IsRedXiii(text))return new("レッドXIII","rextsudo"+TypingSession.KatakanaKey+'\r'+TypingSession.LatinModeKey+"XIII",true){ExactCase=true};
        var japanese=text.Any(c=>c is >= 'ぁ' and <= 'ゖ' or 'ー' or '～' or '〜');
        var keys=new StringBuilder();
        foreach(var c in text)
        {
            if(Kana.TryGetValue(c,out var k))keys.Append(k);
            else if(char.IsAsciiLetterOrDigit(c) || c is ' ' or '-' or '\'' or '~')keys.Append(c);
            else return null; // Never silently discard an unknown character.
        }
        return keys.Length<=384 ? new(text,keys.ToString(),japanese) : null;
    }
}

