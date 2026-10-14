using System;
using System.Collections.Generic;
using System.Text;
using Domain;

namespace RicoBotsOn.Tests;

public class BoardValidatorTests
{
    private static BoardDefinition Valid() => new(
        5, 5,
        new List<WallEdge>(),
        new List<(int X, int Y)> { (4, 4) },
        new List<BotStart> { new(0, 0) });

    [Fact]
    public void Validate_AcceptsAMinimalBoard()
    {
        BoardValidator.Validate(Valid());
    }

    [Theory]
    [InlineData(4, 5)]
    [InlineData(5, 4)]
    [InlineData(21, 5)]
    [InlineData(5, 21)]
    public void Validate_RejectsSizesOutsideTheLimits(int width, int height)
    {
        Assert.Throws<InvalidOperationException>(() =>
            BoardValidator.Validate(Valid() with { Width = width, Height = height }));
    }

    [Fact]
    public void Validate_RejectsABotOutsideTheBoard()
    {
        var def = Valid() with { Bots = new List<BotStart> { new(5, 0) } };
        Assert.Throws<InvalidOperationException>(() => BoardValidator.Validate(def));
    }

    [Fact]
    public void Validate_RejectsNoBots()
    {
        var def = Valid() with { Bots = new List<BotStart>() };
        Assert.Throws<InvalidOperationException>(() => BoardValidator.Validate(def));
    }

    [Fact]
    public void Validate_RejectsTwoBotsOnTheSameCell()
    {
        var def = Valid() with { Bots = new List<BotStart> { new(0, 0), new(0, 0) } };
        Assert.Throws<InvalidOperationException>(() => BoardValidator.Validate(def));
    }

    [Fact]
    public void Validate_RejectsNoTargets()
    {
        var def = Valid() with { Targets = new List<(int X, int Y)>() };
        Assert.Throws<InvalidOperationException>(() => BoardValidator.Validate(def));
    }

    [Fact]
    public void Validate_RejectsATargetOutsideTheBoard()
    {
        var def = Valid() with { Targets = new List<(int X, int Y)> { (0, 5) } };
        Assert.Throws<InvalidOperationException>(() => BoardValidator.Validate(def));
    }

    [Fact]
    public void Validate_RejectsWhenTheOnlyTargetHoldsTheOnlyBot()
    {
        var def = Valid() with { Targets = new List<(int X, int Y)> { (0, 0) } };
        Assert.Throws<InvalidOperationException>(() => BoardValidator.Validate(def));
    }

    [Theory]
    [InlineData(0, 0, Direction.North)]
    [InlineData(0, 0, Direction.West)]
    [InlineData(5, 0, Direction.East)]
    public void Validate_RejectsOutOfBoundsWalls(int x, int y, Direction side)
    {
        var def = Valid() with { Walls = new List<WallEdge> { new(x, y, side) } };
        Assert.Throws<InvalidOperationException>(() => BoardValidator.Validate(def));
    }
}
