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

## SPRINT 2 — Quản lý danh mục (Chức năng 3)
- **Thời gian hoàn thành**: 2026-11-09 13:26:00
- **Trạng thái**: PASS
- **Lệnh đã chạy**:
  1. `dotnet ef migrations add AddCategorySlugs`: Tạo migration bổ sung cột `Slug` và unique index cho `tblJobType` và `tblBenefit` (có script UPDATE dữ liệu cũ trước khi tạo index).
  2. `dotnet ef database update`: Áp dụng migration thành công lên CSDL SQL Server `.\SQLEXPRESS`.
  3. `dotnet build TimViecLam.sln`: Build succeeded (0 Warning(s), 0 Error(s)).
  4. `dotnet test --filter "FullyQualifiedName~Sprint2"`: Passed: 39, Failed: 0, Skipped: 0, Total: 39, Duration: 1 s.
  5. Smoke test HTTP trên ứng dụng chạy thực tế:
     - `GET /api/geo/provinces` -> 200 OK | Cache-Control: public, max-age=3600
     - `GET /api/geo/districts?provinceId=1` -> 200 OK | Cache-Control: public, max-age=3600
     - `GET /api/geo/districts` (thiếu tham số) -> 400 BadRequest
     - `GET /Admin/Category` (khách chưa đăng nhập) -> 302 Redirect (về /dang-nhap?ReturnUrl=/Admin/Category)
     - `GET /Admin/Location` (khách chưa đăng nhập) -> 302 Redirect (về /dang-nhap?ReturnUrl=/Admin/Location)
- **Danh sách mã AC đã phủ**:
  - `AC-S2.1`: Quét reflection và integration test xác nhận toàn bộ Controllers danh mục (`CategoryController`, `JobTypeController`, `SkillController`, `BenefitController`, `LocationController`) được bảo vệ bởi `[Area("Admin")]` và `[Authorize(Policy = "SuperAdminOnly")]`; Khách bị chuyển hướng 302 tới đăng nhập, Candidate/Employer nhận 403 Forbidden, SuperAdmin truy cập thành công 200 OK.
  - `AC-S2.2`: Index phân trang và tìm kiếm chính xác (tìm theo tên, từ khóa không dấu); Tối ưu truy vấn DB (đếm và lấy trang với <= 2 truy vấn cho danh sách).
  - `AC-S2.3`: Create validate tên bắt buộc và chống trùng lặp không phân biệt hoa thường và dấu tiếng Việt (thông qua `SearchTextBuilder`); Slug sinh tự động bằng `SlugHelper`, trường hợp trùng slug với tên khác được tự động thêm hậu tố `-1`, `-2`.
  - `AC-S2.4`: Edit cập nhật thông tin và cập nhật Slug tương ứng; chuyển đổi trạng thái `IsActive` (bật/tắt) hoạt động chính xác.
  - `AC-S2.5`: Delete danh mục đang được sử dụng bởi `tblJob`, `tblJobSkill`, `tblJobBenefit` bị chặn kèm thông báo tiếng Việt đề nghị chuyển sang tắt; danh mục chưa sử dụng được xóa an toàn khỏi DB.
  - `AC-S2.6`: Danh mục sau khi bị tắt (`IsActive = false`) không còn xuất hiện trong danh sách dữ liệu dùng cho bộ lọc công khai và API.
  - `AC-S2.7`: `IMemoryCache` tự động được xóa/làm mới sau các thao tác Create/Edit/Delete, đảm bảo dữ liệu mới nhất được phản ánh tức thì.
  - `AC-S2.8`: API Geo: `GET /api/geo/provinces` trả về đủ 63 tỉnh/thành phố và header `Cache-Control: public, max-age=3600`; `GET /api/geo/districts` và `GET /api/geo/wards` lọc đúng theo cha; tham số thiếu/sai trả về 400 Bad Request; id hợp lệ nhưng không có con trả về mảng rỗng `[]`.
  - `AC-S2.9`: DataSeeder chạy nhiều lần (idempotent) số lượng bản ghi không đổi; toàn bộ danh mục, loại công việc, kỹ năng, phúc lợi, tỉnh/quận/phường đều có `Slug` hợp lệ và duy nhất trong phạm vi cha.
  - `AC-S2.10`: Mọi thao tác POST dữ liệu không kèm antiforgery token bị từ chối với 400 Bad Request (`[ValidateAntiForgeryToken]`).
