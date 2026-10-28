namespace KeyboundPilot;

internal sealed class GlyphCoverage
{
    public static string TypingCharacters => new string(Enumerable.Range(32,95).Select(x=>(char)x).ToArray())
        + "ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔゕゖー～〜";
    private readonly HashSet<char> characters = [];
    public int Count=>characters.Count;
    public string Text => new(characters.Order().ToArray());

    public bool Include(IEnumerable<string> text)
    {
        var changed = false;
        foreach (var value in text)
            foreach (var character in value)
                if (!char.IsControl(character) && !char.IsSurrogate(character))
                    changed |= characters.Add(character);
        return changed;
    }
}
