using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RicoBotsOn.Tests
{
    public class GameSessionProvingTests
    {
        private static GameSession NewProvingSession(out Bot mover)
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            board.AddTarget(4, 0);  
            mover = new Bot("b1", 0, 0);
            var session = new GameSession("test", board, [mover]);
            session.SelectNewTarget(new Random(1));  
            session.SubmitClaim("player1", 1);
            TestHelpers.CloseClaimWindow(session);
            return session;
        }

        [Fact]
        public void EnsureProvingStarted_DoesNothing_WhileTheWindowIsStillOpen()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            board.AddTarget(4, 0);
            var session = new GameSession("test", board, new List<Bot> { new("b1", 0, 0) });
            session.SelectNewTarget(new Random(1));
            session.SubmitClaim("player1", 1);

            session.EnsureProvingStarted();

            Assert.Equal(RoundPhase.WaitingForClaims, session.Phase);
        }

        [Fact]
        public void EnsureProvingStarted_TransitionsToProving_OnceTheWindowHasClosed()
        {
            var session = NewProvingSession(out _);

            session.EnsureProvingStarted();

            Assert.Equal(RoundPhase.Proving, session.Phase);
        }

        [Fact]
        public void ApplyProvingMove_ByTheWrongPlayer_Throws()
        {
            var session = NewProvingSession(out var mover);

            var ex = Assert.Throws<InvalidOperationException>(() =>
            session.ApplyProvingMove("someone-else", mover.Id, Direction.East));
            Assert.Contains("not your turn", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ApplyProvingMove_ReachingTheTargetWithinTheClaim_SucceedsAndScoresAPoint()
        {
            var session = NewProvingSession(out var mover);

            session.ApplyProvingMove("player1", mover.Id, Direction.East);

            Assert.Equal(RoundPhase.RoundOver, session.Phase);
            Assert.True(session.RoundSucceeded);
            Assert.Equal(1, session.Scores["player1"]);
        }

        [Fact]
        public void ApplyProvingMove_UsingUpTheClaimWithoutReachingTarget_AdvancesToTheNextClaimant()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            board.AddTarget(4, 4);  // needs two moves (east, then south) from (0,0)
            var mover = new Bot("r1", 0, 0);
            var session = new GameSession("test", board, new List<Bot> { mover });
            session.SelectNewTarget(new Random(1));
            session.SubmitClaim("player1", 1);  // impossible in one move
            session.SubmitClaim("player2", 2);
            TestHelpers.CloseClaimWindow(session);

            session.ApplyProvingMove("player1", mover.Id, Direction.East);  // uses the only move, misses

            Assert.Equal(RoundPhase.Proving, session.Phase);
            Assert.Equal(0, session.CurrentAttempt!.MoveCount);  // reset for the next claimant
            Assert.Equal((0, 0), (mover.X, mover.Y));  // bot snapped back to round start
        }

        [Fact]
        public void ApplyProvingMove_LastClaimantFailing_EndsTheRoundWithNoWinner()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            board.AddTarget(4, 4);  // needs two moves; only one was claimed
            var mover = new Bot("r1", 0, 0);
            var session = new GameSession("test", board, new List<Bot> { mover });
            session.SelectNewTarget(new Random(1));
            session.SubmitClaim("player1", 1);
            TestHelpers.CloseClaimWindow(session);

            session.ApplyProvingMove("player1", mover.Id, Direction.East);  // uses the only move, misses

            Assert.Equal(RoundPhase.RoundOver, session.Phase);
            Assert.False(session.RoundSucceeded);
        }

        [Fact]
        public void ResetCurrentProverAttempt_ZeroesMoveCountAndRestoresPosition_WithoutAdvancing()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            board.AddTarget(4, 4);
            var mover = new Bot("r1", 0, 0);
            var session = new GameSession("test", board, new List<Bot> { mover });
            session.SelectNewTarget(new Random(1));
            session.SubmitClaim("player1", 5);
            TestHelpers.CloseClaimWindow(session);
            session.ApplyProvingMove("player1", mover.Id, Direction.East);  // now at (4, 0), one move used

            session.ResetCurrentProverAttempt("player1");

            Assert.Equal(RoundPhase.Proving, session.Phase);
            Assert.Equal(0, session.CurrentAttempt!.MoveCount);
            Assert.Equal((0, 0), (mover.X, mover.Y));
        }
    }
}
