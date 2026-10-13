# HƯỚNG DẪN & SCRIPT TẠO DATABASE VÀ CÁC BẢNG CHO DỰ ÁN TIMVIECLAM

Tài liệu này chứa toàn bộ script SQL dùng để khởi tạo Database **`TimViecLam`**, 20 bảng thực thể, các ràng buộc (Primary Key, Foreign Key, Unique) và các chỉ mục (Indexes) phục vụ tối ưu hóa hiệu năng truy vấn.

---

## 1. CÁC CÁCH KHỞI TẠO DATABASE

### Cách 1: Tự động qua Entity Framework Core CLI (Khuyên dùng)
Chạy lệnh sau tại thư mục gốc solution `TimViecLam`:
```powershell
dotnet ef database update --project TimViecLam\TimViecLam.csproj --startup-project TimViecLam\TimViecLam.csproj
```
*Lưu ý: Chuỗi kết nối trong `appsettings.json` mặc định là `Server=.\SQLEXPRESS;Database=TimViecLam;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true`.*

---

### Cách 2: Thực thi script SQL trên SSMS (SQL Server Management Studio)
1. Mở **SSMS** và kết nối tới SQL Server (`.\SQLEXPRESS` hoặc `localhost`).
2. Mở file [data/initial_create.sql](file:///e:/VSCode/C%C3%B4ng%20ngh%E1%BB%87%20ph%E1%BA%A7n%20m%E1%BB%81m/T%C3%ACm%20Vi%E1%BB%87c%20L%C3%A0m/TimViecLam/data/initial_create.sql) hoặc copy toàn bộ nội dung script ở **Mục 2** bên dưới.
3. Bấm **Execute (F5)** để chạy.

---

### Cách 3: Chạy script qua PowerShell / sqlcmd
```powershell
sqlcmd -S .\SQLEXPRESS -E -i "e:\VSCode\Công nghệ phần mềm\Tìm Việc Làm\TimViecLam\data\initial_create.sql"
```

---

## 2. TOÀN BỘ SCRIPT SQL DDL (DATABASE, BẢNG, INDEXES)

```sql
-- =========================================================================
-- BƯỚC 1: TẠO DATABASE TIMVIECLAM (NẾU CHƯA TỒN TẠI)
-- =========================================================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'TimViecLam')
BEGIN
    CREATE DATABASE [TimViecLam]
    COLLATE Vietnamese_CI_AS;
END
GO

USE [TimViecLam];
GO

-- =========================================================================
-- BƯỚC 2: KHỞI TẠO BẢNG LỊCH SỬ MIGRATION CỦA EF CORE
-- =========================================================================
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

-- =========================================================================
-- BẢNG 1: tblBenefit (Chế độ phúc lợi việc làm)
-- =========================================================================
CREATE TABLE [tblBenefit] (
    [BenefitId] int NOT NULL IDENTITY(1,1),
    [Name] nvarchar(150) NOT NULL,
    [Icon] nvarchar(100) NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    CONSTRAINT [PK_tblBenefit] PRIMARY KEY ([BenefitId])
);
GO

-- =========================================================================
-- BẢNG 2: tblJobCategory (Ngành nghề tuyển dụng)
-- =========================================================================
CREATE TABLE [tblJobCategory] (
    [CategoryId] int NOT NULL IDENTITY(1,1),
    [Name] nvarchar(150) NOT NULL,
    [Slug] nvarchar(160) NOT NULL,
    [Icon] nvarchar(100) NULL,
    [Description] nvarchar(500) NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    CONSTRAINT [PK_tblJobCategory] PRIMARY KEY ([CategoryId])
);
GO

-- =========================================================================
-- BẢNG 3: tblJobType (Hình thức việc làm: FullTime, PartTime, Internship...)
-- =========================================================================
CREATE TABLE [tblJobType] (
    [JobTypeId] int NOT NULL IDENTITY(1,1),
    [Name] nvarchar(100) NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    CONSTRAINT [PK_tblJobType] PRIMARY KEY ([JobTypeId])
);
GO

-- =========================================================================
-- BẢNG 4: tblSkill (Kỹ năng chuyên môn)
-- =========================================================================
CREATE TABLE [tblSkill] (
    [SkillId] int NOT NULL IDENTITY(1,1),
    [Name] nvarchar(100) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    CONSTRAINT [PK_tblSkill] PRIMARY KEY ([SkillId])
);
GO

-- =========================================================================
-- BẢNG 5: tblProvince (Tỉnh / Thành phố)
-- =========================================================================
CREATE TABLE [tblProvince] (
    [ProvinceId] int NOT NULL IDENTITY(1,1),
    [Code] nvarchar(20) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    CONSTRAINT [PK_tblProvince] PRIMARY KEY ([ProvinceId])
);
GO

-- =========================================================================
-- BẢNG 6: tblDistrict (Quận / Huyện)
-- =========================================================================
CREATE TABLE [tblDistrict] (
    [DistrictId] int NOT NULL IDENTITY(1,1),
    [ProvinceId] int NOT NULL,
    [Code] nvarchar(20) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    CONSTRAINT [PK_tblDistrict] PRIMARY KEY ([DistrictId]),
    CONSTRAINT [FK_tblDistrict_tblProvince_ProvinceId] FOREIGN KEY ([ProvinceId]) REFERENCES [tblProvince] ([ProvinceId]) ON DELETE NO ACTION
);
GO

-- =========================================================================
-- BẢNG 7: tblWard (Phường / Xã)
-- =========================================================================
CREATE TABLE [tblWard] (
    [WardId] int NOT NULL IDENTITY(1,1),
    [DistrictId] int NOT NULL,
    [Code] nvarchar(20) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    CONSTRAINT [PK_tblWard] PRIMARY KEY ([WardId]),
    CONSTRAINT [FK_tblWard_tblDistrict_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [tblDistrict] ([DistrictId]) ON DELETE NO ACTION
);
GO

-- =========================================================================
-- BẢNG 8: tblUser (Người dùng: Candidate, Employer, SuperAdmin)
-- =========================================================================
CREATE TABLE [tblUser] (
    [UserId] int NOT NULL IDENTITY(1,1),
    [Username] nvarchar(50) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [PasswordHash] nvarchar(256) NOT NULL,
    [FullName] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [Avatar] nvarchar(255) NULL,
    [Role] nvarchar(20) NOT NULL DEFAULT N'Candidate',
    [IsActive] bit NOT NULL DEFAULT 1,
    [FailedLoginCount] int NOT NULL DEFAULT 0,
    [LockoutEnd] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_tblUser] PRIMARY KEY ([UserId])
);
GO

-- =========================================================================
-- BẢNG 9: tblCompany (Hồ sơ Nhà tuyển dụng / Doanh nghiệp)
-- =========================================================================
CREATE TABLE [tblCompany] (
    [CompanyId] int NOT NULL IDENTITY(1,1),
    [UserId] int NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Slug] nvarchar(220) NOT NULL,
    [Logo] nvarchar(255) NULL,
    [Website] nvarchar(255) NULL,
    [TaxCode] nvarchar(50) NULL,
    [CompanySize] nvarchar(50) NOT NULL DEFAULT N'Small',
    [Description] nvarchar(max) NULL,
    [Phone] nvarchar(20) NULL,
    [Email] nvarchar(200) NULL,
    [Address] nvarchar(300) NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'Pending',
    [RejectReason] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_tblCompany] PRIMARY KEY ([CompanyId]),
    CONSTRAINT [FK_tblCompany_tblUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [tblUser] ([UserId]) ON DELETE NO ACTION
);
GO

-- =========================================================================
-- BẢNG 10: tblWorkplace (Địa điểm làm việc / Chi nhánh doanh nghiệp)
-- =========================================================================
CREATE TABLE [tblWorkplace] (
    [WorkplaceId] int NOT NULL IDENTITY(1,1),
    [CompanyId] int NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Address] nvarchar(300) NOT NULL,
    [ProvinceId] int NOT NULL,
    [DistrictId] int NOT NULL,
    [WardId] int NULL,
    [Latitude] float NULL,
    [Longitude] float NULL,
    [IsHeadOffice] bit NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_tblWorkplace] PRIMARY KEY ([WorkplaceId]),
    CONSTRAINT [FK_tblWorkplace_tblCompany_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [tblCompany] ([CompanyId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblWorkplace_tblDistrict_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [tblDistrict] ([DistrictId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblWorkplace_tblProvince_ProvinceId] FOREIGN KEY ([ProvinceId]) REFERENCES [tblProvince] ([ProvinceId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblWorkplace_tblWard_WardId] FOREIGN KEY ([WardId]) REFERENCES [tblWard] ([WardId]) ON DELETE SET NULL
);
GO

-- =========================================================================
-- BẢNG 11: tblJob (Tin tuyển dụng / Việc làm)
-- =========================================================================
CREATE TABLE [tblJob] (
    [JobId] int NOT NULL IDENTITY(1,1),
    [CompanyId] int NOT NULL,
    [WorkplaceId] int NULL,
    [CategoryId] int NOT NULL,
    [JobTypeId] int NOT NULL,
    [Title] nvarchar(250) NOT NULL,
    [Slug] nvarchar(280) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Requirements] nvarchar(max) NULL,
    [SalaryMin] decimal(18,2) NULL,
    [SalaryMax] decimal(18,2) NULL,
    [SalaryUnit] nvarchar(10) NOT NULL DEFAULT N'VND',
    [IsSalaryNegotiable] bit NOT NULL DEFAULT 0,
    [ExperienceLevel] nvarchar(50) NOT NULL DEFAULT N'Không yêu cầu',
    [EducationLevel] nvarchar(50) NOT NULL DEFAULT N'Không yêu cầu',
    [WorkMode] nvarchar(30) NOT NULL DEFAULT N'OnSite',
    [Headcount] int NOT NULL DEFAULT 1,
    [ExpiresAt] datetime2 NOT NULL,
    [ApprovalStatus] nvarchar(30) NOT NULL DEFAULT N'Draft',
    [IsFeatured] bit NOT NULL DEFAULT 0,
    [ViewCount] int NOT NULL DEFAULT 0,
    [PublishedAt] datetime2 NULL,
    [RejectReason] nvarchar(500) NULL,
    [SearchText] nvarchar(2000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_tblJob] PRIMARY KEY ([JobId]),
    CONSTRAINT [FK_tblJob_tblCompany_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [tblCompany] ([CompanyId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblJob_tblJobCategory_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [tblJobCategory] ([CategoryId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblJob_tblJobType_JobTypeId] FOREIGN KEY ([JobTypeId]) REFERENCES [tblJobType] ([JobTypeId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblJob_tblWorkplace_WorkplaceId] FOREIGN KEY ([WorkplaceId]) REFERENCES [tblWorkplace] ([WorkplaceId]) ON DELETE SET NULL
);
GO

-- =========================================================================
-- BẢNG 12: tblJobSkill (Liên kết Việc làm - Kỹ năng - Khóa chính kép)
-- =========================================================================
CREATE TABLE [tblJobSkill] (
    [JobId] int NOT NULL,
    [SkillId] int NOT NULL,
    CONSTRAINT [PK_tblJobSkill] PRIMARY KEY ([JobId], [SkillId]),
    CONSTRAINT [FK_tblJobSkill_tblJob_JobId] FOREIGN KEY ([JobId]) REFERENCES [tblJob] ([JobId]) ON DELETE CASCADE,
    CONSTRAINT [FK_tblJobSkill_tblSkill_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [tblSkill] ([SkillId]) ON DELETE NO ACTION
);
GO

-- =========================================================================
-- BẢNG 13: tblJobBenefit (Liên kết Việc làm - Phúc lợi - Khóa chính kép)
-- =========================================================================
CREATE TABLE [tblJobBenefit] (
    [JobId] int NOT NULL,
    [BenefitId] int NOT NULL,
    CONSTRAINT [PK_tblJobBenefit] PRIMARY KEY ([JobId], [BenefitId]),
    CONSTRAINT [FK_tblJobBenefit_tblBenefit_BenefitId] FOREIGN KEY ([BenefitId]) REFERENCES [tblBenefit] ([BenefitId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblJobBenefit_tblJob_JobId] FOREIGN KEY ([JobId]) REFERENCES [tblJob] ([JobId]) ON DELETE CASCADE
);
GO

-- =========================================================================
-- BẢNG 14: tblCandidateProfile (Hồ sơ ứng viên - Quan hệ 1-1 với tblUser)
-- =========================================================================
CREATE TABLE [tblCandidateProfile] (
    [ProfileId] int NOT NULL IDENTITY(1,1),
    [UserId] int NOT NULL,
    [Headline] nvarchar(200) NULL,
    [DesiredPosition] nvarchar(200) NULL,
    [ExpectedSalary] decimal(18,2) NULL,
    [ProvinceId] int NULL,
    [Summary] nvarchar(max) NULL,
    [DefaultCvPath] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_tblCandidateProfile] PRIMARY KEY ([ProfileId]),
    CONSTRAINT [FK_tblCandidateProfile_tblProvince_ProvinceId] FOREIGN KEY ([ProvinceId]) REFERENCES [tblProvince] ([ProvinceId]) ON DELETE SET NULL,
    CONSTRAINT [FK_tblCandidateProfile_tblUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [tblUser] ([UserId]) ON DELETE CASCADE
);
GO

-- =========================================================================
-- BẢNG 15: tblApplication (Đơn ứng tuyển việc làm)
-- =========================================================================
CREATE TABLE [tblApplication] (
    [ApplicationId] int NOT NULL IDENTITY(1,1),
    [JobId] int NOT NULL,
    [CandidateUserId] int NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [CoverLetter] nvarchar(2000) NULL,
    [CvPath] nvarchar(255) NOT NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'New',
    [EmployerNote] nvarchar(1000) NULL,
    [AppliedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_tblApplication] PRIMARY KEY ([ApplicationId]),
    CONSTRAINT [FK_tblApplication_tblJob_JobId] FOREIGN KEY ([JobId]) REFERENCES [tblJob] ([JobId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblApplication_tblUser_CandidateUserId] FOREIGN KEY ([CandidateUserId]) REFERENCES [tblUser] ([UserId]) ON DELETE NO ACTION
);
GO

-- =========================================================================
-- BẢNG 16: tblSavedJob (Danh sách việc làm đã lưu)
-- =========================================================================
CREATE TABLE [tblSavedJob] (
    [SavedJobId] int NOT NULL IDENTITY(1,1),
    [UserId] int NOT NULL,
    [JobId] int NOT NULL,
    [SavedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_tblSavedJob] PRIMARY KEY ([SavedJobId]),
    CONSTRAINT [FK_tblSavedJob_tblJob_JobId] FOREIGN KEY ([JobId]) REFERENCES [tblJob] ([JobId]) ON DELETE CASCADE,
    CONSTRAINT [FK_tblSavedJob_tblUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [tblUser] ([UserId]) ON DELETE CASCADE
);
GO

-- =========================================================================
-- BẢNG 17: tblJobAlert (Cảnh báo & thông báo việc làm phù hợp)
-- =========================================================================
CREATE TABLE [tblJobAlert] (
    [AlertId] int NOT NULL IDENTITY(1,1),
    [UserId] int NOT NULL,
    [Keyword] nvarchar(200) NULL,
    [ProvinceId] int NULL,
    [CategoryId] int NULL,
    [MinSalary] decimal(18,2) NULL,
    [Frequency] nvarchar(20) NOT NULL DEFAULT N'Daily',
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_tblJobAlert] PRIMARY KEY ([AlertId]),
    CONSTRAINT [FK_tblJobAlert_tblJobCategory_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [tblJobCategory] ([CategoryId]) ON DELETE SET NULL,
    CONSTRAINT [FK_tblJobAlert_tblProvince_ProvinceId] FOREIGN KEY ([ProvinceId]) REFERENCES [tblProvince] ([ProvinceId]) ON DELETE SET NULL,
    CONSTRAINT [FK_tblJobAlert_tblUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [tblUser] ([UserId]) ON DELETE CASCADE
);
GO

-- =========================================================================
-- BẢNG 18: tblPost (Bài viết tin tức / Cẩm nang nghề nghiệp)
-- =========================================================================
CREATE TABLE [tblPost] (
    [PostId] int NOT NULL IDENTITY(1,1),
    [Title] nvarchar(300) NOT NULL,
    [Slug] nvarchar(350) NOT NULL,
    [Summary] nvarchar(500) NULL,
    [Content] nvarchar(max) NULL,
    [ThumbnailImage] nvarchar(300) NULL,
    [Category] nvarchar(100) NULL,
    [IsPinned] bit NOT NULL DEFAULT 0,
    [IsPublished] bit NOT NULL DEFAULT 0,
    [PublishedAt] datetime2 NULL,
    [ViewCount] int NOT NULL DEFAULT 0,
    [MetaTitle] nvarchar(300) NULL,
    [MetaDescription] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_tblPost] PRIMARY KEY ([PostId])
);
GO

-- =========================================================================
-- BẢNG 19: tblMenu (Menu điều hướng hệ thống)
-- =========================================================================
CREATE TABLE [tblMenu] (
    [MenuId] int NOT NULL IDENTITY(1,1),
    [MenuName] nvarchar(100) NOT NULL,
    [Url] nvarchar(500) NULL,
    [Icon] nvarchar(50) NULL,
    [ParentMenuId] int NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    [Position] nvarchar(30) NULL,
    [OpenNewTab] bit NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_tblMenu] PRIMARY KEY ([MenuId]),
    CONSTRAINT [FK_tblMenu_tblMenu_ParentMenuId] FOREIGN KEY ([ParentMenuId]) REFERENCES [tblMenu] ([MenuId]) ON DELETE NO ACTION
);
GO

-- =========================================================================
-- BẢNG 20: tblAuditLog (Nhật ký kiểm toán / theo dõi thao tác người dùng)
-- =========================================================================
CREATE TABLE [tblAuditLog] (
    [LogId] int NOT NULL IDENTITY(1,1),
    [UserId] int NULL,
    [Action] nvarchar(100) NOT NULL,
    [EntityName] nvarchar(100) NOT NULL,
    [EntityId] nvarchar(50) NULL,
    [Details] nvarchar(2000) NULL,
    [IpAddress] nvarchar(50) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_tblAuditLog] PRIMARY KEY ([LogId]),
    CONSTRAINT [FK_tblAuditLog_tblUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [tblUser] ([UserId]) ON DELETE SET NULL
);
GO

-- =========================================================================
-- BƯỚC 3: CÁC CHỈ MỤC TỐI ƯU HÓA (INDEXES) & RÀNG BUỘC DUY NHẤT (UNIQUE)
-- =========================================================================

-- Unique User & Candidate Profile
CREATE UNIQUE INDEX [IX_tblUser_Username] ON [tblUser] ([Username]);
CREATE UNIQUE INDEX [IX_tblUser_Email] ON [tblUser] ([Email]);
CREATE INDEX [IX_tblUser_Role] ON [tblUser] ([Role]);
CREATE UNIQUE INDEX [IX_tblCandidateProfile_UserId] ON [tblCandidateProfile] ([UserId]);
CREATE INDEX [IX_tblCandidateProfile_ProvinceId] ON [tblCandidateProfile] ([ProvinceId]);

-- Unique Company & Slugs
CREATE UNIQUE INDEX [IX_tblCompany_Slug] ON [tblCompany] ([Slug]);
CREATE UNIQUE INDEX [IX_tblCompany_UserId] ON [tblCompany] ([UserId]);
CREATE INDEX [IX_tblCompany_Status] ON [tblCompany] ([Status]);

-- Slugs danh mục hành chính
CREATE UNIQUE INDEX [IX_tblProvince_Code] ON [tblProvince] ([Code]);
CREATE INDEX [IX_tblProvince_Slug] ON [tblProvince] ([Slug]);
CREATE INDEX [IX_tblDistrict_Code] ON [tblDistrict] ([Code]);
CREATE INDEX [IX_tblDistrict_Slug] ON [tblDistrict] ([Slug]);
CREATE INDEX [IX_tblDistrict_ProvinceId] ON [tblDistrict] ([ProvinceId]);
CREATE INDEX [IX_tblWard_Code] ON [tblWard] ([Code]);
CREATE INDEX [IX_tblWard_Slug] ON [tblWard] ([Slug]);
CREATE INDEX [IX_tblWard_DistrictId] ON [tblWard] ([DistrictId]);

-- Danh mục việc làm, loại hình, kỹ năng
CREATE UNIQUE INDEX [IX_tblJobCategory_Slug] ON [tblJobCategory] ([Slug]);
CREATE UNIQUE INDEX [IX_tblJobType_Code] ON [tblJobType] ([Code]);
CREATE UNIQUE INDEX [IX_tblSkill_Slug] ON [tblSkill] ([Slug]);
CREATE INDEX [IX_tblBenefit_Name] ON [tblBenefit] ([Name]);

-- Chỉ mục tin tuyển dụng (Job) - Hỗ trợ lọc theo thời hạn, lương, ngành nghề
CREATE INDEX [IX_tblJob_ApprovalStatus_ExpiresAt] ON [tblJob] ([ApprovalStatus], [ExpiresAt]);
CREATE INDEX [IX_tblJob_CategoryId] ON [tblJob] ([CategoryId]);
CREATE INDEX [IX_tblJob_CompanyId] ON [tblJob] ([CompanyId]);
CREATE INDEX [IX_tblJob_JobTypeId] ON [tblJob] ([JobTypeId]);
CREATE INDEX [IX_tblJob_SalaryMin_SalaryMax] ON [tblJob] ([SalaryMin], [SalaryMax]);
CREATE INDEX [IX_tblJob_Slug] ON [tblJob] ([Slug]);
CREATE INDEX [IX_tblJob_WorkplaceId] ON [tblJob] ([WorkplaceId]);

-- Chỉ mục tìm việc theo địa điểm & tọa độ bản đồ
CREATE INDEX [IX_tblWorkplace_CompanyId] ON [tblWorkplace] ([CompanyId]);
CREATE INDEX [IX_tblWorkplace_DistrictId] ON [tblWorkplace] ([DistrictId]);
CREATE INDEX [IX_tblWorkplace_WardId] ON [tblWorkplace] ([WardId]);
CREATE INDEX [IX_tblWorkplace_Latitude_Longitude] ON [tblWorkplace] ([Latitude], [Longitude]);
CREATE INDEX [IX_tblWorkplace_ProvinceId_DistrictId] ON [tblWorkplace] ([ProvinceId], [DistrictId]);

-- Chỉ mục ứng tuyển & lưu việc (Unique: 1 ứng viên chỉ nộp 1 lần / lưu 1 lần)
CREATE INDEX [IX_tblApplication_JobId] ON [tblApplication] ([JobId]);
CREATE INDEX [IX_tblApplication_CandidateUserId] ON [tblApplication] ([CandidateUserId]);
CREATE UNIQUE INDEX [IX_tblApplication_JobId_CandidateUserId] ON [tblApplication] ([JobId], [CandidateUserId]);
CREATE INDEX [IX_tblSavedJob_JobId] ON [tblSavedJob] ([JobId]);
CREATE UNIQUE INDEX [IX_tblSavedJob_UserId_JobId] ON [tblSavedJob] ([UserId], [JobId]);

-- Chỉ mục liên kết Job-Skill và Job-Benefit
CREATE INDEX [IX_tblJobSkill_SkillId] ON [tblJobSkill] ([SkillId]);
CREATE INDEX [IX_tblJobBenefit_BenefitId] ON [tblJobBenefit] ([BenefitId]);

-- Cảnh báo việc làm & Bài viết & Menu & Audit Log
CREATE INDEX [IX_tblJobAlert_UserId] ON [tblJobAlert] ([UserId]);
CREATE INDEX [IX_tblJobAlert_ProvinceId] ON [tblJobAlert] ([ProvinceId]);
CREATE INDEX [IX_tblJobAlert_CategoryId] ON [tblJobAlert] ([CategoryId]);
CREATE UNIQUE INDEX [IX_tblPost_Slug] ON [tblPost] ([Slug]);
CREATE INDEX [IX_tblPost_IsPublished] ON [tblPost] ([IsPublished]);
CREATE INDEX [IX_tblMenu_ParentMenuId] ON [tblMenu] ([ParentMenuId]);
CREATE INDEX [IX_tblAuditLog_UserId] ON [tblAuditLog] ([UserId]);
CREATE INDEX [IX_tblAuditLog_EntityName] ON [tblAuditLog] ([EntityName]);
CREATE INDEX [IX_tblAuditLog_CreatedAt] ON [tblAuditLog] ([CreatedAt]);

-- Ghi nhận Migration InitialCreate hoàn tất
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261013070331_InitialCreate', N'8.0.10');
GO

COMMIT;
GO
```
