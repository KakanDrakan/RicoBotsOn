using Domain;
using RicoBotsOn.Tests;
using Xunit;

namespace RicochetRobots.Domain.Tests;

public class MatchClaimTests
{
    private static Match NewMatchWithTarget()
    {
        var board = TestHelpers.FiveByFiveNoWalls();
        board.AddTarget(4, 4);
        var match = new Match(board, new List<Bot> { new("r1", 0, 0) });
        match.SelectNewTarget(new Random(1));
        return match;
    }

    [Fact]
    public void SubmitClaim_FirstClaim_OpensA30SecondWindow()
    {
        var match = NewMatchWithTarget();

        match.SubmitClaim("player1", 5);

        Assert.NotNull(match.ClaimWindow.ActiveDeadlineUtc);
        var remaining = match.ClaimWindow.ActiveDeadlineUtc!.Value - DateTime.UtcNow;
        Assert.InRange(remaining.TotalSeconds, 29, 30);
    }

    [Fact]
    public void SubmitClaim_AllowsAClaimWorseThanTheCurrentBest()
    {
        var match = NewMatchWithTarget();
        match.SubmitClaim("player1", 3);

        match.SubmitClaim("player2", 10);

        Assert.Equal(2, match.ClaimWindow.Claims.Count);
    }

    [Fact]
    public void SubmitClaim_IgnoresAnExactDuplicateFromTheSamePlayer()
    {
        var match = NewMatchWithTarget();
        match.SubmitClaim("player1", 5);

        match.SubmitClaim("player1", 5);

        Assert.Single(match.ClaimWindow.Claims);
    }

    [Fact]
    public void SubmitClaim_SameCountFromADifferentPlayer_IsAllowed()
    {
        var match = NewMatchWithTarget();
        match.SubmitClaim("player1", 5);

        match.SubmitClaim("player2", 5);

        Assert.Equal(2, match.ClaimWindow.Claims.Count);
    }

    [Fact]
    public void SubmitClaim_AfterTheWindowHasClosed_Throws()
    {
        var match = NewMatchWithTarget();
        match.SubmitClaim("player1", 5);
        TestHelpers.CloseClaimWindow(match);

        Assert.Throws<InvalidOperationException>(() => match.SubmitClaim("player2", 3));
    }

    [Fact]
    public void ClaimsInProvingOrder_SortsByMoveCountThenByClaimTime()
    {
        var match = NewMatchWithTarget();
        match.SubmitClaim("player1", 8);
        match.SubmitClaim("player2", 3);
        match.SubmitClaim("player3", 3);

        var order = match.ClaimWindow.ClaimsInProvingOrder;

        Assert.Equal(new[] { "player2", "player3", "player1" }, order.Select(c => c.PlayerId));
    }
}