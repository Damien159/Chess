using System.Text;

namespace Chess.Models;

// Der komplette Zustand einer Partie: Brett, wer am Zug ist, Status, Zugliste, Regelgedächtnis.
// Die Oberfläche benutzt nur GetMoves, TryMove, Resign und die Remis-Methoden.
// Diese Klasse kennt weder Blazor noch die Datenbank. Deshalb lässt sie sich einfach testen.
public class Game
{
    // Das Feld, das beim letzten Doppelschritt eines Bauern übersprungen wurde.
    // En passant gilt nur direkt im nächsten Zug, danach wird es wieder null.
    private Square? _enPassantTarget;
    // Rochaderechte. Sie gehen verloren, sobald König oder der jeweilige Turm zieht
    // oder der Turm geschlagen wird (siehe UpdateCastlingRights).
    private bool _whiteKingSide = true;
    private bool _whiteQueenSide = true;
    private bool _blackKingSide = true;
    private bool _blackQueenSide = true;

    // Halbzüge seit dem letzten Bauernzug oder Schlagzug
    private int _halfmoveClock;

    // Zählt, wie oft jede Stellung vorkam (für die dreifache Wiederholung). Schlüssel ist PositionKey().
    private readonly Dictionary<string, int> _positionCounts = new();
    private readonly List<MoveRecord> _moves = new();
    private PieceColor? _resignedBy;

    public Game()
    {
        _positionCounts[PositionKey()] = 1;
    }

    public Board Board { get; } = Board.CreateStartPosition();

    public PieceColor CurrentTurn { get; private set; } = PieceColor.White;

    public GameStatus Status { get; private set; } = GameStatus.Ongoing;

    public IReadOnlyList<MoveRecord> Moves => _moves;

    // Wer hat aktuell Remis angeboten? (null = kein Angebot)
    public PieceColor? DrawOfferedBy { get; private set; }

    public bool IsOver => Status is not (GameStatus.Ongoing or GameStatus.Check);

    public PieceColor? Winner => Status switch
    {
        GameStatus.Checkmate => Opponent(CurrentTurn),
        GameStatus.Resigned when _resignedBy is { } loser => Opponent(loser),
        _ => null
    };

    // Ergebnis im PGN-Format: 1-0, 0-1 oder 1/2-1/2 (null = Partie läuft noch)
    public string? Result => !IsOver
        ? null
        : Winner switch
        {
            PieceColor.White => "1-0",
            PieceColor.Black => "0-1",
            _ => "1/2-1/2"
        };

    public bool IsOwnPiece(Square square) =>
        Board[square] is { } piece && piece.Color == CurrentTurn;

    /// <summary>
    /// Legale Ziele einer Figur des Spielers am Zug. Leer, wenn die Partie vorbei ist
    /// oder die Figur dem Gegner gehört.
    /// </summary>
    public IReadOnlyList<Square> GetMoves(Square from)
    {
        if (IsOver || !IsOwnPiece(from))
        {
            return Array.Empty<Square>();
        }

        return GetLegalMoves(from);
    }

    public bool IsPromotion(Square from, Square to) =>
        Board[from] is { Type: PieceType.Pawn } && (to.Rank == 0 || to.Rank == 7);

    // Geschlagene Figuren der angegebenen Farbe, wertvollste zuerst
    public IReadOnlyList<Piece> GetCaptured(PieceColor capturedColor) =>
        _moves
            .Where(m => m.Captured is { } c && c.Color == capturedColor)
            .Select(m => m.Captured!)
            .OrderByDescending(p => Value(p.Type))
            .ToList();

