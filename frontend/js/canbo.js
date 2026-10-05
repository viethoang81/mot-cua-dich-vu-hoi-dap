
var API_BASE = "http://localhost:5000/api/hoidap";

// Thông tin cán bộ đang đăng nhập
var CANBO = { maCB: "", hoTen: "" };

// Danh sách câu hỏi đã load
var danhSach = [];

// ID câu hỏi đang xem
var currentId = null;

/* ĐĂNG NHẬP  */
function doLogin() {
  var ma    = document.getElementById("loginMaCB").value.trim();
  var hoTen = document.getElementById("loginHoTen").value.trim();
  var err   = document.getElementById("loginError");

  if (!ma || !hoTen) {
    err.textContent = "Vui lòng nhập đầy đủ mã cán bộ và họ tên.";
    err.style.display = "block";
    return;
  }

  CANBO.maCB  = ma;
  CANBO.hoTen = hoTen;
  err.style.display = "none";

  document.getElementById("cbMaDisplay").textContent    = "Mã CB: " + ma;
  document.getElementById("cbHoTenDisplay").textContent = hoTen.toUpperCase();

  document.getElementById("loginView").style.display = "none";
  document.getElementById("mainView").style.display  = "block";

  loadTatCa();
}

function doLogout() {
  CANBO = { maCB: "", hoTen: "" };
  danhSach = []; currentId = null;
  document.getElementById("mainView").style.display  = "none";
  document.getElementById("loginView").style.display = "block";
  document.getElementById("loginMaCB").value  = "";
  document.getElementById("loginHoTen").value = "";
}

// Cho phép Enter để đăng nhập
document.addEventListener("DOMContentLoaded", function () {
  ["loginMaCB","loginHoTen"].forEach(function(id) {
    document.getElementById(id).addEventListener("keydown", function(e) {
      if (e.key === "Enter") doLogin();
    });
  });
});

/*  LOAD DANH SÁCH */
function loadTatCa() {
  fetch(API_BASE + "/tatca")
    .then(function(r) { return r.json(); })
    .then(function(data) {
      danhSach = data || [];
      applyFilter();
    })
    .catch(function() {
      showToast("Không kết nối được API", "error");
    });
}

function applyFilter() {
  var trangThai = document.getElementById("filterTrangThai").value;
  var filtered  = trangThai
    ? danhSach.filter(function(x) { return x.trangThai === trangThai; })
    : danhSach;
  renderList(filtered);
}

function renderList(list) {
  var body = document.getElementById("cbListBody");
  if (!list || list.length === 0) {
    body.innerHTML =
      '<div class="empty-state" style="padding:40px 16px">' +
      '<svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="#c5cad6" stroke-width="1.2">' +
      '<path d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>' +
      '<span class="empty-text">Không có câu hỏi nào</span></div>';
    return;
  }

  body.innerHTML = list.map(function(item) {
    var badge = item.trangThai === "DaTraLoi"
      ? '<span class="badge-datra">Đã trả lời</span>'
      : '<span class="badge-dagui">Đã gửi</span>';
    var activeClass = item.id === currentId ? " active" : "";
    var shortNoi = (item.noiDung || "").length > 55
      ? item.noiDung.slice(0, 52) + "..."
      : item.noiDung;

    return '<div class="cb-item' + activeClass + '" onclick="xemChiTiet(' + item.id + ')">' +
      '<div class="cb-item-title">' + escHtml(shortNoi) + '</div>' +
      '<div class="cb-item-meta">' +
        '<span class="msv">SV: ' + escHtml(item.maSinhVien) + '</span>' +
        '<span>' + escHtml(item.ngayTao) + '</span>' +
        '<span>' + badge + '</span>' +
      '</div>' +
    '</div>';
  }).join("");
}

/*  XEM CHI TIẾT  */
function xemChiTiet(id) {
  currentId = id;

  // Highlight item đang chọn
  document.querySelectorAll(".cb-item").forEach(function(el) {
    el.classList.remove("active");
  });
  var clicked = document.querySelector('.cb-item[onclick="xemChiTiet(' + id + ')"]');
  if (clicked) clicked.classList.add("active");

  fetch(API_BASE + "/" + id + "/chitiet")
    .then(function(r) { return r.json(); })
    .then(function(data) { renderDetail(data); })
    .catch(function() { showToast("Không tải được chi tiết", "error"); });
}

