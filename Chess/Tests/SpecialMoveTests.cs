using Chess.Models;
using Xunit;
using static Chess.Tests.TestHelper;

namespace Chess.Tests;

public class SpecialMoveTests
{
    // Springer und Läufer von Weiß sind weg, damit der weiße König rochieren kann
    private static readonly string[] CastlingSetup =
        ["e2-e4", "e7-e5", "g1-f3", "b8-c6", "f1-c4", "f8-c5"];

    [Fact]
    public void Castling_KingSide_MovesKingAndRook()
    {
        var game = new Game();
        Play(game, CastlingSetup);

        Assert.Contains(Sq("g1"), game.GetMoves(Sq("e1")));
        game.TryMove(Sq("e1"), Sq("g1"));

        Assert.Equal(PieceType.King, game.Board[Sq("g1")]!.Type);
        Assert.Equal(PieceType.Rook, game.Board[Sq("f1")]!.Type);
        Assert.Null(game.Board[Sq("h1")]);
        Assert.Equal("O-O", game.Moves.Last().Notation);
    }

    [Fact]
    public void Castling_IsNotAllowed_AfterKingHasMoved()
    {
        var game = new Game();
        Play(game, CastlingSetup);

        // König geht weg und wieder zurück: Das Rochaderecht ist trotzdem verloren
        Play(game, "e1-e2", "a7-a6", "e2-e1", "a6-a5");

        Assert.DoesNotContain(Sq("g1"), game.GetMoves(Sq("e1")));
    }

    [Fact]
    public void EnPassant_CapturesThePassedPawn()
    {
        var game = new Game();
        Play(game, "e2-e4", "a7-a6", "e4-e5", "d7-d5");

        Assert.Contains(Sq("d6"), game.GetMoves(Sq("e5")));
        game.TryMove(Sq("e5"), Sq("d6"));

        Assert.Null(game.Board[Sq("d5")]); // der geschlagene Bauer ist weg
        Assert.Equal(PieceColor.White, game.Board[Sq("d6")]!.Color);
        Assert.Equal("exd6", game.Moves.Last().Notation);
    }

    [Fact]
    public void Promotion_ReplacesPawnWithChosenPiece()
    {
        var game = new Game();
        Play(game,
            "a2-a4", "b7-b5", "a4-b5", "a7-a6",
            "b5-a6", "g8-f6", "a6-a7", "h7-h6");

        game.TryMove(Sq("a7"), Sq("b8"), PieceType.Knight);

        var piece = game.Board[Sq("b8")]!;
        Assert.Equal(PieceType.Knight, piece.Type);
        Assert.Equal(PieceColor.White, piece.Color);
        Assert.Equal("axb8=N", game.Moves.Last().Notation);
    }
}
