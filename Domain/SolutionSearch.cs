using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public static class SolutionSearch
    {
        public static bool CanReach(Board board, (int X, int Y)[] positions, int botIndex, (int X, int Y) target, int maxMoves)
        {
            if (maxMoves <= 0) return false;
            bool last = maxMoves == 1;

            for (int b = 0; b < positions.Length; b++)
            {
                if (last && b != botIndex) continue;  // final move is always the target bot's

                for (int d = 0; d < 4; d++)
                {
                    var from = positions[b];
                    var to = MoveResolver.Resolve(board, positions, b, (Direction)d);
                    if (to == from) continue;

                    if (b == botIndex && to == target) return true;
                    if (last) continue;

                    positions[b] = to;
                    bool found = CanReach(board, positions, botIndex, target, maxMoves - 1);
                    positions[b] = from;
                    if (found) return true;
                }
            }
            return false;
        }
    }
}
