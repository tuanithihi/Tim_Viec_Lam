/*
==============================================================================
 DỰ ÁN: TIMVIECLAM — SÀN TÌM VIỆC LÀM TRỰC TUYẾN
 SCRIPT CƠ SỞ DỮ LIỆU CHỈ DÀNH RIÊNG CHO SPRINT 1 (QUẢN LÝ NGƯỜI DÙNG & XÁC THỰC)
 HỆ QUẢN TRỊ: Microsoft SQL Server (SSMS / Azure Data Studio / LocalDB)
 PHẠM VI:
 - Gồm đúng 2 bảng thuộc phạm vi Sprint 1: tblUser và tblCompany.
 - KHÔNG bao gồm các bảng từ Sprint 2 trở đi (Danh mục, Tin tuyển dụng, Ứng tuyển...)
   để các thành viên khác tự tạo và quản lý bảng riêng trong các Sprint sau.
==============================================================================
*/

-- ── 1. TẠO DATABASE (NẾU CHƯA CÓ) VÀ CHUYỂN NGỮ CẢNH ─────────────────────────
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'TimViecLam')
BEGIN
    CREATE DATABASE [TimViecLam];
END;
GO

USE [TimViecLam];
GO

-- ── 2. BẢNG LỊCH SỬ DI CHUYỂN ENTITY FRAMEWORK (ĐỒNG BỘ SPRINT 1) ───────────
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261102094133_AddUserResetToken')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261013070331_InitialCreate', N'8.0.10'),
           (N'20261102094133_AddUserResetToken', N'8.0.10');
END;
GO

-- ── 3. BẢNG 1: tblUser (QUẢN LÝ NGƯỜI DÙNG & XÁC THỰC) ─────────────────────
IF OBJECT_ID(N'[tblUser]') IS NULL
BEGIN
    CREATE TABLE [tblUser] (
        [UserId] int NOT NULL IDENTITY(1,1),
        [Username] nvarchar(50) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [PasswordHash] nvarchar(256) NOT NULL,
        [FullName] nvarchar(100) NULL,
        [Phone] nvarchar(20) NULL,
        [Avatar] nvarchar(255) NULL,
        [Role] nvarchar(20) NOT NULL DEFAULT ('Candidate'),
        [IsActive] bit NOT NULL DEFAULT (1),
        [FailedLoginCount] int NOT NULL DEFAULT (0),
        [LockoutEnd] datetime2 NULL,
        [ResetTokenHash] nvarchar(256) NULL,
        [ResetTokenExpiresAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_tblUser] PRIMARY KEY ([UserId])
    );

    -- Chỉ mục duy nhất (Unique Index) cho Username và Email
    CREATE UNIQUE NONCLUSTERED INDEX [IX_tblUser_Username] ON [tblUser] ([Username]);
    CREATE UNIQUE NONCLUSTERED INDEX [IX_tblUser_Email] ON [tblUser] ([Email]);
END;
GO

-- ── 4. BẢNG 2: tblCompany (DOANH NGHIỆP LIÊN KẾT NHÀ TUYỂN DỤNG) ────────────
IF OBJECT_ID(N'[tblCompany]') IS NULL
BEGIN
    CREATE TABLE [tblCompany] (
        [CompanyId] int NOT NULL IDENTITY(1,1),
        [UserId] int NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Slug] nvarchar(220) NOT NULL,
        [Logo] nvarchar(255) NULL,
        [Website] nvarchar(255) NULL,
        [TaxCode] nvarchar(50) NULL,
        [CompanySize] nvarchar(50) NOT NULL DEFAULT ('Small'),
        [Description] nvarchar(max) NULL,
        [Phone] nvarchar(20) NULL,
        [Email] nvarchar(200) NULL,
        [Address] nvarchar(300) NULL,
        [Status] nvarchar(30) NOT NULL DEFAULT ('Pending'),
        [RejectReason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_tblCompany] PRIMARY KEY ([CompanyId]),
        CONSTRAINT [FK_tblCompany_tblUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [tblUser] ([UserId]) ON DELETE CASCADE
    );

    -- Chỉ mục duy nhất (Unique Index)
    CREATE UNIQUE NONCLUSTERED INDEX [IX_tblCompany_UserId] ON [tblCompany] ([UserId]);
    CREATE UNIQUE NONCLUSTERED INDEX [IX_tblCompany_Slug] ON [tblCompany] ([Slug]);
END;
GO

-- ── 5. DỮ LIỆU MẪU SPRINT 1 (MẬT KHẨU MẶC ĐỊNH CHO TẤT CẢ: 123456) ──────────
-- Băm theo chuẩn PBKDF2 (HMAC-SHA256, 128-bit Salt, 100.000 vòng lặp)
SET IDENTITY_INSERT [tblUser] ON;

IF NOT EXISTS (SELECT 1 FROM [tblUser] WHERE [Username] = 'admin')
BEGIN
    INSERT INTO [tblUser] ([UserId], [Username], [Email], [PasswordHash], [FullName], [Phone], [Role], [IsActive], [FailedLoginCount], [CreatedAt])
    VALUES (1, 'admin', 'admin@timvieclam.vn', 'AQAAAAIAAYagAAAAEMOA577yNRrWSENPKea5vrqZTNon8FEFTpAHz1IxApd2+fsatyFvUQ+sG374sPOoVw==', N'Quản Trị Viên Sàn', '0900000001', 'SuperAdmin', 1, 0, GETUTCDATE());
