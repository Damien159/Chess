using Chess.Models;
using Xunit;
using static Chess.Tests.TestHelper;

namespace Chess.Tests;

public class StalemateTests
{
    [Fact]
    public void LoydTenMoveStalemate_IsDetectedAsDraw()
    {
        var game = new Game();

        // Sam Loyds berühmtes Patt: Schwarz hat nach 10 Zügen keinen legalen Zug mehr,
        // steht aber nicht im Schach.
        Play(game,
            "e2-e3", "a7-a5",
            "d1-h5", "a8-a6",
            "h5-a5", "h7-h5",
            "a5-c7", "a6-h6",
            "h2-h4", "f7-f6",
            "c7-d7", "e8-f7",
            "d7-b7", "d8-d3",
            "b7-b8", "d3-h7",
            "b8-c8", "f7-g6",
            "c8-e6");

        Assert.Equal(GameStatus.Stalemate, game.Status);
        Assert.Equal("1/2-1/2", game.Result);
        Assert.Null(game.Winner);
        Assert.True(game.IsOver);
    }
}
