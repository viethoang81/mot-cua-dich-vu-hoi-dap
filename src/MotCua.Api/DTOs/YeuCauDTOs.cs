namespace MotCua.Api.DTOs;

public record YeuCauResponse(
    int    Id,
    string MaYeuCau,
    string MaSinhVien,
    string HoTen,
    string Email,
    string NoiDung,
    string? TepDinhKem,
    string TrangThai,
    string NgayTao
);

public record PhanHoiRequest(
    int    YeuCauId,
    string CanBoId,
    string CanBoHoTen,
    string NoiDung
);

public record PhanHoiResponse(
    int    Id,
    int    YeuCauId,
    string CanBoHoTen,
    string NoiDung,
    string NgayTao
);

public record YeuCauChiTietResponse(
    int    Id,
    string MaYeuCau,
    string MaSinhVien,
    string HoTen,
    string NoiDung,
    string TrangThai,
    string NgayTao,
    List<PhanHoiResponse> PhanHois
);