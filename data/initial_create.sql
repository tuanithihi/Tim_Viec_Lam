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

