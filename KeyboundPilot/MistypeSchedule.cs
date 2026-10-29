namespace KeyboundPilot;

/// <summary>Counts submitted answers, never keys, IME commits or abandoned targets.</summary>
public sealed class MistypeSchedule
{
    private readonly Func<int,int,int> random;
    private bool enabled;
    private int minimum=15,maximum=25;
    public int Remaining {get;private set;}
    public bool Due => enabled && Remaining==1;
    public MistypeSchedule(Func<int,int,int>? sample=null) => random=sample??Random.Shared.Next;
    public bool Configure(bool value,int count) => Configure(value,Math.Max(1,Math.Clamp(count,1,9999)-5),Math.Clamp(count,1,9999)+5);
    public bool Configure(bool value,int min,int max)
    {
        min=Math.Clamp(min,1,10004);max=Math.Clamp(max,min,10004);
        if(enabled==value && minimum==min && maximum==max)return false;
        enabled=value;minimum=min;maximum=max;Reset();return true;
    }
    public void Reset() => Remaining=enabled?random(minimum,maximum+1):0;
    public void Submitted()
    {
        if(!enabled)return;
        if(Remaining<=1)Reset();else Remaining--;
    }
    public static TypingPlan? WithMistake(TypingPlan original)
    {
        if(original.ExactCase)return original with {Expected=original.Expected[..^1]+"A",Keys=original.Keys[..^1]+"A"};
        var text=original.Expected.ToCharArray();
        for(var i=0;i<text.Length;i++)
        {
            var c=text[i];
            if(original.Japanese)
            {
                if(c is not (>= 'ぁ' and <= 'ゖ' or 'ー' or '～' or '〜'))continue;
                text[i]=c=='あ'?'い':'あ';
            }
            else
            {
                if(!char.IsAsciiLetterOrDigit(c))continue;
                text[i]=c is 'a' or 'A'?'b':'a';
            }
            return TypingPlan.Create(new string(text));
        }
        // Supported punctuation-only prompts can still receive a one-character error.
        text[0]=original.Japanese?'あ':'a';
        return TypingPlan.Create(new string(text));
    }
}
