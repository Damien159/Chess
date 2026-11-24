namespace Chess.Models;

// Ein Zug, den der Computer spielen will. Promotion ist nur bei Bauernumwandlung gesetzt.
public record ComputerMove(Square From, Square To, PieceType? Promotion);

// Stufe 1: wählt zufällig einen der legalen Züge.
// Das Random-Objekt lässt sich von außen übergeben, damit Tests mit festem Startwert
// immer dieselben Züge bekommen.
public class ComputerPlayer(Random? random = null)
{
    private readonly Random _random = random ?? Random.Shared;

    // Gibt null zurück, wenn die Partie vorbei ist oder kein Zug möglich ist.
    public ComputerMove? ChooseMove(Game game)
    {
        if (game.IsOver)
        {
            return null;
        }

        var moves = new List<ComputerMove>();

        for (var rank = 0; rank < 8; rank++)
        {
            for (var file = 0; file < 8; file++)
            {
                var from = new Square(rank, file);

                // GetMoves liefert nur legale Züge und nur für den Spieler am Zug
                foreach (var to in game.GetMoves(from))
                {
                    var promotion = game.IsPromotion(from, to) ? PieceType.Queen : (PieceType?)null;
                    moves.Add(new ComputerMove(from, to, promotion));
                }
            }
        }

        return moves.Count == 0 ? null : moves[_random.Next(moves.Count)];
    }
}
