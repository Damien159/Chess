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

    public static bool IsInside(int rank, int file) =>
        rank is >= 0 and < 8 && file is >= 0 and < 8;

    public void Move(Square from, Square to)
    {
        _squares[to.Rank, to.File] = _squares[from.Rank, from.File];
        _squares[from.Rank, from.File] = null;
    }

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
