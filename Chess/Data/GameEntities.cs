namespace Chess.Data;

// Tabelle Games: eine Partie eines Benutzers. Die Züge hängen über Moves daran.
public class StoredGame
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Name des GameStatus, z. B. "Checkmate" oder "Ongoing"
    public string Status { get; set; } = "Ongoing";

    // 1-0, 0-1, 1/2-1/2 oder null, solange die Partie läuft
    public string? Result { get; set; }

    public List<StoredMove> Moves { get; set; } = new();
}

// Tabelle Moves: ein Halbzug. Es werden nur Züge gespeichert, keine Stellungen.
// Die Stellung entsteht beim Ansehen durch Nachspielen mit derselben Schachlogik.
public class StoredMove
{
    public int Id { get; set; }

    public int GameId { get; set; }

    public StoredGame Game { get; set; } = null!;

    // Nummer des Halbzugs, beginnend bei 1
    public int Ply { get; set; }

    public int FromRank { get; set; }
    public int FromFile { get; set; }
    public int ToRank { get; set; }
    public int ToFile { get; set; }

    // PieceType als Zahl, nur bei Umwandlung gesetzt
    public int? Promotion { get; set; }

    public string Notation { get; set; } = "";
}
