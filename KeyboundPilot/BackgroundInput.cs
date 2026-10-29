using System.Text;
namespace KeyboundPilot;

/// <summary>Simulate the existing romaji schedule without using the operating system IME.</summary>
public static class BackgroundInput
{
    public static string Steps(TypingPlan plan)
    {
        var result=new StringBuilder();
        // Preserve the exact Red XIII spelling (including the intentional typo variant).
        if(plan.ExactCase)
        {
            Add('レ',2);Add('ッ',4);Add('ド',2);
            result.Append(TypingSession.InternalCommitKey,3); // F7, prefix commit, Latin mode.
            result.Append(plan.Expected.AsSpan(3));
        }
        else
        {
            foreach(var c in plan.Expected)
            {
                var single=TypingPlan.Create(c.ToString());
                Add(c,single?.Keys.Length ?? 1);
            }
        }
        if(plan.Japanese)
        {
            if(plan.Expected.Contains('ゔ'))result.Append(TypingSession.InternalCommitKey);
            result.Append(TypingSession.InternalCommitKey);
        }
        return result.ToString();
        void Add(char c,int length)
        {result.Append(TypingSession.InternalWaitKey,Math.Max(0,length-1));result.Append(c);}
    }
}
