using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public static class BoardValidator
    {
        public const int MinSize = 5;
        public const int MaxSize = 20;
        public const int MaxBots = 10;

        public static void Validate(BoardDefinition def)
        {
            ValidateSize(def);
            ValidateWalls(def);
            ValidateBots(def);
            ValidateTargets(def);
        }

        private static void ValidateSize(BoardDefinition def)
        {
            if (def.Width < MinSize || def.Width > MaxSize || def.Height < MinSize || def.Height > MaxSize)
                throw Invalid($"Board size must be {MinSize} to {MaxSize} in both directions.");
        }

        private static void ValidateWalls(BoardDefinition def)
        {
            if (def.Walls.Count > def.Width * def.Height * 2)
                throw Invalid("Too many walls.");

            foreach (var wall in def.Walls)
            {
                if (wall.Side != Direction.East && wall.Side != Direction.South)
                    throw Invalid("Walls must use the East or South side.");

                if (!InBounds(def, wall.X, wall.Y))
                    throw Invalid($"Wall at ({wall.X}, {wall.Y}) is outside the board.");
            }
        }

        private static void ValidateBots(BoardDefinition def)
        {
            if (def.Bots.Count < 1 || def.Bots.Count > MaxBots)
                throw Invalid($"A board needs 1 to {MaxBots} bots.");

            if (def.Bots.Any(b => !InBounds(def, b.X, b.Y)))
                throw Invalid("A bot is outside the board.");

            if (def.Bots.Distinct().Count() != def.Bots.Count)
                throw Invalid("Two bots cannot start on the same cell.");
        }

        private static void ValidateTargets(BoardDefinition def)
        {
            if (def.Targets.Count == 0)
                throw Invalid("A board needs at least one target.");

            if (def.Targets.Any(t => !InBounds(def, t.X, t.Y)))
                throw Invalid("A target is outside the board.");

            if (def.Targets.Distinct().Count() != def.Targets.Count)
                throw Invalid("Targets must be unique.");

            if (!def.Targets.Any(t => def.Bots.Any(b => b.X != t.X || b.Y != t.Y)))
                throw Invalid("Every target is occupied by the only bot.");
        }

        private static bool InBounds(BoardDefinition def, int x, int y) =>
            x >= 0 && x < def.Width && y >= 0 && y < def.Height;

        private static InvalidOperationException Invalid(string message) => new(message);
    }
}
