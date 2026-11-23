namespace Chess.Models;

// Ein gespielter Zug. Er wird in der Zugliste angezeigt und in der Datenbank gespeichert.
// Captured ist bei en passant der Bauer neben dem Zielfeld, nicht auf dem Zielfeld.
public record MoveRecord(
    Square From,
    Square To,
    PieceType? Promotion,
    string Notation,
    Piece Piece,
    Piece? Captured);
