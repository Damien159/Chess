namespace Chess.Models;

// Eine Figur besteht nur aus Typ und Farbe und ist unveränderlich (record).
// Deshalb können sich Brett und Brettkopie dieselben Objekte teilen.
public record Piece(PieceType Type, PieceColor Color)
{
    public string Symbol => Type switch
    {
        PieceType.King => "\u265A",
        PieceType.Queen => "\u265B",
        PieceType.Rook => "\u265C",
        PieceType.Bishop => "\u265D",
        PieceType.Knight => "\u265E",
        // \uFE0E erzwingt die Textdarstellung. Ohne würde der Bauer als bunter Emoji erscheinen.
        PieceType.Pawn => "\u265F\uFE0E",
        _ => throw new ArgumentOutOfRangeException(nameof(Type))
    };
}
