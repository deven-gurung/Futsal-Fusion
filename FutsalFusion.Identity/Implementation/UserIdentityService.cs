using System.Text;
using FutsalFusion.Application.DTOs.Identity;
using FutsalFusion.Application.Interfaces.Identity;
using FutsalFusion.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace FutsalFusion.Identity.Implementation;

public class UserIdentityService : IUserIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public UserIdentityService(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Tuple<string, string>> Register(RegisterDto register, string? returnUrl = null)
    {
        try
        {
            var user = new User()
            {
                Name = register.Name,
                UserName = register.Email, 
                Email = register.Email 
            };
            
            var result = await _userManager.CreateAsync(user, register.Password);

            if (!result.Succeeded) return new Tuple<string, string>(string.Empty, string.Empty);
            
            await _userManager.AddToRoleAsync(user, register.Role);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
            return new Tuple<string, string>(user.Id.ToString(), WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> ConfirmEmail(Guid userId, string code)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            if (user == null)
            {
                return false;
            }
            
            var result = await _userManager.ConfirmEmailAsync(user, code);
            
            return result.Succeeded;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<string> Login(LoginDto login, string? returnUrl = null)
    {
        try
        {
            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, login.RememberMe, lockoutOnFailure: true);

            if (result.IsLockedOut) return "Locked";

            return result.Succeeded ? "Success" : "Invalid";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task LogOut()
    {
        try
        {
            await _signInManager.SignOutAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Tuple<string, string>> ForgetPassword(ForgotPasswordDto forgotPassword)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(forgotPassword.Email);

            if (user == null) return new Tuple<string, string>(string.Empty, string.Empty);
            
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            return new Tuple<string, string>(user.Id.ToString(), WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<string> ResetPassword(ResetPasswordDto resetPassword)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);

            if (user == null) return string.Empty;
            
            var result = await _userManager.ResetPasswordAsync(user, resetPassword.Code, resetPassword.Password);

            return result.Succeeded ? "Success" : string.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}