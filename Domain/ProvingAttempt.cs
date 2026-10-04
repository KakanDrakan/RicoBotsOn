using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class ProvingAttempt
    {
        public string PlayerId { get; }
        public int ClaimedMoveCount { get; }
        public int MoveCount { get; private set; }

        public ProvingAttempt(string playerId, int claimedMoveCount)
        {
            PlayerId = playerId;
            ClaimedMoveCount = claimedMoveCount;
        }

        public void RegisterMove() => MoveCount++;
        public void ResetMoveCount() => MoveCount = 0;

        public bool Succeeds() => MoveCount <= ClaimedMoveCount;
        public bool HasUsedAllMoves => MoveCount >= ClaimedMoveCount;
        public void UndoMove() => MoveCount = Math.Max(0, MoveCount - 1);
    }
}
