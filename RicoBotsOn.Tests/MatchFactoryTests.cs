using Domain;

namespace RicoBotsOn.Tests;

public class MatchFactoryTests
{
    [Fact]
    public void FromDefinition_BuildsWallsTargetsAndBots()
    {
        var def = new BoardDefinition(
            5, 5,
            new List<WallEdge> { new(1, 1, Direction.East) },
            new List<(int X, int Y)> { (4, 4) },
            new List<BotStart> { new(0, 0), new(1, 0) });

        var match = MatchFactory.FromDefinition(def, new Random(1));

        Assert.True(match.Board.HasWall(1, 1, Direction.East));
        Assert.True(match.Board.HasWall(2, 1, Direction.West));
        Assert.Single(match.Board.Targets);
        Assert.Equal(new[] { "bot1", "bot2" }, match.Bots.Select(b => b.Id));
        Assert.NotNull(match.ActiveTarget);
        Assert.Equal(RoundPhase.WaitingForClaims, match.Phase);
    }

    [Fact]
    public void FromDefinition_RejectsAnInvalidDefinition()
    {
        var def = new BoardDefinition(
            3, 3,
            new List<WallEdge>(),
            new List<(int X, int Y)> { (2, 2) },
            new List<BotStart> { new(0, 0) });

        Assert.Throws<InvalidOperationException>(() => MatchFactory.FromDefinition(def, new Random(1)));
    }
}