- **Giả định đã chọn**:
  - Tích hợp kiểm tra trùng tên danh mục không phân biệt hoa thường và dấu bằng `SearchTextBuilder.Build()`.
  - Geo API cấu hình cache 1 giờ (`Cache-Control: public, max-age=3600`) và `IMemoryCache` ở server để tối ưu hiệu năng cho dữ liệu địa giới ít thay đổi.
- **Rủi ro / việc để giai đoạn sau**:
  - Giao diện đăng tin việc làm của Employer ở Sprint 4A sẽ sử dụng Geo API (`/api/geo/provinces`, `/districts`, `/wards`) và các danh mục từ Sprint 2 để render cascading dropdown và bộ chọn kỹ năng/phúc lợi.

## SPRINT 3 · NHÓM 1 (3A) — Quản lý công ty & Địa điểm làm việc (Chức năng 4)
- **Thời gian hoàn thành**: 2026-11-09 14:18:00
- **Trạng thái**: PASS
- **Lệnh đã chạy**:
  1. `dotnet build TimViecLam.sln`: Build succeeded (0 Warning(s), 0 Error(s)).
  2. `dotnet test --filter "FullyQualifiedName~Sprint3A"`: Passed: 33, Failed: 0, Skipped: 0, Total: 33, Duration: 3 s.
  3. Smoke test HTTP trên ứng dụng chạy thực tế:
     - `GET /nha-tuyen-dung/ho-so` (chưa đăng nhập) -> 302 Redirect (tới /dang-nhap?ReturnUrl=/nha-tuyen-dung/ho-so)
     - `GET /nha-tuyen-dung/dia-diem` (chưa đăng nhập) -> 302 Redirect (tới /dang-nhap?ReturnUrl=/nha-tuyen-dung/dia-diem)
     - `GET /nha-tuyen-dung/dang-tin` (chưa đăng nhập) -> 302 Redirect (tới /dang-nhap?ReturnUrl=/nha-tuyen-dung/dang-tin)
     - `GET /Admin/Company` (chưa đăng nhập) -> 302 Redirect (tới /dang-nhap?ReturnUrl=/Admin/Company)
     - `GET /api/geo/geocode?q=Hanoi` (chưa đăng nhập) -> 302 Redirect (tới /dang-nhap)
- **Danh sách mã AC đã phủ**:
  - `AC-S3A.1`: Chống IDOR: NTD B gọi mọi thao tác xem, sửa, xóa địa điểm làm việc của NTD A đều nhận về 404 Not Found, dữ liệu của công ty A được bảo vệ toàn vẹn tuyệt đối.
  - `AC-S3A.2`: Chống Over-posting: form cập nhật thông tin công ty và tạo địa điểm không bind các trường bảo mật (`CompanyId`, `UserId`, `Status`, `RejectReason`), các giá trị nhạy cảm được gán cố định từ Claims của phiên đăng nhập.
  - `AC-S3A.3`: Tải lên logo: kiểm tra chữ ký Magic bytes (từ chối file .exe đổi đuôi .png), giới hạn dung lượng <= 2MB, quét mã độc trong file SVG (từ chối script, javascript:, event handlers), sinh tên ngẫu nhiên GUID, tạo ảnh thu nhỏ bằng ImageSharp, dọn dẹp file cũ khi cập nhật hoặc xóa logo.
  - `AC-S3A.4`: Kiểm tra hợp lệ địa điểm: tọa độ bắt buộc nằm trong phạm vi lãnh thổ Việt Nam (Lat 8–24, Lng 102–110); toàn vẹn phân cấp hành chính: Quận/Huyện phải thuộc Tỉnh/Thành phố đã chọn, Phường/Xã phải thuộc Quận/Huyện đã chọn.
  - `AC-S3A.5`: Dịch vụ Geocode Nominatim (`/api/geo/geocode`): yêu cầu đăng nhập (khách -> 401/302), có rate limit, cơ chế cache 24h ngăn ngừa gọi lại request ra ngoài, gửi header `User-Agent` định danh chuẩn quy định, giới hạn tối đa 1 req/giây toàn hệ thống và timeout 5 giây phản hồi lỗi thân thiện.
  - `AC-S3A.6`: Chặn xóa địa điểm làm việc nếu đang có tin tuyển dụng (`tblJob`) liên kết.
  - `AC-S3A.7`: NTD ở trạng thái `Pending`, `Rejected`, `Suspended` xem được trang trạng thái kèm lý do từ chối/tạm khóa và bị chặn truy cập khu vực đăng tin (`[Authorize(Policy = "ApprovedEmployer")]`).
  - `AC-S3A.8`: SuperAdmin kiểm duyệt công ty: các hành động Approve, Reject (bắt buộc nhập lý do), Suspend (bắt buộc nhập lý do), Unsuspend hoạt động chính xác; bảo vệ bởi `[Authorize(Policy = "SuperAdminOnly")]`.
  - `AC-S3A.9`: Tên công ty khi đổi sẽ tự động sinh lại `Slug` duy nhất (trường hợp trùng được tự động thêm hậu tố `-1`, `-2`).
  - `AC-S3A.10`: Truy vấn danh sách địa điểm tối ưu với <= 2 truy vấn DB (đếm + phân trang).
  - `AC-S3A.11`: Toàn bộ mã nguồn Controllers công ty và địa điểm không gọi `.GetAwaiter().GetResult()`, không bind trực tiếp entity `tblCompany`/`tblWorkplace`.
