using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RicoBotsOn.Tests
{
    public class GameSessionClaimTests
    {
        private static GameSession NewSessionWithTarget()
        {
            var board = TestHelpers.FiveByFiveNoWalls();
            board.AddTarget(4, 4);
            var session = new GameSession("test", board, new List<Bot> { new("b1", 0, 0) });
            session.SelectNewTarget(new Random(1));
            return session;
        }

        [Fact]
        public void SubmitClaim_FirstClaim_OpensA30SecondWindow()
        {
            var session = NewSessionWithTarget();

            session.SubmitClaim("player1", 5);

            Assert.NotNull(session.ClaimWindow.ActiveDeadlineUtc);
            var remaining = session.ClaimWindow.ActiveDeadlineUtc!.Value - DateTime.UtcNow;
            Assert.InRange(remaining.TotalSeconds, 29, 30);
        }

        [Fact]
        public void SubmitClaim_AllowsAClaimWorseThanTheCurrentBest()
        {
            var session = NewSessionWithTarget();
            session.SubmitClaim("player1", 3);

            session.SubmitClaim("player2", 10);

            Assert.Equal(2, session.ClaimWindow.Claims.Count);
        }

        [Fact]
        public void SubmitClaim_IgnoresAnExactDuplicateFromTheSamePlayer()
        {
            var session = NewSessionWithTarget();
            session.SubmitClaim("player1", 5);

            session.SubmitClaim("player1", 5);

            Assert.Single(session.ClaimWindow.Claims);
        }

        [Fact]
        public void SubmitClaim_SameCountFromADifferentPlayer_IsAllowed()
        {
            var session = NewSessionWithTarget();
            session.SubmitClaim("player1", 5);

            session.SubmitClaim("player2", 5);

            Assert.Equal(2, session.ClaimWindow.Claims.Count);
        }

        [Fact]
        public void SubmitClaim_AfterTheWindowHasClosed_Throws()
        {
            var session = NewSessionWithTarget();
            session.SubmitClaim("player1", 5);
            TestHelpers.CloseClaimWindow(session);

            Assert.Throws<InvalidOperationException>(() => session.SubmitClaim("player2", 3));
        }

        [Fact]
        public void ClaimsInProvingOrder_SortsByMoveCountThenByClaimTime()
        {
            var session = NewSessionWithTarget();
            session.SubmitClaim("player1", 8);
            session.SubmitClaim("player2", 3);
            session.SubmitClaim("player3", 3);

            var order = session.ClaimWindow.ClaimsInProvingOrder;

            Assert.Equal(new[] { "player2", "player3", "player1" }, order.Select(c => c.PlayerId));
        }
    }
}
