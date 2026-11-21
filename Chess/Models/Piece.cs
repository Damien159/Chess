namespace Chess.Models;

public record Piece(PieceType Type, PieceColor Color)
{
    public string Symbol => Type switch
    {
        PieceType.King => "\u265A",
        PieceType.Queen => "\u265B",
        PieceType.Rook => "\u265C",
        PieceType.Bishop => "\u265D",
        PieceType.Knight => "\u265E",
        PieceType.Pawn => "\u265F\uFE0E",
        _ => throw new ArgumentOutOfRangeException(nameof(Type))
    };
}
