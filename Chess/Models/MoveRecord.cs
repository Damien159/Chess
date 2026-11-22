namespace Chess.Models;

public record MoveRecord(
    Square From,
    Square To,
    PieceType? Promotion,
    string Notation,
    Piece Piece,
    Piece? Captured);