END;

IF NOT EXISTS (SELECT 1 FROM [tblUser] WHERE [Username] = 'nhatuyendung_hanoi')
BEGIN
    INSERT INTO [tblUser] ([UserId], [Username], [Email], [PasswordHash], [FullName], [Phone], [Role], [IsActive], [FailedLoginCount], [CreatedAt])
    VALUES (2, 'nhatuyendung_hanoi', 'tuyendung.hn@fpt.com', 'AQAAAAIAAYagAAAAEMOA577yNRrWSENPKea5vrqZTNon8FEFTpAHz1IxApd2+fsatyFvUQ+sG374sPOoVw==', N'FPT Software HR', '0900000002', 'Employer', 1, 0, GETUTCDATE());
END;

IF NOT EXISTS (SELECT 1 FROM [tblUser] WHERE [Username] = 'nhatuyendung_hcm')
BEGIN
    INSERT INTO [tblUser] ([UserId], [Username], [Email], [PasswordHash], [FullName], [Phone], [Role], [IsActive], [FailedLoginCount], [CreatedAt])
    VALUES (3, 'nhatuyendung_hcm', 'recruitment@vng.com.vn', 'AQAAAAIAAYagAAAAEMOA577yNRrWSENPKea5vrqZTNon8FEFTpAHz1IxApd2+fsatyFvUQ+sG374sPOoVw==', N'VNG Talent Acquisition', '0900000003', 'Employer', 1, 0, GETUTCDATE());
END;

IF NOT EXISTS (SELECT 1 FROM [tblUser] WHERE [Username] = 'nhatuyendung_danang')
BEGIN
    INSERT INTO [tblUser] ([UserId], [Username], [Email], [PasswordHash], [FullName], [Phone], [Role], [IsActive], [FailedLoginCount], [CreatedAt])
    VALUES (4, 'nhatuyendung_danang', 'hr@enclave.vn', 'AQAAAAIAAYagAAAAEMOA577yNRrWSENPKea5vrqZTNon8FEFTpAHz1IxApd2+fsatyFvUQ+sG374sPOoVw==', N'Enclave IT HR Team', '0900000004', 'Employer', 1, 0, GETUTCDATE());
END;

IF NOT EXISTS (SELECT 1 FROM [tblUser] WHERE [Username] = 'ungvien_01')
BEGIN
    INSERT INTO [tblUser] ([UserId], [Username], [Email], [PasswordHash], [FullName], [Phone], [Role], [IsActive], [FailedLoginCount], [CreatedAt])
    VALUES (5, 'ungvien_01', 'ungvien01@gmail.com', 'AQAAAAIAAYagAAAAEMOA577yNRrWSENPKea5vrqZTNon8FEFTpAHz1IxApd2+fsatyFvUQ+sG374sPOoVw==', N'Nguyễn Văn Ứng Viên', '0900000005', 'Candidate', 1, 0, GETUTCDATE());
END;

SET IDENTITY_INSERT [tblUser] OFF;
GO

SET IDENTITY_INSERT [tblCompany] ON;

IF NOT EXISTS (SELECT 1 FROM [tblCompany] WHERE [Slug] = 'fpt-software')
BEGIN
    INSERT INTO [tblCompany] ([CompanyId], [UserId], [Name], [Slug], [Email], [Phone], [Address], [CompanySize], [Status], [CreatedAt])
    VALUES (1, 2, N'Công ty Cổ phần Phần mềm FPT (FPT Software)', 'fpt-software', 'tuyendung.hn@fpt.com', '02473007300', N'Tòa nhà FPT Cầu Giấy, Phố Duy Tân, Cầu Giấy, Hà Nội', N'Large', 'Approved', GETUTCDATE());
END;

IF NOT EXISTS (SELECT 1 FROM [tblCompany] WHERE [Slug] = 'vng-corporation')
BEGIN
    INSERT INTO [tblCompany] ([CompanyId], [UserId], [Name], [Slug], [Email], [Phone], [Address], [CompanySize], [Status], [CreatedAt])
    VALUES (2, 3, N'Công ty Cổ phần VNG (VNG Corporation)', 'vng-corporation', 'recruitment@vng.com.vn', '02839623888', N'Z06 Đường số 13, KCX Tân Thuận, Quận 7, TP.HCM', N'Large', 'Approved', GETUTCDATE());
END;

IF NOT EXISTS (SELECT 1 FROM [tblCompany] WHERE [Slug] = 'enclave-software')
BEGIN
    INSERT INTO [tblCompany] ([CompanyId], [UserId], [Name], [Slug], [Email], [Phone], [Address], [CompanySize], [Status], [CreatedAt])
    VALUES (3, 4, N'Công ty TNHH Giải pháp Phần mềm Enclave', 'enclave-software', 'hr@enclave.vn', '02363956888', N'453-455 Hoàng Diệu, Quận Hải Châu, Đà Nẵng', N'Medium', 'Approved', GETUTCDATE());
END;

SET IDENTITY_INSERT [tblCompany] OFF;
GO

PRINT N'>>> ĐÃ TẠO THÀNH CÔNG CSDL TIMVIECLAM CHUẨN RIÊNG CHO SPRINT 1! <<<';
GO
