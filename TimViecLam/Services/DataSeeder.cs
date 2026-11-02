using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TimViecLam.Areas.Admin.Data;
using TimViecLam.Helpers;
using TimViecLam.Models;

namespace TimViecLam.Services;

/// <summary>
/// Trình nạp dữ liệu mẫu (Seeder) đảm bảo tính idempotent (chạy nhiều lần không trùng lặp, không ném ngoại lệ).
/// </summary>
public static class DataSeeder
{
    private class ProvinceDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<DistrictDto>? Districts { get; set; }
    }

    private class DistrictDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    private class WardDto
    {
        public string DistrictCode { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Chạy toàn bộ quy trình Seed theo thứ tự quan hệ phụ thuộc.
    /// </summary>
    public static async Task SeedAllAsync(DataContext context, IPasswordService passwordService, string? seedFolderPath = null)
    {
        await SeedAdministrativeDataAsync(context, seedFolderPath);
        await SeedJobCategoriesAsync(context);
        await SeedJobTypesAsync(context);
        await SeedSkillsAsync(context);
        await SeedBenefitsAsync(context);
        await SeedUsersAndCompaniesAsync(context, passwordService);
        await SeedJobsAsync(context);
        await SeedMenusAndPostsAsync(context);
    }

    /// <summary>
    /// 1. Seed dữ liệu hành chính: 63 Tỉnh/Thành phố, Quận/Huyện và Phường/Xã mẫu từ file JSON.
    /// Bổ sung cột Slug cho Province, District, Ward.
    /// </summary>
    public static async Task SeedAdministrativeDataAsync(DataContext context, string? seedFolderPath = null)
    {
        seedFolderPath ??= Path.Combine(AppContext.BaseDirectory, "Data", "Seed");

        if (!Directory.Exists(seedFolderPath))
        {
            var candidates = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "Data", "Seed"),
                Path.Combine(Directory.GetCurrentDirectory(), "data", "Seed"),
                Path.Combine(Directory.GetCurrentDirectory(), "TimViecLam", "Data", "Seed"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "QuanLyPhongTro", "data", "Seed")
            };
            foreach (var cand in candidates)
            {
                if (Directory.Exists(cand))
                {
                    seedFolderPath = cand;
                    break;
                }
            }
        }

        var provDistPath = Path.Combine(seedFolderPath, "provinces_districts.json");
        if (File.Exists(provDistPath))
        {
            var json = await File.ReadAllTextAsync(provDistPath);
            var provinces = JsonSerializer.Deserialize<List<ProvinceDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (provinces != null && provinces.Count > 0)
            {
                var existingProvinces = await context.Provinces
                    .Include(p => p.Districts)
                    .ToDictionaryAsync(p => p.Code);

                foreach (var pDto in provinces)
                {
                    if (!existingProvinces.TryGetValue(pDto.Code, out var province))
                    {
                        province = new tblProvince
                        {
                            Code = pDto.Code,
                            Name = pDto.Name,
                            FullName = pDto.FullName,
                            Slug = SlugHelper.Slugify(pDto.Name)
                        };
                        context.Provinces.Add(province);
                        await context.SaveChangesAsync();
                        existingProvinces[province.Code] = province;
                    }
                    else if (string.IsNullOrEmpty(province.Slug))
                    {
                        province.Slug = SlugHelper.Slugify(province.Name);
                    }

                    if (pDto.Districts != null && pDto.Districts.Count > 0)
                    {
                        var existingDistricts = await context.Districts
                            .Where(d => d.ProvinceId == province.ProvinceId)
                            .ToDictionaryAsync(d => d.Code);

                        foreach (var dDto in pDto.Districts)
                        {
                            if (!existingDistricts.TryGetValue(dDto.Code, out var district))
                            {
                                district = new tblDistrict
                                {
                                    ProvinceId = province.ProvinceId,
                                    Code = dDto.Code,
                                    Name = dDto.Name,
                                    FullName = dDto.FullName,
                                    Slug = SlugHelper.Slugify(dDto.Name)
                                };
                                context.Districts.Add(district);
                            }
                            else if (string.IsNullOrEmpty(district.Slug))
                            {
                                district.Slug = SlugHelper.Slugify(district.Name);
                            }
                        }
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        // Seed Wards mẫu
        var wardsPath = Path.Combine(seedFolderPath, "wards_sample.json");
        if (File.Exists(wardsPath))
        {
            var json = await File.ReadAllTextAsync(wardsPath);
            var wards = JsonSerializer.Deserialize<List<WardDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (wards != null && wards.Count > 0)
            {
                var districtCodes = wards.Select(w => w.DistrictCode).Distinct().ToList();
                var districts = await context.Districts
                    .Where(d => districtCodes.Contains(d.Code))
                    .ToDictionaryAsync(d => d.Code);

                var existingWards = await context.Wards
                    .ToDictionaryAsync(w => w.Code);

                foreach (var wDto in wards)
                {
                    if (districts.TryGetValue(wDto.DistrictCode, out var district))
                    {
                        if (!existingWards.TryGetValue(wDto.Code, out var ward))
                        {
                            ward = new tblWard
                            {
                                DistrictId = district.DistrictId,
                                Code = wDto.Code,
                                Name = wDto.Name,
                                FullName = wDto.FullName,
                                Slug = SlugHelper.Slugify(wDto.Name)
                            };
                            context.Wards.Add(ward);
                        }
                        else if (string.IsNullOrEmpty(ward.Slug))
                        {
                            ward.Slug = SlugHelper.Slugify(ward.Name);
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// 2. Seed danh mục ngành nghề (>= 15 ngành nghề phổ biến).
    /// </summary>
    public static async Task SeedJobCategoriesAsync(DataContext context)
    {
        var categories = new (string Name, string Slug, string Icon, string Desc, int Order)[]
        {
            ("Công nghệ thông tin / Phần mềm", "cong-nghe-thong-tin-phan-mem", "bi-laptop", "Lập trình viên, kỹ sư phần mềm, DevOps, QA/QC, AI/Data", 1),
            ("Marketing / Truyền thông / Quảng cáo", "marketing-truyen-thong-quang-cao", "bi-megaphone", "Digital Marketing, SEO, Content Creator, PR, Event", 2),
            ("Kế toán / Kiểm toán / Thuế", "ke-toan-kiem-toan-thue", "bi-calculator", "Kế toán tổng hợp, kế toán thuế, kiểm toán viên, tài chính", 3),
            ("Bán hàng / Kinh doanh / Phát triển thị trường", "kinh-doanh-ban-hang", "bi-graph-up-arrow", "Sales B2B, B2C, Quản lý kinh doanh, Account Manager", 4),
            ("Hành chính / Nhân sự / Tuyển dụng", "hanh-chinh-nhan-su", "bi-people", "Tuyển dụng, C&B, đào tạo nội bộ, hành chính văn phòng", 5),
            ("Tài chính / Ngân hàng / Chứng khoán", "tai-chinh-ngan-hang", "bi-bank", "Chuyên viên tín dụng, thẩm định giá, môi giới chứng khoán", 6),
            ("Xây dựng / Kiến trúc / Quy hoạch", "xay-dung-kien-truc", "bi-building", "Kỹ sư xây dựng, giám sát công trình, thiết kế kiến trúc", 7),
            ("Bất động sản / Quản lý dự án", "bat-dong-san", "bi-houses", "Môi giới BĐS, tư vấn đầu tư, quản lý tòa nhà", 8),
            ("Thiết kế đồ họa / Sáng tạo nghệ thuật", "thiet-ke-do-hoa", "bi-palette", "Graphic Designer, UI/UX Designer, Motion Graphic, 3D", 9),
            ("Giáo dục / Đào tạo / Giảng dạy", "giao-duc-dao-tao", "bi-mortarboard", "Giáo viên tiếng Anh, giảng viên, gia sư, cố vấn học tập", 10),
            ("Y tế / Dược phẩm / Chăm sóc sức khỏe", "y-te-duoc-pham", "bi-heart-pulse", "Bác sĩ, dược sĩ, điều dưỡng, trình dược viên", 11),
            ("Logistics / Xuất nhập khẩu / Chuỗi cung ứng", "logistics-xuat-nhap-khau", "bi-truck", "Nhân viên chứng từ, giao nhận hàng hải, quản lý kho bãi", 12),
            ("Khách sạn / Du lịch / Nhà hàng (F&B)", "khach-san-du-lich-nha-hang", "bi-cup-hot", "Quản lý nhà hàng, đầu bếp, lễ tân, hướng dẫn viên", 13),
            ("Sản xuất / Vận hành / Kỹ thuật cơ khí", "san-xuat-van-hanh-co-khi", "bi-gear", "Kỹ sư cơ khí, điện - điện tử, quản lý chất lượng QA", 14),
            ("Dịch vụ khách hàng / Chăm sóc khách hàng", "cham-soc-khach-hang", "bi-headset", "Tư vấn viên, hỗ trợ khách hàng, tổng đài viên", 15),
            ("Luật / Pháp lý / Tuân thủ", "luat-phap-ly", "bi-shield-check", "Chuyên viên pháp chế doanh nghiệp, luật sư tư vấn", 16)
        };

        var existingSlugs = await context.JobCategories.Select(c => c.Slug).ToListAsync();

        foreach (var item in categories)
        {
            if (!existingSlugs.Contains(item.Slug))
            {
                context.JobCategories.Add(new tblJobCategory
                {
                    Name = item.Name,
                    Slug = item.Slug,
                    Icon = item.Icon,
                    Description = item.Desc,
                    SortOrder = item.Order,
                    IsActive = true
                });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 3. Seed hình thức làm việc (5 hình thức chuẩn).
    /// </summary>
    public static async Task SeedJobTypesAsync(DataContext context)
    {
        var jobTypes = new (string Name, string Code, string Desc, int Order)[]
        {
            ("Toàn thời gian", "FullTime", "Làm việc đủ thời gian theo tiêu chuẩn doanh nghiệp (40-48h/tuần)", 1),
            ("Bán thời gian", "PartTime", "Làm việc theo ca hoặc theo giờ linh hoạt", 2),
            ("Thực tập", "Internship", "Dành cho sinh viên thực tập tích lũy kinh nghiệm", 3),
            ("Hợp đồng", "Contract", "Làm việc theo hợp đồng dự án ngắn hạn hoặc thời vụ", 4),
            ("Freelance", "Freelance", "Làm việc tự do theo gói công việc / bàn giao sản phẩm", 5)
        };

        var existingCodes = await context.JobTypes.Select(t => t.Code).ToListAsync();

        foreach (var item in jobTypes)
        {
            if (!existingCodes.Contains(item.Code))
            {
                context.JobTypes.Add(new tblJobType
                {
                    Name = item.Name,
                    Code = item.Code,
                    Description = item.Desc,
                    SortOrder = item.Order,
                    IsActive = true
                });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 4. Seed danh mục kỹ năng (>= 40 kỹ năng).
    /// </summary>
    public static async Task SeedSkillsAsync(DataContext context)
    {
        var skills = new (string Name, string Slug, string Desc)[]
        {
            ("C#", "csharp", "Ngôn ngữ lập trình C# hướng đối tượng"),
            (".NET Core", "dotnet-core", "Nền tảng .NET Core mã nguồn mở hiệu năng cao"),
            ("ASP.NET MVC", "aspnet-mvc", "Framework phát triển web MVC trên nền tảng .NET"),
            ("Java", "java", "Ngôn ngữ lập trình Java hướng đối tượng"),
            ("Spring Boot", "spring-boot", "Framework phát triển backend Java"),
            ("Python", "python", "Ngôn ngữ lập trình đa dụng Python"),
            ("Django", "django", "Web framework cấp cao cho Python"),
            ("FastAPI", "fastapi", "Framework API Python tốc độ cao"),
            ("JavaScript", "javascript", "Ngôn ngữ kịch bản web phổ biến"),
            ("TypeScript", "typescript", "Siêu tập hợp kiểu tĩnh của JavaScript"),
            ("React", "react", "Thư viện giao diện người dùng front-end của Meta"),
            ("Angular", "angular", "Framework phát triển web SPA của Google"),
            ("Vue.js", "vue-js", "Framework JavaScript tiến bộ xây dựng UI"),
            ("Node.js", "node-js", "Môi trường thực thi JavaScript phía máy chủ"),
            ("PHP", "php", "Ngôn ngữ kịch bản web mã nguồn mở"),
            ("Laravel", "laravel", "Framework PHP hàng đầu"),
            ("Golang", "golang", "Ngôn ngữ Go đồng thời cao của Google"),
            ("Docker", "docker", "Nền tảng đóng gói ứng dụng trong container"),
            ("Kubernetes", "kubernetes", "Hệ thống điều phối container K8s"),
            ("AWS", "aws", "Amazon Web Services điện toán đám mây"),
            ("Azure", "azure", "Microsoft Azure Cloud Platform"),
            ("GCP", "gcp", "Google Cloud Platform"),
            ("SQL Server", "sql-server", "Hệ quản trị CSDL quan hệ của Microsoft"),
            ("PostgreSQL", "postgresql", "Hệ quản trị CSDL quan hệ mã nguồn mở mạnh mẽ"),
            ("MySQL", "mysql", "Hệ quản trị CSDL quan hệ phổ biến"),
            ("MongoDB", "mongodb", "CSDL NoSQL dạng tài liệu"),
            ("Redis", "redis", "Kho lưu trữ dữ liệu in-memory tốc độ cực nhanh"),
            ("Git", "git", "Hệ thống quản lý phiên bản phân tán"),
            ("CI/CD", "ci-cd", "Tích hợp và phân phối liên tục DevOps"),
            ("Microservices", "microservices", "Kiến trúc dịch vụ vi mô"),
            ("HTML/CSS", "html-css", "Ngôn ngữ đánh dấu và tạo kiểu trang web"),
            ("Tailwind CSS", "tailwind-css", "Framework CSS tiện ích"),
            ("UI/UX Design", "ui-ux-design", "Thiết kế trải nghiệm và giao diện người dùng"),
            ("Figma", "figma", "Công cụ thiết kế UI/UX cộng tác trực tuyến"),
            ("Linux", "linux", "Hệ điều hành máy chủ Linux"),
            ("Agile/Scrum", "agile-scrum", "Mô hình quản lý dự án linh hoạt"),
            ("Data Analysis", "data-analysis", "Phân tích và trực quan hóa dữ liệu kinh doanh"),
            ("Machine Learning", "machine-learning", "Học máy và mô hình trí tuệ nhân tạo"),
            ("SEO", "seo", "Tối ưu hóa công cụ tìm kiếm"),
            ("Content Marketing", "content-marketing", "Sáng tạo nội dung tiếp thị giá trị"),
            ("Sales B2B", "sales-b2b", "Kỹ năng bán hàng doanh nghiệp"),
            ("Tiếng Anh giao tiếp", "tieng-anh-giao-tiep", "Năng lực giao tiếp tiếng Anh lưu loát trong công việc")
        };

        var existingSlugs = await context.Skills.Select(s => s.Slug).ToListAsync();

        foreach (var item in skills)
        {
            if (!existingSlugs.Contains(item.Slug))
            {
                context.Skills.Add(new tblSkill
                {
                    Name = item.Name,
                    Slug = item.Slug,
                    Description = item.Desc,
                    IsActive = true
                });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 5. Seed danh mục phúc lợi đãi ngộ (>= 10 phúc lợi).
    /// </summary>
    public static async Task SeedBenefitsAsync(DataContext context)
    {
        var benefits = new (string Name, string Icon, string Desc)[]
        {
            ("Bảo hiểm sức khỏe cao cấp", "bi-shield-plus", "Gói bảo hiểm sức khỏe Bảo Việt / PVI toàn diện cho nhân viên và người thân"),
            ("Thưởng tháng 13 & thưởng hiệu suất KPI", "bi-cash-coin", "Thưởng tháng 13 cố định và thưởng kinh doanh theo hiệu quả quý/năm"),
            ("Du lịch / Teambuilding hàng năm", "bi-airplane", "Chuyến du lịch nghỉ dưỡng cao cấp hàng năm trong và ngoài nước"),
            ("Cung cấp laptop cao cấp", "bi-laptop", "Trang bị MacBook Pro hoặc Dell XPS đời mới cùng màn hình phụ 27 inch"),
            ("Làm việc linh hoạt & Làm việc từ xa (WFH)", "bi-house-check", "Hỗ trợ làm việc kết hợp linh hoạt 2 ngày WFH/tuần"),
            ("Phụ cấp ăn trưa, gửi xe và điện thoại", "bi-wallet2", "Trợ cấp ăn trưa 1.500.000 VNĐ/tháng và miễn phí gửi xe tại tòa nhà"),
            ("Khám sức khỏe định kỳ hàng năm", "bi-heart-pulse", "Khám sức khỏe tổng quát định kỳ tại các bệnh viện quốc tế"),
            ("Đào tạo nội bộ & tài trợ chứng chỉ quốc tế", "bi-mortarboard", "Tài trợ 100% chi phí thi các chứng chỉ chuyên ngành quốc tế (AWS, Azure, PMP...)"),
            ("Nghỉ phép 15 ngày/năm", "bi-calendar2-check", "Số ngày nghỉ phép hàng năm từ 15 ngày trở lên có hưởng lương"),
            ("Phòng gym, trà cà phê & đồ ăn nhẹ miễn phí", "bi-cup-hot", "Pantry đầy đủ trà, cafe chất lượng cao, trái cây và snack ăn vặt"),
            ("Đánh giá tăng lương 2 lần/năm", "bi-graph-up", "Kỳ xem xét đánh giá hiệu suất và điều chỉnh lương 6 tháng một lần")
        };

        var existingNames = await context.Benefits.Select(b => b.Name).ToListAsync();

        foreach (var item in benefits)
        {
            if (!existingNames.Contains(item.Name))
            {
                context.Benefits.Add(new tblBenefit
                {
                    Name = item.Name,
                    Icon = item.Icon,
                    Description = item.Desc,
                    IsActive = true
                });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 6. Seed tài khoản demo (băm mật khẩu bằng PBKDF2 salt), công ty đã duyệt, địa điểm làm việc tọa độ thật.
    /// </summary>
    public static async Task SeedUsersAndCompaniesAsync(DataContext context, IPasswordService passwordService)
    {
        // ── Danh sách tài khoản demo ──────────────────────────────────────────
        // Mật khẩu demo: Demo@123456 (đáp ứng tiêu chuẩn chữ hoa, thường, số, ký tự đặc biệt)
        var defaultPasswordHash = passwordService.HashPassword("Demo@123456");

        var demoUsers = new (string Username, string Email, string Role, string FullName, string Phone)[]
        {
            ("admin", "admin@timvieclam.vn", "SuperAdmin", "Quản Trị Viên Sàn", "0900000001"),
            ("nhatuyendung_hanoi", "tuyendung.hn@fpt.com", "Employer", "FPT Software HR", "0900000002"),
            ("nhatuyendung_hcm", "recruitment@vng.com.vn", "Employer", "VNG Talent Acquisition", "0900000003"),
            ("nhatuyendung_danang", "hr@enclave.vn", "Employer", "Enclave IT HR Team", "0900000004"),
            ("ungvien_01", "ungvien01@gmail.com", "Candidate", "Nguyễn Văn Ứng Viên", "0900000005")
        };

        foreach (var u in demoUsers)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Username == u.Username || x.Email == u.Email);
            if (user == null)
            {
                user = new tblUser
                {
                    Username = u.Username,
                    Email = u.Email,
                    PasswordHash = defaultPasswordHash,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Role = u.Role,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(user);
            }
            else if (user.PasswordHash.Contains(':'))
            {
                // Nâng cấp mật khẩu sang định dạng PasswordHasher mới
                user.PasswordHash = passwordService.HashPassword("Demo@123456");
            }
        }
        await context.SaveChangesAsync();

        // ── Seed Candidate Profile cho ungvien_01 ──────────────────────────────
        var candidate = await context.Users.FirstAsync(u => u.Username == "ungvien_01");
        var hanoi = await context.Provinces.FirstOrDefaultAsync(p => p.Code == "01" || p.Name.Contains("Hà Nội"));

        var profile = await context.CandidateProfiles.FirstOrDefaultAsync(cp => cp.UserId == candidate.UserId);
        if (profile == null)
        {
            context.CandidateProfiles.Add(new tblCandidateProfile
            {
                UserId = candidate.UserId,
                Headline = "Senior .NET Core / Fullstack Engineer",
                DesiredPosition = "Kỹ sư phần mềm .NET Senior",
                ExpectedSalary = 35000000,
                ProvinceId = hanoi?.ProvinceId,
                Summary = "Hơn 5 năm kinh nghiệm phát triển hệ thống web doanh nghiệp với ASP.NET Core, SQL Server, Redis, Docker và kiến trúc Microservices.",
                DefaultCvPath = "/uploads/cvs/cv_nguyen_van_a.pdf"
            });
            await context.SaveChangesAsync();
        }

        // ── Seed 3 Công ty đã duyệt (Approved) ─────────────────────────────────
        var userHn = await context.Users.FirstAsync(u => u.Username == "nhatuyendung_hanoi");
        var userHcm = await context.Users.FirstAsync(u => u.Username == "nhatuyendung_hcm");
        var userDn = await context.Users.FirstAsync(u => u.Username == "nhatuyendung_danang");

        var companies = new (int UserId, string Name, string Slug, string Tax, string Size, string Phone, string Email, string Addr)[]
        {
            (userHn.UserId, "Công ty Cổ phần Phần mềm FPT (FPT Software)", "fpt-software", "0101778163", "Enterprise", "02473007575", "contact@fsoft.com.vn", "Tòa nhà FPT, Số 10 Phạm Văn Bạch, Cầu Giấy, Hà Nội"),
            (userHcm.UserId, "Công ty Cổ phần VNG (VNG Corporation)", "vng-corporation", "0303538466", "Enterprise", "02839623888", "careers@vng.com.vn", "Z06 Đường số 13, Khu chế xuất Tân Thuận, Phường Tân Thuận Đông, Quận 7, TP.HCM"),
            (userDn.UserId, "Công ty TNHH Giải pháp Phần mềm Enclave", "enclave-software", "0400591244", "Medium", "02363574175", "contact@enclave.vn", "Tòa nhà Enclave, 453-455 Hoàng Diệu, Quận Hải Châu, TP. Đà Nẵng")
        };

        foreach (var c in companies)
        {
            var company = await context.Companies.FirstOrDefaultAsync(x => x.Slug == c.Slug);
            if (company == null)
            {
                company = new tblCompany
                {
                    UserId = c.UserId,
                    Name = c.Name,
                    Slug = c.Slug,
                    TaxCode = c.Tax,
                    CompanySize = c.Size,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Addr,
                    Status = "Approved",
                    Description = $"{c.Name} là tập đoàn công nghệ hàng đầu, môi trường làm việc chuẩn quốc tế.",
                    Website = "https://" + c.Slug + ".com",
                    CreatedAt = DateTime.Now
                };
                context.Companies.Add(company);
            }
        }
        await context.SaveChangesAsync();

        // ── Seed 6 Địa điểm làm việc có tọa độ thật tại Hà Nội, TP.HCM, Đà Nẵng ─
        var companyFpt = await context.Companies.FirstAsync(c => c.Slug == "fpt-software");
        var companyVng = await context.Companies.FirstAsync(c => c.Slug == "vng-corporation");
        var companyEnclave = await context.Companies.FirstAsync(c => c.Slug == "enclave-software");

        var pCauGiay = await context.Districts.FirstOrDefaultAsync(d => d.Name.Contains("Cầu Giấy"))
                       ?? await context.Districts.FirstOrDefaultAsync();
        var pNamTuLiem = await context.Districts.FirstOrDefaultAsync(d => d.Name.Contains("Nam Từ Liêm"))
                         ?? pCauGiay;
        var pQ7 = await context.Districts.FirstOrDefaultAsync(d => d.Name.Contains("Quận 7"))
                  ?? pCauGiay;
        var pQ1 = await context.Districts.FirstOrDefaultAsync(d => d.Name.Contains("Quận 1"))
                  ?? pCauGiay;
        var pHaiChau = await context.Districts.FirstOrDefaultAsync(d => d.Name.Contains("Hải Châu"))
                       ?? pCauGiay;
        var pNguHanhSon = await context.Districts.FirstOrDefaultAsync(d => d.Name.Contains("Ngũ Hành Sơn"))
                          ?? pHaiChau;

        var workplaces = new (int CompId, string Name, string Addr, int ProvId, int DistId, double Lat, double Lng, bool IsHead)[]
        {
            // Hà Nội (Lat 21.0, Lng 105.8)
            (companyFpt.CompanyId, "FPT Tower Cầu Giấy (Trụ sở chính)", "Số 10 Phạm Văn Bạch, Cầu Giấy, Hà Nội", pCauGiay!.ProvinceId, pCauGiay.DistrictId, 21.028511, 105.789123, true),
            (companyFpt.CompanyId, "FPT Software Keangnam Landmark", "Tầng 12 Keangnam Landmark 72, Phạm Hùng, Nam Từ Liêm, Hà Nội", pNamTuLiem!.ProvinceId, pNamTuLiem.DistrictId, 21.017342, 105.783921, false),

            // TP.HCM (Lat 10.7, Lng 106.7)
            (companyVng.CompanyId, "VNG Campus Tân Thuận (Trụ sở chính)", "Z06 Đường số 13, Khu chế xuất Tân Thuận, Quận 7, TP.HCM", pQ7!.ProvinceId, pQ7.DistrictId, 10.749123, 106.721456, true),
            (companyVng.CompanyId, "VNG Văn phòng Bitexco", "Tầng 25 Bitexco Financial Tower, 2 Hải Triều, Quận 1, TP.HCM", pQ1!.ProvinceId, pQ1.DistrictId, 10.771654, 106.704321, false),

            // Đà Nẵng (Lat 16.0, Lng 108.2)
            (companyEnclave.CompanyId, "Enclave Building Hải Châu (Trụ sở chính)", "453-455 Hoàng Diệu, Hải Châu, Đà Nẵng", pHaiChau!.ProvinceId, pHaiChau.DistrictId, 16.054321, 108.216543, true),
            (companyEnclave.CompanyId, "Chi nhánh FPT Complex Đà Nẵng", "Khu đô thị công nghệ FPT Đà Nẵng, Ngũ Hành Sơn, Đà Nẵng", pNguHanhSon!.ProvinceId, pNguHanhSon.DistrictId, 15.987654, 108.254321, false)
        };

        foreach (var wp in workplaces)
        {
            var exists = await context.Workplaces.AnyAsync(w => w.CompanyId == wp.CompId && w.Name == wp.Name);
            if (!exists)
            {
                context.Workplaces.Add(new tblWorkplace
                {
                    CompanyId = wp.CompId,
                    Name = wp.Name,
                    Address = wp.Addr,
                    ProvinceId = wp.ProvId,
                    DistrictId = wp.DistId,
                    Latitude = wp.Lat,
                    Longitude = wp.Lng,
                    IsHeadOffice = wp.IsHead,
                    CreatedAt = DateTime.Now
                });
            }
        }
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 7. Seed >= 40 việc làm Published đa dạng (lương, ngành, hình thức, kinh nghiệm, có tin remote, sắp hết hạn, pending, rejected).
    /// </summary>
    public static async Task SeedJobsAsync(DataContext context)
    {
        var existingCount = await context.Jobs.CountAsync();
        if (existingCount >= 45)
        {
            return; // Đã đủ dữ liệu, bảo đảm idempotent
        }

        var companies = await context.Companies.Include(c => c.Workplaces).ToListAsync();
        var categories = await context.JobCategories.ToListAsync();
        var jobTypes = await context.JobTypes.ToListAsync();
        var skills = await context.Skills.ToListAsync();
        var benefits = await context.Benefits.ToListAsync();

        if (companies.Count == 0 || categories.Count == 0 || jobTypes.Count == 0)
        {
            return;
        }

        var fpt = companies.First(c => c.Slug == "fpt-software");
        var vng = companies.First(c => c.Slug == "vng-corporation");
        var enclave = companies.First(c => c.Slug == "enclave-software");

        var itCat = categories.First(c => c.Slug == "cong-nghe-thong-tin-phan-mem");
        var mktCat = categories.FirstOrDefault(c => c.Slug.Contains("marketing")) ?? categories[1];
        var salesCat = categories.FirstOrDefault(c => c.Slug.Contains("kinh-doanh")) ?? categories[2];
        var accCat = categories.FirstOrDefault(c => c.Slug.Contains("ke-toan")) ?? categories[3];
        var hrCat = categories.FirstOrDefault(c => c.Slug.Contains("nhan-su")) ?? categories[4];
        var finCat = categories.FirstOrDefault(c => c.Slug.Contains("tai-chinh")) ?? categories[5];
        var desCat = categories.FirstOrDefault(c => c.Slug.Contains("thiet-ke")) ?? categories[6];
        var csCat = categories.FirstOrDefault(c => c.Slug.Contains("cham-soc")) ?? categories[7];

        var fullTime = jobTypes.First(t => t.Code == "FullTime");
        var partTime = jobTypes.FirstOrDefault(t => t.Code == "PartTime") ?? fullTime;
        var internship = jobTypes.FirstOrDefault(t => t.Code == "Internship") ?? fullTime;
        var contract = jobTypes.FirstOrDefault(t => t.Code == "Contract") ?? fullTime;
        var freelance = jobTypes.FirstOrDefault(t => t.Code == "Freelance") ?? fullTime;

        var sampleJobs = new List<tblJob>();

        void AddJob(
            tblCompany comp,
            tblWorkplace? wp,
            tblJobCategory cat,
            tblJobType type,
            string title,
            string slug,
            decimal? minSalary,
            decimal? maxSalary,
            bool negotiable,
            string exp,
            string edu,
            string mode,
            DateTime expires,
            string status,
            bool isFeatured,
            string[] skillSlugs)
        {
            var compName = comp.Name;
            var wpAddr = wp != null ? wp.Address : "Làm việc từ xa (Remote)";
            var skillNames = string.Join(" ", skills.Where(s => skillSlugs.Contains(s.Slug)).Select(s => s.Name));
            var searchText = SearchTextBuilder.BuildFromParts(title, compName, skillNames, wpAddr);

            var job = new tblJob
            {
                CompanyId = comp.CompanyId,
                WorkplaceId = wp?.WorkplaceId,
                CategoryId = cat.CategoryId,
                JobTypeId = type.JobTypeId,
                Title = title,
                Slug = slug,
                Description = $"<p>Chúng tôi đang tìm kiếm vị trí <strong>{title}</strong> đồng hành phát triển cùng công ty.</p><ul><li>Tham gia thiết kế, phát triển và tối ưu hóa hệ thống.</li><li>Phối hợp cùng team chuyên nghiệp, áp dụng quy trình chuẩn Agile/Scrum.</li></ul>",
                Requirements = "<ul><li>Có tinh thần trách nhiệm cao, ham học hỏi công nghệ mới.</li><li>Kỹ năng giao tiếp và làm việc nhóm tốt.</li></ul>",
                SalaryMin = minSalary,
                SalaryMax = maxSalary,
                SalaryUnit = "VND",
                IsSalaryNegotiable = negotiable,
                ExperienceLevel = exp,
                EducationLevel = edu,
                WorkMode = mode,
                Headcount = 2,
                ExpiresAt = expires,
                ApprovalStatus = status,
                IsFeatured = isFeatured,
                ViewCount = 10,
                PublishedAt = status == "Published" ? DateTime.Now.AddDays(-5) : null,
                SearchText = searchText,
                CreatedAt = DateTime.Now
            };

            // Gán skills liên kết
            foreach (var skSlug in skillSlugs)
            {
                var skill = skills.FirstOrDefault(s => s.Slug == skSlug);
                if (skill != null)
                {
                    job.JobSkills.Add(new tblJobSkill { Skill = skill });
                }
            }

            // Gán 3-4 benefits ngẫu nhiên
            for (int i = 0; i < Math.Min(3, benefits.Count); i++)
            {
                job.JobBenefits.Add(new tblJobBenefit { Benefit = benefits[i] });
            }

            sampleJobs.Add(job);
        }

        var wpFptHn = fpt.Workplaces.FirstOrDefault(w => w.Name.Contains("FPT Tower"));
        var wpFptKeangnam = fpt.Workplaces.FirstOrDefault(w => w.Name.Contains("Keangnam"));
        var wpVngQ7 = vng.Workplaces.FirstOrDefault(w => w.Name.Contains("VNG Campus"));
        var wpVngBitexco = vng.Workplaces.FirstOrDefault(w => w.Name.Contains("Bitexco"));
        var wpEnclaveHc = enclave.Workplaces.FirstOrDefault(w => w.Name.Contains("Enclave"));
        var wpEnclaveComplex = enclave.Workplaces.FirstOrDefault(w => w.Name.Contains("Complex"));

        DateTime future30 = DateTime.Now.AddDays(30);
        DateTime future60 = DateTime.Now.AddDays(60);
        DateTime future2 = DateTime.Now.AddDays(2); // Sắp hết hạn
        DateTime past10 = DateTime.Now.AddDays(-10); // Đã hết hạn

        // ── 1 -> 15: Tin Công nghệ thông tin (IT / Phần mềm) ─────────────────
        AddJob(fpt, wpFptHn, itCat, fullTime, "Senior .NET Core Backend Engineer", "senior-dotnet-core-backend-fpt", 30000000, 50000000, false, "2-5 năm", "Đại học", "OnSite", future30, "Published", true, new[] { "csharp", "dotnet-core", "sql-server", "docker" });
        AddJob(fpt, wpFptKeangnam, itCat, fullTime, "Lập trình viên ASP.NET MVC / C#", "lap-trinh-vien-aspnet-mvc-fpt", 18000000, 28000000, false, "1-2 năm", "Cao đẳng", "Hybrid", future30, "Published", false, new[] { "csharp", "aspnet-mvc", "sql-server" });
        AddJob(fpt, null, itCat, fullTime, "Chuyên gia Cloud AWS / DevOps Engineer (Remote)", "cloud-aws-devops-engineer-remote-fpt", 40000000, 65000000, false, "Trên 5 năm", "Đại học", "Remote", future30, "Published", true, new[] { "aws", "docker", "kubernetes", "ci-cd" });
        AddJob(fpt, wpFptHn, itCat, internship, "Thực tập sinh Lập trình .NET (Có trợ cấp)", "thuc-tap-sinh-lap-trinh-dotnet-fpt", 5000000, 8000000, false, "Không yêu cầu", "Đại học", "OnSite", future30, "Published", false, new[] { "csharp", "sql-server" });
        AddJob(fpt, wpFptKeangnam, itCat, fullTime, "Frontend Developer (React / TypeScript)", "frontend-developer-react-fpt", 22000000, 38000000, false, "2-5 năm", "Đại học", "Hybrid", future2, "Published", false, new[] { "react", "typescript", "javascript" }); // Sắp hết hạn

        AddJob(vng, wpVngQ7, itCat, fullTime, "Senior Java Backend Engineer (ZaloPay)", "senior-java-backend-vng", 35000000, 60000000, false, "2-5 năm", "Đại học", "OnSite", future30, "Published", true, new[] { "java", "spring-boot", "microservices", "redis" });
        AddJob(vng, wpVngBitexco, itCat, fullTime, "Chuyên viên Phân tích Dữ liệu (Data Analyst)", "data-analyst-vng", 25000000, 45000000, false, "1-2 năm", "Đại học", "Hybrid", future60, "Published", false, new[] { "python", "data-analysis", "sql-server" });
        AddJob(vng, null, itCat, fullTime, "AI / Machine Learning Research Engineer (Remote)", "ai-machine-learning-engineer-remote-vng", 45000000, 80000000, false, "2-5 năm", "Sau đại học", "Remote", future30, "Published", true, new[] { "python", "machine-learning", "fastapi" });
        AddJob(vng, wpVngQ7, itCat, contract, "Golang Microservices Developer (Contract)", "golang-microservices-developer-vng", 30000000, 50000000, false, "2-5 năm", "Đại học", "OnSite", future30, "Published", false, new[] { "golang", "docker", "microservices" });
        AddJob(vng, wpVngBitexco, itCat, fullTime, "Kỹ sư Cầu nối tiếng Nhật / Tiếng Anh (BrSE)", "ky-su-cau-noi-brse-vng", 30000000, 55000000, false, "2-5 năm", "Đại học", "OnSite", future30, "Published", false, new[] { "tieng-anh-giao-tiep", "agile-scrum" });

        AddJob(enclave, wpEnclaveHc, itCat, fullTime, "Fullstack Developer (Node.js & Vue.js)", "fullstack-developer-node-vue-enclave", 20000000, 35000000, false, "1-2 năm", "Đại học", "OnSite", future30, "Published", false, new[] { "node-js", "vue-js", "javascript" });
        AddJob(enclave, wpEnclaveComplex, itCat, fullTime, "Kỹ sư QA/QC Automation Tester", "qa-automation-tester-enclave", 18000000, 30000000, false, "1-2 năm", "Đại học", "Hybrid", future30, "Published", false, new[] { "python", "ci-cd" });
        AddJob(enclave, null, itCat, freelance, "Lập trình viên PHP / Laravel (Freelance)", "php-laravel-freelance-enclave", null, null, true, "1-2 năm", "Không yêu cầu", "Remote", future30, "Published", false, new[] { "php", "laravel", "mysql" }); // Thỏa thuận
        AddJob(enclave, wpEnclaveHc, itCat, fullTime, "Scrum Master / Project Manager", "scrum-master-pm-enclave", 35000000, 55000000, false, "Trên 5 năm", "Đại học", "OnSite", future30, "Published", true, new[] { "agile-scrum", "tieng-anh-giao-tiep" });
        AddJob(enclave, wpEnclaveComplex, itCat, partTime, "Lập trình viên Mobile Flutter / React Native", "mobile-developer-react-native-enclave", 15000000, 25000000, false, "1-2 năm", "Đại học", "Hybrid", future30, "Published", false, new[] { "react", "typescript" });

        // ── 16 -> 25: Tin Marketing & Truyền thông ───────────────────────────
        AddJob(vng, wpVngQ7, mktCat, fullTime, "Trưởng nhóm Digital Marketing (Team Lead)", "digital-marketing-team-lead-vng", 25000000, 40000000, false, "2-5 năm", "Đại học", "OnSite", future30, "Published", true, new[] { "seo", "content-marketing" });
        AddJob(vng, wpVngBitexco, mktCat, fullTime, "Chuyên viên Tối ưu hóa SEO Website", "chuyen-vien-seo-website-vng", 15000000, 22000000, false, "1-2 năm", "Cao đẳng", "Hybrid", future30, "Published", false, new[] { "seo", "content-marketing" });
        AddJob(fpt, wpFptHn, mktCat, fullTime, "Chuyên viên Quan hệ Công chúng & Báo chí (PR)", "chuyen-vien-pr-bao-chi-fpt", 18000000, 28000000, false, "1-2 năm", "Đại học", "OnSite", future30, "Published", false, new[] { "content-marketing", "tieng-anh-giao-tiep" });
        AddJob(fpt, null, mktCat, freelance, "Content Creator / Copywriter (Làm từ xa)", "content-creator-copywriter-remote-fpt", 10000000, 18000000, false, "Dưới 1 năm", "Không yêu cầu", "Remote", future30, "Published", false, new[] { "content-marketing" });
        AddJob(enclave, wpEnclaveHc, mktCat, fullTime, "Chuyên viên Social Media Marketing", "social-media-marketing-enclave", 12000000, 20000000, false, "1-2 năm", "Đại học", "Hybrid", future2, "Published", false, new[] { "content-marketing" }); // Sắp hết hạn

        // ── 26 -> 32: Tin Bán hàng & Kinh doanh ──────────────────────────────
        AddJob(fpt, wpFptHn, salesCat, fullTime, "Giám đốc Phát triển Kinh doanh B2B (BD Manager)", "bd-manager-b2b-fpt", 40000000, 70000000, false, "Trên 5 năm", "Đại học", "OnSite", future30, "Published", true, new[] { "sales-b2b", "tieng-anh-giao-tiep" });
        AddJob(fpt, wpFptKeangnam, salesCat, fullTime, "Chuyên viên Kinh doanh Giải pháp Phần mềm B2B", "sales-b2b-giai-phap-phan-mem-fpt", 18000000, 35000000, false, "1-2 năm", "Đại học", "OnSite", future30, "Published", false, new[] { "sales-b2b" });
        AddJob(vng, wpVngQ7, salesCat, fullTime, "Account Executive (Dịch vụ Đám mây VNG Cloud)", "account-executive-vng-cloud", 20000000, 40000000, false, "1-2 năm", "Đại học", "Hybrid", future30, "Published", false, new[] { "sales-b2b" });
        AddJob(enclave, wpEnclaveHc, salesCat, fullTime, "Nhân viên Tư vấn Dịch vụ CNTT Khách hàng Quốc tế", "tu-van-dich-vu-cntt-quoc-te-enclave", 20000000, 35000000, false, "1-2 năm", "Đại học", "OnSite", future30, "Published", false, new[] { "sales-b2b", "tieng-anh-giao-tiep" });

        // ── 33 -> 38: Tin Kế toán & Tài chính ────────────────────────────────
        AddJob(fpt, wpFptHn, accCat, fullTime, "Kế toán trưởng Doanh nghiệp CNTT", "ke-toan-truong-fpt-software", 35000000, 50000000, false, "Trên 5 năm", "Đại học", "OnSite", future30, "Published", true, new string[] { });
        AddJob(fpt, wpFptKeangnam, accCat, fullTime, "Kế toán Tổng hợp / Kế toán Thuế", "ke-toan-tong-hop-thue-fpt", 15000000, 22000000, false, "1-2 năm", "Đại học", "OnSite", future30, "Published", false, new string[] { });
        AddJob(vng, wpVngBitexco, finCat, fullTime, "Chuyên viên Phân tích Tài chính Doanh nghiệp (FP&A)", "chuyen-vien-tai-chinh-fpa-vng", 25000000, 40000000, false, "2-5 năm", "Đại học", "Hybrid", future30, "Published", false, new[] { "data-analysis" });

        // ── 39 -> 44: Tin Thiết kế đồ họa & UI/UX ───────────────────────────
        AddJob(vng, wpVngQ7, desCat, fullTime, "Lead UI/UX Product Designer", "lead-ui-ux-product-designer-vng", 35000000, 55000000, false, "2-5 năm", "Đại học", "Hybrid", future30, "Published", true, new[] { "ui-ux-design", "figma" });
        AddJob(vng, wpVngBitexco, desCat, fullTime, "Junior UI/UX Designer", "junior-ui-ux-designer-vng", 14000000, 22000000, false, "Dưới 1 năm", "Cao đẳng", "OnSite", future30, "Published", false, new[] { "ui-ux-design", "figma" });
        AddJob(enclave, wpEnclaveHc, desCat, freelance, "2D/3D Graphic Designer (Dự án Game)", "graphic-designer-game-enclave", null, null, true, "1-2 năm", "Không yêu cầu", "Remote", future30, "Published", false, new[] { "ui-ux-design" });

        // ── 45 -> 48: Tin Nhân sự & CSKH ─────────────────────────────────────
        AddJob(fpt, wpFptHn, hrCat, fullTime, "Chuyên viên Tuyển dụng IT (IT Recruiter)", "chuyen-vien-tuyen-dung-it-fpt", 15000000, 25000000, false, "1-2 năm", "Đại học", "OnSite", future30, "Published", false, new[] { "tieng-anh-giao-tiep" });
        AddJob(vng, wpVngQ7, csCat, fullTime, "Chuyên viên Chăm sóc Khách hàng VIP", "chuyen-vien-cskh-vip-vng", 12000000, 18000000, false, "Dưới 1 năm", "Cao đẳng", "OnSite", future30, "Published", false, new[] { "tieng-anh-giao-tiep" });

        // ── 49 -> 58: Tin bổ sung đa dạng các ngành nghề khác ─────────────────
        var logCat = categories.FirstOrDefault(c => c.Slug.Contains("logistics")) ?? itCat;
        var eduCat = categories.FirstOrDefault(c => c.Slug.Contains("giao-duc")) ?? itCat;
        var constCat = categories.FirstOrDefault(c => c.Slug.Contains("xay-dung")) ?? itCat;
        var reCat = categories.FirstOrDefault(c => c.Slug.Contains("bat-dong-san")) ?? itCat;
        var mfgCat = categories.FirstOrDefault(c => c.Slug.Contains("san-xuat")) ?? itCat;

        AddJob(fpt, wpFptKeangnam, logCat, fullTime, "Chuyên viên Logistics & Quản lý Chuỗi cung ứng", "logistics-supply-chain-fpt", 16000000, 24000000, false, "1-2 năm", "Đại học", "OnSite", future30, "Published", false, new[] { "tieng-anh-giao-tiep" });
        AddJob(fpt, wpFptHn, eduCat, partTime, "Giảng viên Lập trình .NET & C# (Bán thời gian)", "giang-vien-lap-trinh-dotnet-fpt", 20000000, 35000000, false, "2-5 năm", "Đại học", "Hybrid", future30, "Published", false, new[] { "csharp", "dotnet-core" });
        AddJob(vng, wpVngQ7, constCat, fullTime, "Kỹ sư Quản lý Hạ tầng Cơ điện Trung tâm Dữ liệu", "datacenter-me-engineer-vng", 22000000, 35000000, false, "2-5 năm", "Đại học", "OnSite", future30, "Published", true, new[] { "linux" });
        AddJob(vng, wpVngBitexco, reCat, fullTime, "Chuyên viên Quản lý & Vận hành Mặt bằng BĐS", "facility-real-estate-vng", 18000000, 26000000, false, "1-2 năm", "Cao đẳng", "OnSite", future30, "Published", false, new string[] { });
        AddJob(fpt, wpFptHn, mfgCat, fullTime, "Kỹ sư Lập trình Nhúng & IoT (C/C++)", "embedded-iot-engineer-fpt", 20000000, 38000000, false, "1-2 năm", "Đại học", "OnSite", future30, "Published", false, new[] { "linux" });
        AddJob(vng, wpVngQ7, itCat, fullTime, "Mobile Android / Kotlin Developer", "mobile-android-developer-vng", 22000000, 36000000, false, "1-2 năm", "Đại học", "Hybrid", future30, "Published", false, new string[] { });
        AddJob(enclave, wpEnclaveHc, itCat, fullTime, "Database Administrator (DBA SQL Server / PostgreSQL)", "dba-sql-server-enclave", 25000000, 40000000, false, "2-5 năm", "Đại học", "OnSite", future30, "Published", false, new[] { "sql-server", "postgresql" });
        AddJob(fpt, wpFptHn, itCat, fullTime, "Kỹ sư An toàn Thông tin & Cyber Security", "cyber-security-engineer-fpt", 30000000, 52000000, false, "2-5 năm", "Đại học", "OnSite", future30, "Published", true, new[] { "linux", "ci-cd" });
        AddJob(enclave, wpEnclaveComplex, itCat, fullTime, "Fullstack .NET Core & React Developer", "fullstack-dotnet-react-enclave", 24000000, 42000000, false, "2-5 năm", "Đại học", "Hybrid", future30, "Published", false, new[] { "csharp", "dotnet-core", "react" });
        AddJob(enclave, null, itCat, freelance, "Chuyên viên Tối ưu Hiệu năng Web (Remote Freelance)", "web-performance-consultant-remote", null, null, true, "2-5 năm", "Không yêu cầu", "Remote", future30, "Published", false, new[] { "javascript", "react" });

        // ── Tin Pending, Rejected, Expired (Để test lọc kiểm duyệt & trạng thái)
        AddJob(fpt, wpFptHn, itCat, fullTime, "Tin tuyển dụng đang chờ duyệt (Pending)", "tin-tuyen-dung-cho-duyet-fpt", 20000000, 30000000, false, "1-2 năm", "Đại học", "OnSite", future30, "Pending", false, new[] { "csharp" });
        AddJob(vng, wpVngQ7, itCat, fullTime, "Tin tuyển dụng bị từ chối (Rejected)", "tin-tuyen-dung-bi-tu-choi-vng", 20000000, 30000000, false, "1-2 năm", "Đại học", "OnSite", future30, "Rejected", false, new[] { "java" });
        AddJob(enclave, wpEnclaveHc, itCat, fullTime, "Tin tuyển dụng đã hết hạn (Expired)", "tin-tuyen-dung-da-het-han-enclave", 20000000, 30000000, false, "1-2 năm", "Đại học", "OnSite", past10, "Expired", false, new[] { "python" });

        foreach (var job in sampleJobs)
        {
            var exists = await context.Jobs.AnyAsync(j => j.Slug == job.Slug);
            if (!exists)
            {
                context.Jobs.Add(job);
            }
        }

        await context.SaveChangesAsync();

        // ── Seed Ứng tuyển mẫu & Lưu tin mẫu cho ungvien_01 ───────────────────
        var candidate = await context.Users.FirstOrDefaultAsync(u => u.Username == "ungvien_01");
        var targetJob = await context.Jobs.FirstOrDefaultAsync(j => j.Slug == "senior-dotnet-core-backend-fpt");

        if (candidate != null && targetJob != null)
        {
            var appExists = await context.Applications.AnyAsync(a => a.JobId == targetJob.JobId && a.CandidateUserId == candidate.UserId);
            if (!appExists)
            {
                context.Applications.Add(new tblApplication
                {
                    JobId = targetJob.JobId,
                    CandidateUserId = candidate.UserId,
                    FullName = candidate.FullName ?? "Nguyễn Văn Ứng Viên",
                    Email = candidate.Email,
                    Phone = candidate.Phone ?? "0900000005",
                    CvPath = "/uploads/cvs/cv_nguyen_van_a.pdf",
                    CoverLetter = "Tôi rất quan tâm đến vị trí Senior .NET Core tại FPT Software và tin tưởng kinh nghiệm của mình hoàn toàn phù hợp.",
                    Status = "New",
                    AppliedAt = DateTime.Now
                });
            }

            var savedExists = await context.SavedJobs.AnyAsync(s => s.JobId == targetJob.JobId && s.UserId == candidate.UserId);
            if (!savedExists)
            {
                context.SavedJobs.Add(new tblSavedJob
                {
                    JobId = targetJob.JobId,
                    UserId = candidate.UserId,
                    SavedAt = DateTime.Now
                });
            }

            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 8. Seed Menu điều hướng và các bài viết tin tức mẫu.
    /// </summary>
    public static async Task SeedMenusAndPostsAsync(DataContext context)
    {
        // ── Menus ─────────────────────────────────────────────────────────────
        var menus = new (string Name, string Url, string? Icon, int Order, string Pos)[]
        {
            ("Trang chủ", "/", "bi-house", 1, "header"),
            ("Tìm việc làm", "/Jobs", "bi-briefcase", 2, "header"),
            ("Công ty", "/Companies", "bi-building", 3, "header"),
            ("Cẩm nang nghề nghiệp", "/Blog", "bi-journal-text", 4, "header"),
            ("Liên hệ", "/Contact", "bi-envelope", 5, "header"),

            // Footer
            ("Về TimViecLam", "/About", null, 1, "footer"),
            ("Điều khoản sử dụng", "/Terms", null, 2, "footer"),
            ("Chính sách bảo mật", "/Privacy", null, 3, "footer"),
            ("Quy chế hoạt động", "/Regulations", null, 4, "footer")
        };

        foreach (var m in menus)
        {
            var exists = await context.Menus.AnyAsync(x => x.MenuName == m.Name && x.Position == m.Pos);
            if (!exists)
            {
                context.Menus.Add(new tblMenu
                {
                    MenuName = m.Name,
                    Url = m.Url,
                    Icon = m.Icon,
                    SortOrder = m.Order,
                    Position = m.Pos,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }
        }

        // ── Posts ─────────────────────────────────────────────────────────────
        var posts = new (string Title, string Slug, string Summary, string Cat)[]
        {
            ("Bí quyết viết CV chuẩn ATS chinh phục nhà tuyển dụng IT", "bi-quyet-viet-cv-chuan-ats", "Hướng dẫn cấu trúc CV súc tích, tối ưu từ khóa kỹ năng giúp vượt qua vòng lọc hồ sơ tự động.", "Kinh nghiệm ứng tuyển"),
            ("Top 10 câu hỏi phỏng vấn .NET và ASP.NET Core hay gặp nhất", "top-10-cau-hoi-phong-van-dotnet-core", "Tổng hợp kiến thức trọng tâm về Dependency Injection, Middleware, Async/Await và EF Core.", "Phỏng vấn"),
            ("Xu hướng mức lương ngành Công nghệ thông tin tại Việt Nam năm 2026", "xu-huong-muc-luong-nganh-cntt-2026", "Báo cáo chi tiết dải lương theo năm kinh nghiệm cho các vị trí Backend, Frontend, Cloud và AI.", "Báo cáo thị trường")
        };

        foreach (var p in posts)
        {
            var exists = await context.Posts.AnyAsync(x => x.Slug == p.Slug);
            if (!exists)
            {
                context.Posts.Add(new tblPost
                {
                    Title = p.Title,
                    Slug = p.Slug,
                    Summary = p.Summary,
                    Category = p.Cat,
                    Content = $"<p>{p.Summary}</p><p>Chi tiết bài viết đang được cập nhật...</p>",
                    IsPublished = true,
                    PublishedAt = DateTime.Now.AddDays(-3),
                    ViewCount = 120,
                    CreatedAt = DateTime.Now
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
