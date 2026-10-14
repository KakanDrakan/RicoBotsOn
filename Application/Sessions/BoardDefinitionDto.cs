using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Sessions
{
    public record WallDto(int X, int Y, string Side);
    public record BoardTargetDto(int X, int Y);
    public record BoardBotDto(int X, int Y);

    public record BoardDefinitionDto(
        int Version,
        int Width,
        int Height,
        List<WallDto> Walls,
        List<BoardTargetDto> Targets,
        List<BoardBotDto> Bots);
}