    /// <summary>
    /// Führt einen Zug aus, wenn er legal ist. Ablauf: prüfen, Notation berechnen, ziehen,
    /// Rochaderechte und en passant aktualisieren, Spieler wechseln, Status neu bestimmen.
    /// </summary>
    /// <returns>true, wenn der Zug ausgeführt wurde.</returns>
    public bool TryMove(Square from, Square to, PieceType? promotion = null)
    {
        if (!GetMoves(from).Contains(to))
        {
            return false;
        }

        var piece = Board[from]!;
        var mover = CurrentTurn;
        var captured = GetCapturedPiece(from, to);
        var isCapture = captured is not null;

        PieceType? promoteTo = IsPromotion(from, to) ? NormalizePromotion(promotion) : null;

        // Notation muss VOR dem Zug berechnet werden (Mehrdeutigkeit prüfen)
        var notation = BuildNotation(from, to, piece, isCapture, promoteTo);

        ApplyMove(Board, from, to, promoteTo);
        UpdateCastlingRights(from, to);

        // Nur nach einem Doppelschritt wird das übersprungene Feld gemerkt, bei jedem anderen Zug gelöscht.
        _enPassantTarget = piece.Type == PieceType.Pawn && Math.Abs(to.Rank - from.Rank) == 2
            ? new Square((from.Rank + to.Rank) / 2, from.File)
            : null;

        _halfmoveClock = piece.Type == PieceType.Pawn || isCapture ? 0 : _halfmoveClock + 1;

        CurrentTurn = Opponent(CurrentTurn);
        UpdateStatus();

        // Das + (Schach) oder # (Matt) kennt man erst nach dem Zug, deshalb wird es hier angehängt.
        var suffix = Status == GameStatus.Checkmate
            ? "#"
            : IsKingAttacked(Board, CurrentTurn) ? "+" : "";

        _moves.Add(new MoveRecord(from, to, promoteTo, notation + suffix, piece, captured));

        // Ein Remisangebot verfällt, wenn der Gegner des Anbietenden zieht
        if (IsOver || (DrawOfferedBy is { } offerer && offerer != mover))
        {
            DrawOfferedBy = null;
        }

        return true;
    }

    public bool Resign(PieceColor color)
    {
        if (IsOver)
        {
            return false;
        }

        _resignedBy = color;
        Status = GameStatus.Resigned;
        DrawOfferedBy = null;
        return true;
    }

    public bool OfferDraw(PieceColor by)
    {
        if (IsOver || DrawOfferedBy is not null)
        {
            return false;
        }

        DrawOfferedBy = by;
        return true;
    }

    public bool AcceptDraw()
    {
        if (IsOver || DrawOfferedBy is null)
        {
            return false;
        }

        DrawOfferedBy = null;
        Status = GameStatus.DrawByAgreement;
        return true;
    }

    public void DeclineDraw() => DrawOfferedBy = null;

    // ---------- Notation ----------

    // Welche Figur wird geschlagen? Bei en passant steht sie nicht auf dem Zielfeld, sondern daneben.
    private Piece? GetCapturedPiece(Square from, Square to)
    {
        if (Board[to] is { } target)
        {
            return target;
        }

        // En passant: Bauer geht diagonal auf ein leeres Feld
        if (Board[from] is { Type: PieceType.Pawn } && from.File != to.File)
        {
            return Board[from.Rank, to.File];
        }

        return null;
    }

    private static PieceType NormalizePromotion(PieceType? type) =>
        type is PieceType.Queen or PieceType.Rook or PieceType.Bishop or PieceType.Knight
            ? type.Value
            : PieceType.Queen;

    // Schachnotation: Figurbuchstabe (Bauer ohne), bei Mehrdeutigkeit Linie oder Reihe, x beim Schlagen,
    // Zielfeld, =Q bei Umwandlung, O-O und O-O-O bei Rochade. + und # kommen erst in TryMove dazu.
    private string BuildNotation(Square from, Square to, Piece piece, bool isCapture, PieceType? promotion)
    {
        // Rochade
        if (piece.Type == PieceType.King && Math.Abs(to.File - from.File) == 2)
        {
            return to.File == 6 ? "O-O" : "O-O-O";
        }

        var sb = new StringBuilder();

        if (piece.Type == PieceType.Pawn)
        {
            if (isCapture)
            {
                sb.Append(FileName(from.File));
            }
        }
        else
        {
            sb.Append(PieceLetter(piece.Type));
            sb.Append(Disambiguation(from, to, piece));
        }

        if (isCapture)
        {
            sb.Append('x');
        }

        sb.Append(SquareName(to));

        if (promotion is { } promoted)
        {
            sb.Append('=').Append(PieceLetter(promoted));
        }

        return sb.ToString();
    }

