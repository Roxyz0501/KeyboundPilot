namespace KeyboundPilot;

/// <summary>Monotonic scheduling. At most one character per tick; late frames never catch up in a burst.</summary>
public sealed class TypingSession
{
    private readonly Func<int, int, int> random;
    private long due;
    private string word = "";
    private int index;
    public bool Running { get; private set; }
    public string Status { get; private set; } = "Stopped";
    public string CurrentWord => word;
    public int Position => index;
    public long Sent { get; private set; }
    public TypingSession(Func<int, int, int>? sample = null) => random = sample ?? Random.Shared.Next;
    public void Start(long now) { Stop(); Running = true; Status = "Arming"; due = now + 3000; Sent = 0; }
    public void Stop(string reason = "Stopped") { Running = false; Status = reason; word = ""; index = 0; }
    public void Tick(long now, bool ready, string? candidate, bool currentStillVisible, int min, int max, Func<char, bool> send)
    {
        if (!Running || now < due) return;
        if (!ready) { Stop("FocusLost"); return; }
        if (word.Length > 0 && !currentStillVisible) { word = ""; index = 0; due = now + max; Status = "Waiting"; return; }
        if (word.Length > 0 && index >= word.Length)
        {
            // Do not retype a static/stale word. A vanished word or a different keyword is needed.
            Status = "Waiting";
            if (now - due > 3000) Stop("NoProgress");
            return;
        }
        if (word.Length == 0)
        {
            if (string.IsNullOrEmpty(candidate)) { Status = "Waiting"; due = now + 100; return; }
            if (candidate.Length > 96 || candidate.Any(c => c is < ' ' or > '~')) { Stop("Unsupported"); return; }
            word = candidate; index = 0;
            due = now + random(min, max + 1); Status = "Typing"; return;
        }
        if (!send(word[index])) { Stop("SendFailed"); return; }
        index++; Sent++; Status = "Typing"; due = now + random(min, max + 1);
    }
}
