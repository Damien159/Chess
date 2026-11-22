namespace Chess.Models;

public class Game
{
    public Board Board { get; } = Board.CreateStartPosition();

    public PieceColor CurrentTurn { get; private set; } = PieceColor.White;

    public GameStatus Status { get; private set; } = GameStatus.Ongoing;

    public bool IsOver => Status is GameStatus.Checkmate or GameStatus.Stalemate;

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

    public bool TryMove(Square from, Square to)
    {
        if (!GetMoves(from).Contains(to))
        {
            return false;
        }

        Board.Move(from, to);
        CurrentTurn = Opponent(CurrentTurn);
        UpdateStatus();
        return true;
    }

    private List<Square> GetLegalMoves(Square from) =>
        MoveGenerator.GetMoves(Board, from)
            .Where(to => !LeavesKingInCheck(from, to))
            .ToList();

    private bool LeavesKingInCheck(Square from, Square to)
    {
        var copy = Board.Clone();
        copy.Move(from, to);
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
        var inCheck = IsKingAttacked(Board, CurrentTurn);
        var hasMoves = HasAnyLegalMove();

        Status = (inCheck, hasMoves) switch
        {
            (true, false) => GameStatus.Checkmate,
            (false, false) => GameStatus.Stalemate,
            (true, true) => GameStatus.Check,
            _ => GameStatus.Ongoing
        };
    }

    private static PieceColor Opponent(PieceColor color) =>
        color == PieceColor.White ? PieceColor.Black : PieceColor.White;
}
