using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimViecLam.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblBenefit",
                columns: table => new
                {
                    BenefitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBenefit", x => x.BenefitId);
                });

            migrationBuilder.CreateTable(
                name: "tblJobCategory",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblJobCategory", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "tblJobType",
                columns: table => new
                {
                    JobTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblJobType", x => x.JobTypeId);
                });

            migrationBuilder.CreateTable(
                name: "tblMenu",
                columns: table => new
                {
                    MenuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ParentMenuId = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OpenNewTab = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblMenu", x => x.MenuId);
                    table.ForeignKey(
                        name: "FK_tblMenu_tblMenu_ParentMenuId",
                        column: x => x.ParentMenuId,
                        principalTable: "tblMenu",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tblPost",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThumbnailImage = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    MetaTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPost", x => x.PostId);
                });

            migrationBuilder.CreateTable(
                name: "tblProvince",
                columns: table => new
                {
                    ProvinceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblProvince", x => x.ProvinceId);
                });

            migrationBuilder.CreateTable(
                name: "tblSkill",
                columns: table => new
                {
                    SkillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSkill", x => x.SkillId);
                });

            migrationBuilder.CreateTable(
                name: "tblUser",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginCount = table.Column<int>(type: "int", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblUser", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "tblDistrict",
                columns: table => new
                {
                    DistrictId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblDistrict", x => x.DistrictId);
                    table.ForeignKey(
                        name: "FK_tblDistrict_tblProvince_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "tblProvince",
                        principalColumn: "ProvinceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tblAuditLog",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblAuditLog", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_tblAuditLog_tblUser_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tblCandidateProfile",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Headline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DesiredPosition = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExpectedSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProvinceId = table.Column<int>(type: "int", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultCvPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCandidateProfile", x => x.ProfileId);
                    table.ForeignKey(
                        name: "FK_tblCandidateProfile_tblProvince_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "tblProvince",
                        principalColumn: "ProvinceId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tblCandidateProfile_tblUser_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblCompany",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TaxCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompanySize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RejectReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCompany", x => x.CompanyId);
                    table.ForeignKey(
                        name: "FK_tblCompany_tblUser_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tblJobAlert",
                columns: table => new
                {
                    AlertId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Keyword = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProvinceId = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    MinSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblJobAlert", x => x.AlertId);
                    table.ForeignKey(
                        name: "FK_tblJobAlert_tblJobCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "tblJobCategory",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tblJobAlert_tblProvince_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "tblProvince",
                        principalColumn: "ProvinceId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tblJobAlert_tblUser_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblWard",
                columns: table => new
                {
                    WardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblWard", x => x.WardId);
                    table.ForeignKey(
                        name: "FK_tblWard_tblDistrict_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "tblDistrict",
                        principalColumn: "DistrictId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tblWorkplace",
                columns: table => new
                {
                    WorkplaceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    WardId = table.Column<int>(type: "int", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    IsHeadOffice = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblWorkplace", x => x.WorkplaceId);
                    table.ForeignKey(
                        name: "FK_tblWorkplace_tblCompany_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "tblCompany",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tblWorkplace_tblDistrict_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "tblDistrict",
                        principalColumn: "DistrictId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tblWorkplace_tblProvince_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "tblProvince",
                        principalColumn: "ProvinceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tblWorkplace_tblWard_WardId",
                        column: x => x.WardId,
                        principalTable: "tblWard",
                        principalColumn: "WardId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tblJob",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    WorkplaceId = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    JobTypeId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Requirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SalaryMin = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SalaryMax = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SalaryUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsSalaryNegotiable = table.Column<bool>(type: "bit", nullable: false),
                    ExperienceLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EducationLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Headcount = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SearchText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblJob", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_tblJob_tblCompany_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "tblCompany",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tblJob_tblJobCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "tblJobCategory",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tblJob_tblJobType_JobTypeId",
                        column: x => x.JobTypeId,
                        principalTable: "tblJobType",
                        principalColumn: "JobTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tblJob_tblWorkplace_WorkplaceId",
                        column: x => x.WorkplaceId,
                        principalTable: "tblWorkplace",
                        principalColumn: "WorkplaceId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tblApplication",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    CandidateUserId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CoverLetter = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CvPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployerNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblApplication", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_tblApplication_tblJob_JobId",
                        column: x => x.JobId,
                        principalTable: "tblJob",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tblApplication_tblUser_CandidateUserId",
                        column: x => x.CandidateUserId,
                        principalTable: "tblUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tblJobBenefit",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "int", nullable: false),
                    BenefitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblJobBenefit", x => new { x.JobId, x.BenefitId });
                    table.ForeignKey(
                        name: "FK_tblJobBenefit_tblBenefit_BenefitId",
                        column: x => x.BenefitId,
                        principalTable: "tblBenefit",
                        principalColumn: "BenefitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tblJobBenefit_tblJob_JobId",
                        column: x => x.JobId,
                        principalTable: "tblJob",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblJobSkill",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblJobSkill", x => new { x.JobId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_tblJobSkill_tblJob_JobId",
                        column: x => x.JobId,
                        principalTable: "tblJob",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblJobSkill_tblSkill_SkillId",
                        column: x => x.SkillId,
                        principalTable: "tblSkill",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tblSavedJob",
                columns: table => new
                {
                    SavedJobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSavedJob", x => x.SavedJobId);
                    table.ForeignKey(
                        name: "FK_tblSavedJob_tblJob_JobId",
                        column: x => x.JobId,
                        principalTable: "tblJob",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblSavedJob_tblUser_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblApplication_CandidateUserId",
                table: "tblApplication",
                column: "CandidateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblApplication_JobId",
                table: "tblApplication",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_tblApplication_JobId_CandidateUserId",
                table: "tblApplication",
                columns: new[] { "JobId", "CandidateUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblAuditLog_CreatedAt",
                table: "tblAuditLog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_tblAuditLog_EntityName",
                table: "tblAuditLog",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_tblAuditLog_UserId",
                table: "tblAuditLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblBenefit_Name",
                table: "tblBenefit",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_tblCandidateProfile_ProvinceId",
                table: "tblCandidateProfile",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_tblCandidateProfile_UserId",
                table: "tblCandidateProfile",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblCompany_Slug",
                table: "tblCompany",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblCompany_Status",
                table: "tblCompany",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tblCompany_UserId",
                table: "tblCompany",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblDistrict_Code",
                table: "tblDistrict",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_tblDistrict_ProvinceId",
                table: "tblDistrict",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_tblDistrict_Slug",
                table: "tblDistrict",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_tblJob_ApprovalStatus_ExpiresAt",
                table: "tblJob",
                columns: new[] { "ApprovalStatus", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_tblJob_CategoryId",
                table: "tblJob",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_tblJob_CompanyId",
                table: "tblJob",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_tblJob_JobTypeId",
                table: "tblJob",
                column: "JobTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_tblJob_SalaryMin_SalaryMax",
                table: "tblJob",
                columns: new[] { "SalaryMin", "SalaryMax" });

            migrationBuilder.CreateIndex(
                name: "IX_tblJob_Slug",
                table: "tblJob",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_tblJob_WorkplaceId",
                table: "tblJob",
                column: "WorkplaceId");

            migrationBuilder.CreateIndex(
                name: "IX_tblJobAlert_CategoryId",
                table: "tblJobAlert",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_tblJobAlert_ProvinceId",
                table: "tblJobAlert",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_tblJobAlert_UserId",
                table: "tblJobAlert",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblJobBenefit_BenefitId",
                table: "tblJobBenefit",
                column: "BenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_tblJobCategory_Slug",
                table: "tblJobCategory",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblJobSkill_SkillId",
                table: "tblJobSkill",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_tblJobType_Code",
                table: "tblJobType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblMenu_ParentMenuId",
                table: "tblMenu",
                column: "ParentMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPost_IsPublished",
                table: "tblPost",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_tblPost_Slug",
                table: "tblPost",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblProvince_Code",
                table: "tblProvince",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblProvince_Slug",
                table: "tblProvince",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_tblSavedJob_JobId",
                table: "tblSavedJob",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_tblSavedJob_UserId_JobId",
                table: "tblSavedJob",
                columns: new[] { "UserId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblSkill_Slug",
                table: "tblSkill",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblUser_Email",
                table: "tblUser",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblUser_Role",
                table: "tblUser",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_tblUser_Username",
                table: "tblUser",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblWard_Code",
                table: "tblWard",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_tblWard_DistrictId",
                table: "tblWard",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_tblWard_Slug",
                table: "tblWard",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_tblWorkplace_CompanyId",
                table: "tblWorkplace",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_tblWorkplace_DistrictId",
                table: "tblWorkplace",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_tblWorkplace_Latitude_Longitude",
                table: "tblWorkplace",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_tblWorkplace_ProvinceId_DistrictId",
                table: "tblWorkplace",
                columns: new[] { "ProvinceId", "DistrictId" });

            migrationBuilder.CreateIndex(
                name: "IX_tblWorkplace_WardId",
                table: "tblWorkplace",
                column: "WardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblApplication");

            migrationBuilder.DropTable(
                name: "tblAuditLog");

            migrationBuilder.DropTable(
                name: "tblCandidateProfile");

            migrationBuilder.DropTable(
                name: "tblJobAlert");

            migrationBuilder.DropTable(
                name: "tblJobBenefit");

            migrationBuilder.DropTable(
                name: "tblJobSkill");

            migrationBuilder.DropTable(
                name: "tblMenu");

            migrationBuilder.DropTable(
                name: "tblPost");

            migrationBuilder.DropTable(
                name: "tblSavedJob");

            migrationBuilder.DropTable(
                name: "tblBenefit");

            migrationBuilder.DropTable(
                name: "tblSkill");

            migrationBuilder.DropTable(
                name: "tblJob");

            migrationBuilder.DropTable(
                name: "tblJobCategory");

            migrationBuilder.DropTable(
                name: "tblJobType");

            migrationBuilder.DropTable(
                name: "tblWorkplace");

            migrationBuilder.DropTable(
                name: "tblCompany");

            migrationBuilder.DropTable(
                name: "tblWard");

            migrationBuilder.DropTable(
                name: "tblUser");

            migrationBuilder.DropTable(
                name: "tblDistrict");

            migrationBuilder.DropTable(
                name: "tblProvince");
        }
    }
}
