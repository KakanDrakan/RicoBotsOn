using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public record WallEdge(int X, int Y, Direction Side);
    public record BotStart(int X, int Y);

    public record BoardDefinition(
        int Width,
        int Height,
        IReadOnlyList<WallEdge> Walls,
        IReadOnlyList<(int X, int Y)> Targets,
        IReadOnlyList<BotStart> Bots);
}
