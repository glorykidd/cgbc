using cgbc.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace cgbc.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ConnectionCard> ConnectionCards => Set<ConnectionCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConnectionCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SubmittedAt).IsDescending();
            entity.HasIndex(e => e.IsRead);
        });
    }
}
