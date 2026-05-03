using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Links;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LexiLink.Modules.Games.Infrastructure;

public class GamesContext : DbContext
{
    public DbSet<Category> Categories { get; set; } = null!;

    public DbSet<Link> Links { get; set; } = null!;

    public DbSet<Game> Games { get; set; } = null!;

    private readonly ILoggerFactory _loggerFactory;

    public GamesContext(DbContextOptions options, ILoggerFactory loggerFactory)
        : base(options)
    {
        _loggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder.UseLoggerFactory(_loggerFactory).EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
}
