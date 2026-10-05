namespace MotCua.Domain.Entities;

public enum TrangThaiYeuCau
{
    DaGui    = 1,
    DaNhan   = 2,
    DangXuLy = 3,
    DaTraLoi = 4,
    DaDong   = 5
}

public class YeuCauHoiDap
{
    public int     Id           { get; set; }
    public string  MaSinhVien   { get; set; } = default!;
    public string  HoTen        { get; set; } = default!;
    public string  Email        { get; set; } = default!;
    public string  NoiDung      { get; set; } = default!;
    public string? TepDinhKem   { get; set; }
    public string  MaYeuCau     { get; set; } = default!;
    public TrangThaiYeuCau TrangThai { get; set; } = TrangThaiYeuCau.DaGui;
    public DateTime  NgayTao     { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public ICollection<TinNhan> TinNhans { get; set; } = new List<TinNhan>();
    public ICollection<PhanHoi> PhanHois { get; set; } = new List<PhanHoi>();
}

public class TinNhan
{
    public int     Id       { get; set; }
    public int     YeuCauId { get; set; }
    public string  ToEmail  { get; set; } = default!;
    public string  Subject  { get; set; } = default!;
    public string  Body     { get; set; } = default!;
    public bool    DaGui    { get; set; } = false;
    public DateTime  TaoLuc { get; set; } = DateTime.UtcNow;
    public DateTime? GuiLuc { get; set; }
    public string? LoiGui   { get; set; }
    public int     SoLanThu { get; set; } = 0;

    public YeuCauHoiDap YeuCau { get; set; } = default!;
}

public class PhanHoi
{
    public int     Id         { get; set; }
    public int     YeuCauId   { get; set; }
    public string  NoiDung    { get; set; } = default!;
    public string  CanBoId    { get; set; } = default!;
    public string  CanBoHoTen { get; set; } = default!;
    public DateTime NgayTao   { get; set; } = DateTime.UtcNow;

    public YeuCauHoiDap YeuCau { get; set; } = default!;
}