using System.Text;

namespace Chess.Models;

public class Game
{
    private Square? _enPassantTarget;
    private bool _whiteKingSide = true;
    private bool _whiteQueenSide = true;
    private bool _blackKingSide = true;
    private bool _blackQueenSide = true;

    // Halbzüge seit dem letzten Bauernzug oder Schlagzug
    private int _halfmoveClock;

    private readonly Dictionary<string, int> _positionCounts = new();

    public Game()
    {
        _positionCounts[PositionKey()] = 1;
    }

    public Board Board { get; } = Board.CreateStartPosition();

    public PieceColor CurrentTurn { get; private set; } = PieceColor.White;

    public GameStatus Status { get; private set; } = GameStatus.Ongoing;

    public bool IsOver => Status is not (GameStatus.Ongoing or GameStatus.Check);

    public PieceColor? Winner =>
        Status == GameStatus.Checkmate ? Opponent(CurrentTurn) : null;

    public bool IsOwnPiece(Square square) =>
        Board[square] is { } piece && piece.Color == CurrentTurn;

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

    public bool TryMove(Square from, Square to, PieceType? promotion = null)
    {
        if (!GetMoves(from).Contains(to))
        {
            return false;
        }

        var piece = Board[from]!;
        var isCapture = Board[to] is not null
            || (piece.Type == PieceType.Pawn && from.File != to.File);

        ApplyMove(Board, from, to, promotion);
        UpdateCastlingRights(from, to);

        _enPassantTarget = piece.Type == PieceType.Pawn && Math.Abs(to.Rank - from.Rank) == 2
            ? new Square((from.Rank + to.Rank) / 2, from.File)
            : null;

        _halfmoveClock = piece.Type == PieceType.Pawn || isCapture ? 0 : _halfmoveClock + 1;

        CurrentTurn = Opponent(CurrentTurn);
        UpdateStatus();
        return true;
    }

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

    private List<Square> GetLegalMoves(Square from)
    {
        var moves = MoveGenerator.GetMoves(Board, from);
        AddSpecialMoves(from, moves);

        return moves
            .Where(to => !LeavesKingInCheck(from, to))
            .ToList();
    }

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