    // Wenn zwei gleiche Figuren dasselbe Feld erreichen können: Datei, Reihe oder beides angeben
    private string Disambiguation(Square from, Square to, Piece piece)
    {
        var others = new List<Square>();

        for (var rank = 0; rank < 8; rank++)
        {
            for (var file = 0; file < 8; file++)
            {
                var square = new Square(rank, file);
                if (square == from)
                {
                    continue;
                }

                if (Board[square] is { } other
                    && other.Type == piece.Type
                    && other.Color == piece.Color
                    && GetLegalMoves(square).Contains(to))
                {
                    others.Add(square);
                }
            }
        }

        if (others.Count == 0)
        {
            return "";
        }

        if (others.All(o => o.File != from.File))
        {
            return FileName(from.File);
        }

        if (others.All(o => o.Rank != from.Rank))
        {
            return (8 - from.Rank).ToString();
        }

        return FileName(from.File) + (8 - from.Rank);
    }

    private static string FileName(int file) => ((char)('a' + file)).ToString();

    private static string SquareName(Square square) => FileName(square.File) + (8 - square.Rank);

    private static char PieceLetter(PieceType type) => type switch
    {
        PieceType.King => 'K',
        PieceType.Queen => 'Q',
        PieceType.Rook => 'R',
        PieceType.Bishop => 'B',
        PieceType.Knight => 'N',
        _ => 'P'
    };

    private static int Value(PieceType type) => type switch
    {
        PieceType.Queen => 9,
        PieceType.Rook => 5,
        PieceType.Bishop => 3,
        PieceType.Knight => 3,
        _ => 1
    };

    // ---------- Zugausführung und Regeln ----------

    // Führt einen Zug aus, inklusive en passant, Rochade und Umwandlung.
    // Wird für echte Züge und für die Schach-Prüfung auf der Kopie benutzt.
    private static void ApplyMove(Board board, Square from, Square to, PieceType? promotion)
    {
        var piece = board[from]!;

        // En passant: Bauer geht diagonal auf ein leeres Feld
        if (piece.Type == PieceType.Pawn && from.File != to.File && board[to] is null)
        {
            board.Set(new Square(from.Rank, to.File), null);
        }

        board.Move(from, to);

        // Rochade: König zieht zwei Felder, Turm springt auf die andere Seite
        if (piece.Type == PieceType.King && Math.Abs(to.File - from.File) == 2)
        {
            if (to.File == 6)
            {
                board.Move(new Square(from.Rank, 7), new Square(from.Rank, 5));
            }
            else
            {
                board.Move(new Square(from.Rank, 0), new Square(from.Rank, 3));
            }
        }

        // Umwandlung
        if (piece.Type == PieceType.Pawn && (to.Rank == 0 || to.Rank == 7))
        {
            board.Set(to, new Piece(promotion ?? PieceType.Queen, piece.Color));
        }
    }

    // Prüft Start- und Zielfeld: Zieht König oder Turm weg, oder wird ein Turm auf seinem
    // Ausgangsfeld geschlagen, verfällt das jeweilige Rochaderecht. Zurückziehen bringt es nicht zurück.
    private void UpdateCastlingRights(Square from, Square to)
    {
        foreach (var square in new[] { from, to })
        {
            if (square == new Square(7, 4)) { _whiteKingSide = false; _whiteQueenSide = false; }
            if (square == new Square(0, 4)) { _blackKingSide = false; _blackQueenSide = false; }
            if (square == new Square(7, 7)) { _whiteKingSide = false; }
            if (square == new Square(7, 0)) { _whiteQueenSide = false; }
            if (square == new Square(0, 7)) { _blackKingSide = false; }
            if (square == new Square(0, 0)) { _blackQueenSide = false; }
        }
    }

    // Aus möglichen Zügen werden legale: Grundzüge und Sonderzüge sammeln, dann alle entfernen,
    // die den eigenen König im Schach lassen würden.
    private List<Square> GetLegalMoves(Square from)
    {
        var moves = MoveGenerator.GetMoves(Board, from);
        AddSpecialMoves(from, moves);

        return moves
            .Where(to => !LeavesKingInCheck(from, to))
            .ToList();
    }

