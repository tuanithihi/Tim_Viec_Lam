-- =========================================================================
-- SCRIPT TẠO DATABASE VÀ BẢNG DỰ ÁN TIMVIECLAM
-- Chạy trực tiếp trong SQL Server Management Studio (SSMS) - Nhấn F5 để thực thi
-- =========================================================================
USE master;
GO
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'TimViecLam')
BEGIN
    CREATE DATABASE [TimViecLam] COLLATE Vietnamese_CI_AS;
END
GO
USE [TimViecLam];
GO

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

CREATE TABLE [tblBenefit] (
    [BenefitId] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    [Icon] nvarchar(100) NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_tblBenefit] PRIMARY KEY ([BenefitId])
);
GO

CREATE TABLE [tblJobCategory] (
    [CategoryId] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    [Slug] nvarchar(160) NOT NULL,
    [Icon] nvarchar(100) NULL,
    [Description] nvarchar(500) NULL,
    [SortOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_tblJobCategory] PRIMARY KEY ([CategoryId])
);
GO

CREATE TABLE [tblJobType] (
    [JobTypeId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NULL,
    [SortOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_tblJobType] PRIMARY KEY ([JobTypeId])
);
GO

CREATE TABLE [tblMenu] (
    [MenuId] int NOT NULL IDENTITY,
    [MenuName] nvarchar(100) NOT NULL,
    [Url] nvarchar(500) NULL,
    [Icon] nvarchar(50) NULL,
    [ParentMenuId] int NULL,
    [SortOrder] int NOT NULL,
    [Position] nvarchar(30) NULL,
    [OpenNewTab] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_tblMenu] PRIMARY KEY ([MenuId]),
    CONSTRAINT [FK_tblMenu_tblMenu_ParentMenuId] FOREIGN KEY ([ParentMenuId]) REFERENCES [tblMenu] ([MenuId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [tblPost] (
    [PostId] int NOT NULL IDENTITY,
    [Title] nvarchar(300) NOT NULL,
    [Slug] nvarchar(350) NOT NULL,
    [Summary] nvarchar(500) NULL,
    [Content] nvarchar(max) NULL,
    [ThumbnailImage] nvarchar(300) NULL,
    [Category] nvarchar(100) NULL,
    [IsPinned] bit NOT NULL,
    [IsPublished] bit NOT NULL,
    [PublishedAt] datetime2 NULL,
    [ViewCount] int NOT NULL,
    [MetaTitle] nvarchar(300) NULL,
    [MetaDescription] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_tblPost] PRIMARY KEY ([PostId])
);
GO

CREATE TABLE [tblProvince] (
    [ProvinceId] int NOT NULL IDENTITY,
    [Code] nvarchar(20) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    CONSTRAINT [PK_tblProvince] PRIMARY KEY ([ProvinceId])
);
GO

CREATE TABLE [tblSkill] (
    [SkillId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_tblSkill] PRIMARY KEY ([SkillId])
);
GO

CREATE TABLE [tblUser] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(50) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [PasswordHash] nvarchar(256) NOT NULL,
    [FullName] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [Avatar] nvarchar(255) NULL,
    [Role] nvarchar(20) NOT NULL,
    [IsActive] bit NOT NULL,
    [FailedLoginCount] int NOT NULL,
    [LockoutEnd] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_tblUser] PRIMARY KEY ([UserId])
);
GO

