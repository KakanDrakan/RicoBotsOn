namespace Application.Sessions
{
    public record BoardStateResponse(
    int Width,
    int Height,
    List<CellWalls> Walls,
    List<TargetCell> Targets);

    public record CellWalls(int X, int Y, bool North, bool East, bool South, bool West);
    public record BotState(string Id, int X, int Y);
    public record TargetCell(int X, int Y);
    public record ActiveTarget(int X, int Y, string BotId);
}

