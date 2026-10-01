using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RicoBotsOn.Tests
{
    public class GameSessionRoundTests
    {
        [Fact]
        public void StartNextRound_BeforeTheRoundIsOver_Throws()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            board.AddTarget(4, 4);
            var session = new GameSession("test", board, new List<Bot> { new("b1", 0, 0) });
            session.SelectNewTarget(new Random(1));

            Assert.Throws<InvalidOperationException>(() => session.StartNextRound(new Random(1)));
        }

        [Fact]
        public void StartNextRound_AfterASuccess_IncrementsTheRoundNumberAndPicksANewTarget()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            board.AddTarget(4, 0);
            board.AddTarget(4, 4);
            var mover = new Bot("b1", 0, 0);
            var session = new GameSession("test", board, new List<Bot> { mover });
            session.SelectNewTarget(new Random(1));
            session.SubmitClaim("player1", 1);
            TestHelpers.CloseClaimWindow(session);
            session.ApplyProvingMove("player1", mover.Id, Direction.East);  // solves it
            var roundBefore = session.RoundNumber;

            session.StartNextRound(new Random(1));

            Assert.Equal(roundBefore + 1, session.RoundNumber);
            Assert.Equal(RoundPhase.WaitingForClaims, session.Phase);
            Assert.NotNull(session.ActiveTarget);
        }

        [Fact]
        public void StartNextRound_AfterAFailure_ResetsBotsBeforePickingANewTarget()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            board.AddTarget(4, 4);  // needs two moves; only one was claimed, so this fails
            var mover = new Bot("b1", 0, 0);
            var session = new GameSession("test", board, new List<Bot> { mover });
            session.SelectNewTarget(new Random(1));
            session.SubmitClaim("player1", 1);
            TestHelpers.CloseClaimWindow(session);
            session.ApplyProvingMove("player1", mover.Id, Direction.East);  // fails, bot left at (4, 0)

            session.StartNextRound(new Random(1));

            Assert.Equal((0, 0), (mover.X, mover.Y));
        }
    }
}
