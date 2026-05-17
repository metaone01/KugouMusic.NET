using KgWebApi.Net.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KgWebApi.Net.Data;

public sealed class KgWebApiDbContext(DbContextOptions<KgWebApiDbContext> options) : DbContext(options)
{
    public DbSet<KgSessionEntity> Sessions => Set<KgSessionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var session = modelBuilder.Entity<KgSessionEntity>();
        session.ToTable("KgSessions");
        session.HasKey(x => x.SessionKey);
        session.Property(x => x.SessionKey).HasMaxLength(128);
        session.Property(x => x.UserId).IsRequired();
        session.Property(x => x.Token).IsRequired();
        session.Property(x => x.VipType).IsRequired();
        session.Property(x => x.VipToken).IsRequired();
        session.Property(x => x.Dfid).IsRequired();
        session.Property(x => x.Mid).IsRequired();
        session.Property(x => x.Uuid).IsRequired();
        session.Property(x => x.InstallDev).IsRequired();
        session.Property(x => x.InstallMac).IsRequired();
        session.Property(x => x.InstallGuid).IsRequired();
        session.Property(x => x.UpdatedAtUtc).IsRequired();
    }
}