- **Giả định đã chọn**:
  - Giới hạn tốc độ gọi Nominatim OpenStreetMap được kiểm soát bằng `SemaphoreSlim(1, 1)` và timestamp tracking ở tầng ứng dụng kết hợp `IMemoryCache` 24 giờ.
  - Bản đồ tương tác sử dụng thư viện Leaflet và OpenStreetMap tiles, cho phép kéo thả ghim vị trí hoặc click bản đồ để tự động lấy tọa độ Lat/Lng.
- **Rủi ro / việc để giai đoạn sau**:
  - Ghi nhật ký kiểm duyệt công ty vào bảng `tblAuditLog` sẽ được kết nối ở Sprint 3B.
  - Đăng tin tuyển dụng và tìm kiếm việc làm theo tọa độ/bán kính sẽ được triển khai ở Sprint 4A.

## SPRINT 3 · NHÓM 2 (3B) — Quản lý tin tuyển dụng & Kiểm duyệt (Chức năng 5)
- **Thời gian hoàn thành**: 2026-11-09 15:35:00
- **Trạng thái**: PASS
- **Lệnh đã chạy**:
  1. `dotnet build TimViecLam.sln`: Build succeeded (0 Warning(s), 0 Error(s)).
  2. `dotnet test "TimViecLam\TimViecLam.sln" --filter "FullyQualifiedName~Sprint3B"`: Passed: 17, Failed: 0, Skipped: 0, Total: 17, Duration: 765 ms.
  3. Smoke test HTTP trên ứng dụng chạy thực tế:
     - `GET /nha-tuyen-dung/tin-tuyen-dung` (chưa đăng nhập) -> 302 Redirect (về /dang-nhap)
     - `GET /nha-tuyen-dung/tin-tuyen-dung/them` (chưa đăng nhập) -> 302 Redirect (về /dang-nhap)
     - `GET /admin/duyet-tin` (chưa đăng nhập) -> 302 Redirect (về /dang-nhap)
     - `POST /nha-tuyen-dung/tin-tuyen-dung/them` (không có Antiforgery token) -> 400 Bad Request
