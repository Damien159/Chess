using Chess.Models;
using Xunit;

namespace Chess.Tests;

internal static class TestHelper
{
    // "e2" wird zu Square(Reihe 6, Linie 4).
    // Reihe 0 ist die 8. Reihe (oben), Reihe 7 die 1. Reihe (unten).
    public static Square Sq(string name) =>
        new(8 - (name[1] - '0'), name[0] - 'a');

    // Spielt mehrere Züge wie "e2-e4" nacheinander und bricht ab, wenn einer verboten ist.
    public static void Play(Game game, params string[] moves)
    {
        foreach (var move in moves)
        {
            var parts = move.Split('-');
            var ok = game.TryMove(Sq(parts[0]), Sq(parts[1]));
            Assert.True(ok, $"Der Zug {move} sollte erlaubt sein, war es aber nicht.");
        }
    }

    // Räumt das Brett leer und stellt nur die angegebenen Figuren auf.
    // So lassen sich Stellungen testen, die man mit Zügen kaum erreicht (zum Beispiel König gegen König).
    public static void SetUp(Game game, params (string At, PieceType Type, PieceColor Color)[] pieces)
    {
        for (var rank = 0; rank < 8; rank++)
        {
            for (var file = 0; file < 8; file++)
            {
                game.Board.Set(new Square(rank, file), null);
            }
        }

        foreach (var (at, type, color) in pieces)
        {
            game.Board.Set(Sq(at), new Piece(type, color));
        }
    }
}
