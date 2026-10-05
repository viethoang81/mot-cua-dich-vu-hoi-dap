var API_BASE = "http://localhost:5000/api/hoidap";

var CURRENT_USER = {
  maSinhVien: "0034167",
  hoTen:      "NGUYỄN VIẾT HOÀNG",
  email:      "0034167@huce.edu.vn"
};

var rowCount = 0;

/* KHỞI TẠO */
window.addEventListener("DOMContentLoaded", function () {
  loadList();
});

/* CHUYỂN VIEW */
function showForm() {
  document.getElementById("listView").style.display = "none";
  document.getElementById("formView").style.display = "block";
  document.getElementById("apiError").style.display = "none";
}

function showList() {
  document.getElementById("formView").style.display = "none";
  document.getElementById("listView").style.display = "block";
  loadList();
}

/* LOAD DANH SÁCH */
function loadList() {
  fetch(API_BASE + "/sinhvien/" + CURRENT_USER.maSinhVien)
    .then(function (r) { return r.json(); })
    .then(function (data) {
      rowCount = 0;
      var tbody = document.getElementById("requestBody");
      tbody.innerHTML = "";

      if (!data || data.length === 0) {
        tbody.innerHTML =
          '<tr id="emptyRow"><td colspan="2" class="empty-cell">' +
          '<div class="empty-state">' +
          '<svg width="44" height="44" viewBox="0 0 24 24" fill="none" stroke="#c5cad6" stroke-width="1.2">' +
          '<path d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>' +
          '<span class="empty-text">Chưa có yêu cầu nào</span>' +
          '<span class="empty-hint">Nhấn <strong>+ Thêm mới</strong> để gửi câu hỏi</span>' +
          "</div></td></tr>";
        return;
      }

      data.forEach(function (item) {
        rowCount++;
        renderRow(item, rowCount);
      });
    })
    .catch(function () {
      console.warn("Không kết nối được API.");
    });
}

/* XỬ LÝ FILE */
function handleFile(input) {
  if (input.files && input.files[0]) {
    var file   = input.files[0];
    var sizeKB = (file.size / 1024).toFixed(1);
    document.getElementById("fileLabel").innerHTML =
      "<strong>" + file.name + "</strong> &nbsp;—&nbsp;" + sizeKB + " KB &nbsp;✓";
  }
}

/* SUBMIT */
function submitRequest() {
  var content   = document.getElementById("contentInput").value.trim();
  var fileInput = document.getElementById("fileInput");

  if (!content) {
    var el = document.getElementById("contentInput");
    el.style.borderColor = "#e24b4a";
    el.focus();
    setTimeout(function () { el.style.borderColor = ""; }, 2000);
    return;
  }

  var btn = document.getElementById("submitBtn");
  btn.disabled  = true;
  btn.innerHTML = '<span class="spinner"></span>Đang gửi...';
  document.getElementById("apiError").style.display = "none";

  var fd = new FormData();
  fd.append("maSinhVien", CURRENT_USER.maSinhVien);
  fd.append("hoTen",      CURRENT_USER.hoTen);
  fd.append("email",      CURRENT_USER.email);
  fd.append("noiDung",    content);
  if (fileInput.files[0]) fd.append("tepDinhKem", fileInput.files[0]);

  fetch(API_BASE, { method: "POST", body: fd })
    .then(function (r) {
      if (!r.ok) return r.json().then(function (e) { throw new Error(e.message || "Lỗi server"); });
      return r.json();
    })
    .then(function (data) {
      document.getElementById("ticketInfo").innerHTML =
        makeRow("Mã yêu cầu", "<strong>" + data.maYeuCau + "</strong>") +
        makeRow("Dịch vụ",    "Hỏi đáp") +
        makeRow("Nội dung",   data.noiDung.length > 50 ? data.noiDung.slice(0,47)+"..." : data.noiDung) +
        makeRow("Trạng thái", '<span class="badge-sent">Đã gửi</span>') +
        makeRow("Ngày tạo",   data.ngayTao);

      document.getElementById("emailDisplay").textContent = CURRENT_USER.email;

      document.getElementById("contentInput").value = "";
      document.getElementById("fileInput").value    = "";
      document.getElementById("fileLabel").innerHTML = "";
      document.getElementById("charCount").textContent = "";

      document.getElementById("overlay").classList.add("show");
    })
    .catch(function (err) {
      var errBox = document.getElementById("apiError");
      errBox.textContent = "Lỗi: " + err.message;
      errBox.style.display = "block";
    })
    .finally(function () {
      btn.disabled  = false;
      btn.innerHTML = "GỬI YÊU CẦU";
    });
}

