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

## GIAI ĐOẠN 1 — Mô hình dữ liệu, Migration, Seed
- **Thời gian hoàn thành**: 2026-10-13 14:12:00
- **Trạng thái**: PASS
- **Lệnh đã chạy**:
  1. `dotnet build TimViecLam.sln`: Build succeeded (0 Warning(s), 0 Error(s))
  2. `dotnet ef migrations add InitialCreate`: Thành công, sinh snapshot và migration 20 bảng.
  3. `dotnet ef migrations script -o data/initial_create.sql`: Thành công, sinh file SQL hợp lệ (15,982 bytes).
  4. `dotnet test --filter "FullyQualifiedName~Phase1"`: Passed: 21, Failed: 0, Skipped: 0, Total: 21, Duration: 3 s
  5. Smoke test HTTP:
     - `GET /` -> 200 OK
     - `GET /health` -> 200 OK
     - `GET /khong-ton-tai` -> 404 Not Found
- **Danh sách mã AC đã phủ**:
  - `AC-1.1`: Migration áp dụng thành công trên SQLite in-memory, khởi tạo đầy đủ 20 bảng; file script migration `data/initial_create.sql` hợp lệ.
  - `AC-1.2`: Chạy `SeedAllAsync` 2 lần liên tiếp: số lượng bản ghi không đổi, không ném ngoại lệ (idempotent 100%).
  - `AC-1.3`: Unique constraints được thực thi: trùng Username, Email, Company.Slug, (JobId, CandidateUserId), (UserId, JobId) đều ném `DbUpdateException`.
  - `AC-1.4`: Xóa Province/Company đang có dữ liệu con tham chiếu bị chặn (Restrict) thành công.
  - `AC-1.5`: Tất cả Index bắt buộc tồn tại trên EF Model: Job(ApprovalStatus, ExpiresAt), Job(CategoryId), Job(JobTypeId), Job(CompanyId), Job(SalaryMin, SalaryMax), Job(Slug), Workplace(Latitude, Longitude), Workplace(ProvinceId, DistrictId), Slug của Company/Province/District, Application(JobId, CandidateUserId).
  - `AC-1.6`: `SearchTextBuilder` chuẩn hóa chuỗi tiếng Việt không dấu, chữ thường chính xác; an toàn khi null, rỗng hoặc toàn ký tự đặc biệt.
  - `AC-1.7`: Dữ liệu seed thỏa mãn toàn bộ ràng buộc nghiệp vụ: tin Published có ExpiresAt tương lai; SalaryMin <= SalaryMax; tọa độ thật Việt Nam (Lat 8–24, Lng 102–110); 100% việc làm có `SearchText` không rỗng.
  - `AC-1.8`: Mật khẩu được băm an toàn qua PBKDF2 với Salt 128-bit và 100,000 vòng lặp (IPasswordService), không có mật khẩu dạng văn bản thuần hoặc SHA-256 không salt.
- **Giả định đã chọn**:
  - Mật khẩu tài khoản demo là `Demo@123456`, được băm bằng PBKDF2 salt và định dạng lưu trữ `salt_base64:hash_base64`.
  - Tích hợp tự động chạy Seeder khi database có thể kết nối (`context.Database.CanConnect()`) trong `Program.cs`.
- **Rủi ro / việc để giai đoạn sau**:
  - Chưa có Cookie Authentication và luồng Đăng ký / Đăng nhập / Đổi mật khẩu cho người dùng (sẽ làm ở Giai đoạn 2).
  - Chưa có giao diện trang danh sách việc làm và bộ lọc tìm kiếm (sẽ triển khai ở Giai đoạn 2 và Giai đoạn 4).
