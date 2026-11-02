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

## ĐỐI SOÁT TRƯỚC SPRINT 1 — Đối soát hiện trạng sau Giai đoạn 0 & 1
- **Thời gian thực hiện**: 2026-11-02 16:35:00
- **Trạng thái**: PASS (Đủ điều kiện sẵn sàng bước vào Sprint 1)
- **Kết quả lệnh chạy**:
  1. `dotnet build TimViecLam.sln`: Build succeeded (0 Warning(s), 0 Error(s))
  2. `dotnet test TimViecLam.sln`: 0 test khả dụng (do test tạm của Phase0/Phase1 đã dọn dẹp theo quy tắc, chỉ giữ hạ tầng csproj + TestHelper.cs)
  3. Kiểm tra Seed trên SQLite in-memory: Chạy 2 lần độc lập, kết quả 100% idempotent:
     - Lần 1: 63 Tỉnh, 126 Quận/Huyện, 29 Phường/Xã, 16 Ngành, 5 Hình thức, 42 Kỹ năng, 11 Phúc lợi, 5 Người dùng, 3 Công ty, 6 Địa điểm, 45 Việc làm, 3 Bài viết, 9 Menus.
     - Lần 2: 63 Tỉnh, 126 Quận/Huyện, 29 Phường/Xã, 16 Ngành, 5 Hình thức, 42 Kỹ năng, 11 Phúc lợi, 5 Người dùng, 3 Công ty, 6 Địa điểm, 45 Việc làm, 3 Bài viết, 9 Menus.
     - Chi tiết nghiệp vụ: 63/63 tỉnh có quận/huyện; 16 ngành (≥15); 5 hình thức; 42 kỹ năng (≥40); 11 phúc lợi (≥10); 3 công ty Approved; 6 địa điểm có tọa độ VN thật; 42 việc làm Published (≥40), 5 tin Remote, 1 tin Pending, 1 tin Rejected, 1 tin Expired; 0 việc làm thiếu SearchText (100% việc làm có SearchText chuẩn hóa).
  4. Kiểm tra mã nguồn & giao diện:
     - `variables.css`: `--brand-primary: #e03c31`, hover `#c93126`, active `#b3281e`, light `#fef2f2`, border `#fecaca`. Chuẩn xác, không cần chỉnh sửa.
     - `_Layout.cshtml`: `<html lang="vi">`, đầy đủ SEO meta tags và cấu trúc components.
     - `Program.cs`: Security headers đầy đủ (`nosniff`, `SAMEORIGIN`, `strict-origin-when-cross-origin`, CSP cho OSM tiles, Permissions-Policy, KHÔNG dùng `X-XSS-Protection`), `/health` endpoint hoạt động.
     - Băm mật khẩu: `PasswordService` sử dụng PBKDF2 (`Rfc2898DeriveBytes.Pbkdf2`) với HMAC-SHA256, Salt 128-bit, 100,000 vòng lặp. Không có SHA256 thô dùng băm mật khẩu, không có `GetAwaiter().GetResult()`.
     - Không có secret/API key/mật khẩu thật bị theo dõi trong Git.
- **Bảng đối soát CSDL thực tế so với đặc tả các Sprint**:
  | Thực thể / Cột / Chỉ mục | Hiện trạng | Trạng thái | Sprint bổ sung |
  | --- | --- | --- | --- |
  | `tblUser` (Username/Email unique, PasswordHash, Role, IsActive, FailedLoginCount, LockoutEnd) | Đã có đầy đủ | CÓ | - |
  | `tblUser` (`ResetTokenHash`, `ResetTokenExpiresAt`) | Chưa có trong entity và CSDL | THIẾU | **Sprint 1** |
  | `tblJobCategory`, `tblSkill`, `tblProvince`, `tblDistrict`, `tblWard` (Slug + index) | Đã có đầy đủ Slug và Index | CÓ | - |
  | `tblJobType` (`Code` thay vì `Slug`, có `IsActive`) | Có `Code` unique, chưa có `Slug` | LỆCH | **Sprint 2** |
  | `tblBenefit` (có `Name` index, `IsActive`, chưa có `Slug`) | Có `Name`, `Icon`, `Description`, `IsActive` | LỆCH | **Sprint 2** |
  | `tblCompany` (Status, RejectReason, Slug unique) | Đã có đầy đủ Status, RejectReason, Slug unique | CÓ | - |
  | `tblWorkplace` (Lat, Lng + Index) | Đã có Latitude, Longitude và Compound Index | CÓ | - |
  | `tblJob`, `tblJobSkill`, `tblJobBenefit`, `tblAuditLog` (+ các Index) | Đã có đầy đủ kèm các Compound Index | CÓ | - |
  | `tblPost`, `tblMenu` | Đã có đầy đủ | CÓ | - |
  | `tblContact` (Liên hệ / Phản hồi) | Chưa có trong entity và CSDL | THIẾU | **Sprint 4B** |
  | `tblApplication` (Unique JobId + CandidateUserId) | Đã có đầy đủ kèm Unique Compound Index | CÓ | - |
  | `tblCandidateProfile`, `tblSavedJob`, `tblJobAlert` | Đã có đầy đủ kèm Foreign Keys và Unique Index | CÓ | - |
- **Các việc đã sửa**:
  - Đã khôi phục các file tài liệu query trong `docs/` để đồng bộ hoàn toàn với lịch sử git (`git status` sạch 100%).
  - Không cần sửa mã nguồn hoặc CSS do các giá trị màu và bảo mật đã đúng chuẩn.

