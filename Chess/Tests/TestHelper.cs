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
}
