using Domain;
using RicoBotsOn.Tests;
using Xunit;

namespace RicochetRobots.Domain.Tests;

public class MatchProvingTests
{
    private static Match NewProvingMatch(out Bot mover)
    {
        var board = TestHelpers.FiveByFiveNoWalls();
        board.AddTarget(4, 0);  // one move east from (0,0)
        mover = new Bot("b1", 0, 0);
        var match = new Match(board, new List<Bot> { mover });
        match.SelectNewTarget(new Random(1));  // only one bot, so it's always the target
        match.SubmitClaim("player1", 1);
        TestHelpers.CloseClaimWindow(match);
        return match;
    }

    [Fact]
    public void EnsureProvingStarted_DoesNothing_WhileTheWindowIsStillOpen()
    {
        var board = TestHelpers.FiveByFiveNoWalls();
        board.AddTarget(4, 0);
        var match = new Match(board, new List<Bot> { new("b1", 0, 0) });
        match.SelectNewTarget(new Random(1));
        match.SubmitClaim("player1", 1);

        match.EnsureProvingStarted();

        Assert.Equal(RoundPhase.WaitingForClaims, match.Phase);
    }

    [Fact]
    public void EnsureProvingStarted_TransitionsToProving_OnceTheWindowHasClosed()
    {
        var match = NewProvingMatch(out _);

        match.EnsureProvingStarted();

        Assert.Equal(RoundPhase.Proving, match.Phase);
    }

    [Fact]
    public void ApplyProvingMove_ByTheWrongPlayer_Throws()
    {
        var match = NewProvingMatch(out var mover);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            match.ApplyProvingMove("someone-else", mover.Id, Direction.East));
        Assert.Contains("not your turn", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyProvingMove_ReachingTheTargetWithinTheClaim_SucceedsAndScoresAPoint()
    {
        var match = NewProvingMatch(out var mover);

        match.ApplyProvingMove("player1", mover.Id, Direction.East);

        Assert.Equal(RoundPhase.RoundOver, match.Phase);
        Assert.True(match.RoundSucceeded);
        Assert.Equal(1, match.Scores["player1"]);
    }

    [Fact]
    public void ApplyProvingMove_UsingUpTheClaimWithoutReachingTarget_AdvancesToTheNextClaimant()
    {
        var board = TestHelpers.FiveByFiveNoWalls();
        board.AddTarget(4, 4);  // needs two moves (east, then south) from (0,0)
        var mover = new Bot("b1", 0, 0);
        var match = new Match(board, new List<Bot> { mover });
        match.SelectNewTarget(new Random(1));
        match.SubmitClaim("player1", 1);  // impossible in one move
        match.SubmitClaim("player2", 2);
        TestHelpers.CloseClaimWindow(match);

        match.ApplyProvingMove("player1", mover.Id, Direction.East);  // uses the only move, misses

        Assert.Equal(RoundPhase.Proving, match.Phase);
        Assert.Equal(0, match.CurrentAttempt!.MoveCount);  // reset for the next claimant
        Assert.Equal((0, 0), (mover.X, mover.Y));  // bot snapped back to round start
    }

    [Fact]
    public void ApplyProvingMove_LastClaimantFailing_EndsTheRoundWithNoWinner()
    {
        var board = TestHelpers.FiveByFiveNoWalls();
        board.AddTarget(4, 4);  // needs two moves; only one was claimed
        var mover = new Bot("b1", 0, 0);
        var match = new Match(board, new List<Bot> { mover });
        match.SelectNewTarget(new Random(1));
        match.SubmitClaim("player1", 1);
        TestHelpers.CloseClaimWindow(match);

        match.ApplyProvingMove("player1", mover.Id, Direction.East);  // uses the only move, misses

        Assert.Equal(RoundPhase.RoundOver, match.Phase);
        Assert.False(match.RoundSucceeded);
    }

    [Fact]
    public void ResetCurrentProverAttempt_ZeroesMoveCountAndRestoresPosition_WithoutAdvancing()
    {
        var board = TestHelpers.FiveByFiveNoWalls();
        board.AddTarget(4, 4);
        var mover = new Bot("b1", 0, 0);
        var match = new Match(board, new List<Bot> { mover });
        match.SelectNewTarget(new Random(1));
        match.SubmitClaim("player1", 5);
        TestHelpers.CloseClaimWindow(match);
        match.ApplyProvingMove("player1", mover.Id, Direction.East);  // now at (4, 0), one move used

        match.ResetCurrentProverAttempt("player1");

        Assert.Equal(RoundPhase.Proving, match.Phase);
        Assert.Equal(0, match.CurrentAttempt!.MoveCount);
        Assert.Equal((0, 0), (mover.X, mover.Y));
    }
}