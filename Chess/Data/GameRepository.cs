using Chess.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess.Data;

public record GameSummary(int Id, DateTime CreatedAt, string? Result, string Status, int MoveCount);

// Einzige Stelle, die Partien liest und schreibt. Für jeden Aufruf wird ein eigener Scope mit
// eigenem DbContext erzeugt, weil Blazor-Komponenten lange leben und ein langlebiger
// DbContext dort Probleme macht.
public class GameRepository(IServiceScopeFactory scopeFactory)
{
    // Legt die Partie an (storedId = null) oder hängt neue Züge an und aktualisiert den Status.
    public async Task<int> SaveAsync(int? storedId, string userId, Game game)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        StoredGame stored;

        if (storedId is { } id)
        {
            stored = await db.Games
                .Include(g => g.Moves)
                .FirstAsync(g => g.Id == id && g.UserId == userId);
        }
        else
        {
            stored = new StoredGame { UserId = userId, CreatedAt = DateTime.UtcNow };
            db.Games.Add(stored);
        }

        // Es werden nur die neuen Züge angehängt, nicht die ganze Partie neu geschrieben.
        for (var i = stored.Moves.Count; i < game.Moves.Count; i++)
        {
            var move = game.Moves[i];
            stored.Moves.Add(new StoredMove
            {
                Ply = i + 1,
                FromRank = move.From.Rank,
                FromFile = move.From.File,
                ToRank = move.To.Rank,
                ToFile = move.To.File,
                Promotion = move.Promotion is { } promotion ? (int)promotion : null,
                Notation = move.Notation
            });
        }

        stored.Status = game.Status.ToString();
        stored.Result = game.Result;
        stored.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return stored.Id;
    }

    public async Task<List<GameSummary>> GetUserGamesAsync(string userId)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await db.Games
            .AsNoTracking()
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new GameSummary(g.Id, g.CreatedAt, g.Result, g.Status, g.Moves.Count))
            .ToListAsync();
    }

    // Gibt nur Partien des angegebenen Benutzers zurück
    public async Task<StoredGame?> GetGameAsync(int id, string userId)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await db.Games
            .AsNoTracking()
            .Include(g => g.Moves)
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
    }
}
