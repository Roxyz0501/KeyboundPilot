using System.Text.Json;
namespace KeyboundPilot;

public sealed class DiagnosticDelta
{
    private int[]? previousNumbers;
    private string[]? previousStrings;
    private readonly Dictionary<string,string> previousUi=[];
    private long sample;
    public void Reset() {previousNumbers=null;previousStrings=null;previousUi.Clear();sample=0;}
    public string? Capture(int[] numbers,string[] strings,IReadOnlyDictionary<string,object> ui)
    {
        var numericChanges=new List<int[]>();var textChanges=new List<object>();var uiChanges=new Dictionary<string,JsonElement>();
        for(var i=0;i<numbers.Length;i++)if(previousNumbers==null || previousNumbers[i]!=numbers[i])numericChanges.Add([i,numbers[i]]);
        for(var i=0;i<strings.Length;i++)if(previousStrings==null || previousStrings[i]!=strings[i])textChanges.Add(new {index=i,value=strings[i]});
        foreach(var (key,value) in ui)
        {
            var json=JsonSerializer.Serialize(value);
            if(!previousUi.TryGetValue(key,out var prior) || prior!=json) {uiChanges[key]=JsonSerializer.Deserialize<JsonElement>(json);previousUi[key]=json;}
        }
        if(numericChanges.Count==0 && textChanges.Count==0 && uiChanges.Count==0)return null;
        var full=previousNumbers==null;previousNumbers=(int[])numbers.Clone();previousStrings=(string[])strings.Clone();
        return JsonSerializer.Serialize(new {sample=++sample,full,numericChanges,textChanges,ui=uiChanges});
    }
}
