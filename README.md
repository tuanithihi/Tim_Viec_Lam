# TimViecLam — Sàn Tìm Việc Làm

## Mục tiêu

Nền tảng tuyển dụng đa nhà tuyển dụng kết nối **Ứng viên (Candidate)**, **Nhà tuyển dụng (Employer)** và **Quản trị viên (SuperAdmin)**.

Tính năng cốt lõi:
- Tìm việc theo **vị trí công việc** (chức danh, từ khóa, kỹ năng)
- Tìm việc theo **địa điểm** (tỉnh/quận/phường, bán kính, bản đồ OpenStreetMap + Leaflet)
- Lọc theo lương, ngành nghề, hình thức, kinh nghiệm, kỹ năng
- Ứng tuyển, lưu việc, nhà tuyển dụng đăng tin, quản trị duyệt tin

## Công nghệ

| Layer | Công nghệ |
|-------|-----------|
| Backend | ASP.NET Core 8 MVC |
| Database | SQL Server / LocalDB (dev) |
| ORM | Entity Framework Core 8 |
| Frontend | Bootstrap 5 + Bootstrap Icons + Vanilla CSS |
| Maps | Leaflet.js + OpenStreetMap |
| Test | xUnit + Moq + EF InMemory/Sqlite + Mvc.Testing |

## Cấu trúc thư mục

```
TimViecLam/
├── TimViecLam/              # Web project
│   ├── Areas/               # Admin & SuperAdmin areas
│   ├── Components/          # ViewComponents (MenuView, ...)
│   ├── Controllers/         # MVC Controllers
│   ├── Data/                # AppDbContext
│   ├── Helpers/             # SlugHelper, DisplayHelper
│   ├── Migrations/          # EF Migrations
│   ├── Models/
│   │   ├── Entities/        # tblXxx entities
│   │   └── ViewModels/      # ViewModels cho View
│   ├── Services/            # IXxxService / XxxService
│   ├── Views/               # Razor Views
│   ├── wwwroot/
│   │   ├── css/             # variables.css → base.css → components.css → pages/
│   │   ├── js/
│   │   └── images/
│   ├── appsettings.json
│   └── Program.cs
├── TimViecLam.Tests/        # Test project
└── docs/
    └── PHASE_LOG.md
```

## Cách chạy

### Yêu cầu
- .NET 8 SDK
- SQL Server hoặc SQL Server LocalDB (đi kèm Visual Studio)

### Thiết lập chuỗi kết nối (User Secrets — không commit lên Git)

```bash
cd TimViecLam

# Khởi tạo user secrets
dotnet user-secrets init

# Thiết lập chuỗi kết nối thật
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=YOUR_SERVER;Database=TimViecLam;Trusted_Connection=True;"

# Nếu dùng Groq AI (tuỳ chọn, GĐ sau)
dotnet user-secrets set "GroqApi:ApiKey" "gsk_YOUR_KEY_HERE"
```

### Chạy ứng dụng

```bash
cd TimViecLam
dotnet run
# Mở trình duyệt: https://localhost:5001 hoặc http://localhost:5000
```

### Chạy test

```bash
# Chạy tất cả test
dotnet test

# Chạy test của một giai đoạn cụ thể
dotnet test --filter "FullyQualifiedName~Phase0"
```

## Quy ước kỹ thuật

- Entity: `tblXxx` + `[Table("tblXxx")]`
- DataAnnotations: ErrorMessage **tiếng Việt**
- Service: `IXxxService` / `XxxService`, đăng ký Scoped
- Truy vấn đọc: `AsNoTracking()`, phân trang server-side
- CSS: chỉ dùng biến CSS, không màu hex cứng trong view/page CSS
- Seed: idempotent (chạy nhiều lần không lỗi)

## Giai đoạn phát triển

Xem chi tiết tại [`docs/PHASE_LOG.md`](docs/PHASE_LOG.md).

| GĐ | Tên | Trạng thái |
|----|-----|-----------|
| 0 | Khung dự án & chuẩn chung | ✅ PASS |
| 1 | Authentication & Authorization | ⬜ Chưa bắt đầu |
| 2 | Job Listing & Search | ⬜ |
| 3 | Apply & Save Job | ⬜ |
| 4 | Employer Dashboard | ⬜ |
| 5 | Map & Location Search | ⬜ |
| 6 | SuperAdmin | ⬜ |