function renderDetail(data) {
  var col = document.getElementById("cbDetailCol");

  var phanHoisHtml = "";
  if (!data.phanHois || data.phanHois.length === 0) {
    phanHoisHtml = '<div class="ph-empty">Chưa có phản hồi nào</div>';
  } else {
    phanHoisHtml = data.phanHois.map(function(ph) {
      return '<div class="ph-item">' +
        '<div class="ph-item-header">' +
          '<span class="ph-item-who">Cán bộ: ' + escHtml(ph.canBoHoTen) + '</span>' +
          '<span class="ph-item-time">' + escHtml(ph.ngayTao) + '</span>' +
        '</div>' +
        '<div class="ph-item-text">' + escHtml(ph.noiDung) + '</div>' +
      '</div>';
    }).join("");
  }

  var badge = data.trangThai === "DaTraLoi"
    ? '<span class="badge-datra">Đã trả lời</span>'
    : '<span class="badge-dagui">Đã gửi</span>';

  col.innerHTML =
    '<div class="cb-detail-inner">' +

      // Header
      '<div class="cb-detail-header">' +
        '<div class="cb-detail-header-top">' +
          '<div>' +
            '<div class="cb-detail-maqc">' + escHtml(data.maYeuCau) + '</div>' +
            '<div class="cb-detail-sv">' + escHtml(data.hoTen) + '</div>' +
          '</div>' +
          badge +
        '</div>' +
        '<div class="cb-detail-meta">' +
          '<span>Mã SV: <strong>' + escHtml(data.maSinhVien) + '</strong></span>' +
          '<span>Ngày gửi: ' + escHtml(data.ngayTao) + '</span>' +
        '</div>' +
      '</div>' +

      // Câu hỏi
      '<div class="cb-question-box">' +
        '<div class="cb-question-label">Nội dung câu hỏi</div>' +
        '<div class="cb-question-text">' + escHtml(data.noiDung) + '</div>' +
      '</div>' +

      // Lịch sử phản hồi
      '<div class="cb-phanhoilist">' +
        '<div class="cb-phanhoilist-label">Lịch sử phản hồi</div>' +
        phanHoisHtml +
      '</div>' +

      // Form trả lời
      '<div class="cb-reply-box">' +
        '<div class="cb-reply-label">Phản hồi của cán bộ</div>' +
        '<textarea class="cb-reply-textarea" id="replyInput" rows="3" ' +
          'placeholder="Nhập nội dung trả lời..."></textarea>' +
        '<div class="cb-reply-footer">' +
          '<button class="btn-reply" id="replyBtn" onclick="guiTraLoi(' + data.id + ')">Gửi trả lời</button>' +
        '</div>' +
      '</div>' +

    '</div>';
}

/*  GỬI TRẢ LỜI  */
function guiTraLoi(yeuCauId) {
  var noiDung = document.getElementById("replyInput").value.trim();
  if (!noiDung) {
    var el = document.getElementById("replyInput");
    el.style.borderColor = "#e24b4a";
    el.focus();
    setTimeout(function() { el.style.borderColor = ""; }, 2000);
    return;
  }

  var btn = document.getElementById("replyBtn");
  btn.disabled  = true;
  btn.textContent = "Đang gửi...";

  fetch(API_BASE + "/" + yeuCauId + "/phanhoi", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      yeuCauId:   yeuCauId,
      canBoId:    CANBO.maCB,
      canBoHoTen: CANBO.hoTen,
      noiDung:    noiDung
    })
  })
  .then(function(r) {
    if (!r.ok) throw new Error("Lỗi server");
    return r.json();
  })
  .then(function() {
    showToast("Gửi phản hồi thành công!", "success");
    loadTatCa();           // Refresh danh sách
    xemChiTiet(yeuCauId);  // Reload chi tiết
  })
  .catch(function() {
    showToast("Gửi thất bại, thử lại!", "error");
    btn.disabled = false;
    btn.textContent = "Gửi trả lời";
  });
}

/*  TIỆN ÍCH  */
function escHtml(text) {
  return String(text || "")
    .replace(/&/g,"&amp;").replace(/</g,"&lt;")
    .replace(/>/g,"&gt;").replace(/"/g,"&quot;");
}

function showToast(msg, type) {
  var t = document.getElementById("toast");
  t.textContent = msg;
  t.className = "cb-toast show " + (type || "");
  setTimeout(function() { t.className = "cb-toast"; }, 2800);
}

function confirmLogout() {
  if (confirm("Bạn có chắc muốn đăng xuất?")) doLogout();
}
