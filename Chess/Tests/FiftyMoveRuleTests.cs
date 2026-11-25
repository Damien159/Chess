using System.Reflection;
using Chess.Models;
using Xunit;
using static Chess.Tests.TestHelper;

namespace Chess.Tests;

public class FiftyMoveRuleTests
{
    // 100 ruhige Halbzüge ohne Wiederholung von Hand zu spielen wäre sehr lang.
    // Deshalb wird der Zähler im Test direkt gesetzt (der Zähler ist privat, daher Reflection).
    private static void SetHalfmoveClock(Game game, int value) =>
        typeof(Game)
            .GetField("_halfmoveClock", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(game, value);

    private static Game RooksPosition()
    {
        var game = new Game();
        SetUp(game,
            ("e1", PieceType.King, PieceColor.White),
            ("a1", PieceType.Rook, PieceColor.White),
            ("e8", PieceType.King, PieceColor.Black),
            ("a8", PieceType.Rook, PieceColor.Black));
        return game;
    }

    [Fact]
    public void After100QuietHalfMoves_GameIsDraw()
    {
        var game = RooksPosition();
        SetHalfmoveClock(game, 99);

        game.TryMove(Sq("a1"), Sq("b1")); // der 100. Halbzug ohne Bauernzug oder Schlagzug

        Assert.Equal(GameStatus.DrawByFiftyMoveRule, game.Status);
        Assert.Equal("1/2-1/2", game.Result);
    }

    [Fact]
    public void After99QuietHalfMoves_GameContinues()
    {
        var game = RooksPosition();
        SetHalfmoveClock(game, 98);

        game.TryMove(Sq("a1"), Sq("b1"));

        Assert.Equal(GameStatus.Ongoing, game.Status);
    }

    [Fact]
    public void PawnMove_ResetsTheCounter()
    {
        var game = new Game();
        SetUp(game,
            ("e1", PieceType.King, PieceColor.White),
            ("e2", PieceType.Pawn, PieceColor.White),
            ("e8", PieceType.King, PieceColor.Black));
        SetHalfmoveClock(game, 99);

        game.TryMove(Sq("e2"), Sq("e3"));

        Assert.Equal(GameStatus.Ongoing, game.Status);
    }
}
