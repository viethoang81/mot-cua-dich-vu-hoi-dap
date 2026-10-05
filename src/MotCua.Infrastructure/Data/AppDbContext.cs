using Microsoft.EntityFrameworkCore;
using MotCua.Domain.Entities;

namespace MotCua.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<YeuCauHoiDap> YeuCauHoiDaps => Set<YeuCauHoiDap>();
    public DbSet<TinNhan>      TinNhans       => Set<TinNhan>();
    public DbSet<PhanHoi>      PhanHois        => Set<PhanHoi>();  

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<YeuCauHoiDap>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.MaSinhVien).HasMaxLength(20).IsRequired();
            e.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.Property(x => x.NoiDung).HasMaxLength(2000).IsRequired();
            e.Property(x => x.TepDinhKem).HasMaxLength(260);
            e.Property(x => x.MaYeuCau).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.MaYeuCau).IsUnique();
            e.HasIndex(x => x.MaSinhVien);
            e.Property(x => x.TrangThai).HasConversion<int>();
        });

        mb.Entity<TinNhan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ToEmail).HasMaxLength(150).IsRequired();
            e.Property(x => x.Subject).HasMaxLength(250).IsRequired();
            e.Property(x => x.Body).IsRequired();
            e.Property(x => x.LoiGui).HasMaxLength(500);
            e.HasOne(x => x.YeuCau)
             .WithMany(y => y.TinNhans)
             .HasForeignKey(x => x.YeuCauId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.DaGui, x.TaoLuc });
        });

        mb.Entity<PhanHoi>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.NoiDung).HasMaxLength(2000).IsRequired();
            e.Property(x => x.CanBoId).HasMaxLength(20).IsRequired();
            e.Property(x => x.CanBoHoTen).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.YeuCau)
             .WithMany(y => y.PhanHois)
             .HasForeignKey(x => x.YeuCauId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.YeuCauId);
        });
    }
}