    // Ergänzt, was vom Spielverlauf abhängt und deshalb nicht im MoveGenerator steht:
    // en passant (braucht den letzten Gegnerzug) und Rochade (braucht die Rochaderechte).
    private void AddSpecialMoves(Square from, List<Square> moves)
    {
        if (Board[from] is not { } piece)
        {
            return;
        }

        if (piece.Type == PieceType.Pawn && _enPassantTarget is { } target)
        {
            var direction = piece.Color == PieceColor.White ? -1 : 1;
            if (target.Rank == from.Rank + direction && Math.Abs(target.File - from.File) == 1)
            {
                moves.Add(target);
            }
        }

        if (piece.Type == PieceType.King)
        {
            AddCastlingMoves(from, piece.Color, moves);
        }
    }

    // Rochade nur, wenn: Recht noch da, Turm steht auf seinem Feld, Felder dazwischen leer,
    // König nicht im Schach und weder Durchgangsfeld noch Zielfeld angegriffen.
    // Bei der langen Rochade muss b1 (b8) nur leer sein, es darf angegriffen sein.
    private void AddCastlingMoves(Square from, PieceColor color, List<Square> moves)
    {
        var rank = color == PieceColor.White ? 7 : 0;
        if (from != new Square(rank, 4))
        {
            return;
        }

        var enemy = Opponent(color);
        var kingSide = color == PieceColor.White ? _whiteKingSide : _blackKingSide;
        var queenSide = color == PieceColor.White ? _whiteQueenSide : _blackQueenSide;

        if (!kingSide && !queenSide)
        {
            return;
        }

        if (AttackChecker.IsAttacked(Board, from, enemy))
        {
            return; // aus dem Schach heraus gibt es keine Rochade
        }

        if (kingSide
            && IsRook(rank, 7, color)
            && Board[rank, 5] is null && Board[rank, 6] is null
            && !AttackChecker.IsAttacked(Board, new Square(rank, 5), enemy)
            && !AttackChecker.IsAttacked(Board, new Square(rank, 6), enemy))
        {
            moves.Add(new Square(rank, 6));
        }

        if (queenSide
            && IsRook(rank, 0, color)
            && Board[rank, 1] is null && Board[rank, 2] is null && Board[rank, 3] is null
            && !AttackChecker.IsAttacked(Board, new Square(rank, 3), enemy)
            && !AttackChecker.IsAttacked(Board, new Square(rank, 2), enemy))
        {
            moves.Add(new Square(rank, 2));
        }
    }

    private bool IsRook(int rank, int file, PieceColor color) =>
        Board[rank, file] is { Type: PieceType.Rook } rook && rook.Color == color;

    // Kernstück der Regelprüfung: Der Zug wird auf einer KOPIE des Bretts ausgeführt (mit ApplyMove,
    // derselben Methode wie beim echten Zug) und danach geprüft, ob der eigene König angegriffen wird.
    // So sind Fesselungen, Königszüge auf bedrohte Felder und en passant ohne Sonderlogik abgedeckt.
    private bool LeavesKingInCheck(Square from, Square to)
    {
        var copy = Board.Clone();
        ApplyMove(copy, from, to, PieceType.Queen);
        return IsKingAttacked(copy, CurrentTurn);
    }

    private static bool IsKingAttacked(Board board, PieceColor color) =>
        board.FindKing(color) is { } king
        && AttackChecker.IsAttacked(board, king, Opponent(color));

