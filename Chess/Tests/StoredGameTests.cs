using Chess.Data;
using Chess.Models;
using Xunit;
using static Chess.Tests.TestHelper;

namespace Chess.Tests;

public class StoredGameTests
{
    // Baut aus einer gespielten Partie die Zeilen, so wie GameRepository sie speichert
    private static StoredGame Store(Game game) => new()
    {
        Moves = game.Moves
            .Select((move, index) => new StoredMove
            {
                Ply = index + 1,
                FromRank = move.From.Rank,
                FromFile = move.From.File,
                ToRank = move.To.Rank,
                ToFile = move.To.File,
                Promotion = move.Promotion is { } promotion ? (int)promotion : null,
                Notation = move.Notation
            })
            .ToList()
    };

    private static void AssertSamePosition(Game expected, Game actual)
    {
        for (var rank = 0; rank < 8; rank++)
        {
            for (var file = 0; file < 8; file++)
            {
                Assert.Equal(expected.Board[rank, file], actual.Board[rank, file]);
            }
        }
    }

    [Fact]
    public void ToGame_RebuildsSamePositionAndMoves()
    {
        var game = new Game();
        Play(game, "e2-e4", "e7-e5", "g1-f3", "b8-c6");

        var rebuilt = Store(game).ToGame();

        Assert.Equal(game.Moves.Select(m => m.Notation), rebuilt.Moves.Select(m => m.Notation));
        Assert.Equal(game.CurrentTurn, rebuilt.CurrentTurn);
        AssertSamePosition(game, rebuilt);
    }

    [Fact]
    public void ToGame_WithPlies_StopsAfterThatManyMoves()
    {
        var game = new Game();
        Play(game, "e2-e4", "e7-e5", "g1-f3", "b8-c6");

        var rebuilt = Store(game).ToGame(2);

        Assert.Equal(2, rebuilt.Moves.Count);
        Assert.Equal(PieceColor.White, rebuilt.CurrentTurn);
        Assert.NotNull(rebuilt.Board[Sq("e5")]);
        Assert.Null(rebuilt.Board[Sq("f3")]); // der dritte Halbzug fehlt noch
    }

    [Fact]
    public void ToGame_UsesPlyNumber_NotListOrder()
    {
        var game = new Game();
        Play(game, "e2-e4", "e7-e5", "g1-f3");
        var stored = Store(game);
        stored.Moves.Reverse();

        var rebuilt = stored.ToGame();

        Assert.Equal(new[] { "e4", "e5", "Nf3" }, rebuilt.Moves.Select(m => m.Notation));
    }

    [Fact]
    public void ToGame_KeepsThePromotionPiece()
    {
        var game = new Game();
        Play(game, "a2-a4", "b7-b5", "a4-b5", "a7-a6", "b5-a6", "g8-f6", "a6-a7", "h7-h6");
        game.TryMove(Sq("a7"), Sq("b8"), PieceType.Knight);

        var rebuilt = Store(game).ToGame();

        Assert.Equal(PieceType.Knight, rebuilt.Board[Sq("b8")]!.Type);
        Assert.Equal("axb8=N", rebuilt.Moves.Last().Notation);
    }

    [Fact]
    public void ToGame_RestoresCheckmateStatus()
    {
        var game = new Game();
        Play(game, "f2-f3", "e7-e5", "g2-g4", "d8-h4");

        var rebuilt = Store(game).ToGame();

        Assert.Equal(GameStatus.Checkmate, rebuilt.Status);
        Assert.Equal("0-1", rebuilt.Result);
    }
}
