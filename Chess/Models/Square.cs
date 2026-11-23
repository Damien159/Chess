namespace Chess.Models;

// Ein Feld auf dem Brett. Rank 0 ist die 8. Reihe (oben), Rank 7 die 1. Reihe (unten).
// File 0 ist Linie a, File 7 ist Linie h. Beispiel: e2 = Square(6, 4).
public readonly record struct Square(int Rank, int File);
