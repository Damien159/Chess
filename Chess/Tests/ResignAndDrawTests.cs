using Chess.Models;
using Xunit;
using static Chess.Tests.TestHelper;

namespace Chess.Tests;

public class ResignAndDrawTests
{
    [Fact]
    public void Resign_EndsGame_AndOpponentWins()
    {
        var game = new Game();

        var resigned = game.Resign(PieceColor.White);

        Assert.True(resigned);
        Assert.Equal(GameStatus.Resigned, game.Status);
        Assert.Equal(PieceColor.Black, game.Winner);
        Assert.Equal("0-1", game.Result);
        Assert.Empty(game.GetMoves(Sq("e2"))); // gesperrt, weil die Partie vorbei ist
    }

    [Fact]
    public void Resign_IsIgnored_WhenGameIsAlreadyOver()
    {
        var game = new Game();
        Play(game, "f2-f3", "e7-e5", "g2-g4", "d8-h4"); // Narrenmatt

        var resigned = game.Resign(PieceColor.White);

        Assert.False(resigned);
        Assert.Equal(GameStatus.Checkmate, game.Status);
        Assert.Equal("0-1", game.Result);
    }

    [Fact]
    public void AcceptedDrawOffer_EndsGameAsDraw()
    {
        var game = new Game();

        Assert.True(game.OfferDraw(PieceColor.White));
        Assert.Equal(PieceColor.White, game.DrawOfferedBy);

        Assert.True(game.AcceptDraw());

        Assert.Equal(GameStatus.DrawByAgreement, game.Status);
        Assert.Equal("1/2-1/2", game.Result);
        Assert.Null(game.DrawOfferedBy);
    }

    [Fact]
    public void DeclinedDrawOffer_KeepsGameRunning()
    {
        var game = new Game();
        game.OfferDraw(PieceColor.White);

        game.DeclineDraw();

        Assert.Null(game.DrawOfferedBy);
        Assert.False(game.IsOver);
        Assert.False(game.AcceptDraw()); // ohne Angebot gibt es nichts anzunehmen
    }

    [Fact]
    public void DrawOffer_Expires_AfterOpponentMoves()
    {
        var game = new Game();

        game.OfferDraw(PieceColor.White);
        Play(game, "e2-e4");
        Assert.Equal(PieceColor.White, game.DrawOfferedBy); // der eigene Zug ändert nichts am Angebot

        Play(game, "e7-e5");
        Assert.Null(game.DrawOfferedBy); // der Gegner hat gezogen, statt anzunehmen
    }
}
