using Microsoft.AspNetCore.Mvc;
using MotCua.Api.DTOs;
using MotCua.Domain.Entities;
using MotCua.Domain.Interfaces;
using MotCua.Infrastructure.Email;

namespace MotCua.Api.Controllers;

[ApiController]
[Route("api/hoidap")]
public class HoiDapController : ControllerBase
{
    private readonly IYeuCauRepository         _yeuCauRepo;
    private readonly ITinNhanRepository        _tinNhanRepo;
    private readonly IPhanHoiRepository        _phanHoiRepo;
    private readonly ILogger<HoiDapController> _logger;
    private readonly string                    _uploadDir;

    public HoiDapController(
        IYeuCauRepository yeuCauRepo,
        ITinNhanRepository tinNhanRepo,
        IPhanHoiRepository phanHoiRepo,
        IWebHostEnvironment env,
        ILogger<HoiDapController> logger)
    {
        _yeuCauRepo  = yeuCauRepo;
        _tinNhanRepo = tinNhanRepo;
        _phanHoiRepo = phanHoiRepo;
        _logger      = logger;
        _uploadDir   = Path.Combine(env.WebRootPath, "uploads");
        Directory.CreateDirectory(_uploadDir);
    }

    // POST /api/hoidap
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> TaoYeuCau(
        [FromForm] string maSinhVien,
        [FromForm] string hoTen,
        [FromForm] string email,
        [FromForm] string noiDung,
        IFormFile? tepDinhKem,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(maSinhVien) ||
            string.IsNullOrWhiteSpace(hoTen)      ||
            string.IsNullOrWhiteSpace(email)      ||
            string.IsNullOrWhiteSpace(noiDung))
            return BadRequest(new { message = "Vui long dien day du thong tin bat buoc." });

        string? savedFileName = null;
        if (tepDinhKem is { Length: > 0 })
        {
            if (tepDinhKem.Length > 10 * 1024 * 1024)
                return BadRequest(new { message = "File khong vuot qua 10MB." });

            var ext       = Path.GetExtension(tepDinhKem.FileName);
            savedFileName = Guid.NewGuid().ToString("N") + ext;
            var filePath  = Path.Combine(_uploadDir, savedFileName);
            await using var fs = System.IO.File.Create(filePath);
            await tepDinhKem.CopyToAsync(fs, ct);
        }

        var yeuCau = new YeuCauHoiDap
        {
            MaSinhVien = maSinhVien.Trim(),
            HoTen      = hoTen.Trim(),
            Email      = email.Trim(),
            NoiDung    = noiDung.Trim(),
            TepDinhKem = savedFileName,
            MaYeuCau   = "DT03-" + (DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 1_000_000).ToString("D6"),
            TrangThai  = TrangThaiYeuCau.DaGui,
            NgayTao    = DateTime.UtcNow
        };

        await _yeuCauRepo.TaoYeuCauAsync(yeuCau, ct);
        _logger.LogInformation("Tao yeu cau {Ma} cho SV {Msv}", yeuCau.MaYeuCau, yeuCau.MaSinhVien);

        var (subject, html) = EmailTemplates.XacNhanTaoYeuCau(yeuCau);
        await _tinNhanRepo.TaoAsync(new TinNhan
        {
            YeuCauId = yeuCau.Id,
            ToEmail  = yeuCau.Email,
            Subject  = subject,
            Body     = html
        }, ct);

