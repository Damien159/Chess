using Chess.Models;
using Xunit;
using static Chess.Tests.TestHelper;

namespace Chess.Tests;

public class ComputerPlayerTests
{
    [Fact]
    public void ChooseMove_ReturnsLegalMove_ForPlayerToMove()
    {
        var game = new Game();

        var move = new ComputerPlayer(new Random(1)).ChooseMove(game);

        Assert.NotNull(move);
        Assert.Equal(PieceColor.White, game.Board[move!.From]!.Color);
        Assert.Contains(move.To, game.GetMoves(move.From));
    }

    [Fact]
    public void ChooseMove_ReturnsNull_WhenGameIsOver()
    {
        var game = new Game();
        Play(game, "f2-f3", "e7-e5", "g2-g4", "d8-h4"); // Narrenmatt

        var move = new ComputerPlayer(new Random(1)).ChooseMove(game);

        Assert.Null(move);
    }

    [Fact]
    public void ChooseMove_IsDeterministic_WithSameSeed()
    {
        var first = new ComputerPlayer(new Random(7)).ChooseMove(new Game());
        var second = new ComputerPlayer(new Random(7)).ChooseMove(new Game());

        Assert.Equal(first, second);
    }

    [Fact]
    public void ComputerVsComputer_PlaysOnlyLegalMoves_UntilTheGameEnds()
    {
        var game = new Game();
        var computer = new ComputerPlayer(new Random(1));
        var plies = 0;

        while (!game.IsOver && plies < 400)
        {
            var move = computer.ChooseMove(game);

            Assert.NotNull(move);
            Assert.True(game.TryMove(move!.From, move.To, move.Promotion));
            plies++;
        }

        Assert.Equal(plies, game.Moves.Count);
    }
}
