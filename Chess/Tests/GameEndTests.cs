using Chess.Models;
using Xunit;
using static Chess.Tests.TestHelper;

namespace Chess.Tests;

public class GameEndTests
{
    [Fact]
    public void FoolsMate_IsCheckmate_AndBlackWins()
    {
        var game = new Game();

        Play(game, "f2-f3", "e7-e5", "g2-g4", "d8-h4");

        Assert.Equal(GameStatus.Checkmate, game.Status);
        Assert.Equal(PieceColor.Black, game.Winner);
        Assert.Equal("0-1", game.Result);
        Assert.Equal("Qh4#", game.Moves.Last().Notation);
        Assert.True(game.IsOver);
    }

    [Fact]
    public void ThreefoldRepetition_EndsInDraw()
    {
        var game = new Game();

        // Beide Springer gehen hin und zurück: Die Grundstellung entsteht erneut
        Play(game, "g1-f3", "g8-f6", "f3-g1", "f6-g8");
        Assert.Equal(GameStatus.Ongoing, game.Status); // erst das zweite Mal, noch kein Remis

        Play(game, "g1-f3", "g8-f6", "f3-g1", "f6-g8");

        Assert.Equal(GameStatus.DrawByRepetition, game.Status);
        Assert.Equal("1/2-1/2", game.Result);
    }
}
