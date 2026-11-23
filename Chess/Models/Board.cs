namespace Chess.Models;

public class Board
{
    private static readonly PieceType[] BackRank =
    [
        PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
        PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
    ];

    // Zeile 0 = 8. Reihe (oben), Zeile 7 = 1. Reihe (unten)
    private readonly Piece?[,] _squares = new Piece?[8, 8];

    public Piece? this[int rank, int file] => _squares[rank, file];

    public Piece? this[Square square] => _squares[square.Rank, square.File];

    // Prüft die Brettgrenzen. Überall, wo gerechnet wird, schützt das vor Zugriffen außerhalb des Arrays.
    public static bool IsInside(int rank, int file) =>
        rank is >= 0 and < 8 && file is >= 0 and < 8;

    // Setzt oder entfernt eine Figur direkt. Gebraucht für en passant (der geschlagene Bauer
    // steht neben dem Zielfeld) und für die Umwandlung.
    public void Set(Square square, Piece? piece) =>
        _squares[square.Rank, square.File] = piece;

    // Bewegt nur die Figur, ohne Regelprüfung (die steckt in Game).
    // Steht auf dem Zielfeld eine Figur, wird sie überschrieben, also geschlagen.
    public void Move(Square from, Square to)
    {
        _squares[to.Rank, to.File] = _squares[from.Rank, from.File];
        _squares[from.Rank, from.File] = null;
    }

    // Kopie des Bretts für die Selbstschach-Prüfung. Flaches Kopieren reicht,
    // weil Piece unveränderlich ist.
    public Board Clone()
    {
        var copy = new Board();
        Array.Copy(_squares, copy._squares, _squares.Length);
        return copy;
    }

    // Sucht den König einer Farbe. Wird gebraucht, um zu prüfen, ob er angegriffen wird.
    public Square? FindKing(PieceColor color)
    {
        for (var rank = 0; rank < 8; rank++)
        {
            for (var file = 0; file < 8; file++)
            {
                if (_squares[rank, file] is { Type: PieceType.King } king && king.Color == color)
                {
                    return new Square(rank, file);
                }
            }
        }

        return null;
    }

    // Baut die Grundstellung: Schwarz oben (Zeilen 0 und 1), Weiß unten (Zeilen 6 und 7).
    public static Board CreateStartPosition()
    {
        var board = new Board();

        for (var file = 0; file < 8; file++)
        {
            board._squares[0, file] = new Piece(BackRank[file], PieceColor.Black);
            board._squares[1, file] = new Piece(PieceType.Pawn, PieceColor.Black);
            board._squares[6, file] = new Piece(PieceType.Pawn, PieceColor.White);
            board._squares[7, file] = new Piece(BackRank[file], PieceColor.White);
        }

        return board;
    }
}
