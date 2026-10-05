using MotCua.Domain.Entities;

namespace MotCua.Infrastructure.Email;

public static class EmailTemplates
{
    public static (string subject, string html) XacNhanTaoYeuCau(YeuCauHoiDap yc)
    {
        var subject    = "[HUCE Mot Cua] Xac nhan cau hoi - " + yc.MaYeuCau;
        var ngayTao    = yc.NgayTao.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        var noiDungRut = yc.NoiDung.Length <= 80 ? yc.NoiDung : yc.NoiDung.Substring(0, 77) + "...";
        var nam        = DateTime.Now.Year.ToString();

        var css =
            "body{margin:0;padding:20px;background:#f0f2f8;font-family:Arial,sans-serif;}" +
            ".w{max-width:540px;margin:0 auto;background:#fff;border-radius:10px;overflow:hidden;}" +
            ".h{background:#1a2a6c;padding:20px 24px;color:#fff;}" +
            ".h h1{margin:0;font-size:14px;font-weight:700;}" +
            ".h p{margin:4px 0 0;font-size:12px;opacity:.75;}" +
            ".b{padding:22px 24px;font-size:14px;color:#222;line-height:1.7;}" +
            ".box{background:#f7f8fc;border:1px solid #e5e7eb;border-radius:8px;padding:12px 16px;margin:16px 0;}" +
            ".r{display:flex;justify-content:space-between;padding:5px 0;font-size:13px;border-bottom:1px solid #eee;}" +
            ".r:last-child{border:none;}" +
            ".l{color:#6b7280;}" +
            ".v{font-weight:600;}" +
            ".badge{background:#faeeda;color:#ba7517;border-radius:20px;padding:2px 10px;font-size:12px;}" +
            ".note{background:#e8f4fd;border-left:3px solid #2563a8;padding:10px 14px;font-size:13px;color:#1a3a6c;margin-top:14px;}" +
            ".ft{padding:14px 24px;background:#f7f8fc;font-size:11px;color:#9ca3af;text-align:center;}";

        var html =
            "<!DOCTYPE html><html><head><meta charset='UTF-8'>" +
            "<style>" + css + "</style></head><body>" +
            "<div class='w'>" +
              "<div class='h'>" +
                "<h1>HE THONG DANG KY THU TUC HANH CHINH MOT CUA</h1>" +
                "<p>Truong Dai hoc Xay dung Ha Noi (HUCE)</p>" +
              "</div>" +
              "<div class='b'>" +
                "<p>Kinh gui <strong>" + yc.HoTen + "</strong>,</p>" +
                "<p>Cau hoi cua ban da duoc <strong>tiep nhan thanh cong</strong> vao luc <strong>" + ngayTao + "</strong>.</p>" +
                "<div class='box'>" +
                  "<div class='r'><span class='l'>Ma yeu cau</span><span class='v'>" + yc.MaYeuCau + "</span></div>" +
                  "<div class='r'><span class='l'>Ma sinh vien</span><span class='v'>" + yc.MaSinhVien + "</span></div>" +
                  "<div class='r'><span class='l'>Dich vu</span><span class='v'>DT03 - Hoi dap</span></div>" +
                  "<div class='r'><span class='l'>Noi dung</span><span class='v'>" + noiDungRut + "</span></div>" +
                  "<div class='r'><span class='l'>Trang thai</span><span class='v'><span class='badge'>Da gui</span></span></div>" +
                "</div>" +
                "<div class='note'>Nha truong se phan hoi trong vong <strong>1-3 ngay lam viec</strong>. " +
                "Vui long dang nhap he thong de theo doi trang thai.</div>" +
              "</div>" +
              "<div class='ft'>Copyright " + nam + " Truong Dai hoc Xay dung Ha Noi</div>" +
            "</div>" +
            "</body></html>";

        return (subject, html);
    }
}
