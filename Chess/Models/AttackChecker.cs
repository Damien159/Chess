namespace Chess.Models;

// Beantwortet die Frage: Wird dieses Feld von dieser Farbe angegriffen?
// Der Trick: Statt alle gegnerischen Züge zu berechnen, schaut man vom Feld aus rückwärts,
// ob dort eine Figur steht, die es erreichen würde. Das wird für Schach und Rochade gebraucht.
public static class AttackChecker
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

    public static bool IsAttacked(Board board, Square target, PieceColor by)
    {
        // Bauern: schlagen diagonal nach vorne (Weiß Richtung Reihe 8 = Zeile 0)
        var pawnDirection = by == PieceColor.White ? -1 : 1;
        foreach (var fileOffset in new[] { -1, 1 })
        {
            if (IsPiece(board, target.Rank - pawnDirection, target.File + fileOffset, by, PieceType.Pawn))
            {
                return true;
            }
        }

        // Springer
        foreach (var (dr, df) in KnightSteps)
        {
            if (IsPiece(board, target.Rank + dr, target.File + df, by, PieceType.Knight))
            {
                return true;
            }
        }

        // König (Nachbarfelder)
        foreach (var (dr, df) in Diagonals.Concat(Straights))
        {
            if (IsPiece(board, target.Rank + dr, target.File + df, by, PieceType.King))
            {
                return true;
            }
        }

        // Läufer/Dame diagonal, Turm/Dame gerade
        return IsAttackedBySlider(board, target, by, Diagonals, PieceType.Bishop)
            || IsAttackedBySlider(board, target, by, Straights, PieceType.Rook);
    }

    private static bool IsPiece(Board board, int rank, int file, PieceColor color, PieceType type) =>
        Board.IsInside(rank, file)
        && board[rank, file] is { } piece
        && piece.Color == color
        && piece.Type == type;

    // Läuft vom Zielfeld in jede Richtung bis zur ersten Figur. Ist es ein passender Angreifer
    // (Läufer oder Turm, oder die Dame), wird das Feld angegriffen. Jede andere Figur blockiert den Weg.
    private static bool IsAttackedBySlider(
        Board board, Square target, PieceColor by,
        (int Rank, int File)[] directions, PieceType sliderType)
    {
        foreach (var (dr, df) in directions)
        {
            var rank = target.Rank + dr;
            var file = target.File + df;

            while (Board.IsInside(rank, file))
            {
                if (board[rank, file] is { } piece)
                {
                    if (piece.Color == by
                        && (piece.Type == sliderType || piece.Type == PieceType.Queen))
                    {
                        return true;
                    }

                    break;
                }

                rank += dr;
                file += df;
            }
        }

        return false;
    }
}
