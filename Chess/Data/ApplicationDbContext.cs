using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chess.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<StoredGame> Games => Set<StoredGame>();

    public DbSet<StoredMove> Moves => Set<StoredMove>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<StoredGame>(game =>
        {
            game.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            game.HasMany(g => g.Moves)
                .WithOne(m => m.Game)
                .HasForeignKey(m => m.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            game.HasIndex(g => g.UserId);
        });

        builder.Entity<StoredMove>(move =>
        {
            move.HasIndex(m => new { m.GameId, m.Ply }).IsUnique();
        });
    }
}
