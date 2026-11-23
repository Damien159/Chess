using Chess.Models;
using Xunit;
using static Chess.Tests.TestHelper;

namespace Chess.Tests;

public class MoveRuleTests
{
    [Fact]
    public void Knight_InStartPosition_HasExactlyTwoMoves()
    {
        var game = new Game();

        var moves = game.GetMoves(Sq("g1"));

        Assert.Equal(2, moves.Count);
        Assert.Contains(Sq("f3"), moves);
        Assert.Contains(Sq("h3"), moves);
    }

    [Fact]
    public void Pawn_OnStartRank_CanMoveOneOrTwoSquares()
    {
        var game = new Game();

        var moves = game.GetMoves(Sq("e2"));

        Assert.Equal(2, moves.Count);
        Assert.Contains(Sq("e3"), moves);
        Assert.Contains(Sq("e4"), moves);
    }

    [Fact]
    public void Player_CannotMoveOpponentsPiece()
    {
        var game = new Game();

        var moved = game.TryMove(Sq("e7"), Sq("e5"));

        Assert.False(moved);
        Assert.Equal(PieceColor.White, game.CurrentTurn);
    }

    [Fact]
    public void PinnedPawn_CannotMove()
    {
        var game = new Game();
        Assert.Equal(2, game.GetMoves(Sq("d2")).Count); // zum Vergleich: ohne Fesselung geht es

        // Der schwarze Läufer auf b4 fesselt den Bauern d2 an den König auf e1
        Play(game, "a2-a4", "e7-e6", "h2-h3", "f8-b4");

        Assert.Empty(game.GetMoves(Sq("d2")));
    }
}
