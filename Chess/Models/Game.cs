namespace Chess.Models;

public class Game
{
    public Board Board { get; } = Board.CreateStartPosition();

    public PieceColor CurrentTurn { get; private set; } = PieceColor.White;

    public bool IsOwnPiece(Square square) =>
        Board[square] is { } piece && piece.Color == CurrentTurn;

    public IReadOnlyList<Square> GetMoves(Square from)
    {
        if (!IsOwnPiece(from))
        {
            return Array.Empty<Square>();
        }

        return MoveGenerator.GetMoves(Board, from);
    }

    public bool TryMove(Square from, Square to)
    {
        if (!GetMoves(from).Contains(to))
        {
            return false;
        }

        Board.Move(from, to);
        CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
        return true;
    }
}
