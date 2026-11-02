using System.ComponentModel.DataAnnotations;
using TimViecLam.Models;

namespace TimViecLam.Models.ViewModels;

/// <summary>
/// ViewModel đăng ký ứng viên.
/// </summary>
public class RegisterCandidateViewModel
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ bao gồm chữ cái, số và dấu gạch dưới.")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [StringLength(200, ErrorMessage = "Email tối đa 200 ký tự.")]
    [Display(Name = "Địa chỉ Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ và tên không được để trống.")]
    [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [RegularExpression(@"^(0[35789])[0-9]{8}$", ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam (10 số, đầu số 03, 05, 07, 08, 09).")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Xác nhận mật khẩu không khớp.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel đăng ký nhà tuyển dụng (kèm thông tin công ty).
/// </summary>
public class RegisterEmployerViewModel
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ bao gồm chữ cái, số và dấu gạch dưới.")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [StringLength(200, ErrorMessage = "Email tối đa 200 ký tự.")]
    [Display(Name = "Email người đại diện")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ và tên không được để trống.")]
    [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
    [Display(Name = "Họ tên người liên hệ")]
    public string FullName { get; set; } = string.Empty;

    [RegularExpression(@"^(0[35789])[0-9]{8}$", ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam (10 số, đầu số 03, 05, 07, 08, 09).")]
    [Display(Name = "Số điện thoại liên hệ")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Tên công ty không được để trống.")]
    [StringLength(200, ErrorMessage = "Tên công ty tối đa 200 ký tự.")]
    [Display(Name = "Tên doanh nghiệp / Công ty")]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Mã số thuế tối đa 50 ký tự.")]
    [Display(Name = "Mã số thuế")]
    public string? TaxCode { get; set; }

    [Required(ErrorMessage = "Địa chỉ công ty không được để trống.")]
    [StringLength(300, ErrorMessage = "Địa chỉ tối đa 300 ký tự.")]
    [Display(Name = "Địa chỉ trụ sở chính")]
    public string CompanyAddress { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Số điện thoại công ty tối đa 20 ký tự.")]
    [Display(Name = "Điện thoại công ty")]
    public string? CompanyPhone { get; set; }

    [EmailAddress(ErrorMessage = "Email công ty không đúng định dạng.")]
    [StringLength(200, ErrorMessage = "Email công ty tối đa 200 ký tự.")]
    [Display(Name = "Email công ty")]
    public string? CompanyEmail { get; set; }

    [StringLength(50)]
    [Display(Name = "Quy mô doanh nghiệp")]
    public string CompanySize { get; set; } = "Small";

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Xác nhận mật khẩu không khớp.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel đăng nhập.
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc email.")]
    [Display(Name = "Tên đăng nhập hoặc Email")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool RememberMe { get; set; } = false;

    public string? ReturnUrl { get; set; }
}

/// <summary>
/// ViewModel thông tin hồ sơ cá nhân.
/// </summary>
public class ProfileViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ và tên không được để trống.")]
    [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [RegularExpression(@"^(0[35789])[0-9]{8}$", ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam (10 số, đầu số 03, 05, 07, 08, 09).")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [Display(Name = "Ảnh đại diện")]
    public string? Avatar { get; set; }

    public tblCompany? Company { get; set; }
}

/// <summary>
/// ViewModel đổi mật khẩu.
/// </summary>
public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu hiện tại")]
    public string OldPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận lại mật khẩu mới.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Xác nhận mật khẩu mới không khớp.")]
    [Display(Name = "Xác nhận mật khẩu mới")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel yêu cầu quên mật khẩu.
/// </summary>
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng.")]
    [Display(Name = "Địa chỉ email")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel đặt lại mật khẩu từ token.
/// </summary>
public class ResetPasswordViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Xác nhận mật khẩu mới không khớp.")]
    [Display(Name = "Xác nhận mật khẩu mới")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
