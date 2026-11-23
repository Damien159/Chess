namespace Chess.Models;

public static class MoveGenerator
{
    private static readonly (int Rank, int File)[] KnightSteps =
    [
        (-2, -1), (-2, 1), (-1, -2), (-1, 2), (1, -2), (1, 2), (2, -1), (2, 1)
    ];

    private static readonly (int Rank, int File)[] Diagonals =
    [
        (-1, -1), (-1, 1), (1, -1), (1, 1)
    ];

    private static readonly (int Rank, int File)[] Straights =
    [
        (-1, 0), (1, 0), (0, -1), (0, 1)
    ];

    private static readonly (int Rank, int File)[] AllDirections = [.. Diagonals, .. Straights];

    // Liefert alle Felder, die eine Figur nach ihrer Bewegungsart erreichen kann.
    // Achtung: ohne Schach-Prüfung und ohne Sonderzüge (Rochade, en passant). Das ergänzt Game.
    // Es gibt nur drei Grundmuster: Einzelschritte, Gleiten und der Bauer als Sonderfall.
    public static List<Square> GetMoves(Board board, Square from)
    {
        var moves = new List<Square>();

        if (board[from] is not { } piece)
        {
            return moves;
        }

        switch (piece.Type)
        {
            case PieceType.Pawn:
                AddPawnMoves(board, from, piece.Color, moves);
                break;
            case PieceType.Knight:
                AddStepMoves(board, from, piece.Color, KnightSteps, moves);
                break;
            case PieceType.Bishop:
                AddSlideMoves(board, from, piece.Color, Diagonals, moves);
                break;
            case PieceType.Rook:
                AddSlideMoves(board, from, piece.Color, Straights, moves);
                break;
            case PieceType.Queen:
                AddSlideMoves(board, from, piece.Color, AllDirections, moves);
                break;
            case PieceType.King:
                AddStepMoves(board, from, piece.Color, AllDirections, moves);
                break;
        }

        return moves;
    }

    // Der Bauer ist der Sonderfall: Er zieht vorwärts, schlägt aber nur diagonal.
    private static void AddPawnMoves(Board board, Square from, PieceColor color, List<Square> moves)
    {
        // Weiß zieht Richtung Zeile 0 (die 8. Reihe), Schwarz Richtung Zeile 7.
        var direction = color == PieceColor.White ? -1 : 1;
        var startRank = color == PieceColor.White ? 6 : 1;
        var oneStep = from.Rank + direction;

        if (Board.IsInside(oneStep, from.File) && board[oneStep, from.File] is null)
        {
            moves.Add(new Square(oneStep, from.File));

            var twoSteps = from.Rank + 2 * direction;
            // Doppelschritt nur aus der Grundstellung und nur, wenn auch das zweite Feld frei ist.
            if (from.Rank == startRank && board[twoSteps, from.File] is null)
            {
                moves.Add(new Square(twoSteps, from.File));
            }
        }

        // Schlagen nur diagonal und nur, wenn dort eine gegnerische Figur steht.
        foreach (var fileOffset in new[] { -1, 1 })
        {
            var file = from.File + fileOffset;
            if (Board.IsInside(oneStep, file)
                && board[oneStep, file] is { } target
                && target.Color != color)
            {
                moves.Add(new Square(oneStep, file));
            }
        }
    }

    // Für Figuren mit festen Sprüngen (Springer, König): jedes Zielfeld, das auf dem Brett liegt
    // und nicht von einer eigenen Figur besetzt ist.
    private static void AddStepMoves(
        Board board, Square from, PieceColor color,
        (int Rank, int File)[] steps, List<Square> moves)
    {
        foreach (var (dr, df) in steps)
        {
            var rank = from.Rank + dr;
            var file = from.File + df;

            if (Board.IsInside(rank, file) && board[rank, file]?.Color != color)
            {
                moves.Add(new Square(rank, file));
            }
        }
    }

    // Für Läufer, Turm und Dame: In jede Richtung gehen, bis das Brett endet oder eine Figur im Weg steht.
    private static void AddSlideMoves(
        Board board, Square from, PieceColor color,
        (int Rank, int File)[] directions, List<Square> moves)
    {
        foreach (var (dr, df) in directions)
        {
            var rank = from.Rank + dr;
            var file = from.File + df;

            while (Board.IsInside(rank, file))
            {
                var target = board[rank, file];

                if (target is null)
                {
                    moves.Add(new Square(rank, file));
                }
                else
                {
                    // Gegnerische Figur: Das Feld ist schlagbar, danach ist der Weg blockiert.
                    // Eigene Figur: Der Weg ist blockiert, das Feld nicht erreichbar.
                    if (target.Color != color)
                    {
                        moves.Add(new Square(rank, file));
                    }

                    break;
                }

                rank += dr;
                file += df;
            }
        }
    }
}
