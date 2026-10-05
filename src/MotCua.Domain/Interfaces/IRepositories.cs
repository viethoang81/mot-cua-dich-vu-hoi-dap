using MotCua.Domain.Entities;

namespace MotCua.Domain.Interfaces;

public interface IYeuCauRepository
{
    Task<YeuCauHoiDap>       TaoYeuCauAsync(YeuCauHoiDap yeuCau, CancellationToken ct = default);
    Task<YeuCauHoiDap?>      LayTheoIdAsync(int id, CancellationToken ct = default);
    Task<List<YeuCauHoiDap>> LayTheoSinhVienAsync(string maSinhVien, CancellationToken ct = default);
    Task                     CapNhatAsync(YeuCauHoiDap yeuCau, CancellationToken ct = default);
    Task<List<YeuCauHoiDap>> LayTatCaAsync(CancellationToken ct = default);
}

public interface ITinNhanRepository
{
    Task                TaoAsync(TinNhan tinNhan, CancellationToken ct = default);
    Task<List<TinNhan>> LayChuaGuiAsync(int limit = 50, CancellationToken ct = default);
    Task                CapNhatAsync(TinNhan tinNhan, CancellationToken ct = default);
}

public interface IEmailSender
{
    Task GuiAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}

public interface IPhanHoiRepository
{
    Task                TaoAsync(PhanHoi phanHoi, CancellationToken ct = default);
    Task<List<PhanHoi>> LayTheoYeuCauAsync(int yeuCauId, CancellationToken ct = default);
}