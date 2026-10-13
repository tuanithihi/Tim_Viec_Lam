# NHẬT KÝ TIẾN ĐỘ THEO DÕI CÁC GIAI ĐOẠN (PHASE LOG)

## GIAI ĐOẠN 0 — Khung dự án & chuẩn chung
- **Thời gian hoàn thành**: 2026-10-13 13:42:00
- **Trạng thái**: PASS
- **Lệnh đã chạy**:
  1. `dotnet build TimViecLam.sln`: Build succeeded (0 Warning(s), 0 Error(s))
  2. `dotnet test --filter "FullyQualifiedName~Phase0"`: Passed: 35, Failed: 0, Skipped: 0, Total: 35
  3. Smoke test HTTP:
     - `GET /` -> 200 OK
     - `GET /health` -> 200 OK
     - `GET /khong-ton-tai` -> 404 Not Found
     - `GET /css/variables.css` -> 200 OK
- **Danh sách mã AC đã phủ**:
  - `AC-0.1`: Solution cấu trúc chuẩn gồm TimViecLam và TimViecLam.Tests, loại trừ test khỏi project web.
  - `AC-0.2`: Trang chủ GET / trả về 200, chứa `<html lang="vi">`, `<title>`, `meta viewport`.
  - `AC-0.3`: Security headers đầy đủ (nosniff, SAMEORIGIN, strict-origin-when-cross-origin, permissions-policy, CSP hỗ trợ OSM tiles, KHÔNG có header X-XSS-Protection lỗi thời).
  - `AC-0.4`: `/health` trả 200; trang lỗi 404/500/403 tùy biến, không để lộ stack trace.
  - `AC-0.5`: Design system CSS với `--brand-primary: #e03c31` và chuỗi import variables -> base -> components.
  - `AC-0.6`: Không chứa bí mật trong appsettings.json (chỉ placeholder rỗng hoặc LocalDB mẫu).
  - `AC-0.7`: Không có mã màu hex cứng trong View và CSS các trang.
  - `AC-0.8`: Cấu trúc thư mục đầy đủ chuẩn dự án tham chiếu; SlugHelper bỏ dấu tiếng Việt chính xác.
- **Giả định đã chọn**:
  - Sử dụng CSS Design System lấy cảm hứng từ QuanLyPhongTro, bảo tồn toàn bộ biến CSS và màu chủ đạo #e03c31.
  - Cấu hình Content Security Policy hỗ trợ nạp tile bản đồ OpenStreetMap (*.tile.openstreetmap.org) cho các tính năng tìm kiếm việc làm theo bản đồ ở các giai đoạn sau.
- **Rủi ro / việc để giai đoạn sau**:
  - Chưa có bảng cơ sở dữ liệu Entity Framework Core (sẽ tạo các tblXxx từ Giai đoạn 1).
  - Chưa tích hợp xác thực / ủy quyền Cookie Authentication (sẽ triển khai ở Giai đoạn 1).