    private bool HasAnyLegalMove()
    {
        for (var rank = 0; rank < 8; rank++)
        {
            for (var file = 0; file < 8; file++)
            {
                var square = new Square(rank, file);
                if (IsOwnPiece(square) && GetLegalMoves(square).Count > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void UpdateStatus()
    {
        var repetitions = RecordPosition();
        var inCheck = IsKingAttacked(Board, CurrentTurn);
        var hasMoves = HasAnyLegalMove();

        // Matt und Patt haben Vorrang vor allen Remisregeln
        if (!hasMoves)
        {
            Status = inCheck ? GameStatus.Checkmate : GameStatus.Stalemate;
            return;
        }

        if (HasInsufficientMaterial())
        {
            Status = GameStatus.DrawByInsufficientMaterial;
        }
        else if (repetitions >= 3)
        {
            Status = GameStatus.DrawByRepetition;
        }
        else if (_halfmoveClock >= 100)
        {
            Status = GameStatus.DrawByFiftyMoveRule;
        }
        else
        {
            Status = inCheck ? GameStatus.Check : GameStatus.Ongoing;
        }
    }

    // Zählt, wie oft die aktuelle Stellung vorkam (inklusive jetzt)
    private int RecordPosition()
    {
        var key = PositionKey();
        _positionCounts[key] = _positionCounts.GetValueOrDefault(key) + 1;
        return _positionCounts[key];
    }

    // Gleiche Stellung = gleiche Figuren, gleicher Spieler am Zug,
    // gleiche Rochaderechte, gleiche en-passant-Möglichkeit
    private string PositionKey()
    {
        var sb = new StringBuilder();

        for (var rank = 0; rank < 8; rank++)
        {
            for (var file = 0; file < 8; file++)
            {
                if (Board[rank, file] is { } piece)
                {
                    var letter = piece.Type switch
                    {
                        PieceType.Pawn => 'p',
                        PieceType.Knight => 'n',
                        PieceType.Bishop => 'b',
                        PieceType.Rook => 'r',
                        PieceType.Queen => 'q',
                        _ => 'k'
                    };
                    sb.Append(piece.Color == PieceColor.White ? char.ToUpper(letter) : letter);
                }
                else
                {
                    sb.Append('.');
                }
            }
        }

        sb.Append(CurrentTurn == PieceColor.White ? " w " : " b ");
        sb.Append(_whiteKingSide ? 'K' : '-');
        sb.Append(_whiteQueenSide ? 'Q' : '-');
        sb.Append(_blackKingSide ? 'k' : '-');
        sb.Append(_blackQueenSide ? 'q' : '-');

        if (IsEnPassantPossible() && _enPassantTarget is { } target)
        {
            sb.Append(' ').Append(target.Rank).Append(target.File);
        }

        return sb.ToString();
    }

    // En passant zählt nur, wenn ein Bauer den Zug wirklich legal ausführen kann
    private bool IsEnPassantPossible()
    {
        if (_enPassantTarget is not { } target)
        {
            return false;
        }

        var direction = CurrentTurn == PieceColor.White ? -1 : 1;
        var pawnRank = target.Rank - direction;

        foreach (var fileOffset in new[] { -1, 1 })
        {
            var file = target.File + fileOffset;
            if (Board.IsInside(pawnRank, file)
                && Board[pawnRank, file] is { Type: PieceType.Pawn } pawn
                && pawn.Color == CurrentTurn
                && GetLegalMoves(new Square(pawnRank, file)).Contains(target))
            {
                return true;
            }
        }

        return false;
    }

    // Remis bei: König gegen König, König+Läufer oder König+Springer gegen König,
    // sowie nur Läufer auf gleichfarbigen Feldern
    private bool HasInsufficientMaterial()
    {
        var minorPieces = new List<(PieceType Type, int SquareColor)>();

        for (var rank = 0; rank < 8; rank++)
        {
            for (var file = 0; file < 8; file++)
            {
                if (Board[rank, file] is not { } piece || piece.Type == PieceType.King)
                {
                    continue;
                }

                if (piece.Type is PieceType.Pawn or PieceType.Rook or PieceType.Queen)
                {
                    return false;
                }

                minorPieces.Add((piece.Type, (rank + file) % 2));
            }
        }

        if (minorPieces.Count <= 1)
        {
            return true;
        }

        return minorPieces.All(p => p.Type == PieceType.Bishop)
            && minorPieces.All(p => p.SquareColor == minorPieces[0].SquareColor);
    }

    private static PieceColor Opponent(PieceColor color) =>
        color == PieceColor.White ? PieceColor.Black : PieceColor.White;
}