/* RENDER ROW — hiển thị badge trạng thái đúng */
function renderRow(item, num) {
  var tbody   = document.getElementById("requestBody");
  var tr      = document.createElement("tr");
  var safeNoi = escHtml(item.noiDung || "");
  var safeMa  = escHtml(item.maYeuCau || "");
  var ngayTao = item.ngayTao || "";

  // Badge trạng thái
  var badgeHtml;
  if (item.trangThai === "DaTraLoi") {
    badgeHtml = '<span style="color:#3b6d11;font-weight:600;font-style:italic">Đã trả lời ✓</span>';
  } else {
    badgeHtml = '<span class="status-sent">Đã gửi</span>';
  }

  tr.innerHTML =
    "<td>" + num + "</td>" +
    "<td>" +
      '<div class="req-title">' + safeNoi + "</div>" +
      '<div class="req-meta">Mã yêu cầu: <strong>' + safeMa + "</strong></div>" +
      '<div class="req-meta">Trạng thái: ' + badgeHtml + "</div>" +
      '<div class="req-meta">Ngày tạo: ' + ngayTao + "</div>" +
      '<button class="btn-process" ' +
        'data-id="' + item.id + '" ' +
        'onclick="showDetailModal(this)">' +
        "&#9432; Quá trình xử lý" +
      "</button>" +
    "</td>";
  tbody.appendChild(tr);
}

function makeRow(label, value) {
  return '<div class="ticket-row">' +
    '<span class="lbl">' + label + "</span>" +
    '<span class="val">' + value + "</span>" +
    "</div>";
}

/* MODAL THÀNH CÔNG */
function closeModal() {
  document.getElementById("overlay").classList.remove("show");
  showList();
}
function handleOverlayClick(e) {
  if (e.target === document.getElementById("overlay")) closeModal();
}

/* MODAL CHI TIẾT — load từ API để hiện phản hồi cán bộ */
function showDetailModal(btn) {
  var id = btn.getAttribute("data-id");

  // Reset nội dung cũ
  document.getElementById("detailContent").textContent = "Đang tải...";
  document.getElementById("detailPhanHoi").innerHTML   = "";
  document.getElementById("detailTime").textContent    = "";
  document.getElementById("detailMaYc").textContent    = "";
  document.getElementById("detailStatus").textContent  = "";
  document.getElementById("detailOverlay").classList.add("show");

  fetch(API_BASE + "/" + id + "/chitiet")
    .then(function(r) { return r.json(); })
    .then(function(data) {
      document.getElementById("detailTime").textContent   = data.ngayTao;
      document.getElementById("detailMaYc").textContent  = data.maYeuCau;
      document.getElementById("detailContent").textContent = data.noiDung;

      // Trạng thái
      var statusEl = document.getElementById("detailStatus");
      if (data.trangThai === "DaTraLoi") {
        statusEl.textContent = "Đã trả lời";
        statusEl.style.color = "#3b6d11";
      } else {
        statusEl.textContent = "Đã gửi";
        statusEl.style.color = "#ba7517";
      }

      // Phản hồi cán bộ
      var phEl = document.getElementById("detailPhanHoi");
      if (!data.phanHois || data.phanHois.length === 0) {
        phEl.innerHTML =
          '<p style="font-size:12px;color:#9ca3af;font-style:italic;margin-top:8px">' +
          'Chưa có phản hồi từ cán bộ.</p>';
      } else {
        phEl.innerHTML = data.phanHois.map(function(ph) {
          return '<div style="margin-top:10px;background:#eaf3de;border-left:3px solid #3b6d11;' +
            'border-radius:6px;padding:10px 12px">' +
            '<div style="display:flex;justify-content:space-between;font-size:11px;margin-bottom:5px">' +
              '<strong style="color:#3b6d11">Cán bộ: ' + escHtml(ph.canBoHoTen) + '</strong>' +
              '<span style="color:#9ca3af">' + escHtml(ph.ngayTao) + '</span>' +
            '</div>' +
            '<div style="font-size:13px;line-height:1.6">' + escHtml(ph.noiDung) + '</div>' +
          '</div>';
        }).join("");
      }
    })
    .catch(function() {
      document.getElementById("detailContent").textContent = "Không tải được dữ liệu.";
    });
}

function closeDetailModal() {
  document.getElementById("detailOverlay").classList.remove("show");
}
function handleDetailOverlayClick(e) {
  if (e.target === document.getElementById("detailOverlay")) closeDetailModal();
}

/* TIỆN ÍCH */
function escHtml(text) {
  return String(text || "")
    .replace(/&/g,"&amp;").replace(/</g,"&lt;")
    .replace(/>/g,"&gt;").replace(/"/g,"&quot;");
}
function confirmLogout() {
  if (confirm("Bạn có chắc muốn đăng xuất?")) alert("Đã đăng xuất. (Demo)");
}
function updateCount(el, countId, max) {
  var len = el.value.length;
  document.getElementById(countId).textContent = len + " / " + max;
  if (len > max) el.value = el.value.slice(0, max);
}