## SPRINT 1 — Quản lý người dùng & Xác thực (Chức năng 2)
- **Thời gian hoàn thành**: 2026-11-02 16:58:00
- **Trạng thái**: PASS
- **Lệnh đã chạy**:
  1. `dotnet build TimViecLam.sln`: Build succeeded (0 Warning(s), 0 Error(s))
  2. `dotnet ef migrations add AddUserResetToken`: Tạo migration thêm `ResetTokenHash` và `ResetTokenExpiresAt` cho `tblUser`.
  3. `dotnet test --filter "FullyQualifiedName~Sprint1"`: Passed: 46, Failed: 0, Skipped: 0, Total: 46, Duration: 4 s
  4. Smoke test HTTP:
     - `GET /` -> 200 OK
     - `GET /health` -> 200 OK
     - `GET /dang-nhap` -> 200 OK
     - `GET /dang-ky` -> 200 OK
     - `GET /dang-ky-nha-tuyen-dung` -> 200 OK
     - `GET /quen-mat-khau` -> 200 OK
     - `GET /dang-xuat` -> 302 Redirect
     - `GET /Admin` (chưa đăng nhập) -> 302 Redirect (tới /dang-nhap)
- **Danh sách mã AC đã phủ**:
  - `AC-S1.1`: Hai user cùng mật khẩu sinh hai hash khác nhau nhờ salt ngẫu nhiên; hash định dạng Identity PasswordHasher (PBKDF2), không phải SHA-256 thô; Verify đúng/sai chính xác; mật khẩu seed ở định dạng mới.
  - `AC-S1.2`: Đăng ký trùng username/email hoặc mật khẩu yếu (< 8 ký tự, thiếu chữ/số) bị từ chối với thông báo tiếng Việt; Đăng ký nhà tuyển dụng tạo User + tblCompany (Status=Pending) nguyên tử trong ExecutionStrategy & Transaction, rollback sạch khi lỗi.
  - `AC-S1.3`: Đăng nhập đúng sinh cookie auth HttpOnly, SameSite=Lax; Đăng nhập sai trả về thông báo chung giống hệt cho tài khoản không tồn tại và sai mật khẩu (kèm chạy băm giả chống Timing Attack); Sai 5 lần liên tiếp kích hoạt khóa tạm 15 phút (kể cả nhập đúng trong lúc bị khóa); Hết hạn khóa đăng nhập lại bình thường; Khóa user A không ảnh hưởng user B.
  - `AC-S1.4`: Ma trận quyền (Khách / Candidate / Employer-Pending / Employer-Approved / SuperAdmin) truy cập trang công khai (200), `/Admin` (302 hoặc 403 AccessDenied), SuperAdmin vào `/Admin` (200 OK).
  - `AC-S1.5`: Employer bị chuyển trạng thái `Suspended` giữa phiên -> CookieValidatePrincipal phát hiện và hủy phiên (RejectPrincipal + SignOut) ngay request kế tiếp.
  - `AC-S1.6`: Token đặt lại mật khẩu: sinh ngẫu nhiên mạnh (32 bytes), DB lưu bản băm SHA-256 (không lưu token thô); token hết hạn 30 phút; dùng 1 lần (dùng lần 2 thất bại; token sai thất bại).
  - `AC-S1.7`: `returnUrl` chỉ nhận URL nội bộ, URL độc hại `https://evil.com` và `//evil.com` bị bỏ qua, chuyển hướng về trang chủ (chống Open Redirect).
  - `AC-S1.8`: POST thiếu antiforgery token bị từ chối với 400 Bad Request; GET `/dang-xuat` không đăng xuất được (chuyển hướng an toàn về `/`).
  - `AC-S1.9`: Vượt ngưỡng rate limit (AddRateLimiter `auth-policy`) nhận 429 Too Many Requests.
  - `AC-S1.10`: Toàn bộ mã nguồn Controllers và Services: 0 cuộc gọi `.GetAwaiter().GetResult()`; 0 hàm băm mật khẩu bằng SHA-256 thô.
  - `AC-S1.11`: Cập nhật hồ sơ validate SĐT chuẩn Việt Nam; Chống IDOR (chỉ sửa hồ sơ của chính mình); Đổi mật khẩu yêu cầu mật khẩu cũ chính xác và mật khẩu mới đạt chính sách.
  - `AC-S1.12`: Migration `AddUserResetToken` áp dụng thành công trên SQLite in-memory, phương thức `Down` rollback sạch sẽ hai cột `ResetTokenHash` và `ResetTokenExpiresAt`.
- **Giả định đã chọn**:
  - Tích hợp `IPasswordService` sử dụng `Microsoft.AspNetCore.Identity.PasswordHasher<tblUser>` hỗ trợ salt 128-bit và PBKDF2 HMAC-SHA256 chuẩn bảo mật của ASP.NET Core.
  - Rate Limiter cấu hình 30 requests/phút cho các endpoint nhạy cảm (auth-policy).
  - Token đặt lại mật khẩu được băm SHA-256 trước khi lưu vào DB để đảm bảo kể cả khi lộ CSDL cũng không thể chiếm quyền tài khoản.
- **Rủi ro / việc để giai đoạn sau**:
  - Quản lý danh mục kỹ năng, ngành nghề, địa giới hành chính sẽ được triển khai giao diện và chức năng ở Sprint 2.
  - Chức năng duyệt/từ chối công ty nhà tuyển dụng của SuperAdmin sẽ hoàn thiện ở Sprint 3A.
