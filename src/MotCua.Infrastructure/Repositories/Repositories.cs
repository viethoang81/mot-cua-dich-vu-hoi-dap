using Microsoft.EntityFrameworkCore;
using MotCua.Domain.Entities;
using MotCua.Domain.Interfaces;
using MotCua.Infrastructure.Data;

namespace MotCua.Infrastructure.Repositories;

public class YeuCauRepository : IYeuCauRepository
{
    private readonly AppDbContext _db;
    public YeuCauRepository(AppDbContext db) => _db = db;

    public async Task<YeuCauHoiDap> TaoYeuCauAsync(YeuCauHoiDap yeuCau, CancellationToken ct = default)
    {
        _db.YeuCauHoiDaps.Add(yeuCau);
        await _db.SaveChangesAsync(ct);
        return yeuCau;
    }

    public Task<YeuCauHoiDap?> LayTheoIdAsync(int id, CancellationToken ct = default)
        => _db.YeuCauHoiDaps
              .Include(x => x.TinNhans)
              .Include(x => x.PhanHois)
              .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<YeuCauHoiDap>> LayTheoSinhVienAsync(string maSinhVien, CancellationToken ct = default)
        => _db.YeuCauHoiDaps
              .Where(x => x.MaSinhVien == maSinhVien)
              .OrderByDescending(x => x.NgayTao)
              .ToListAsync(ct);

    public async Task CapNhatAsync(YeuCauHoiDap yeuCau, CancellationToken ct = default)
    {
        yeuCau.NgayCapNhat = DateTime.UtcNow;
        _db.YeuCauHoiDaps.Update(yeuCau);
        await _db.SaveChangesAsync(ct);
    }

    public Task<List<YeuCauHoiDap>> LayTatCaAsync(CancellationToken ct = default)
        => _db.YeuCauHoiDaps
              .OrderByDescending(x => x.NgayTao)
              .ToListAsync(ct);
}

public class TinNhanRepository : ITinNhanRepository
{
    private readonly AppDbContext _db;
    public TinNhanRepository(AppDbContext db) => _db = db;

    public async Task TaoAsync(TinNhan tinNhan, CancellationToken ct = default)
    {
        _db.TinNhans.Add(tinNhan);
        await _db.SaveChangesAsync(ct);
    }

    public Task<List<TinNhan>> LayChuaGuiAsync(int limit = 50, CancellationToken ct = default)
        => _db.TinNhans
              .Where(x => !x.DaGui && x.SoLanThu < 3)
              .OrderBy(x => x.TaoLuc)
              .Take(limit)
              .ToListAsync(ct);

    public async Task CapNhatAsync(TinNhan tinNhan, CancellationToken ct = default)
    {
        _db.TinNhans.Update(tinNhan);
        await _db.SaveChangesAsync(ct);
    }
}

public class PhanHoiRepository : IPhanHoiRepository
{
    private readonly AppDbContext _db;
    public PhanHoiRepository(AppDbContext db) => _db = db;

    public async Task TaoAsync(PhanHoi phanHoi, CancellationToken ct = default)
    {
        _db.PhanHois.Add(phanHoi);
        await _db.SaveChangesAsync(ct);
    }

    public Task<List<PhanHoi>> LayTheoYeuCauAsync(int yeuCauId, CancellationToken ct = default)
        => _db.PhanHois
              .Where(x => x.YeuCauId == yeuCauId)
              .OrderBy(x => x.NgayTao)
              .ToListAsync(ct);
}