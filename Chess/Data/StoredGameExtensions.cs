using Chess.Models;

namespace Chess.Data;

public static class StoredGameExtensions
{
    // Baut eine Partie aus den gespeicherten Zügen wieder auf, indem sie mit der echten
    // Schachlogik nachgespielt werden. Mit plies lässt sich nach einem früheren Halbzug anhalten.
    // Aufgabe und Remis durch Einigung stehen nicht in den Zügen. Solche Partien sind beendet
    // und werden nur angesehen, nicht fortgesetzt.
    public static Game ToGame(this StoredGame stored, int? plies = null)
    {
        var game = new Game();

        foreach (var move in stored.Moves.OrderBy(m => m.Ply).Take(plies ?? int.MaxValue))
        {
            game.TryMove(
                new Square(move.FromRank, move.FromFile),
                new Square(move.ToRank, move.ToFile),
                move.Promotion is { } promotion ? (PieceType?)promotion : null);
        }

        return game;
    }
}
