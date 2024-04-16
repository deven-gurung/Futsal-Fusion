using System.ComponentModel.DataAnnotations;

namespace FutsalFusion.Application.DTOs.Account;

public class ForgotPasswordRequestDto
{
    [Display(Name = "Email Address : "), Required(ErrorMessage = "The Email Address Field is required"), StringLength(50, ErrorMessage = "{0} not be exceed 50 char"), DataType(DataType.EmailAddress, ErrorMessage = "Invalid email address.")]
    [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Invalid email.")]
    public string EmailAddress { get; set; }

    [Required]
    public string Captcha { get; set; }
    
    [Required(ErrorMessage = "The OTP field is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "The OTP field must be exact 6 characters")]
    public string OTP { get; set; }
}