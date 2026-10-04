using Domain;
using RicoBotsOn.Tests;
using Xunit;

namespace RicochetRobots.Domain.Tests;

public class MatchHistoryTests
{
    // Unreachable target, so East/West shuffles never end the round.
    private static Match NewProvingMatch(int claim, out Bot mover)
    {
        var board = TestHelpers.FiveByFiveNoWalls();
        board.AddTarget(4, 4);
        mover = new Bot("b1", 0, 0);
        var match = new Match(board, new List<Bot> { mover });
        match.SelectNewTarget(new Random(1));
        match.SubmitClaim("player1", claim);
        TestHelpers.CloseClaimWindow(match);
        match.EnsureProvingStarted();
        return match;
    }

    [Fact]
    public void ApplyProvingMove_RecordsTheMoveInHistory()
    {
        var match = NewProvingMatch(5, out var mover);

        match.ApplyProvingMove("player1", mover.Id, Direction.East);

        Assert.Equal(new MoveRecord("b1", 0, 0, 4, 0), Assert.Single(match.MoveHistory));
    }

    [Fact]
    public void ApplyProvingMove_ZeroLengthMove_IsNotCountedOrRecorded()
    {
        var match = NewProvingMatch(5, out var mover);

        match.ApplyProvingMove("player1", mover.Id, Direction.West);  // already against the edge

        Assert.Equal(0, match.CurrentAttempt!.MoveCount);
        Assert.Empty(match.MoveHistory);
        Assert.Equal(RoundPhase.Proving, match.Phase);
    }

    [Fact]
    public void UndoProvingMove_RestoresPositionPopsHistoryAndDecrementsCount()
    {
        var match = NewProvingMatch(5, out var mover);
        match.ApplyProvingMove("player1", mover.Id, Direction.East);  // (4, 0)
        match.ApplyProvingMove("player1", mover.Id, Direction.West);  // (0, 0)

        var undone = match.UndoProvingMove("player1");

        Assert.Equal(new MoveRecord("b1", 4, 0, 0, 0), undone);
        Assert.Equal((4, 0), (mover.X, mover.Y));
        Assert.Single(match.MoveHistory);
        Assert.Equal(1, match.CurrentAttempt!.MoveCount);
    }

    [Fact]
    public void UndoProvingMove_Chains_BackToTheRoundStart()
    {
        var match = NewProvingMatch(5, out var mover);
        match.ApplyProvingMove("player1", mover.Id, Direction.East);
        match.ApplyProvingMove("player1", mover.Id, Direction.West);

        match.UndoProvingMove("player1");
        match.UndoProvingMove("player1");

        Assert.Equal((0, 0), (mover.X, mover.Y));
        Assert.Empty(match.MoveHistory);
        Assert.Equal(0, match.CurrentAttempt!.MoveCount);
    }

    [Fact]
    public void UndoProvingMove_WithNothingToUndo_Throws()
    {
        var match = NewProvingMatch(5, out _);

        var ex = Assert.Throws<InvalidOperationException>(() => match.UndoProvingMove("player1"));
        Assert.Contains("nothing to undo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UndoProvingMove_ByTheWrongPlayer_Throws()
    {
        var match = NewProvingMatch(5, out var mover);
        match.ApplyProvingMove("player1", mover.Id, Direction.East);

        var ex = Assert.Throws<InvalidOperationException>(() => match.UndoProvingMove("someone-else"));
        Assert.Contains("not your turn", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(match.MoveHistory);
    }

    [Fact]
    public void UndoProvingMove_OutsideTheProvingPhase_Throws()
    {
        var board = TestHelpers.FiveByFiveNoWalls();
        board.AddTarget(4, 4);
        var match = new Match(board, new List<Bot> { new("b1", 0, 0) });
        match.SelectNewTarget(new Random(1));  // still WaitingForClaims

        Assert.Throws<InvalidOperationException>(() => match.UndoProvingMove("player1"));
    }

    [Fact]
    public void ResetCurrentProverAttempt_ClearsTheHistory()
    {
        var match = NewProvingMatch(5, out var mover);
        match.ApplyProvingMove("player1", mover.Id, Direction.East);

        match.ResetCurrentProverAttempt("player1");

        Assert.Empty(match.MoveHistory);
    }

    [Fact]
    public void AdvancingToTheNextClaimant_ClearsTheHistory()
    {
        var board = TestHelpers.FiveByFiveNoWalls();
        board.AddTarget(4, 4);
        var mover = new Bot("b1", 0, 0);
        var match = new Match(board, new List<Bot> { mover });
        match.SelectNewTarget(new Random(1));
        match.SubmitClaim("player1", 1);
        match.SubmitClaim("player2", 5);
        TestHelpers.CloseClaimWindow(match);

        match.ApplyProvingMove("player1", mover.Id, Direction.East);  // uses the only move, misses

        Assert.Equal("player2", match.CurrentAttempt!.PlayerId);
        Assert.Empty(match.MoveHistory);
    }

    [Fact]
    public void WhenTheLastClaimantFails_TheHistoryIsKeptUntilTheNextRound()
    {
        var match = NewProvingMatch(1, out var mover);
        match.ApplyProvingMove("player1", mover.Id, Direction.East);  // round over, nobody solved it

        Assert.Equal(RoundPhase.RoundOver, match.Phase);
        Assert.Single(match.MoveHistory);

        match.StartNextRound(new Random(1));

        Assert.Empty(match.MoveHistory);
    }
}