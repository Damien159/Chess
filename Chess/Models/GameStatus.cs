namespace Chess.Models;

public enum GameStatus
{
    Ongoing,
    Check,
    Checkmate,
    Stalemate,
    DrawByRepetition,
    DrawByFiftyMoveRule,
    DrawByInsufficientMaterial
}
