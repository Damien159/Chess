using Chess.Models;
using Xunit;
using static Chess.Tests.TestHelper;

namespace Chess.Tests;

public class InsufficientMaterialTests
{
    [Fact]
    public void KingAndBishopVsKing_IsDraw()
    {
        var game = new Game();
        SetUp(game,
            ("e1", PieceType.King, PieceColor.White),
            ("c1", PieceType.Bishop, PieceColor.White),
            ("e8", PieceType.King, PieceColor.Black));

        game.TryMove(Sq("e1"), Sq("e2"));

        Assert.Equal(GameStatus.DrawByInsufficientMaterial, game.Status);
        Assert.Equal("1/2-1/2", game.Result);
    }

    [Fact]
    public void KingAndRookVsKing_IsNotDraw()
    {
        var game = new Game();
        SetUp(game,
            ("e1", PieceType.King, PieceColor.White),
            ("a1", PieceType.Rook, PieceColor.White),
            ("e8", PieceType.King, PieceColor.Black));

        game.TryMove(Sq("e1"), Sq("e2"));

        Assert.Equal(GameStatus.Ongoing, game.Status); // mit einem Turm ist ein Matt möglich
    }

    [Fact]
    public void BishopsOnSameColoredSquares_IsDraw()
    {
        var game = new Game();
        SetUp(game,
            ("e1", PieceType.King, PieceColor.White),
            ("c1", PieceType.Bishop, PieceColor.White), // dunkles Feld
            ("e8", PieceType.King, PieceColor.Black),
            ("f8", PieceType.Bishop, PieceColor.Black)); // ebenfalls dunkles Feld

        game.TryMove(Sq("e1"), Sq("e2"));

        Assert.Equal(GameStatus.DrawByInsufficientMaterial, game.Status);
    }

    [Fact]
    public void BishopsOnOppositeColoredSquares_IsNotDraw()
    {
        var game = new Game();
        SetUp(game,
            ("e1", PieceType.King, PieceColor.White),
            ("c1", PieceType.Bishop, PieceColor.White), // dunkles Feld
            ("e8", PieceType.King, PieceColor.Black),
            ("c8", PieceType.Bishop, PieceColor.Black)); // helles Feld

        game.TryMove(Sq("e1"), Sq("e2"));

        Assert.Equal(GameStatus.Ongoing, game.Status);
    }
}
