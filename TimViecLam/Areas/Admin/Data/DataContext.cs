using Microsoft.EntityFrameworkCore;
using TimViecLam.Models;

namespace TimViecLam.Areas.Admin.Data;

/// <summary>
/// DbContext trung tâm của sàn tuyển dụng TimViecLam.
/// Quản lý toàn bộ 20 thực thể, cấu hình Index, Unique, Cascade Delete và tương thích SQLite.
/// </summary>
public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    // ═══════════════════════════════════════════════════════════════
    //  DbSets  ─  Mỗi property = 1 bảng trong CSDL
    // ═══════════════════════════════════════════════════════════════

    public DbSet<tblUser> Users { get; set; } = null!;
    public DbSet<tblCompany> Companies { get; set; } = null!;
    public DbSet<tblWorkplace> Workplaces { get; set; } = null!;
    public DbSet<tblProvince> Provinces { get; set; } = null!;
    public DbSet<tblDistrict> Districts { get; set; } = null!;
    public DbSet<tblWard> Wards { get; set; } = null!;
    public DbSet<tblJobCategory> JobCategories { get; set; } = null!;
    public DbSet<tblJobType> JobTypes { get; set; } = null!;
    public DbSet<tblSkill> Skills { get; set; } = null!;
    public DbSet<tblJobSkill> JobSkills { get; set; } = null!;
    public DbSet<tblBenefit> Benefits { get; set; } = null!;
    public DbSet<tblJobBenefit> JobBenefits { get; set; } = null!;
    public DbSet<tblJob> Jobs { get; set; } = null!;
    public DbSet<tblCandidateProfile> CandidateProfiles { get; set; } = null!;
    public DbSet<tblApplication> Applications { get; set; } = null!;
    public DbSet<tblSavedJob> SavedJobs { get; set; } = null!;
    public DbSet<tblJobAlert> JobAlerts { get; set; } = null!;
    public DbSet<tblPost> Posts { get; set; } = null!;
    public DbSet<tblMenu> Menus { get; set; } = null!;
    public DbSet<tblAuditLog> AuditLogs { get; set; } = null!;

    // ═══════════════════════════════════════════════════════════════
    //  OnModelCreating  ─  Cấu hình quan hệ, index, chuyển đổi SQLite
    // ═══════════════════════════════════════════════════════════════

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Tương thích SQLite khi chạy unit test (chuyển decimal sang double) ──
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));
                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name).Property(property.Name).HasConversion<double>();
                }
            }
        }

        // ── tblUser ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<tblUser>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Role);
        });

        // ── tblCompany ──────────────────────────────────────────────────────────
        modelBuilder.Entity<tblCompany>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasIndex(c => c.UserId);
            entity.HasIndex(c => c.Status);

            entity.HasOne(c => c.User)
                  .WithOne(u => u.Company)
                  .HasForeignKey<tblCompany>(c => c.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── tblProvince ─────────────────────────────────────────────────────────
        modelBuilder.Entity<tblProvince>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.HasIndex(p => p.Slug);
        });

        // ── tblDistrict ─────────────────────────────────────────────────────────
        modelBuilder.Entity<tblDistrict>(entity =>
        {
            entity.HasIndex(d => d.Code);
            entity.HasIndex(d => d.Slug);
            entity.HasIndex(d => d.ProvinceId);

            entity.HasOne(d => d.Province)
                  .WithMany(p => p.Districts)
                  .HasForeignKey(d => d.ProvinceId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── tblWard ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<tblWard>(entity =>
        {
            entity.HasIndex(w => w.Code);
            entity.HasIndex(w => w.Slug);
            entity.HasIndex(w => w.DistrictId);

            entity.HasOne(w => w.District)
                  .WithMany(d => d.Wards)
                  .HasForeignKey(w => w.DistrictId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── tblWorkplace ────────────────────────────────────────────────────────
        modelBuilder.Entity<tblWorkplace>(entity =>
        {
            entity.HasIndex(w => new { w.Latitude, w.Longitude });
            entity.HasIndex(w => new { w.ProvinceId, w.DistrictId });

            entity.HasOne(w => w.Company)
                  .WithMany(c => c.Workplaces)
                  .HasForeignKey(w => w.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(w => w.Province)
                  .WithMany(p => p.Workplaces)
                  .HasForeignKey(w => w.ProvinceId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(w => w.District)
                  .WithMany(d => d.Workplaces)
                  .HasForeignKey(w => w.DistrictId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(w => w.Ward)
                  .WithMany(ward => ward.Workplaces)
                  .HasForeignKey(w => w.WardId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── tblJobCategory ──────────────────────────────────────────────────────
        modelBuilder.Entity<tblJobCategory>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();
        });

        // ── tblJobType ──────────────────────────────────────────────────────────
        modelBuilder.Entity<tblJobType>(entity =>
        {
            entity.HasIndex(t => t.Code).IsUnique();
        });

        // ── tblSkill ────────────────────────────────────────────────────────────
        modelBuilder.Entity<tblSkill>(entity =>
        {
            entity.HasIndex(s => s.Slug).IsUnique();
        });

        // ── tblJobSkill (Khóa chính kép) ────────────────────────────────────────
        modelBuilder.Entity<tblJobSkill>(entity =>
        {
            entity.HasKey(js => new { js.JobId, js.SkillId });

            entity.HasOne(js => js.Job)
                  .WithMany(j => j.JobSkills)
                  .HasForeignKey(js => js.JobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(js => js.Skill)
                  .WithMany(s => s.JobSkills)
                  .HasForeignKey(js => js.SkillId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── tblBenefit ──────────────────────────────────────────────────────────
        modelBuilder.Entity<tblBenefit>(entity =>
        {
            entity.HasIndex(b => b.Name);
        });

        // ── tblJobBenefit (Khóa chính kép) ──────────────────────────────────────
        modelBuilder.Entity<tblJobBenefit>(entity =>
        {
            entity.HasKey(jb => new { jb.JobId, jb.BenefitId });

            entity.HasOne(jb => jb.Job)
                  .WithMany(j => j.JobBenefits)
                  .HasForeignKey(jb => jb.JobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(jb => jb.Benefit)
                  .WithMany(b => b.JobBenefits)
                  .HasForeignKey(jb => jb.BenefitId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── tblJob ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<tblJob>(entity =>
        {
            entity.HasIndex(j => new { j.ApprovalStatus, j.ExpiresAt });
            entity.HasIndex(j => j.CategoryId);
            entity.HasIndex(j => j.JobTypeId);
            entity.HasIndex(j => j.CompanyId);
            entity.HasIndex(j => new { j.SalaryMin, j.SalaryMax });
            entity.HasIndex(j => j.Slug);

            entity.HasOne(j => j.Company)
                  .WithMany(c => c.Jobs)
                  .HasForeignKey(j => j.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(j => j.Workplace)
                  .WithMany(w => w.Jobs)
                  .HasForeignKey(j => j.WorkplaceId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(j => j.Category)
                  .WithMany(c => c.Jobs)
                  .HasForeignKey(j => j.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(j => j.JobType)
                  .WithMany(t => t.Jobs)
                  .HasForeignKey(j => j.JobTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── tblCandidateProfile (Quan hệ 1-1 với tblUser) ───────────────────────
        modelBuilder.Entity<tblCandidateProfile>(entity =>
        {
            entity.HasIndex(cp => cp.UserId).IsUnique();

            entity.HasOne(cp => cp.User)
                  .WithOne(u => u.CandidateProfile)
                  .HasForeignKey<tblCandidateProfile>(cp => cp.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cp => cp.Province)
                  .WithMany(p => p.CandidateProfiles)
                  .HasForeignKey(cp => cp.ProvinceId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── tblApplication (Unique: JobId + CandidateUserId) ───────────────────
        modelBuilder.Entity<tblApplication>(entity =>
        {
            entity.HasIndex(a => new { a.JobId, a.CandidateUserId }).IsUnique();
            entity.HasIndex(a => a.JobId);
            entity.HasIndex(a => a.CandidateUserId);

            entity.HasOne(a => a.Job)
                  .WithMany(j => j.Applications)
                  .HasForeignKey(a => a.JobId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.CandidateUser)
                  .WithMany(u => u.Applications)
                  .HasForeignKey(a => a.CandidateUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── tblSavedJob (Unique: UserId + JobId) ────────────────────────────────
        modelBuilder.Entity<tblSavedJob>(entity =>
        {
            entity.HasIndex(sj => new { sj.UserId, sj.JobId }).IsUnique();

            entity.HasOne(sj => sj.User)
                  .WithMany(u => u.SavedJobs)
                  .HasForeignKey(sj => sj.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sj => sj.Job)
                  .WithMany(j => j.SavedJobs)
                  .HasForeignKey(sj => sj.JobId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── tblJobAlert ─────────────────────────────────────────────────────────
        modelBuilder.Entity<tblJobAlert>(entity =>
        {
            entity.HasIndex(ja => ja.UserId);

            entity.HasOne(ja => ja.User)
                  .WithMany(u => u.JobAlerts)
                  .HasForeignKey(ja => ja.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ja => ja.Province)
                  .WithMany()
                  .HasForeignKey(ja => ja.ProvinceId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(ja => ja.Category)
                  .WithMany(c => c.JobAlerts)
                  .HasForeignKey(ja => ja.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── tblPost ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<tblPost>(entity =>
        {
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.HasIndex(p => p.IsPublished);
        });

        // ── tblMenu ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<tblMenu>(entity =>
        {
            entity.HasIndex(m => m.ParentMenuId);

            entity.HasOne(m => m.ParentMenu)
                  .WithMany(m => m.ChildMenus)
                  .HasForeignKey(m => m.ParentMenuId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── tblAuditLog ─────────────────────────────────────────────────────────
        modelBuilder.Entity<tblAuditLog>(entity =>
        {
            entity.HasIndex(al => al.UserId);
            entity.HasIndex(al => al.EntityName);
            entity.HasIndex(al => al.CreatedAt);

            entity.HasOne(al => al.User)
                  .WithMany(u => u.AuditLogs)
                  .HasForeignKey(al => al.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
