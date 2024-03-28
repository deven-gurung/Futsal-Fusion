using System.Text.Encodings.Web;
using FutsalFusion.Application.DTOs.Account;
using FutsalFusion.Application.DTOs.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FutsalFusion.Application.DTOs.Identity;
using FutsalFusion.Application.Interfaces.Identity;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Application.Interfaces.Services;
using FutsalFusion.Attribute;
using FutsalFusion.Domain.Entities;
using FutsalFusion.Domain.Utilities;

namespace FutsalFusion.Controllers;

public class AccountController : Controller
{
    private readonly IEmailService _emailSender;
    private readonly IGenericRepository _genericRepository;
    private readonly IUserIdentityService _userIdentityService;

    public AccountController(IUserIdentityService userIdentityService, IEmailService emailSender, IGenericRepository genericRepository)
    {
        _userIdentityService = userIdentityService;
        _emailSender = emailSender;
        _genericRepository = genericRepository;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }
    
    [HttpPost]
    [AllowAnonymous]
    public IActionResult Login(LoginRequestDto login)
    {
        var user = _genericRepository.GetFirstOrDefault<AppUser>(x => x.UserName == login.Username);

        login.Username = Password.DecryptStringAES(login.HiddenUsername);
        login.Password = Password.DecryptStringAES(login.HiddenPassword);

        if (user != null)
        {
            var isValid = Password.VerifyPassword(login.Password, user.Password, Password.PasswordSalt);

            if (isValid)
            {
                var role = _genericRepository.GetById<AppRole>(user.RoleId);

                var userDetail = new UserDetailDto()
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    ImageName = user.ImageURL,
                    RoleId = user.RoleId,
                    RoleName = role.Name,
                };
                
                HttpContext.Session.SetComplexData("User", userDetail);
                
                return RedirectToAction("Index", "Home");
            }
        }

        TempData["Warning"] = "Invalid username or password";
        
        return View();
    }
    
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto register, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        
        returnUrl = returnUrl ?? Url.Content("~/");
        
        if (ModelState.IsValid)
        {
            var user = await _userIdentityService.Register(register);
            
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new
            {
                userId = user.Item1, 
                code = user.Item2
            }, protocol: HttpContext.Request.Scheme);

            var emailOption = new EmailActionDto()
            {
                Email = register.Email,
                Subject = "Email Confirmation",
                Body = $"<a href='{HtmlEncoder.Default.Encode(callbackUrl ?? "")}'>"
            };
            
            await _emailSender.SendEmail(emailOption);

            TempData["Success"] = "Email Successfully Sent";

            return RedirectToAction("RegisterConfirmation");
        }
        
        return View();
    }
    
    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterConfirmation()
    {
        return View();
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(Guid userId,string code)
    {
        var user = await _userIdentityService.ConfirmEmail(userId, code);

        return View(user ? "ConfirmEmail" : "Error");
    }
    
    // [HttpPost]
    // [AllowAnonymous]
    // [ValidateAntiForgeryToken]
    // public async Task<IActionResult> Login(LoginDto login, string? returnUrl = null)
    // {
    //     ViewData["ReturnUrl"] = returnUrl;
    //     
    //     returnUrl = returnUrl ?? Url.Content("~/");
    //
    //     var result = await _userIdentityService.Login(login, returnUrl);
    //
    //     return result switch
    //     {
    //         "Locked" => View("Locked"),
    //         "Invalid" => View("Invalid"),
    //         "Success" => View(),
    //         _ => View("Error")
    //     };
    // }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOut()
    {
        await _userIdentityService.LogOut();
        
        return RedirectToAction(nameof(HomeController.Index),"Home");
    }
    
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgetPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgetPassword(ForgotPasswordDto forgotPassword)
    {
        var result = await _userIdentityService.ForgetPassword(forgotPassword);

        if (result.Item1 == string.Empty || result.Item2 == string.Empty)
        {
            return View("Error");
        }
        
        var callbackUrl = Url.Action("ResetPassword", "Account", new
        {
            userId = result.Item1, 
            code = result.Item2
        }, protocol: HttpContext.Request.Scheme);
        
        var emailOption = new EmailActionDto()
        {
            Email = forgotPassword.Email,
            Subject = "Forget Password",
            Body = $"<a href='{HtmlEncoder.Default.Encode(callbackUrl ?? "")}'>"
        };
            
        await _emailSender.SendEmail(emailOption);
        
        return RedirectToAction("ForgotPasswordConfirmation");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? code = null)
    {
        return code == null ? View("Error") : View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPassword)
    {
        var result = await _userIdentityService.ResetPassword(resetPassword);

        if (result == string.Empty)
        {
            return View("Error");
        }
        
        var emailOption = new EmailActionDto()
        {
            Email = resetPassword.Email,
            Subject = "Forget Password",
            Body = $"<a href='{HtmlEncoder.Default.Encode("")}'>"
        };
            
        await _emailSender.SendEmail(emailOption);
        
        return RedirectToAction("ResetPasswordConfirmation");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }
}