namespace Chess.Models;

public static class MoveGenerator
{
    private static readonly (int Rank, int File)[] KnightSteps =
    [
        (-2, -1), (-2, 1), (-1, -2), (-1, 2), (1, -2), (1, 2), (2, -1), (2, 1)
    ];

    private static readonly (int Rank, int File)[] Diagonals =
    [
        (-1, -1), (-1, 1), (1, -1), (1, 1)
    ];

    private static readonly (int Rank, int File)[] Straights =
    [
        (-1, 0), (1, 0), (0, -1), (0, 1)
    ];

    private static readonly (int Rank, int File)[] AllDirections = [.. Diagonals, .. Straights];

    public static List<Square> GetMoves(Board board, Square from)
    {
        var moves = new List<Square>();

        if (board[from] is not { } piece)
        {
            return moves;
        }

        switch (piece.Type)
        {
            case PieceType.Pawn:
                AddPawnMoves(board, from, piece.Color, moves);
                break;
            case PieceType.Knight:
                AddStepMoves(board, from, piece.Color, KnightSteps, moves);
                break;
            case PieceType.Bishop:
                AddSlideMoves(board, from, piece.Color, Diagonals, moves);
                break;
            case PieceType.Rook:
                AddSlideMoves(board, from, piece.Color, Straights, moves);
                break;
            case PieceType.Queen:
                AddSlideMoves(board, from, piece.Color, AllDirections, moves);
                break;
            case PieceType.King:
                AddStepMoves(board, from, piece.Color, AllDirections, moves);
                break;
        }

        return moves;
    }

    private static void AddPawnMoves(Board board, Square from, PieceColor color, List<Square> moves)
    {
        var direction = color == PieceColor.White ? -1 : 1;
        var startRank = color == PieceColor.White ? 6 : 1;
        var oneStep = from.Rank + direction;

        if (Board.IsInside(oneStep, from.File) && board[oneStep, from.File] is null)
        {
            moves.Add(new Square(oneStep, from.File));

            var twoSteps = from.Rank + 2 * direction;
            if (from.Rank == startRank && board[twoSteps, from.File] is null)
            {
                moves.Add(new Square(twoSteps, from.File));
            }
        }

        foreach (var fileOffset in new[] { -1, 1 })
        {
            var file = from.File + fileOffset;
            if (Board.IsInside(oneStep, file)
                && board[oneStep, file] is { } target
                && target.Color != color)
            {
                moves.Add(new Square(oneStep, file));
            }
        }
    }

    private static void AddStepMoves(
        Board board, Square from, PieceColor color,
        (int Rank, int File)[] steps, List<Square> moves)
    {
        foreach (var (dr, df) in steps)
        {
            var rank = from.Rank + dr;
            var file = from.File + df;

            if (Board.IsInside(rank, file) && board[rank, file]?.Color != color)
            {
                moves.Add(new Square(rank, file));
            }
        }
    }

    private static void AddSlideMoves(
        Board board, Square from, PieceColor color,
        (int Rank, int File)[] directions, List<Square> moves)
    {
        foreach (var (dr, df) in directions)
        {
            var rank = from.Rank + dr;
            var file = from.File + df;

            while (Board.IsInside(rank, file))
            {
                var target = board[rank, file];

                if (target is null)
                {
                    moves.Add(new Square(rank, file));
                }
                else
                {
                    if (target.Color != color)
                    {
                        moves.Add(new Square(rank, file));
                    }

                    break;
                }

                rank += dr;
                file += df;
            }
        }
    }
}
