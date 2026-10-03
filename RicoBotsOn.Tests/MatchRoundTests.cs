using Domain;
using RicoBotsOn.Tests;
using Xunit;

namespace RicochetRobots.Domain.Tests;

public class MatchRoundTests
{
    [Fact]
    public void StartNextRound_BeforeTheRoundIsOver_Throws()
    {
        var board = TestHelpers.FiveByFiveNoWalls();
        board.AddTarget(4, 4);
        var match = new Match(board, new List<Bot> { new("b1", 0, 0) });
        match.SelectNewTarget(new Random(1));

        Assert.Throws<InvalidOperationException>(() => match.StartNextRound(new Random(1)));
    }

    [Fact]
    public void StartNextRound_AfterASuccess_IncrementsTheRoundNumberAndPicksANewTarget()
    {
        var board = TestHelpers.FiveByFiveNoWalls();
        board.AddTarget(4, 0);
        board.AddTarget(0, 4);  // a second option, since the bot ends this round sitting on (4, 0)
        var mover = new Bot("b1", 0, 0);
        var match = new Match(board, new List<Bot> { mover });
        match.SelectNewTarget(new Random(1));
        match.SubmitClaim("player1", 1);
        TestHelpers.CloseClaimWindow(match);
        match.ApplyProvingMove("player1", mover.Id, Direction.East);  // solves it
        var roundBefore = match.RoundNumber;

        match.StartNextRound(new Random(1));

        Assert.Equal(roundBefore + 1, match.RoundNumber);
        Assert.Equal(RoundPhase.WaitingForClaims, match.Phase);
        Assert.NotNull(match.ActiveTarget);
    }

    [Fact]
    public void StartNextRound_AfterAFailure_ResetsBotsBeforePickingANewTarget()
    {
        var board = TestHelpers.FiveByFiveNoWalls();
        board.AddTarget(4, 4);  // needs two moves; only one was claimed, so this fails
        var mover = new Bot("b1", 0, 0);
        var match = new Match(board, new List<Bot> { mover });
        match.SelectNewTarget(new Random(1));
        match.SubmitClaim("player1", 1);
        TestHelpers.CloseClaimWindow(match);
        match.ApplyProvingMove("player1", mover.Id, Direction.East);  // fails, bot left at (4, 0)

        match.StartNextRound(new Random(1));

        Assert.Equal((0, 0), (mover.X, mover.Y));
    }
}