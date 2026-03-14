using cgbc.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace cgbc.Web.Data;

public class AppDbContext : IdentityDbContext<AdminUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ConnectionCard> ConnectionCards => Set<ConnectionCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ConnectionCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SubmittedAt).IsDescending();
            entity.HasIndex(e => e.IsRead);
        });
    }
}
