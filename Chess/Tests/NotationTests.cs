using Chess.Models;
using Xunit;
using static Chess.Tests.TestHelper;

namespace Chess.Tests;

public class NotationTests
{
    [Fact]
    public void CapturesAndPieceLetters_AreWrittenCorrectly()
    {
        var game = new Game();

        Play(game, "e2-e4", "e7-e5", "g1-f3", "b8-c6", "f1-b5", "a7-a6", "b5-c6", "d7-c6");

        var notation = game.Moves.Select(m => m.Notation).ToArray();
        Assert.Equal(new[] { "e4", "e5", "Nf3", "Nc6", "Bb5", "a6", "Bxc6", "dxc6" }, notation);
    }

    [Fact]
    public void TwoKnightsCanReachSameSquare_FileIsAdded()
    {
        var game = new Game();

        // Sb1 und Sf3 können beide nach d2 ziehen
        Play(game, "d2-d3", "d7-d6", "g1-f3", "g8-f6", "b1-d2");

        Assert.Equal("Nbd2", game.Moves.Last().Notation);
    }

    [Fact]
    public void TwoRooksOnSameFile_RankIsAdded()
    {
        var game = new Game();
        SetUp(game,
            ("e1", PieceType.King, PieceColor.White),
            ("a1", PieceType.Rook, PieceColor.White),
            ("a5", PieceType.Rook, PieceColor.White),
            ("h8", PieceType.King, PieceColor.Black));

        // Beide Türme stehen auf der a-Linie, deshalb hilft nur die Reihe zur Unterscheidung
        game.TryMove(Sq("a1"), Sq("a3"));

        Assert.Equal("R1a3", game.Moves.Last().Notation);
    }
}
