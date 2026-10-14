using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Sessions
{
    public static class BoardDefinitionMapper
    {
        public const int SupportedVersion = 1;

        public static BoardDefinition ToDomain(BoardDefinitionDto dto)
        {
            if (dto.Version != SupportedVersion)
                throw new InvalidOperationException($"Unsupported board version {dto.Version}.");

            return new BoardDefinition(
                dto.Width,
                dto.Height,
                dto.Walls.Select(w => new WallEdge(w.X, w.Y, ParseSide(w.Side))).ToList(),
                dto.Targets.Select(t => (t.X, t.Y)).ToList(),
                dto.Bots.Select(b => new BotStart(b.X, b.Y)).ToList());
        }

        private static Direction ParseSide(string? side) => side switch
        {
            "E" => Direction.East,
            "S" => Direction.South,
            _ => throw new InvalidOperationException("Wall side must be \"E\" or \"S\".")
        };
    }
}
