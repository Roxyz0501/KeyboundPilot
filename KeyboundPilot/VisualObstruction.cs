namespace KeyboundPilot;
internal static class VisualObstruction
{
    // Japanese Status sheet: both rows are 視覚妨害. Read the local player status,
    // not prompt opacity, so temporary visible portions retain the same interval.
    public static bool IsStatus(uint id) => id is 5348 or 5472;
}