        return Ok(Map(yeuCau));
    }

    // GET /api/hoidap/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> LayTheoId(int id, CancellationToken ct)
    {
        var yc = await _yeuCauRepo.LayTheoIdAsync(id, ct);
        return yc is null ? NotFound() : Ok(Map(yc));
    }

    // GET /api/hoidap/sinhvien/{maSinhVien}
    [HttpGet("sinhvien/{maSinhVien}")]
    public async Task<IActionResult> LayTheoSinhVien(string maSinhVien, CancellationToken ct)
    {
        var list = await _yeuCauRepo.LayTheoSinhVienAsync(maSinhVien, ct);
        return Ok(list.Select(Map));
    }

    // GET /api/hoidap/tatca
    [HttpGet("tatca")]
    public async Task<IActionResult> LayTatCa(CancellationToken ct)
    {
        var list = await _yeuCauRepo.LayTatCaAsync(ct);
        return Ok(list.Select(Map));
    }

    // GET /api/hoidap/{id}/chitiet
    [HttpGet("{id:int}/chitiet")]
    public async Task<IActionResult> ChiTiet(int id, CancellationToken ct)
    {
        var yc = await _yeuCauRepo.LayTheoIdAsync(id, ct);
        if (yc is null) return NotFound();

        var phanHois = await _phanHoiRepo.LayTheoYeuCauAsync(id, ct);

        return Ok(new YeuCauChiTietResponse(
            yc.Id, yc.MaYeuCau, yc.MaSinhVien, yc.HoTen,
            yc.NoiDung, yc.TrangThai.ToString(),
            yc.NgayTao.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
            phanHois.Select(p => new PhanHoiResponse(
                p.Id, p.YeuCauId, p.CanBoHoTen, p.NoiDung,
                p.NgayTao.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
            )).ToList()
        ));
    }

    // POST /api/hoidap/{id}/phanhoi
    [HttpPost("{id:int}/phanhoi")]
    public async Task<IActionResult> TraLoi(
        int id,
        [FromBody] PhanHoiRequest req,
        CancellationToken ct)
    {
        var yc = await _yeuCauRepo.LayTheoIdAsync(id, ct);
        if (yc is null) return NotFound();

        // Lưu phản hồi
        var ph = new PhanHoi
        {
            YeuCauId   = id,
            NoiDung    = req.NoiDung.Trim(),
            CanBoId    = req.CanBoId,
            CanBoHoTen = req.CanBoHoTen,
            NgayTao    = DateTime.UtcNow
        };
        await _phanHoiRepo.TaoAsync(ph, ct);

        // Cập nhật trạng thái → Đã trả lời
        yc.TrangThai = TrangThaiYeuCau.DaTraLoi;
        await _yeuCauRepo.CapNhatAsync(yc, ct);

        // Gửi email thông báo về Gmail sinh viên
        var subject = "[HUCE Mot Cua] Can bo da tra loi cau hoi " + yc.MaYeuCau;
        var html =
            "<!DOCTYPE html><html><head><meta charset='UTF-8'></head><body>" +
            "<div style='font-family:Arial,sans-serif;max-width:540px;margin:0 auto'>" +
              "<div style='background:#1a2a6c;padding:20px 24px;color:#fff;border-radius:10px 10px 0 0'>" +
                "<h2 style='margin:0;font-size:15px'>HE THONG MOT CUA HUCE</h2>" +
                "<p style='margin:4px 0 0;font-size:12px;opacity:.75'>Can bo da phan hoi cau hoi cua ban</p>" +
              "</div>" +
              "<div style='padding:22px 24px;font-size:14px;color:#222;line-height:1.7;border:1px solid #e5e7eb;border-top:none'>" +
                "<p>Kinh gui <strong>" + yc.HoTen + "</strong>,</p>" +
                "<p>Can bo <strong>" + req.CanBoHoTen + "</strong> da tra loi cau hoi " +
                   "<strong>" + yc.MaYeuCau + "</strong> cua ban.</p>" +
                "<div style='background:#f7f8fc;border-left:3px solid #1a2a6c;border-radius:6px;padding:12px 16px;margin:16px 0'>" +
                  "<div style='font-size:11px;font-weight:700;color:#1a2a6c;margin-bottom:6px'>CAU HOI CUA BAN</div>" +
                  "<div style='font-size:13px'>" + yc.NoiDung + "</div>" +
                "</div>" +
                "<div style='background:#eaf3de;border-left:3px solid #3b6d11;border-radius:6px;padding:12px 16px;margin:16px 0'>" +
                  "<div style='font-size:11px;font-weight:700;color:#3b6d11;margin-bottom:6px'>PHAN HOI CUA CAN BO</div>" +
                  "<div style='font-size:13px'>" + req.NoiDung + "</div>" +
                "</div>" +
                "<p style='font-size:12px;color:#6b7280'>Vui long dang nhap he thong de xem chi tiet.</p>" +
              "</div>" +
              "<div style='padding:12px 24px;background:#f7f8fc;font-size:11px;color:#9ca3af;text-align:center;border-radius:0 0 10px 10px'>" +
                "© " + DateTime.Now.Year + " Truong Dai hoc Xay dung Ha Noi" +
              "</div>" +
            "</div></body></html>";

        await _tinNhanRepo.TaoAsync(new TinNhan
        {
            YeuCauId = yc.Id,
            ToEmail  = yc.Email,
            Subject  = subject,
            Body     = html
        }, ct);

        _logger.LogInformation("Can bo {CB} tra loi yeu cau {Ma}", req.CanBoHoTen, yc.MaYeuCau);

        return Ok(new PhanHoiResponse(
            ph.Id, ph.YeuCauId, ph.CanBoHoTen,
            ph.NoiDung,
            ph.NgayTao.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
        ));
    }

    private static YeuCauResponse Map(YeuCauHoiDap y) => new(
        y.Id, y.MaYeuCau, y.MaSinhVien, y.HoTen, y.Email, y.NoiDung,
        y.TepDinhKem,
        y.TrangThai.ToString(),
        y.NgayTao.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
    );
}