- **Danh sách mã AC đã phủ**:
  - `AC-S3B.1`: Chống IDOR: NTD B gọi mọi thao tác xem, sửa, xóa, đóng, mở lại, nhân bản tin của NTD A đều nhận 404 Not Found, dữ liệu của NTD A không đổi cả ở form lẫn AJAX.
  - `AC-S3B.2`: Chống Over-posting: form tạo/sửa tin không bind các trường bảo mật (`CompanyId`, `ApprovalStatus`, `ViewCount`, `IsFeatured`, `PublishedAt`), mọi giá trị nhạy cảm được gán ở server.
  - `AC-S3B.3`: Validate server: lương tối thiểu > tối đa, hạn nộp quá khứ hoặc quá 90 ngày, > 15 kỹ năng, tiêu đề rỗng hoặc quá dài bị từ chối kèm thông báo tiếng Việt rõ ràng.
  - `AC-S3B.4`: Làm sạch HTML: mô tả chứa `<script>`, `onerror=`, `javascript:` bị loại bỏ hoàn toàn bằng thư viện `HtmlSanitizer` (allow-list); các thẻ an toàn (`<b>`, `<ul>`, `<li>`, `<p>`, `<a href="https://...">`) được giữ nguyên.
  - `AC-S3B.5`: Vòng đời tin: chuyển trạng thái hợp lệ (`Draft` -> `Pending` -> `Published`/`Rejected` -> `Closed`); sửa trường quan trọng của tin `Published` tự động quay về `Pending`; khi cấu hình `ListingSettings:AutoApprove = true` tin tự động duyệt sang `Published`.
  - `AC-S3B.6`: Phân quyền nhà tuyển dụng: NTD thuộc công ty ở trạng thái `Pending`, `Suspended`, `Rejected` bị chặn tạo hoặc gửi tin tuyển dụng.
  - `AC-S3B.7`: Slug duy nhất có hậu tố tự động khi trùng; `SearchText` không dấu sinh chuẩn xác khi tạo và tự động cập nhật khi đổi tiêu đề tin.
  - `AC-S3B.8`: `JobQueryExtensions.WhereVisibleToPublic()` lọc chính xác chỉ tin `Published`, còn hạn nộp (`ExpiresAt > now`) và thuộc công ty `Approved`; loại bỏ hoàn toàn tin `Draft`, `Pending`, `Rejected`, `Expired`, `Closed`.
  - `AC-S3B.9`: Chặn xóa mềm tin tuyển dụng khi đã có hồ sơ ứng tuyển (`tblApplication`).
  - `AC-S3B.10`: SuperAdmin kiểm duyệt tin: xem trước, duyệt tin (`Published` + gán `PublishedAt`), từ chối tin và gỡ tin bắt buộc nhập lý do; chỉ tài khoản SuperAdmin mới có quyền (`[Authorize(Policy = "SuperAdminOnly")]`).
  - `AC-S3B.11`: Duyệt hàng loạt (`BatchApprove`): chỉ duyệt các tin ở trạng thái `Pending`, tự động bỏ qua các tin không hợp lệ, phản hồi chính xác số lượng thực tế đã duyệt.
  - `AC-S3B.12`: Nhật ký kiểm duyệt (`tblAuditLog`): ghi nhận đầy đủ hành động tạo, sửa, đóng, mở lại, xóa tin của NTD; duyệt, từ chối, gỡ tin của SuperAdmin; đồng thời nối thành công ghi log kiểm duyệt hồ sơ công ty từ Sprint 3A (`ApproveCompany`, `RejectCompany`, `SuspendCompany`, `UnsuspendCompany`, `UpdateCompanyProfile`).
  - `AC-S3B.13`: Dữ liệu mẫu (Seed): đối soát hệ thống đã có >= 3 công ty duyệt, >= 6 địa điểm thật tại Hà Nội/TP.HCM/Đà Nẵng, >= 40 tin `Published` phong phú đa dạng (nhiều mức lương, ngành, hình thức, kinh nghiệm, tin từ xa không tọa độ, tin sắp hết hạn, tin `Pending`/`Rejected`/`Expired`).
- **Giả định đã chọn**:
  - Dùng thư viện `HtmlSanitizer` cấu hình allow-list thẻ và URL scheme chuẩn thay vì dùng regex.
  - Bổ sung cấu hình `ListingSettings:AutoApprove` cho phép chuyển đổi linh hoạt chế độ tự động duyệt tin hoặc duyệt thủ công.
  - Khi nhân bản tin (`Duplicate`), tin mới luôn ở trạng thái `Draft` và thêm tiền tố `[Bản sao]` vào tiêu đề.
- **Rủi ro / việc để giai đoạn sau**:
  - Tự động hóa cập nhật trạng thái `Expired` cho các tin quá hạn nộp sẽ được kích hoạt thông qua Background Service ở Sprint tương ứng.
  - Chức năng tìm kiếm việc làm nâng cao theo từ khóa, ngành nghề, bán kính và bản đồ sẽ triển khai tại Sprint 4A.