CREATE TABLE [tblDistrict] (
    [DistrictId] int NOT NULL IDENTITY,
    [ProvinceId] int NOT NULL,
    [Code] nvarchar(20) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    CONSTRAINT [PK_tblDistrict] PRIMARY KEY ([DistrictId]),
    CONSTRAINT [FK_tblDistrict_tblProvince_ProvinceId] FOREIGN KEY ([ProvinceId]) REFERENCES [tblProvince] ([ProvinceId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [tblAuditLog] (
    [LogId] int NOT NULL IDENTITY,
    [UserId] int NULL,
    [Action] nvarchar(100) NOT NULL,
    [EntityName] nvarchar(100) NOT NULL,
    [EntityId] nvarchar(50) NULL,
    [Details] nvarchar(2000) NULL,
    [IpAddress] nvarchar(50) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_tblAuditLog] PRIMARY KEY ([LogId]),
    CONSTRAINT [FK_tblAuditLog_tblUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [tblUser] ([UserId]) ON DELETE SET NULL
);
GO

CREATE TABLE [tblCandidateProfile] (
    [ProfileId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Headline] nvarchar(200) NULL,
    [DesiredPosition] nvarchar(200) NULL,
    [ExpectedSalary] decimal(18,2) NULL,
    [ProvinceId] int NULL,
    [Summary] nvarchar(max) NULL,
    [DefaultCvPath] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_tblCandidateProfile] PRIMARY KEY ([ProfileId]),
    CONSTRAINT [FK_tblCandidateProfile_tblProvince_ProvinceId] FOREIGN KEY ([ProvinceId]) REFERENCES [tblProvince] ([ProvinceId]) ON DELETE SET NULL,
    CONSTRAINT [FK_tblCandidateProfile_tblUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [tblUser] ([UserId]) ON DELETE CASCADE
);
GO

CREATE TABLE [tblCompany] (
    [CompanyId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Slug] nvarchar(220) NOT NULL,
    [Logo] nvarchar(255) NULL,
    [Website] nvarchar(255) NULL,
    [TaxCode] nvarchar(50) NULL,
    [CompanySize] nvarchar(50) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Phone] nvarchar(20) NULL,
    [Email] nvarchar(200) NULL,
    [Address] nvarchar(300) NULL,
    [Status] nvarchar(30) NOT NULL,
    [RejectReason] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_tblCompany] PRIMARY KEY ([CompanyId]),
    CONSTRAINT [FK_tblCompany_tblUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [tblUser] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [tblJobAlert] (
    [AlertId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Keyword] nvarchar(200) NULL,
    [ProvinceId] int NULL,
    [CategoryId] int NULL,
    [MinSalary] decimal(18,2) NULL,
    [Frequency] nvarchar(20) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_tblJobAlert] PRIMARY KEY ([AlertId]),
    CONSTRAINT [FK_tblJobAlert_tblJobCategory_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [tblJobCategory] ([CategoryId]) ON DELETE SET NULL,
    CONSTRAINT [FK_tblJobAlert_tblProvince_ProvinceId] FOREIGN KEY ([ProvinceId]) REFERENCES [tblProvince] ([ProvinceId]) ON DELETE SET NULL,
    CONSTRAINT [FK_tblJobAlert_tblUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [tblUser] ([UserId]) ON DELETE CASCADE
);
GO

CREATE TABLE [tblWard] (
    [WardId] int NOT NULL IDENTITY,
    [DistrictId] int NOT NULL,
    [Code] nvarchar(20) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [Slug] nvarchar(120) NOT NULL,
    CONSTRAINT [PK_tblWard] PRIMARY KEY ([WardId]),
    CONSTRAINT [FK_tblWard_tblDistrict_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [tblDistrict] ([DistrictId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [tblWorkplace] (
    [WorkplaceId] int NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Address] nvarchar(300) NOT NULL,
    [ProvinceId] int NOT NULL,
    [DistrictId] int NOT NULL,
    [WardId] int NULL,
    [Latitude] float NULL,
    [Longitude] float NULL,
    [IsHeadOffice] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_tblWorkplace] PRIMARY KEY ([WorkplaceId]),
    CONSTRAINT [FK_tblWorkplace_tblCompany_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [tblCompany] ([CompanyId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblWorkplace_tblDistrict_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [tblDistrict] ([DistrictId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblWorkplace_tblProvince_ProvinceId] FOREIGN KEY ([ProvinceId]) REFERENCES [tblProvince] ([ProvinceId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblWorkplace_tblWard_WardId] FOREIGN KEY ([WardId]) REFERENCES [tblWard] ([WardId]) ON DELETE SET NULL
);
GO

CREATE TABLE [tblJob] (
    [JobId] int NOT NULL IDENTITY,
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
    [SalaryUnit] nvarchar(10) NOT NULL,
    [IsSalaryNegotiable] bit NOT NULL,
    [ExperienceLevel] nvarchar(50) NOT NULL,
    [EducationLevel] nvarchar(50) NOT NULL,
    [WorkMode] nvarchar(30) NOT NULL,
    [Headcount] int NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [ApprovalStatus] nvarchar(30) NOT NULL,
    [IsFeatured] bit NOT NULL,
    [ViewCount] int NOT NULL,
    [PublishedAt] datetime2 NULL,
    [RejectReason] nvarchar(500) NULL,
    [SearchText] nvarchar(2000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_tblJob] PRIMARY KEY ([JobId]),
    CONSTRAINT [FK_tblJob_tblCompany_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [tblCompany] ([CompanyId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblJob_tblJobCategory_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [tblJobCategory] ([CategoryId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblJob_tblJobType_JobTypeId] FOREIGN KEY ([JobTypeId]) REFERENCES [tblJobType] ([JobTypeId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblJob_tblWorkplace_WorkplaceId] FOREIGN KEY ([WorkplaceId]) REFERENCES [tblWorkplace] ([WorkplaceId]) ON DELETE SET NULL
);
GO

CREATE TABLE [tblApplication] (
    [ApplicationId] int NOT NULL IDENTITY,
    [JobId] int NOT NULL,
    [CandidateUserId] int NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [CoverLetter] nvarchar(2000) NULL,
    [CvPath] nvarchar(255) NOT NULL,
    [Status] nvarchar(30) NOT NULL,
    [EmployerNote] nvarchar(1000) NULL,
    [AppliedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_tblApplication] PRIMARY KEY ([ApplicationId]),
    CONSTRAINT [FK_tblApplication_tblJob_JobId] FOREIGN KEY ([JobId]) REFERENCES [tblJob] ([JobId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblApplication_tblUser_CandidateUserId] FOREIGN KEY ([CandidateUserId]) REFERENCES [tblUser] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [tblJobBenefit] (
    [JobId] int NOT NULL,
    [BenefitId] int NOT NULL,
    CONSTRAINT [PK_tblJobBenefit] PRIMARY KEY ([JobId], [BenefitId]),
    CONSTRAINT [FK_tblJobBenefit_tblBenefit_BenefitId] FOREIGN KEY ([BenefitId]) REFERENCES [tblBenefit] ([BenefitId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_tblJobBenefit_tblJob_JobId] FOREIGN KEY ([JobId]) REFERENCES [tblJob] ([JobId]) ON DELETE CASCADE
);
GO

CREATE TABLE [tblJobSkill] (
    [JobId] int NOT NULL,
    [SkillId] int NOT NULL,
    CONSTRAINT [PK_tblJobSkill] PRIMARY KEY ([JobId], [SkillId]),
    CONSTRAINT [FK_tblJobSkill_tblJob_JobId] FOREIGN KEY ([JobId]) REFERENCES [tblJob] ([JobId]) ON DELETE CASCADE,
    CONSTRAINT [FK_tblJobSkill_tblSkill_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [tblSkill] ([SkillId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [tblSavedJob] (
    [SavedJobId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [JobId] int NOT NULL,
    [SavedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_tblSavedJob] PRIMARY KEY ([SavedJobId]),
    CONSTRAINT [FK_tblSavedJob_tblJob_JobId] FOREIGN KEY ([JobId]) REFERENCES [tblJob] ([JobId]) ON DELETE CASCADE,
    CONSTRAINT [FK_tblSavedJob_tblUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [tblUser] ([UserId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_tblApplication_CandidateUserId] ON [tblApplication] ([CandidateUserId]);
GO

CREATE INDEX [IX_tblApplication_JobId] ON [tblApplication] ([JobId]);
GO

CREATE UNIQUE INDEX [IX_tblApplication_JobId_CandidateUserId] ON [tblApplication] ([JobId], [CandidateUserId]);
GO

CREATE INDEX [IX_tblAuditLog_CreatedAt] ON [tblAuditLog] ([CreatedAt]);
GO

CREATE INDEX [IX_tblAuditLog_EntityName] ON [tblAuditLog] ([EntityName]);
GO

CREATE INDEX [IX_tblAuditLog_UserId] ON [tblAuditLog] ([UserId]);
GO

CREATE INDEX [IX_tblBenefit_Name] ON [tblBenefit] ([Name]);
GO

CREATE INDEX [IX_tblCandidateProfile_ProvinceId] ON [tblCandidateProfile] ([ProvinceId]);
GO

CREATE UNIQUE INDEX [IX_tblCandidateProfile_UserId] ON [tblCandidateProfile] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_tblCompany_Slug] ON [tblCompany] ([Slug]);
GO

CREATE INDEX [IX_tblCompany_Status] ON [tblCompany] ([Status]);
GO

CREATE UNIQUE INDEX [IX_tblCompany_UserId] ON [tblCompany] ([UserId]);
GO

CREATE INDEX [IX_tblDistrict_Code] ON [tblDistrict] ([Code]);
GO

CREATE INDEX [IX_tblDistrict_ProvinceId] ON [tblDistrict] ([ProvinceId]);
GO

CREATE INDEX [IX_tblDistrict_Slug] ON [tblDistrict] ([Slug]);
GO

CREATE INDEX [IX_tblJob_ApprovalStatus_ExpiresAt] ON [tblJob] ([ApprovalStatus], [ExpiresAt]);
GO

CREATE INDEX [IX_tblJob_CategoryId] ON [tblJob] ([CategoryId]);
GO

CREATE INDEX [IX_tblJob_CompanyId] ON [tblJob] ([CompanyId]);
GO

CREATE INDEX [IX_tblJob_JobTypeId] ON [tblJob] ([JobTypeId]);
GO

CREATE INDEX [IX_tblJob_SalaryMin_SalaryMax] ON [tblJob] ([SalaryMin], [SalaryMax]);
GO

CREATE INDEX [IX_tblJob_Slug] ON [tblJob] ([Slug]);
GO

CREATE INDEX [IX_tblJob_WorkplaceId] ON [tblJob] ([WorkplaceId]);
GO

CREATE INDEX [IX_tblJobAlert_CategoryId] ON [tblJobAlert] ([CategoryId]);
GO

CREATE INDEX [IX_tblJobAlert_ProvinceId] ON [tblJobAlert] ([ProvinceId]);
GO

CREATE INDEX [IX_tblJobAlert_UserId] ON [tblJobAlert] ([UserId]);
GO

CREATE INDEX [IX_tblJobBenefit_BenefitId] ON [tblJobBenefit] ([BenefitId]);
GO

CREATE UNIQUE INDEX [IX_tblJobCategory_Slug] ON [tblJobCategory] ([Slug]);
GO

CREATE INDEX [IX_tblJobSkill_SkillId] ON [tblJobSkill] ([SkillId]);
GO

CREATE UNIQUE INDEX [IX_tblJobType_Code] ON [tblJobType] ([Code]);
GO

CREATE INDEX [IX_tblMenu_ParentMenuId] ON [tblMenu] ([ParentMenuId]);
GO

CREATE INDEX [IX_tblPost_IsPublished] ON [tblPost] ([IsPublished]);
GO

CREATE UNIQUE INDEX [IX_tblPost_Slug] ON [tblPost] ([Slug]);
GO

CREATE UNIQUE INDEX [IX_tblProvince_Code] ON [tblProvince] ([Code]);
GO

CREATE INDEX [IX_tblProvince_Slug] ON [tblProvince] ([Slug]);
GO

CREATE INDEX [IX_tblSavedJob_JobId] ON [tblSavedJob] ([JobId]);
GO

CREATE UNIQUE INDEX [IX_tblSavedJob_UserId_JobId] ON [tblSavedJob] ([UserId], [JobId]);
GO

CREATE UNIQUE INDEX [IX_tblSkill_Slug] ON [tblSkill] ([Slug]);
GO

CREATE UNIQUE INDEX [IX_tblUser_Email] ON [tblUser] ([Email]);
GO

CREATE INDEX [IX_tblUser_Role] ON [tblUser] ([Role]);
GO

CREATE UNIQUE INDEX [IX_tblUser_Username] ON [tblUser] ([Username]);
GO

CREATE INDEX [IX_tblWard_Code] ON [tblWard] ([Code]);
GO

CREATE INDEX [IX_tblWard_DistrictId] ON [tblWard] ([DistrictId]);
GO

CREATE INDEX [IX_tblWard_Slug] ON [tblWard] ([Slug]);
GO

CREATE INDEX [IX_tblWorkplace_CompanyId] ON [tblWorkplace] ([CompanyId]);
GO

CREATE INDEX [IX_tblWorkplace_DistrictId] ON [tblWorkplace] ([DistrictId]);
GO

CREATE INDEX [IX_tblWorkplace_Latitude_Longitude] ON [tblWorkplace] ([Latitude], [Longitude]);
GO

CREATE INDEX [IX_tblWorkplace_ProvinceId_DistrictId] ON [tblWorkplace] ([ProvinceId], [DistrictId]);
GO

CREATE INDEX [IX_tblWorkplace_WardId] ON [tblWorkplace] ([WardId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261013070331_InitialCreate', N'8.0.10');
GO

COMMIT;
GO



-- =========================================================================
-- DỮ LIỆU KHỞI TẠO (SEED DATA) CHO CÁC BẢNG
-- =========================================================================
-- Nạp dữ liệu bảng tblJobCategory (16 bản ghi)
SET IDENTITY_INSERT [tblJobCategory] ON;
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (1, N'Công nghệ thông tin / Phần mềm', N'cong-nghe-thong-tin-phan-mem', N'bi-laptop', N'Lập trình viên, kỹ sư phần mềm, DevOps, QA/QC, AI/Data', 1, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (2, N'Marketing / Truyền thông / Quảng cáo', N'marketing-truyen-thong-quang-cao', N'bi-megaphone', N'Digital Marketing, SEO, Content Creator, PR, Event', 2, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (3, N'Kế toán / Kiểm toán / Thuế', N'ke-toan-kiem-toan-thue', N'bi-calculator', N'Kế toán tổng hợp, kế toán thuế, kiểm toán viên, tài chính', 3, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (4, N'Bán hàng / Kinh doanh / Phát triển thị trường', N'kinh-doanh-ban-hang', N'bi-graph-up-arrow', N'Sales B2B, B2C, Quản lý kinh doanh, Account Manager', 4, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (5, N'Hành chính / Nhân sự / Tuyển dụng', N'hanh-chinh-nhan-su', N'bi-people', N'Tuyển dụng, C&B, đào tạo nội bộ, hành chính văn phòng', 5, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (6, N'Tài chính / Ngân hàng / Chứng khoán', N'tai-chinh-ngan-hang', N'bi-bank', N'Chuyên viên tín dụng, thẩm định giá, môi giới chứng khoán', 6, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (7, N'Xây dựng / Kiến trúc / Quy hoạch', N'xay-dung-kien-truc', N'bi-building', N'Kỹ sư xây dựng, giám sát công trình, thiết kế kiến trúc', 7, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (8, N'Bất động sản / Quản lý dự án', N'bat-dong-san', N'bi-houses', N'Môi giới BĐS, tư vấn đầu tư, quản lý tòa nhà', 8, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (9, N'Thiết kế đồ họa / Sáng tạo nghệ thuật', N'thiet-ke-do-hoa', N'bi-palette', N'Graphic Designer, UI/UX Designer, Motion Graphic, 3D', 9, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (10, N'Giáo dục / Đào tạo / Giảng dạy', N'giao-duc-dao-tao', N'bi-mortarboard', N'Giáo viên tiếng Anh, giảng viên, gia sư, cố vấn học tập', 10, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (11, N'Y tế / Dược phẩm / Chăm sóc sức khỏe', N'y-te-duoc-pham', N'bi-heart-pulse', N'Bác sĩ, dược sĩ, điều dưỡng, trình dược viên', 11, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (12, N'Logistics / Xuất nhập khẩu / Chuỗi cung ứng', N'logistics-xuat-nhap-khau', N'bi-truck', N'Nhân viên chứng từ, giao nhận hàng hải, quản lý kho bãi', 12, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (13, N'Khách sạn / Du lịch / Nhà hàng (F&B)', N'khach-san-du-lich-nha-hang', N'bi-cup-hot', N'Quản lý nhà hàng, đầu bếp, lễ tân, hướng dẫn viên', 13, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (14, N'Sản xuất / Vận hành / Kỹ thuật cơ khí', N'san-xuat-van-hanh-co-khi', N'bi-gear', N'Kỹ sư cơ khí, điện - điện tử, quản lý chất lượng QA', 14, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (15, N'Dịch vụ khách hàng / Chăm sóc khách hàng', N'cham-soc-khach-hang', N'bi-headset', N'Tư vấn viên, hỗ trợ khách hàng, tổng đài viên', 15, 1);
INSERT INTO [tblJobCategory] ([CategoryId], [Name], [Slug], [Icon], [Description], [SortOrder], [IsActive]) VALUES (16, N'Luật / Pháp lý / Tuân thủ', N'luat-phap-ly', N'bi-shield-check', N'Chuyên viên pháp chế doanh nghiệp, luật sư tư vấn', 16, 1);
SET IDENTITY_INSERT [tblJobCategory] OFF;
GO

-- Nạp dữ liệu bảng tblJobType (5 bản ghi)
SET IDENTITY_INSERT [tblJobType] ON;
INSERT INTO [tblJobType] ([JobTypeId], [Name], [Code], [Description], [SortOrder], [IsActive]) VALUES (1, N'Toàn thời gian', N'FullTime', N'Làm việc đủ thời gian theo tiêu chuẩn doanh nghiệp (40-48h/tuần)', 1, 1);
INSERT INTO [tblJobType] ([JobTypeId], [Name], [Code], [Description], [SortOrder], [IsActive]) VALUES (2, N'Bán thời gian', N'PartTime', N'Làm việc theo ca hoặc theo giờ linh hoạt', 2, 1);
INSERT INTO [tblJobType] ([JobTypeId], [Name], [Code], [Description], [SortOrder], [IsActive]) VALUES (3, N'Thực tập', N'Internship', N'Dành cho sinh viên thực tập tích lũy kinh nghiệm', 3, 1);
INSERT INTO [tblJobType] ([JobTypeId], [Name], [Code], [Description], [SortOrder], [IsActive]) VALUES (4, N'Hợp đồng', N'Contract', N'Làm việc theo hợp đồng dự án ngắn hạn hoặc thời vụ', 4, 1);
INSERT INTO [tblJobType] ([JobTypeId], [Name], [Code], [Description], [SortOrder], [IsActive]) VALUES (5, N'Freelance', N'Freelance', N'Làm việc tự do theo gói công việc / bàn giao sản phẩm', 5, 1);
SET IDENTITY_INSERT [tblJobType] OFF;
GO

-- Nạp dữ liệu bảng tblSkill (42 bản ghi)
SET IDENTITY_INSERT [tblSkill] ON;
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (1, N'C#', N'csharp', N'Ngôn ngữ lập trình C# hướng đối tượng', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (2, N'.NET Core', N'dotnet-core', N'Nền tảng .NET Core mã nguồn mở hiệu năng cao', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (3, N'ASP.NET MVC', N'aspnet-mvc', N'Framework phát triển web MVC trên nền tảng .NET', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (4, N'Java', N'java', N'Ngôn ngữ lập trình Java hướng đối tượng', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (5, N'Spring Boot', N'spring-boot', N'Framework phát triển backend Java', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (6, N'Python', N'python', N'Ngôn ngữ lập trình đa dụng Python', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (7, N'Django', N'django', N'Web framework cấp cao cho Python', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (8, N'FastAPI', N'fastapi', N'Framework API Python tốc độ cao', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (9, N'JavaScript', N'javascript', N'Ngôn ngữ kịch bản web phổ biến', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (10, N'TypeScript', N'typescript', N'Siêu tập hợp kiểu tĩnh của JavaScript', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (11, N'React', N'react', N'Thư viện giao diện người dùng front-end của Meta', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (12, N'Angular', N'angular', N'Framework phát triển web SPA của Google', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (13, N'Vue.js', N'vue-js', N'Framework JavaScript tiến bộ xây dựng UI', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (14, N'Node.js', N'node-js', N'Môi trường thực thi JavaScript phía máy chủ', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (15, N'PHP', N'php', N'Ngôn ngữ kịch bản web mã nguồn mở', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (16, N'Laravel', N'laravel', N'Framework PHP hàng đầu', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (17, N'Golang', N'golang', N'Ngôn ngữ Go đồng thời cao của Google', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (18, N'Docker', N'docker', N'Nền tảng đóng gói ứng dụng trong container', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (19, N'Kubernetes', N'kubernetes', N'Hệ thống điều phối container K8s', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (20, N'AWS', N'aws', N'Amazon Web Services điện toán đám mây', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (21, N'Azure', N'azure', N'Microsoft Azure Cloud Platform', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (22, N'GCP', N'gcp', N'Google Cloud Platform', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (23, N'SQL Server', N'sql-server', N'Hệ quản trị CSDL quan hệ của Microsoft', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (24, N'PostgreSQL', N'postgresql', N'Hệ quản trị CSDL quan hệ mã nguồn mở mạnh mẽ', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (25, N'MySQL', N'mysql', N'Hệ quản trị CSDL quan hệ phổ biến', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (26, N'MongoDB', N'mongodb', N'CSDL NoSQL dạng tài liệu', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (27, N'Redis', N'redis', N'Kho lưu trữ dữ liệu in-memory tốc độ cực nhanh', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (28, N'Git', N'git', N'Hệ thống quản lý phiên bản phân tán', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (29, N'CI/CD', N'ci-cd', N'Tích hợp và phân phối liên tục DevOps', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (30, N'Microservices', N'microservices', N'Kiến trúc dịch vụ vi mô', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (31, N'HTML/CSS', N'html-css', N'Ngôn ngữ đánh dấu và tạo kiểu trang web', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (32, N'Tailwind CSS', N'tailwind-css', N'Framework CSS tiện ích', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (33, N'UI/UX Design', N'ui-ux-design', N'Thiết kế trải nghiệm và giao diện người dùng', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (34, N'Figma', N'figma', N'Công cụ thiết kế UI/UX cộng tác trực tuyến', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (35, N'Linux', N'linux', N'Hệ điều hành máy chủ Linux', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (36, N'Agile/Scrum', N'agile-scrum', N'Mô hình quản lý dự án linh hoạt', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (37, N'Data Analysis', N'data-analysis', N'Phân tích và trực quan hóa dữ liệu kinh doanh', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (38, N'Machine Learning', N'machine-learning', N'Học máy và mô hình trí tuệ nhân tạo', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (39, N'SEO', N'seo', N'Tối ưu hóa công cụ tìm kiếm', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (40, N'Content Marketing', N'content-marketing', N'Sáng tạo nội dung tiếp thị giá trị', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (41, N'Sales B2B', N'sales-b2b', N'Kỹ năng bán hàng doanh nghiệp', 1);
INSERT INTO [tblSkill] ([SkillId], [Name], [Slug], [Description], [IsActive]) VALUES (42, N'Tiếng Anh giao tiếp', N'tieng-anh-giao-tiep', N'Năng lực giao tiếp tiếng Anh lưu loát trong công việc', 1);
SET IDENTITY_INSERT [tblSkill] OFF;
GO

-- Nạp dữ liệu bảng tblBenefit (11 bản ghi)
SET IDENTITY_INSERT [tblBenefit] ON;
INSERT INTO [tblBenefit] ([BenefitId], [Name], [Icon], [Description], [IsActive]) VALUES (1, N'Bảo hiểm sức khỏe cao cấp', N'bi-shield-plus', N'Gói bảo hiểm sức khỏe Bảo Việt / PVI toàn diện cho nhân viên và người thân', 1);
INSERT INTO [tblBenefit] ([BenefitId], [Name], [Icon], [Description], [IsActive]) VALUES (2, N'Thưởng tháng 13 & thưởng hiệu suất KPI', N'bi-cash-coin', N'Thưởng tháng 13 cố định và thưởng kinh doanh theo hiệu quả quý/năm', 1);
INSERT INTO [tblBenefit] ([BenefitId], [Name], [Icon], [Description], [IsActive]) VALUES (3, N'Du lịch / Teambuilding hàng năm', N'bi-airplane', N'Chuyến du lịch nghỉ dưỡng cao cấp hàng năm trong và ngoài nước', 1);
INSERT INTO [tblBenefit] ([BenefitId], [Name], [Icon], [Description], [IsActive]) VALUES (4, N'Cung cấp laptop cao cấp', N'bi-laptop', N'Trang bị MacBook Pro hoặc Dell XPS đời mới cùng màn hình phụ 27 inch', 1);
INSERT INTO [tblBenefit] ([BenefitId], [Name], [Icon], [Description], [IsActive]) VALUES (5, N'Làm việc linh hoạt & Làm việc từ xa (WFH)', N'bi-house-check', N'Hỗ trợ làm việc kết hợp linh hoạt 2 ngày WFH/tuần', 1);
INSERT INTO [tblBenefit] ([BenefitId], [Name], [Icon], [Description], [IsActive]) VALUES (6, N'Phụ cấp ăn trưa, gửi xe và điện thoại', N'bi-wallet2', N'Trợ cấp ăn trưa 1.500.000 VNĐ/tháng và miễn phí gửi xe tại tòa nhà', 1);
INSERT INTO [tblBenefit] ([BenefitId], [Name], [Icon], [Description], [IsActive]) VALUES (7, N'Khám sức khỏe định kỳ hàng năm', N'bi-heart-pulse', N'Khám sức khỏe tổng quát định kỳ tại các bệnh viện quốc tế', 1);
INSERT INTO [tblBenefit] ([BenefitId], [Name], [Icon], [Description], [IsActive]) VALUES (8, N'Đào tạo nội bộ & tài trợ chứng chỉ quốc tế', N'bi-mortarboard', N'Tài trợ 100% chi phí thi các chứng chỉ chuyên ngành quốc tế (AWS, Azure, PMP...)', 1);
INSERT INTO [tblBenefit] ([BenefitId], [Name], [Icon], [Description], [IsActive]) VALUES (9, N'Nghỉ phép 15 ngày/năm', N'bi-calendar2-check', N'Số ngày nghỉ phép hàng năm từ 15 ngày trở lên có hưởng lương', 1);
INSERT INTO [tblBenefit] ([BenefitId], [Name], [Icon], [Description], [IsActive]) VALUES (10, N'Phòng gym, trà cà phê & đồ ăn nhẹ miễn phí', N'bi-cup-hot', N'Pantry đầy đủ trà, cafe chất lượng cao, trái cây và snack ăn vặt', 1);
INSERT INTO [tblBenefit] ([BenefitId], [Name], [Icon], [Description], [IsActive]) VALUES (11, N'Đánh giá tăng lương 2 lần/năm', N'bi-graph-up', N'Kỳ xem xét đánh giá hiệu suất và điều chỉnh lương 6 tháng một lần', 1);
SET IDENTITY_INSERT [tblBenefit] OFF;
GO

-- Nạp dữ liệu bảng tblProvince (63 bản ghi)
SET IDENTITY_INSERT [tblProvince] ON;
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (1, N'01', N'Hà Nội', N'Thành phố Hà Nội', N'ha-noi');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (2, N'79', N'Hồ Chí Minh', N'Thành phố Hồ Chí Minh', N'ho-chi-minh');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (3, N'48', N'Đà Nẵng', N'Thành phố Đà Nẵng', N'da-nang');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (4, N'31', N'Hải Phòng', N'Thành phố Hải Phòng', N'hai-phong');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (5, N'92', N'Cần Thơ', N'Thành phố Cần Thơ', N'can-tho');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (6, N'40', N'Nghệ An', N'Tỉnh Nghệ An', N'nghe-an');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (7, N'02', N'Hà Giang', N'Tỉnh Hà Giang', N'ha-giang');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (8, N'04', N'Cao Bằng', N'Tỉnh Cao Bằng', N'cao-bang');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (9, N'06', N'Bắc Kạn', N'Tỉnh Bắc Kạn', N'bac-kan');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (10, N'08', N'Tuyên Quang', N'Tỉnh Tuyên Quang', N'tuyen-quang');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (11, N'10', N'Lào Cai', N'Tỉnh Lào Cai', N'lao-cai');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (12, N'11', N'Điện Biên', N'Tỉnh Điện Biên', N'dien-bien');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (13, N'12', N'Lai Châu', N'Tỉnh Lai Châu', N'lai-chau');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (14, N'14', N'Sơn La', N'Tỉnh Sơn La', N'son-la');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (15, N'15', N'Yên Bái', N'Tỉnh Yên Bái', N'yen-bai');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (16, N'17', N'Hòa Bình', N'Tỉnh Hòa Bình', N'hoa-binh');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (17, N'19', N'Thái Nguyên', N'Tỉnh Thái Nguyên', N'thai-nguyen');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (18, N'20', N'Lạng Sơn', N'Tỉnh Lạng Sơn', N'lang-son');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (19, N'22', N'Quảng Ninh', N'Tỉnh Quảng Ninh', N'quang-ninh');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (20, N'24', N'Bắc Giang', N'Tỉnh Bắc Giang', N'bac-giang');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (21, N'25', N'Phú Thọ', N'Tỉnh Phú Thọ', N'phu-tho');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (22, N'26', N'Vĩnh Phúc', N'Tỉnh Vĩnh Phúc', N'vinh-phuc');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (23, N'27', N'Bắc Ninh', N'Tỉnh Bắc Ninh', N'bac-ninh');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (24, N'30', N'Hải Dương', N'Tỉnh Hải Dương', N'hai-duong');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (25, N'33', N'Hưng Yên', N'Tỉnh Hưng Yên', N'hung-yen');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (26, N'34', N'Thái Bình', N'Tỉnh Thái Bình', N'thai-binh');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (27, N'35', N'Hà Nam', N'Tỉnh Hà Nam', N'ha-nam');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (28, N'36', N'Nam Định', N'Tỉnh Nam Định', N'nam-dinh');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (29, N'37', N'Ninh Bình', N'Tỉnh Ninh Bình', N'ninh-binh');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (30, N'38', N'Thanh Hóa', N'Tỉnh Thanh Hóa', N'thanh-hoa');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (31, N'42', N'Hà Tĩnh', N'Tỉnh Hà Tĩnh', N'ha-tinh');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (32, N'44', N'Quảng Bình', N'Tỉnh Quảng Bình', N'quang-binh');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (33, N'45', N'Quảng Trị', N'Tỉnh Quảng Trị', N'quang-tri');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (34, N'46', N'Thừa Thiên Huế', N'Tỉnh Thừa Thiên Huế', N'thua-thien-hue');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (35, N'49', N'Quảng Nam', N'Tỉnh Quảng Nam', N'quang-nam');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (36, N'51', N'Quảng Ngãi', N'Tỉnh Quảng Ngãi', N'quang-ngai');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (37, N'52', N'Bình Định', N'Tỉnh Bình Định', N'binh-dinh');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (38, N'54', N'Phú Yên', N'Tỉnh Phú Yên', N'phu-yen');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (39, N'56', N'Khánh Hòa', N'Tỉnh Khánh Hòa', N'khanh-hoa');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (40, N'58', N'Ninh Thuận', N'Tỉnh Ninh Thuận', N'ninh-thuan');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (41, N'60', N'Bình Thuận', N'Tỉnh Bình Thuận', N'binh-thuan');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (42, N'62', N'Kon Tum', N'Tỉnh Kon Tum', N'kon-tum');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (43, N'64', N'Gia Lai', N'Tỉnh Gia Lai', N'gia-lai');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (44, N'66', N'Đắk Lắk', N'Tỉnh Đắk Lắk', N'dak-lak');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (45, N'67', N'Đắk Nông', N'Tỉnh Đắk Nông', N'dak-nong');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (46, N'68', N'Lâm Đồng', N'Tỉnh Lâm Đồng', N'lam-dong');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (47, N'70', N'Bình Phước', N'Tỉnh Bình Phước', N'binh-phuoc');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (48, N'72', N'Tây Ninh', N'Tỉnh Tây Ninh', N'tay-ninh');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (49, N'74', N'Bình Dương', N'Tỉnh Bình Dương', N'binh-duong');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (50, N'75', N'Đồng Nai', N'Tỉnh Đồng Nai', N'dong-nai');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (51, N'77', N'Bà Rịa - Vũng Tàu', N'Tỉnh Bà Rịa - Vũng Tàu', N'ba-ria-vung-tau');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (52, N'80', N'Long An', N'Tỉnh Long An', N'long-an');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (53, N'82', N'Tiền Giang', N'Tỉnh Tiền Giang', N'tien-giang');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (54, N'83', N'Bến Tre', N'Tỉnh Bến Tre', N'ben-tre');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (55, N'84', N'Trà Vinh', N'Tỉnh Trà Vinh', N'tra-vinh');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (56, N'86', N'Vĩnh Long', N'Tỉnh Vĩnh Long', N'vinh-long');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (57, N'87', N'Đồng Tháp', N'Tỉnh Đồng Tháp', N'dong-thap');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (58, N'88', N'An Giang', N'Tỉnh An Giang', N'an-giang');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (59, N'89', N'Kiên Giang', N'Tỉnh Kiên Giang', N'kien-giang');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (60, N'91', N'Hậu Giang', N'Tỉnh Hậu Giang', N'hau-giang');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (61, N'93', N'Sóc Trăng', N'Tỉnh Sóc Trăng', N'soc-trang');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (62, N'94', N'Bạc Liêu', N'Tỉnh Bạc Liêu', N'bac-lieu');
INSERT INTO [tblProvince] ([ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (63, N'95', N'Cà Mau', N'Tỉnh Cà Mau', N'ca-mau');
SET IDENTITY_INSERT [tblProvince] OFF;
GO

-- Nạp dữ liệu bảng tblDistrict (126 bản ghi)
SET IDENTITY_INSERT [tblDistrict] ON;
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (1, 1, N'001', N'Ba Đình', N'Quận Ba Đình', N'ba-dinh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (2, 1, N'002', N'Hoàn Kiếm', N'Quận Hoàn Kiếm', N'hoan-kiem');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (3, 1, N'003', N'Tây Hồ', N'Quận Tây Hồ', N'tay-ho');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (4, 1, N'004', N'Long Biên', N'Quận Long Biên', N'long-bien');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (5, 1, N'005', N'Cầu Giấy', N'Quận Cầu Giấy', N'cau-giay');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (6, 1, N'006', N'Đống Đa', N'Quận Đống Đa', N'dong-da');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (7, 1, N'007', N'Hai Bà Trưng', N'Quận Hai Bà Trưng', N'hai-ba-trung');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (8, 1, N'008', N'Hoàng Mai', N'Quận Hoàng Mai', N'hoang-mai');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (9, 1, N'009', N'Thanh Xuân', N'Quận Thanh Xuân', N'thanh-xuan');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (10, 1, N'016', N'Sóc Sơn', N'Huyện Sóc Sơn', N'soc-son');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (11, 1, N'017', N'Đông Anh', N'Huyện Đông Anh', N'dong-anh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (12, 1, N'018', N'Gia Lâm', N'Huyện Gia Lâm', N'gia-lam');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (13, 1, N'019', N'Nam Từ Liêm', N'Quận Nam Từ Liêm', N'nam-tu-liem');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (14, 1, N'020', N'Thanh Trì', N'Huyện Thanh Trì', N'thanh-tri');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (15, 1, N'021', N'Bắc Từ Liêm', N'Quận Bắc Từ Liêm', N'bac-tu-liem');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (16, 1, N'268', N'Hà Đông', N'Quận Hà Đông', N'ha-dong');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (17, 1, N'269', N'Sơn Tây', N'Thị xã Sơn Tây', N'son-tay');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (18, 2, N'760', N'Quận 1', N'Quận 1', N'quan-1');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (19, 2, N'761', N'Quận 12', N'Quận 12', N'quan-12');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (20, 2, N'764', N'Gò Vấp', N'Quận Gò Vấp', N'go-vap');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (21, 2, N'765', N'Bình Thạnh', N'Quận Bình Thạnh', N'binh-thanh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (22, 2, N'766', N'Tân Bình', N'Quận Tân Bình', N'tan-binh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (23, 2, N'767', N'Tân Phú', N'Quận Tân Phú', N'tan-phu');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (24, 2, N'768', N'Phú Nhuận', N'Quận Phú Nhuận', N'phu-nhuan');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (25, 2, N'769', N'Thủ Đức', N'Thành phố Thủ Đức', N'thu-duc');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (26, 2, N'770', N'Quận 3', N'Quận 3', N'quan-3');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (27, 2, N'771', N'Quận 10', N'Quận 10', N'quan-10');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (28, 2, N'772', N'Quận 11', N'Quận 11', N'quan-11');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (29, 2, N'773', N'Quận 4', N'Quận 4', N'quan-4');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (30, 2, N'774', N'Quận 5', N'Quận 5', N'quan-5');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (31, 2, N'775', N'Quận 6', N'Quận 6', N'quan-6');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (32, 2, N'776', N'Quận 8', N'Quận 8', N'quan-8');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (33, 2, N'777', N'Bình Tân', N'Quận Bình Tân', N'binh-tan');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (34, 2, N'778', N'Quận 7', N'Quận 7', N'quan-7');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (35, 2, N'783', N'Củ Chi', N'Huyện Củ Chi', N'cu-chi');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (36, 2, N'784', N'Hóc Môn', N'Huyện Hóc Môn', N'hoc-mon');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (37, 2, N'785', N'Bình Chánh', N'Huyện Bình Chánh', N'binh-chanh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (38, 2, N'786', N'Nhà Bè', N'Huyện Nhà Bè', N'nha-be');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (39, 2, N'787', N'Cần Giờ', N'Huyện Cần Giờ', N'can-gio');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (40, 3, N'490', N'Hải Châu', N'Quận Hải Châu', N'hai-chau');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (41, 3, N'491', N'Thanh Khê', N'Quận Thanh Khê', N'thanh-khe');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (42, 3, N'492', N'Sơn Trà', N'Quận Sơn Trà', N'son-tra');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (43, 3, N'493', N'Ngũ Hành Sơn', N'Quận Ngũ Hành Sơn', N'ngu-hanh-son');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (44, 3, N'494', N'Liên Chiểu', N'Quận Liên Chiểu', N'lien-chieu');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (45, 3, N'495', N'Cẩm Lệ', N'Quận Cẩm Lệ', N'cam-le');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (46, 3, N'497', N'Hòa Vang', N'Huyện Hòa Vang', N'hoa-vang');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (47, 4, N'303', N'Hồng Bàng', N'Quận Hồng Bàng', N'hong-bang');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (48, 4, N'304', N'Ngô Quyền', N'Quận Ngô Quyền', N'ngo-quyen');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (49, 4, N'305', N'Lê Chân', N'Quận Lê Chân', N'le-chan');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (50, 4, N'306', N'Hải An', N'Quận Hải An', N'hai-an');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (51, 4, N'307', N'Kiến An', N'Quận Kiến An', N'kien-an');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (52, 4, N'308', N'Đồ Sơn', N'Quận Đồ Sơn', N'do-son');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (53, 4, N'309', N'Dương Kinh', N'Quận Dương Kinh', N'duong-kinh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (54, 4, N'311', N'Thủy Nguyên', N'Huyện Thủy Nguyên', N'thuy-nguyen');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (55, 5, N'916', N'Ninh Kiều', N'Quận Ninh Kiều', N'ninh-kieu');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (56, 5, N'917', N'Ô Môn', N'Quận Ô Môn', N'o-mon');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (57, 5, N'918', N'Bình Thủy', N'Quận Bình Thủy', N'binh-thuy');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (58, 5, N'919', N'Cái Răng', N'Quận Cái Răng', N'cai-rang');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (59, 5, N'923', N'Thốt Nốt', N'Quận Thốt Nốt', N'thot-not');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (60, 6, N'412', N'Vinh', N'Thành phố Vinh', N'vinh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (61, 6, N'413', N'Cửa Lò', N'Thị xã Cửa Lò', N'cua-lo');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (62, 6, N'414', N'Thái Hòa', N'Thị xã Thái Hòa', N'thai-hoa');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (63, 6, N'417', N'Quỳnh Lưu', N'Huyện Quỳnh Lưu', N'quynh-luu');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (64, 6, N'418', N'Diễn Châu', N'Huyện Diễn Châu', N'dien-chau');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (65, 6, N'419', N'Nghi Lộc', N'Huyện Nghi Lộc', N'nghi-loc');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (66, 6, N'420', N'Hưng Nguyên', N'Huyện Hưng Nguyên', N'hung-nguyen');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (67, 6, N'421', N'Nam Đàn', N'Huyện Nam Đàn', N'nam-dan');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (68, 6, N'422', N'Thanh Chương', N'Huyện Thanh Chương', N'thanh-chuong');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (69, 6, N'423', N'Đô Lương', N'Huyện Đô Lương', N'do-luong');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (70, 7, N'024', N'Hà Giang', N'Thành phố Hà Giang', N'ha-giang');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (71, 8, N'040', N'Cao Bằng', N'Thành phố Cao Bằng', N'cao-bang');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (72, 9, N'058', N'Bắc Kạn', N'Thành phố Bắc Kạn', N'bac-kan');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (73, 10, N'070', N'Tuyên Quang', N'Thành phố Tuyên Quang', N'tuyen-quang');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (74, 11, N'080', N'Lào Cai', N'Thành phố Lào Cai', N'lao-cai');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (75, 12, N'094', N'Điện Biên Phủ', N'Thành phố Điện Biên Phủ', N'dien-bien-phu');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (76, 13, N'104', N'Lai Châu', N'Thành phố Lai Châu', N'lai-chau');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (77, 14, N'116', N'Sơn La', N'Thành phố Sơn La', N'son-la');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (78, 15, N'132', N'Yên Bái', N'Thành phố Yên Bái', N'yen-bai');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (79, 16, N'148', N'Hòa Bình', N'Thành phố Hòa Bình', N'hoa-binh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (80, 17, N'164', N'Thái Nguyên', N'Thành phố Thái Nguyên', N'thai-nguyen');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (81, 18, N'178', N'Lạng Sơn', N'Thành phố Lạng Sơn', N'lang-son');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (82, 19, N'193', N'Hạ Long', N'Thành phố Hạ Long', N'ha-long');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (83, 20, N'213', N'Bắc Giang', N'Thành phố Bắc Giang', N'bac-giang');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (84, 21, N'227', N'Việt Trì', N'Thành phố Việt Trì', N'viet-tri');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (85, 22, N'243', N'Vĩnh Yên', N'Thành phố Vĩnh Yên', N'vinh-yen');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (86, 23, N'256', N'Bắc Ninh', N'Thành phố Bắc Ninh', N'bac-ninh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (87, 24, N'288', N'Hải Dương', N'Thành phố Hải Dương', N'hai-duong');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (88, 25, N'323', N'Hưng Yên', N'Thành phố Hưng Yên', N'hung-yen');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (89, 26, N'336', N'Thái Bình', N'Thành phố Thái Bình', N'thai-binh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (90, 27, N'347', N'Phủ Lý', N'Thành phố Phủ Lý', N'phu-ly');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (91, 28, N'356', N'Nam Định', N'Thành phố Nam Định', N'nam-dinh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (92, 29, N'369', N'Ninh Bình', N'Thành phố Ninh Bình', N'ninh-binh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (93, 30, N'380', N'Thanh Hóa', N'Thành phố Thanh Hóa', N'thanh-hoa');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (94, 31, N'439', N'Hà Tĩnh', N'Thành phố Hà Tĩnh', N'ha-tinh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (95, 32, N'450', N'Đồng Hới', N'Thành phố Đồng Hới', N'dong-hoi');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (96, 33, N'461', N'Đông Hà', N'Thành phố Đông Hà', N'dong-ha');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (97, 34, N'474', N'Huế', N'Thành phố Huế', N'hue');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (98, 35, N'502', N'Tam Kỳ', N'Thành phố Tam Kỳ', N'tam-ky');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (99, 36, N'522', N'Quảng Ngãi', N'Thành phố Quảng Ngãi', N'quang-ngai');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (100, 37, N'540', N'Quy Nhơn', N'Thành phố Quy Nhơn', N'quy-nhon');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (101, 38, N'555', N'Tuy Hòa', N'Thành phố Tuy Hòa', N'tuy-hoa');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (102, 39, N'568', N'Nha Trang', N'Thành phố Nha Trang', N'nha-trang');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (103, 40, N'582', N'Phan Rang - Tháp Chàm', N'Thành phố Phan Rang - Tháp Chàm', N'phan-rang-thap-cham');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (104, 41, N'593', N'Phan Thiết', N'Thành phố Phan Thiết', N'phan-thiet');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (105, 42, N'608', N'Kon Tum', N'Thành phố Kon Tum', N'kon-tum');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (106, 43, N'622', N'Pleiku', N'Thành phố Pleiku', N'pleiku');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (107, 44, N'643', N'Buôn Ma Thuột', N'Thành phố Buôn Ma Thuột', N'buon-ma-thuot');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (108, 45, N'660', N'Gia Nghĩa', N'Thành phố Gia Nghĩa', N'gia-nghia');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (109, 46, N'672', N'Đà Lạt', N'Thành phố Đà Lạt', N'da-lat');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (110, 47, N'688', N'Đồng Xoài', N'Thành phố Đồng Xoài', N'dong-xoai');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (111, 48, N'703', N'Tây Ninh', N'Thành phố Tây Ninh', N'tay-ninh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (112, 49, N'718', N'Thủ Dầu Một', N'Thành phố Thủ Dầu Một', N'thu-dau-mot');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (113, 50, N'731', N'Biên Hòa', N'Thành phố Biên Hòa', N'bien-hoa');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (114, 51, N'747', N'Vũng Tàu', N'Thành phố Vũng Tàu', N'vung-tau');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (115, 52, N'794', N'Tân An', N'Thành phố Tân An', N'tan-an');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (116, 53, N'815', N'Mỹ Tho', N'Thành phố Mỹ Tho', N'my-tho');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (117, 54, N'829', N'Bến Tre', N'Thành phố Bến Tre', N'ben-tre');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (118, 55, N'842', N'Trà Vinh', N'Thành phố Trà Vinh', N'tra-vinh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (119, 56, N'855', N'Vĩnh Long', N'Thành phố Vĩnh Long', N'vinh-long');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (120, 57, N'866', N'Cao Lãnh', N'Thành phố Cao Lãnh', N'cao-lanh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (121, 58, N'883', N'Long Xuyên', N'Thành phố Long Xuyên', N'long-xuyen');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (122, 59, N'899', N'Rạch Giá', N'Thành phố Rạch Giá', N'rach-gia');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (123, 60, N'930', N'Vị Thanh', N'Thành phố Vị Thanh', N'vi-thanh');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (124, 61, N'941', N'Sóc Trăng', N'Thành phố Sóc Trăng', N'soc-trang');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (125, 62, N'954', N'Bạc Liêu', N'Thành phố Bạc Liêu', N'bac-lieu');
INSERT INTO [tblDistrict] ([DistrictId], [ProvinceId], [Code], [Name], [FullName], [Slug]) VALUES (126, 63, N'964', N'Cà Mau', N'Thành phố Cà Mau', N'ca-mau');
SET IDENTITY_INSERT [tblDistrict] OFF;
GO

-- Nạp dữ liệu bảng tblWard (29 bản ghi)
SET IDENTITY_INSERT [tblWard] ON;
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (1, 1, N'00001', N'Phúc Xá', N'Phường Phúc Xá', N'phuc-xa');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (2, 1, N'00004', N'Trúc Bạch', N'Phường Trúc Bạch', N'truc-bach');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (3, 1, N'00006', N'Vĩnh Phúc', N'Phường Vĩnh Phúc', N'vinh-phuc');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (4, 1, N'00007', N'Cống Vị', N'Phường Cống Vị', N'cong-vi');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (5, 1, N'00008', N'Liễu Giai', N'Phường Liễu Giai', N'lieu-giai');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (6, 5, N'00157', N'Nghĩa Đô', N'Phường Nghĩa Đô', N'nghia-do');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (7, 5, N'00160', N'Nghĩa Tân', N'Phường Nghĩa Tân', N'nghia-tan');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (8, 5, N'00163', N'Mai Dịch', N'Phường Mai Dịch', N'mai-dich');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (9, 5, N'00166', N'Dịch Vọng', N'Phường Dịch Vọng', N'dich-vong');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (10, 5, N'00167', N'Dịch Vọng Hậu', N'Phường Dịch Vọng Hậu', N'dich-vong-hau');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (11, 18, N'26734', N'Bến Nghé', N'Phường Bến Nghé', N'ben-nghe');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (12, 18, N'26737', N'Bến Thành', N'Phường Bến Thành', N'ben-thanh');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (13, 18, N'26740', N'Cầu Kho', N'Phường Cầu Kho', N'cau-kho');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (14, 18, N'26743', N'Cầu Ông Lãnh', N'Phường Cầu Ông Lãnh', N'cau-ong-lanh');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (15, 18, N'26746', N'Cô Giang', N'Phường Cô Giang', N'co-giang');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (16, 60, N'18151', N'Lê Mao', N'Phường Lê Mao', N'le-mao');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (17, 60, N'18154', N'Lê Lợi', N'Phường Lê Lợi', N'le-loi');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (18, 60, N'18157', N'Hà Huy Tập', N'Phường Hà Huy Tập', N'ha-huy-tap');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (19, 60, N'18160', N'Đội Cung', N'Phường Đội Cung', N'doi-cung');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (20, 60, N'18163', N'Quang Trung', N'Phường Quang Trung', N'quang-trung');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (21, 60, N'18166', N'Cửa Nam', N'Phường Cửa Nam', N'cua-nam');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (22, 60, N'18169', N'Trường Thi', N'Phường Trường Thi', N'truong-thi');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (23, 60, N'18172', N'Hồng Sơn', N'Phường Hồng Sơn', N'hong-son');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (24, 60, N'18175', N'Bến Thủy', N'Phường Bến Thủy', N'ben-thuy');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (25, 60, N'18178', N'Đông Vĩnh', N'Phường Đông Vĩnh', N'dong-vinh');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (26, 60, N'18181', N'Hưng Bình', N'Phường Hưng Bình', N'hung-binh');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (27, 60, N'18184', N'Hưng Dũng', N'Phường Hưng Dũng', N'hung-dung');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (28, 60, N'18187', N'Hưng Phúc', N'Phường Hưng Phúc', N'hung-phuc');
INSERT INTO [tblWard] ([WardId], [DistrictId], [Code], [Name], [FullName], [Slug]) VALUES (29, 60, N'18190', N'Quán Bàu', N'Phường Quán Bàu', N'quan-bau');
SET IDENTITY_INSERT [tblWard] OFF;
GO

-- Nạp dữ liệu bảng tblUser (5 bản ghi)
SET IDENTITY_INSERT [tblUser] ON;
INSERT INTO [tblUser] ([UserId], [Username], [Email], [PasswordHash], [FullName], [Phone], [Avatar], [Role], [IsActive], [FailedLoginCount], [LockoutEnd], [CreatedAt], [UpdatedAt]) VALUES (1, N'admin', N'admin@timvieclam.vn', N'axz4AbkG7HrZuTlhGIbiZA==:PRc8e8yhG1zlDINSkYmZzs+KF67felNENUdn5eIqv1Y=', N'Quản Trị Viên Sàn', N'0900000001', NULL, N'SuperAdmin', 1, 0, NULL, '2026-10-13 14:21:44.828', NULL);
INSERT INTO [tblUser] ([UserId], [Username], [Email], [PasswordHash], [FullName], [Phone], [Avatar], [Role], [IsActive], [FailedLoginCount], [LockoutEnd], [CreatedAt], [UpdatedAt]) VALUES (2, N'nhatuyendung_hanoi', N'tuyendung.hn@fpt.com', N'axz4AbkG7HrZuTlhGIbiZA==:PRc8e8yhG1zlDINSkYmZzs+KF67felNENUdn5eIqv1Y=', N'FPT Software HR', N'0900000002', NULL, N'Employer', 1, 0, NULL, '2026-10-13 14:21:44.858', NULL);
INSERT INTO [tblUser] ([UserId], [Username], [Email], [PasswordHash], [FullName], [Phone], [Avatar], [Role], [IsActive], [FailedLoginCount], [LockoutEnd], [CreatedAt], [UpdatedAt]) VALUES (3, N'nhatuyendung_hcm', N'recruitment@vng.com.vn', N'axz4AbkG7HrZuTlhGIbiZA==:PRc8e8yhG1zlDINSkYmZzs+KF67felNENUdn5eIqv1Y=', N'VNG Talent Acquisition', N'0900000003', NULL, N'Employer', 1, 0, NULL, '2026-10-13 14:21:44.859', NULL);
INSERT INTO [tblUser] ([UserId], [Username], [Email], [PasswordHash], [FullName], [Phone], [Avatar], [Role], [IsActive], [FailedLoginCount], [LockoutEnd], [CreatedAt], [UpdatedAt]) VALUES (4, N'nhatuyendung_danang', N'hr@enclave.vn', N'axz4AbkG7HrZuTlhGIbiZA==:PRc8e8yhG1zlDINSkYmZzs+KF67felNENUdn5eIqv1Y=', N'Enclave IT HR Team', N'0900000004', NULL, N'Employer', 1, 0, NULL, '2026-10-13 14:21:44.860', NULL);
INSERT INTO [tblUser] ([UserId], [Username], [Email], [PasswordHash], [FullName], [Phone], [Avatar], [Role], [IsActive], [FailedLoginCount], [LockoutEnd], [CreatedAt], [UpdatedAt]) VALUES (5, N'ungvien_01', N'ungvien01@gmail.com', N'axz4AbkG7HrZuTlhGIbiZA==:PRc8e8yhG1zlDINSkYmZzs+KF67felNENUdn5eIqv1Y=', N'Nguyễn Văn Ứng Viên', N'0900000005', NULL, N'Candidate', 1, 0, NULL, '2026-10-13 14:21:44.860', NULL);
SET IDENTITY_INSERT [tblUser] OFF;
GO

-- Nạp dữ liệu bảng tblCompany (3 bản ghi)
SET IDENTITY_INSERT [tblCompany] ON;
INSERT INTO [tblCompany] ([CompanyId], [UserId], [Name], [Slug], [Logo], [Website], [TaxCode], [CompanySize], [Description], [Phone], [Email], [Address], [Status], [RejectReason], [CreatedAt], [UpdatedAt]) VALUES (1, 2, N'Công ty Cổ phần Phần mềm FPT (FPT Software)', N'fpt-software', NULL, N'https://fpt-software.com', N'0101778163', N'Enterprise', N'Công ty Cổ phần Phần mềm FPT (FPT Software) là tập đoàn công nghệ hàng đầu, môi trường làm việc chuẩn quốc tế.', N'02473007575', N'contact@fsoft.com.vn', N'Tòa nhà FPT, Số 10 Phạm Văn Bạch, Cầu Giấy, Hà Nội', N'Approved', NULL, '2026-10-13 14:21:44.984', NULL);
INSERT INTO [tblCompany] ([CompanyId], [UserId], [Name], [Slug], [Logo], [Website], [TaxCode], [CompanySize], [Description], [Phone], [Email], [Address], [Status], [RejectReason], [CreatedAt], [UpdatedAt]) VALUES (2, 3, N'Công ty Cổ phần VNG (VNG Corporation)', N'vng-corporation', NULL, N'https://vng-corporation.com', N'0303538466', N'Enterprise', N'Công ty Cổ phần VNG (VNG Corporation) là tập đoàn công nghệ hàng đầu, môi trường làm việc chuẩn quốc tế.', N'02839623888', N'careers@vng.com.vn', N'Z06 Đường số 13, Khu chế xuất Tân Thuận, Phường Tân Thuận Đông, Quận 7, TP.HCM', N'Approved', NULL, '2026-10-13 14:21:45.017', NULL);
INSERT INTO [tblCompany] ([CompanyId], [UserId], [Name], [Slug], [Logo], [Website], [TaxCode], [CompanySize], [Description], [Phone], [Email], [Address], [Status], [RejectReason], [CreatedAt], [UpdatedAt]) VALUES (3, 4, N'Công ty TNHH Giải pháp Phần mềm Enclave', N'enclave-software', NULL, N'https://enclave-software.com', N'0400591244', N'Medium', N'Công ty TNHH Giải pháp Phần mềm Enclave là tập đoàn công nghệ hàng đầu, môi trường làm việc chuẩn quốc tế.', N'02363574175', N'contact@enclave.vn', N'Tòa nhà Enclave, 453-455 Hoàng Diệu, Quận Hải Châu, TP. Đà Nẵng', N'Approved', NULL, '2026-10-13 14:21:45.019', NULL);
SET IDENTITY_INSERT [tblCompany] OFF;
GO

-- Nạp dữ liệu bảng tblWorkplace (6 bản ghi)
SET IDENTITY_INSERT [tblWorkplace] ON;
INSERT INTO [tblWorkplace] ([WorkplaceId], [CompanyId], [Name], [Address], [ProvinceId], [DistrictId], [WardId], [Latitude], [Longitude], [IsHeadOffice], [CreatedAt]) VALUES (1, 1, N'FPT Tower Cầu Giấy (Trụ sở chính)', N'Số 10 Phạm Văn Bạch, Cầu Giấy, Hà Nội', 1, 5, NULL, 21.028511, 105.789123, 1, '2026-10-13 14:21:45.121');
INSERT INTO [tblWorkplace] ([WorkplaceId], [CompanyId], [Name], [Address], [ProvinceId], [DistrictId], [WardId], [Latitude], [Longitude], [IsHeadOffice], [CreatedAt]) VALUES (2, 1, N'FPT Software Keangnam Landmark', N'Tầng 12 Keangnam Landmark 72, Phạm Hùng, Nam Từ Liêm, Hà Nội', 1, 13, NULL, 21.017342, 105.783921, 0, '2026-10-13 14:21:45.142');
INSERT INTO [tblWorkplace] ([WorkplaceId], [CompanyId], [Name], [Address], [ProvinceId], [DistrictId], [WardId], [Latitude], [Longitude], [IsHeadOffice], [CreatedAt]) VALUES (3, 2, N'VNG Campus Tân Thuận (Trụ sở chính)', N'Z06 Đường số 13, Khu chế xuất Tân Thuận, Quận 7, TP.HCM', 2, 34, NULL, 10.749123, 106.721456, 1, '2026-10-13 14:21:45.143');
INSERT INTO [tblWorkplace] ([WorkplaceId], [CompanyId], [Name], [Address], [ProvinceId], [DistrictId], [WardId], [Latitude], [Longitude], [IsHeadOffice], [CreatedAt]) VALUES (4, 2, N'VNG Văn phòng Bitexco', N'Tầng 25 Bitexco Financial Tower, 2 Hải Triều, Quận 1, TP.HCM', 2, 18, NULL, 10.771654, 106.704321, 0, '2026-10-13 14:21:45.144');
INSERT INTO [tblWorkplace] ([WorkplaceId], [CompanyId], [Name], [Address], [ProvinceId], [DistrictId], [WardId], [Latitude], [Longitude], [IsHeadOffice], [CreatedAt]) VALUES (5, 3, N'Enclave Building Hải Châu (Trụ sở chính)', N'453-455 Hoàng Diệu, Hải Châu, Đà Nẵng', 3, 40, NULL, 16.054321, 108.216543, 1, '2026-10-13 14:21:45.144');
INSERT INTO [tblWorkplace] ([WorkplaceId], [CompanyId], [Name], [Address], [ProvinceId], [DistrictId], [WardId], [Latitude], [Longitude], [IsHeadOffice], [CreatedAt]) VALUES (6, 3, N'Chi nhánh FPT Complex Đà Nẵng', N'Khu đô thị công nghệ FPT Đà Nẵng, Ngũ Hành Sơn, Đà Nẵng', 3, 43, NULL, 15.987654, 108.254321, 0, '2026-10-13 14:21:45.145');
SET IDENTITY_INSERT [tblWorkplace] OFF;
GO

-- Nạp dữ liệu bảng tblCandidateProfile (1 bản ghi)
SET IDENTITY_INSERT [tblCandidateProfile] ON;
INSERT INTO [tblCandidateProfile] ([ProfileId], [UserId], [Headline], [DesiredPosition], [ExpectedSalary], [ProvinceId], [Summary], [DefaultCvPath], [CreatedAt], [UpdatedAt]) VALUES (1, 5, N'Senior .NET Core / Fullstack Engineer', N'Kỹ sư phần mềm .NET Senior', 35000000.00, 1, N'Hơn 5 năm kinh nghiệm phát triển hệ thống web doanh nghiệp với ASP.NET Core, SQL Server, Redis, Docker và kiến trúc Microservices.', N'/uploads/cvs/cv_nguyen_van_a.pdf', '2026-10-13 14:21:44.913', NULL);
SET IDENTITY_INSERT [tblCandidateProfile] OFF;
GO

-- Nạp dữ liệu bảng tblJob (45 bản ghi)
SET IDENTITY_INSERT [tblJob] ON;
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (1, 1, 1, 1, 1, N'Senior .NET Core Backend Engineer', N'senior-dotnet-core-backend-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Senior .NET Core Backend Engineer</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 30000000.00, 50000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 1, 10, '2026-10-08 14:21:45.247', NULL, N'senior .net core backend engineer cong ty co phan phan mem fpt fpt software c# .net core docker sql server so 10 pham van bach cau giay ha noi', '2026-10-13 14:21:45.247', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (2, 1, 2, 1, 1, N'Lập trình viên ASP.NET MVC / C#', N'lap-trinh-vien-aspnet-mvc-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Lập trình viên ASP.NET MVC / C#</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 18000000.00, 28000000.00, N'VND', 0, N'1-2 năm', N'Cao đẳng', N'Hybrid', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'lap trinh vien asp.net mvc c# cong ty co phan phan mem fpt fpt software c# asp.net mvc sql server tang 12 keangnam landmark 72 pham hung nam tu liem ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (3, 1, NULL, 1, 1, N'Chuyên gia Cloud AWS / DevOps Engineer (Remote)', N'cloud-aws-devops-engineer-remote-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Chuyên gia Cloud AWS / DevOps Engineer (Remote)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 40000000.00, 65000000.00, N'VND', 0, N'Trên 5 năm', N'Đại học', N'Remote', 2, '2026-11-12 14:21:45.237', N'Published', 1, 10, '2026-10-08 14:21:45.248', NULL, N'chuyen gia cloud aws devops engineer remote cong ty co phan phan mem fpt fpt software docker kubernetes aws ci cd lam viec tu xa remote', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (4, 1, 1, 1, 3, N'Thực tập sinh Lập trình .NET (Có trợ cấp)', N'thuc-tap-sinh-lap-trinh-dotnet-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Thực tập sinh Lập trình .NET (Có trợ cấp)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 5000000.00, 8000000.00, N'VND', 0, N'Không yêu cầu', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'thuc tap sinh lap trinh .net co tro cap cong ty co phan phan mem fpt fpt software c# sql server so 10 pham van bach cau giay ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (5, 1, 2, 1, 1, N'Frontend Developer (React / TypeScript)', N'frontend-developer-react-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Frontend Developer (React / TypeScript)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 22000000.00, 38000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'Hybrid', 2, '2026-10-15 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'frontend developer react typescript cong ty co phan phan mem fpt fpt software javascript typescript react tang 12 keangnam landmark 72 pham hung nam tu liem ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (6, 2, 3, 1, 1, N'Senior Java Backend Engineer (ZaloPay)', N'senior-java-backend-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Senior Java Backend Engineer (ZaloPay)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 35000000.00, 60000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 1, 10, '2026-10-08 14:21:45.248', NULL, N'senior java backend engineer zalopay cong ty co phan vng vng corporation java spring boot redis microservices z06 duong so 13 khu che xuat tan thuan quan 7 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (7, 2, 4, 1, 1, N'Chuyên viên Phân tích Dữ liệu (Data Analyst)', N'data-analyst-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Chuyên viên Phân tích Dữ liệu (Data Analyst)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 25000000.00, 45000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'Hybrid', 2, '2026-12-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'chuyen vien phan tich du lieu data analyst cong ty co phan vng vng corporation python sql server data analysis tang 25 bitexco financial tower 2 hai trieu quan 1 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (8, 2, NULL, 1, 1, N'AI / Machine Learning Research Engineer (Remote)', N'ai-machine-learning-engineer-remote-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>AI / Machine Learning Research Engineer (Remote)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 45000000.00, 80000000.00, N'VND', 0, N'2-5 năm', N'Sau đại học', N'Remote', 2, '2026-11-12 14:21:45.237', N'Published', 1, 10, '2026-10-08 14:21:45.248', NULL, N'ai machine learning research engineer remote cong ty co phan vng vng corporation python fastapi machine learning lam viec tu xa remote', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (9, 2, 3, 1, 4, N'Golang Microservices Developer (Contract)', N'golang-microservices-developer-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Golang Microservices Developer (Contract)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 30000000.00, 50000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'golang microservices developer contract cong ty co phan vng vng corporation golang docker microservices z06 duong so 13 khu che xuat tan thuan quan 7 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (10, 2, 4, 1, 1, N'Kỹ sư Cầu nối tiếng Nhật / Tiếng Anh (BrSE)', N'ky-su-cau-noi-brse-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Kỹ sư Cầu nối tiếng Nhật / Tiếng Anh (BrSE)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 30000000.00, 55000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'ky su cau noi tieng nhat tieng anh brse cong ty co phan vng vng corporation agile scrum tieng anh giao tiep tang 25 bitexco financial tower 2 hai trieu quan 1 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (11, 3, 5, 1, 1, N'Fullstack Developer (Node.js & Vue.js)', N'fullstack-developer-node-vue-enclave', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Fullstack Developer (Node.js & Vue.js)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 20000000.00, 35000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'fullstack developer node.js vue.js cong ty tnhh giai phap phan mem enclave javascript vue.js node.js 453 455 hoang dieu hai chau da nang', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (12, 3, 6, 1, 1, N'Kỹ sư QA/QC Automation Tester', N'qa-automation-tester-enclave', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Kỹ sư QA/QC Automation Tester</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 18000000.00, 30000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'Hybrid', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'ky su qa qc automation tester cong ty tnhh giai phap phan mem enclave python ci cd khu do thi cong nghe fpt da nang ngu hanh son da nang', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (13, 3, NULL, 1, 5, N'Lập trình viên PHP / Laravel (Freelance)', N'php-laravel-freelance-enclave', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Lập trình viên PHP / Laravel (Freelance)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', NULL, NULL, N'VND', 1, N'1-2 năm', N'Không yêu cầu', N'Remote', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'lap trinh vien php laravel freelance cong ty tnhh giai phap phan mem enclave php laravel mysql lam viec tu xa remote', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (14, 3, 5, 1, 1, N'Scrum Master / Project Manager', N'scrum-master-pm-enclave', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Scrum Master / Project Manager</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 35000000.00, 55000000.00, N'VND', 0, N'Trên 5 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 1, 10, '2026-10-08 14:21:45.248', NULL, N'scrum master project manager cong ty tnhh giai phap phan mem enclave agile scrum tieng anh giao tiep 453 455 hoang dieu hai chau da nang', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (15, 3, 6, 1, 2, N'Lập trình viên Mobile Flutter / React Native', N'mobile-developer-react-native-enclave', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Lập trình viên Mobile Flutter / React Native</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 15000000.00, 25000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'Hybrid', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'lap trinh vien mobile flutter react native cong ty tnhh giai phap phan mem enclave typescript react khu do thi cong nghe fpt da nang ngu hanh son da nang', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (16, 2, 3, 2, 1, N'Trưởng nhóm Digital Marketing (Team Lead)', N'digital-marketing-team-lead-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Trưởng nhóm Digital Marketing (Team Lead)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 25000000.00, 40000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 1, 10, '2026-10-08 14:21:45.248', NULL, N'truong nhom digital marketing team lead cong ty co phan vng vng corporation seo content marketing z06 duong so 13 khu che xuat tan thuan quan 7 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (17, 2, 4, 2, 1, N'Chuyên viên Tối ưu hóa SEO Website', N'chuyen-vien-seo-website-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Chuyên viên Tối ưu hóa SEO Website</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 15000000.00, 22000000.00, N'VND', 0, N'1-2 năm', N'Cao đẳng', N'Hybrid', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'chuyen vien toi uu hoa seo website cong ty co phan vng vng corporation seo content marketing tang 25 bitexco financial tower 2 hai trieu quan 1 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (18, 1, 1, 2, 1, N'Chuyên viên Quan hệ Công chúng & Báo chí (PR)', N'chuyen-vien-pr-bao-chi-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Chuyên viên Quan hệ Công chúng & Báo chí (PR)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 18000000.00, 28000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'chuyen vien quan he cong chung bao chi pr cong ty co phan phan mem fpt fpt software content marketing tieng anh giao tiep so 10 pham van bach cau giay ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (19, 1, NULL, 2, 5, N'Content Creator / Copywriter (Làm từ xa)', N'content-creator-copywriter-remote-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Content Creator / Copywriter (Làm từ xa)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 10000000.00, 18000000.00, N'VND', 0, N'Dưới 1 năm', N'Không yêu cầu', N'Remote', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'content creator copywriter lam tu xa cong ty co phan phan mem fpt fpt software content marketing lam viec tu xa remote', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (20, 3, 5, 2, 1, N'Chuyên viên Social Media Marketing', N'social-media-marketing-enclave', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Chuyên viên Social Media Marketing</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 12000000.00, 20000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'Hybrid', 2, '2026-10-15 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'chuyen vien social media marketing cong ty tnhh giai phap phan mem enclave content marketing 453 455 hoang dieu hai chau da nang', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (21, 1, 1, 4, 1, N'Giám đốc Phát triển Kinh doanh B2B (BD Manager)', N'bd-manager-b2b-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Giám đốc Phát triển Kinh doanh B2B (BD Manager)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 40000000.00, 70000000.00, N'VND', 0, N'Trên 5 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 1, 10, '2026-10-08 14:21:45.248', NULL, N'giam doc phat trien kinh doanh b2b bd manager cong ty co phan phan mem fpt fpt software sales b2b tieng anh giao tiep so 10 pham van bach cau giay ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (22, 1, 2, 4, 1, N'Chuyên viên Kinh doanh Giải pháp Phần mềm B2B', N'sales-b2b-giai-phap-phan-mem-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Chuyên viên Kinh doanh Giải pháp Phần mềm B2B</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 18000000.00, 35000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'chuyen vien kinh doanh giai phap phan mem b2b cong ty co phan phan mem fpt fpt software sales b2b tang 12 keangnam landmark 72 pham hung nam tu liem ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (23, 2, 3, 4, 1, N'Account Executive (Dịch vụ Đám mây VNG Cloud)', N'account-executive-vng-cloud', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Account Executive (Dịch vụ Đám mây VNG Cloud)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 20000000.00, 40000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'Hybrid', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'account executive dich vu dam may vng cloud cong ty co phan vng vng corporation sales b2b z06 duong so 13 khu che xuat tan thuan quan 7 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (24, 3, 5, 4, 1, N'Nhân viên Tư vấn Dịch vụ CNTT Khách hàng Quốc tế', N'tu-van-dich-vu-cntt-quoc-te-enclave', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Nhân viên Tư vấn Dịch vụ CNTT Khách hàng Quốc tế</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 20000000.00, 35000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'nhan vien tu van dich vu cntt khach hang quoc te cong ty tnhh giai phap phan mem enclave sales b2b tieng anh giao tiep 453 455 hoang dieu hai chau da nang', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (25, 1, 1, 3, 1, N'Kế toán trưởng Doanh nghiệp CNTT', N'ke-toan-truong-fpt-software', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Kế toán trưởng Doanh nghiệp CNTT</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 35000000.00, 50000000.00, N'VND', 0, N'Trên 5 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 1, 10, '2026-10-08 14:21:45.248', NULL, N'ke toan truong doanh nghiep cntt cong ty co phan phan mem fpt fpt software so 10 pham van bach cau giay ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (26, 1, 2, 3, 1, N'Kế toán Tổng hợp / Kế toán Thuế', N'ke-toan-tong-hop-thue-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Kế toán Tổng hợp / Kế toán Thuế</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 15000000.00, 22000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'ke toan tong hop ke toan thue cong ty co phan phan mem fpt fpt software tang 12 keangnam landmark 72 pham hung nam tu liem ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (27, 2, 4, 6, 1, N'Chuyên viên Phân tích Tài chính Doanh nghiệp (FP&A)', N'chuyen-vien-tai-chinh-fpa-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Chuyên viên Phân tích Tài chính Doanh nghiệp (FP&A)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 25000000.00, 40000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'Hybrid', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'chuyen vien phan tich tai chinh doanh nghiep fp a cong ty co phan vng vng corporation data analysis tang 25 bitexco financial tower 2 hai trieu quan 1 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (28, 2, 3, 9, 1, N'Lead UI/UX Product Designer', N'lead-ui-ux-product-designer-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Lead UI/UX Product Designer</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 35000000.00, 55000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'Hybrid', 2, '2026-11-12 14:21:45.237', N'Published', 1, 10, '2026-10-08 14:21:45.248', NULL, N'lead ui ux product designer cong ty co phan vng vng corporation ui ux design figma z06 duong so 13 khu che xuat tan thuan quan 7 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (29, 2, 4, 9, 1, N'Junior UI/UX Designer', N'junior-ui-ux-designer-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Junior UI/UX Designer</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 14000000.00, 22000000.00, N'VND', 0, N'Dưới 1 năm', N'Cao đẳng', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'junior ui ux designer cong ty co phan vng vng corporation ui ux design figma tang 25 bitexco financial tower 2 hai trieu quan 1 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (30, 3, 5, 9, 5, N'2D/3D Graphic Designer (Dự án Game)', N'graphic-designer-game-enclave', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>2D/3D Graphic Designer (Dự án Game)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', NULL, NULL, N'VND', 1, N'1-2 năm', N'Không yêu cầu', N'Remote', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'2d 3d graphic designer du an game cong ty tnhh giai phap phan mem enclave ui ux design 453 455 hoang dieu hai chau da nang', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (31, 1, 1, 5, 1, N'Chuyên viên Tuyển dụng IT (IT Recruiter)', N'chuyen-vien-tuyen-dung-it-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Chuyên viên Tuyển dụng IT (IT Recruiter)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 15000000.00, 25000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'chuyen vien tuyen dung it it recruiter cong ty co phan phan mem fpt fpt software tieng anh giao tiep so 10 pham van bach cau giay ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (32, 2, 3, 15, 1, N'Chuyên viên Chăm sóc Khách hàng VIP', N'chuyen-vien-cskh-vip-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Chuyên viên Chăm sóc Khách hàng VIP</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 12000000.00, 18000000.00, N'VND', 0, N'Dưới 1 năm', N'Cao đẳng', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'chuyen vien cham soc khach hang vip cong ty co phan vng vng corporation tieng anh giao tiep z06 duong so 13 khu che xuat tan thuan quan 7 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (33, 1, 2, 12, 1, N'Chuyên viên Logistics & Quản lý Chuỗi cung ứng', N'logistics-supply-chain-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Chuyên viên Logistics & Quản lý Chuỗi cung ứng</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 16000000.00, 24000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'chuyen vien logistics quan ly chuoi cung ung cong ty co phan phan mem fpt fpt software tieng anh giao tiep tang 12 keangnam landmark 72 pham hung nam tu liem ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (34, 1, 1, 10, 2, N'Giảng viên Lập trình .NET & C# (Bán thời gian)', N'giang-vien-lap-trinh-dotnet-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Giảng viên Lập trình .NET & C# (Bán thời gian)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 20000000.00, 35000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'Hybrid', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'giang vien lap trinh .net c# ban thoi gian cong ty co phan phan mem fpt fpt software c# .net core so 10 pham van bach cau giay ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (35, 2, 3, 7, 1, N'Kỹ sư Quản lý Hạ tầng Cơ điện Trung tâm Dữ liệu', N'datacenter-me-engineer-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Kỹ sư Quản lý Hạ tầng Cơ điện Trung tâm Dữ liệu</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 22000000.00, 35000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 1, 10, '2026-10-08 14:21:45.248', NULL, N'ky su quan ly ha tang co dien trung tam du lieu cong ty co phan vng vng corporation linux z06 duong so 13 khu che xuat tan thuan quan 7 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (36, 2, 4, 8, 1, N'Chuyên viên Quản lý & Vận hành Mặt bằng BĐS', N'facility-real-estate-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Chuyên viên Quản lý & Vận hành Mặt bằng BĐS</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 18000000.00, 26000000.00, N'VND', 0, N'1-2 năm', N'Cao đẳng', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'chuyen vien quan ly van hanh mat bang bds cong ty co phan vng vng corporation tang 25 bitexco financial tower 2 hai trieu quan 1 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (37, 1, 1, 14, 1, N'Kỹ sư Lập trình Nhúng & IoT (C/C++)', N'embedded-iot-engineer-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Kỹ sư Lập trình Nhúng & IoT (C/C++)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 20000000.00, 38000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'ky su lap trinh nhung iot c c++ cong ty co phan phan mem fpt fpt software linux so 10 pham van bach cau giay ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (38, 2, 3, 1, 1, N'Mobile Android / Kotlin Developer', N'mobile-android-developer-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Mobile Android / Kotlin Developer</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 22000000.00, 36000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'Hybrid', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'mobile android kotlin developer cong ty co phan vng vng corporation z06 duong so 13 khu che xuat tan thuan quan 7 tp.hcm', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (39, 3, 5, 1, 1, N'Database Administrator (DBA SQL Server / PostgreSQL)', N'dba-sql-server-enclave', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Database Administrator (DBA SQL Server / PostgreSQL)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 25000000.00, 40000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'database administrator dba sql server postgresql cong ty tnhh giai phap phan mem enclave sql server postgresql 453 455 hoang dieu hai chau da nang', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (40, 1, 1, 1, 1, N'Kỹ sư An toàn Thông tin & Cyber Security', N'cyber-security-engineer-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Kỹ sư An toàn Thông tin & Cyber Security</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 30000000.00, 52000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Published', 1, 10, '2026-10-08 14:21:45.248', NULL, N'ky su an toan thong tin cyber security cong ty co phan phan mem fpt fpt software ci cd linux so 10 pham van bach cau giay ha noi', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (41, 3, 6, 1, 1, N'Fullstack .NET Core & React Developer', N'fullstack-dotnet-react-enclave', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Fullstack .NET Core & React Developer</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 24000000.00, 42000000.00, N'VND', 0, N'2-5 năm', N'Đại học', N'Hybrid', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'fullstack .net core react developer cong ty tnhh giai phap phan mem enclave c# .net core react khu do thi cong nghe fpt da nang ngu hanh son da nang', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (42, 3, NULL, 1, 5, N'Chuyên viên Tối ưu Hiệu năng Web (Remote Freelance)', N'web-performance-consultant-remote', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Chuyên viên Tối ưu Hiệu năng Web (Remote Freelance)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', NULL, NULL, N'VND', 1, N'2-5 năm', N'Không yêu cầu', N'Remote', 2, '2026-11-12 14:21:45.237', N'Published', 0, 10, '2026-10-08 14:21:45.248', NULL, N'chuyen vien toi uu hieu nang web remote freelance cong ty tnhh giai phap phan mem enclave javascript react lam viec tu xa remote', '2026-10-13 14:21:45.248', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (43, 1, 1, 1, 1, N'Tin tuyển dụng đang chờ duyệt (Pending)', N'tin-tuyen-dung-cho-duyet-fpt', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Tin tuyển dụng đang chờ duyệt (Pending)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 20000000.00, 30000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Pending', 0, 10, NULL, NULL, N'tin tuyen dung dang cho duyet pending cong ty co phan phan mem fpt fpt software c# so 10 pham van bach cau giay ha noi', '2026-10-13 14:21:45.249', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (44, 2, 3, 1, 1, N'Tin tuyển dụng bị từ chối (Rejected)', N'tin-tuyen-dung-bi-tu-choi-vng', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Tin tuyển dụng bị từ chối (Rejected)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 20000000.00, 30000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'OnSite', 2, '2026-11-12 14:21:45.237', N'Rejected', 0, 10, NULL, NULL, N'tin tuyen dung bi tu choi rejected cong ty co phan vng vng corporation java z06 duong so 13 khu che xuat tan thuan quan 7 tp.hcm', '2026-10-13 14:21:45.249', NULL);
INSERT INTO [tblJob] ([JobId], [CompanyId], [WorkplaceId], [CategoryId], [JobTypeId], [Title], [Slug], [Description], [Requirements], [SalaryMin], [SalaryMax], [SalaryUnit], [IsSalaryNegotiable], [ExperienceLevel], [EducationLevel], [WorkMode], [Headcount], [ExpiresAt], [ApprovalStatus], [IsFeatured], [ViewCount], [PublishedAt], [RejectReason], [SearchText], [CreatedAt], [UpdatedAt]) VALUES (45, 3, 5, 1, 1, N'Tin tuyển dụng đã hết hạn (Expired)', N'tin-tuyen-dung-da-het-han-enclave', N'<p>Chúng tôi đang tìm kiếm vị trí <strong>Tin tuyển dụng đã hết hạn (Expired)</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>', N'<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>', 20000000.00, 30000000.00, N'VND', 0, N'1-2 năm', N'Đại học', N'OnSite', 2, '2026-10-03 14:21:45.237', N'Expired', 0, 10, NULL, NULL, N'tin tuyen dung da het han expired cong ty tnhh giai phap phan mem enclave python 453 455 hoang dieu hai chau da nang', '2026-10-13 14:21:45.249', NULL);
SET IDENTITY_INSERT [tblJob] OFF;
GO

-- Nạp dữ liệu bảng tblJobSkill (82 bản ghi)
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (1, 1);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (2, 1);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (4, 1);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (34, 1);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (41, 1);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (43, 1);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (1, 2);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (34, 2);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (41, 2);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (2, 3);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (6, 4);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (44, 4);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (6, 5);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (7, 6);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (8, 6);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (12, 6);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (45, 6);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (8, 8);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (5, 9);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (11, 9);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (42, 9);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (5, 10);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (15, 10);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (5, 11);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (15, 11);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (41, 11);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (42, 11);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (11, 13);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (11, 14);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (13, 15);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (13, 16);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (9, 17);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (1, 18);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (3, 18);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (9, 18);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (3, 19);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (3, 20);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (1, 23);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (2, 23);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (4, 23);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (7, 23);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (39, 23);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (39, 24);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (13, 25);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (6, 27);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (3, 29);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (12, 29);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (40, 29);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (6, 30);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (9, 30);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (28, 33);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (29, 33);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (30, 33);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (28, 34);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (29, 34);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (35, 35);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (37, 35);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (40, 35);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (10, 36);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (14, 36);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (7, 37);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (27, 37);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (8, 38);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (16, 39);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (17, 39);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (16, 40);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (17, 40);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (18, 40);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (19, 40);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (20, 40);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (21, 41);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (22, 41);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (23, 41);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (24, 41);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (10, 42);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (14, 42);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (18, 42);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (21, 42);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (24, 42);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (31, 42);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (32, 42);
INSERT INTO [tblJobSkill] ([JobId], [SkillId]) VALUES (33, 42);
GO

-- Nạp dữ liệu bảng tblJobBenefit (135 bản ghi)
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (1, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (2, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (3, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (4, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (5, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (6, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (7, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (8, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (9, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (10, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (11, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (12, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (13, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (14, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (15, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (16, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (17, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (18, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (19, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (20, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (21, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (22, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (23, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (24, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (25, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (26, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (27, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (28, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (29, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (30, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (31, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (32, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (33, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (34, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (35, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (36, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (37, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (38, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (39, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (40, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (41, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (42, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (43, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (44, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (45, 1);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (1, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (2, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (3, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (4, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (5, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (6, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (7, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (8, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (9, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (10, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (11, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (12, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (13, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (14, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (15, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (16, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (17, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (18, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (19, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (20, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (21, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (22, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (23, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (24, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (25, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (26, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (27, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (28, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (29, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (30, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (31, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (32, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (33, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (34, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (35, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (36, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (37, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (38, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (39, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (40, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (41, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (42, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (43, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (44, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (45, 2);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (1, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (2, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (3, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (4, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (5, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (6, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (7, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (8, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (9, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (10, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (11, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (12, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (13, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (14, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (15, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (16, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (17, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (18, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (19, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (20, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (21, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (22, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (23, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (24, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (25, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (26, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (27, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (28, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (29, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (30, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (31, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (32, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (33, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (34, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (35, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (36, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (37, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (38, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (39, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (40, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (41, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (42, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (43, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (44, 3);
INSERT INTO [tblJobBenefit] ([JobId], [BenefitId]) VALUES (45, 3);
GO

-- Nạp dữ liệu bảng tblApplication (1 bản ghi)
SET IDENTITY_INSERT [tblApplication] ON;
INSERT INTO [tblApplication] ([ApplicationId], [JobId], [CandidateUserId], [FullName], [Email], [Phone], [CoverLetter], [CvPath], [Status], [EmployerNote], [AppliedAt], [UpdatedAt]) VALUES (1, 1, 5, N'Nguyễn Văn Ứng Viên', N'ungvien01@gmail.com', N'0900000005', N'Tôi rất quan tâm đến vị trí Senior .NET Core tại FPT Software và tin tưởng kinh nghiệm của mình hoàn toàn phù hợp.', N'/uploads/cvs/cv_nguyen_van_a.pdf', N'New', NULL, '2026-10-13 14:21:45.579', NULL);
SET IDENTITY_INSERT [tblApplication] OFF;
GO

-- Nạp dữ liệu bảng tblSavedJob (1 bản ghi)
SET IDENTITY_INSERT [tblSavedJob] ON;
INSERT INTO [tblSavedJob] ([SavedJobId], [UserId], [JobId], [SavedAt]) VALUES (1, 5, 1, '2026-10-13 14:21:45.604');
SET IDENTITY_INSERT [tblSavedJob] OFF;
GO

-- Nạp dữ liệu bảng tblMenu (9 bản ghi)
SET IDENTITY_INSERT [tblMenu] ON;
INSERT INTO [tblMenu] ([MenuId], [MenuName], [Url], [Icon], [ParentMenuId], [SortOrder], [Position], [OpenNewTab], [IsActive], [CreatedAt]) VALUES (1, N'Trang chủ', N'/', N'bi-house', NULL, 1, N'header', 0, 1, '2026-10-13 14:21:45.640');
INSERT INTO [tblMenu] ([MenuId], [MenuName], [Url], [Icon], [ParentMenuId], [SortOrder], [Position], [OpenNewTab], [IsActive], [CreatedAt]) VALUES (2, N'Tìm việc làm', N'/Jobs', N'bi-briefcase', NULL, 2, N'header', 0, 1, '2026-10-13 14:21:45.656');
INSERT INTO [tblMenu] ([MenuId], [MenuName], [Url], [Icon], [ParentMenuId], [SortOrder], [Position], [OpenNewTab], [IsActive], [CreatedAt]) VALUES (3, N'Công ty', N'/Companies', N'bi-building', NULL, 3, N'header', 0, 1, '2026-10-13 14:21:45.657');
INSERT INTO [tblMenu] ([MenuId], [MenuName], [Url], [Icon], [ParentMenuId], [SortOrder], [Position], [OpenNewTab], [IsActive], [CreatedAt]) VALUES (4, N'Cẩm nang nghề nghiệp', N'/Blog', N'bi-journal-text', NULL, 4, N'header', 0, 1, '2026-10-13 14:21:45.658');
INSERT INTO [tblMenu] ([MenuId], [MenuName], [Url], [Icon], [ParentMenuId], [SortOrder], [Position], [OpenNewTab], [IsActive], [CreatedAt]) VALUES (5, N'Liên hệ', N'/Contact', N'bi-envelope', NULL, 5, N'header', 0, 1, '2026-10-13 14:21:45.659');
INSERT INTO [tblMenu] ([MenuId], [MenuName], [Url], [Icon], [ParentMenuId], [SortOrder], [Position], [OpenNewTab], [IsActive], [CreatedAt]) VALUES (6, N'Về TimViecLam', N'/About', NULL, NULL, 1, N'footer', 0, 1, '2026-10-13 14:21:45.659');
INSERT INTO [tblMenu] ([MenuId], [MenuName], [Url], [Icon], [ParentMenuId], [SortOrder], [Position], [OpenNewTab], [IsActive], [CreatedAt]) VALUES (7, N'Điều khoản sử dụng', N'/Terms', NULL, NULL, 2, N'footer', 0, 1, '2026-10-13 14:21:45.660');
INSERT INTO [tblMenu] ([MenuId], [MenuName], [Url], [Icon], [ParentMenuId], [SortOrder], [Position], [OpenNewTab], [IsActive], [CreatedAt]) VALUES (8, N'Chính sách bảo mật', N'/Privacy', NULL, NULL, 3, N'footer', 0, 1, '2026-10-13 14:21:45.661');
INSERT INTO [tblMenu] ([MenuId], [MenuName], [Url], [Icon], [ParentMenuId], [SortOrder], [Position], [OpenNewTab], [IsActive], [CreatedAt]) VALUES (9, N'Quy chế hoạt động', N'/Regulations', NULL, NULL, 4, N'footer', 0, 1, '2026-10-13 14:21:45.661');
SET IDENTITY_INSERT [tblMenu] OFF;
GO

-- Nạp dữ liệu bảng tblPost (3 bản ghi)
SET IDENTITY_INSERT [tblPost] ON;
INSERT INTO [tblPost] ([PostId], [Title], [Slug], [Summary], [Content], [ThumbnailImage], [Category], [IsPinned], [IsPublished], [PublishedAt], [ViewCount], [MetaTitle], [MetaDescription], [CreatedAt], [UpdatedAt]) VALUES (1, N'Bí quyết viết CV chuẩn ATS chinh phục nhà tuyển dụng IT', N'bi-quyet-viet-cv-chuan-ats', N'Hướng dẫn cấu trúc CV súc tích, tối ưu từ khóa kỹ năng giúp vượt qua vòng lọc hồ sơ tự động.', N'<p>Hướng dẫn cấu trúc CV súc tích, tối ưu từ khóa kỹ năng giúp vượt qua vòng lọc hồ sơ tự động.</p><p>Chi tiết bài viết đang được cập nhật...</p>', NULL, N'Kinh nghiệm ứng tuyển', 0, 1, '2026-10-10 14:21:45.667', 120, NULL, NULL, '2026-10-13 14:21:45.667', NULL);
INSERT INTO [tblPost] ([PostId], [Title], [Slug], [Summary], [Content], [ThumbnailImage], [Category], [IsPinned], [IsPublished], [PublishedAt], [ViewCount], [MetaTitle], [MetaDescription], [CreatedAt], [UpdatedAt]) VALUES (2, N'Top 10 câu hỏi phỏng vấn .NET và ASP.NET Core hay gặp nhất', N'top-10-cau-hoi-phong-van-dotnet-core', N'Tổng hợp kiến thức trọng tâm về Dependency Injection, Middleware, Async/Await và EF Core.', N'<p>Tổng hợp kiến thức trọng tâm về Dependency Injection, Middleware, Async/Await và EF Core.</p><p>Chi tiết bài viết đang được cập nhật...</p>', NULL, N'Phỏng vấn', 0, 1, '2026-10-10 14:21:45.685', 120, NULL, NULL, '2026-10-13 14:21:45.685', NULL);
INSERT INTO [tblPost] ([PostId], [Title], [Slug], [Summary], [Content], [ThumbnailImage], [Category], [IsPinned], [IsPublished], [PublishedAt], [ViewCount], [MetaTitle], [MetaDescription], [CreatedAt], [UpdatedAt]) VALUES (3, N'Xu hướng mức lương ngành Công nghệ thông tin tại Việt Nam năm 2026', N'xu-huong-muc-luong-nganh-cntt-2026', N'Báo cáo chi tiết dải lương theo năm kinh nghiệm cho các vị trí Backend, Frontend, Cloud và AI.', N'<p>Báo cáo chi tiết dải lương theo năm kinh nghiệm cho các vị trí Backend, Frontend, Cloud và AI.</p><p>Chi tiết bài viết đang được cập nhật...</p>', NULL, N'Báo cáo thị trường', 0, 1, '2026-10-10 14:21:45.685', 120, NULL, NULL, '2026-10-13 14:21:45.685', NULL);
SET IDENTITY_INSERT [tblPost] OFF;
GO


