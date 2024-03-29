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
using FutsalFusion.Domain.Constants;
using FutsalFusion.Domain.Entities;
using FutsalFusion.Domain.Utilities;

namespace FutsalFusion.Controllers;

public class AccountController : Controller
{
    private readonly IEmailService _emailSender;
    private readonly IGenericRepository _genericRepository;
    private readonly IFileUploadService _fileUploadService;
    private readonly IUserIdentityService _userIdentityService;

    public AccountController(IUserIdentityService userIdentityService, IEmailService emailSender, IGenericRepository genericRepository, IFileUploadService fileUploadService)
    {
        _userIdentityService = userIdentityService;
        _emailSender = emailSender;
        _genericRepository = genericRepository;
        _fileUploadService = fileUploadService;
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
        login.Username = Password.DecryptStringAES(login.HiddenUsername);
        login.Password = Password.DecryptStringAES(login.HiddenPassword).Replace(login.HiddenChangePassword, "");
        
        var user = _genericRepository.GetFirstOrDefault<AppUser>(x => x.UserName == login.Username);

        if (user != null)
        {
            var isValid = Password.VerifyPassword(login.Password, user.Password, Password.PasswordSalt);

            if (isValid)
            {
                var role = _genericRepository.GetById<AppRole>(user.RoleId);

                var roleRights = _genericRepository.Get<RoleRights>(x => x.RoleId == role.Id);

                var menus = _genericRepository.Get<Menu>(x => roleRights.Select(y => y.MenuId).Contains(x.Id));

                var userAccessDetails = from menu in menus
                    join roleRight in roleRights
                        on menu.Id equals roleRight.MenuId
                    select new UserAccessDetailDto()
                    {
                        RoleId = role.Id,
                        UserId = user.Id,
                        ShowMenu = true,
                        MenuId = menu.Id,
                        MenuDetail = new MenuDetailDto()
                        {
                            MenuId = menu.Id,
                            ParentMenuId = menu.ParentMenuId,
                            IconClass = menu.IconClass,
                            MenuName = menu.Name,
                            SequenceNo = menu.SequenceNo,
                            URL = menu.URL
                        }
                    };
                
                var userDetail = new UserDetailDto()
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    ImageName = user.ImageURL,
                    RoleId = user.RoleId,
                    RoleName = role.Name,
                    UserAccessDetails = userAccessDetails.ToList()
                };
                
                HttpContext.Session.SetComplexData("User", userDetail);
                
                return RedirectToAction("Index", "Home");
            }
        }

        TempData["Warning"] = "Invalid username or password";
        
        return View(new LoginRequestDto());
    }
    
    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        
        return RedirectToAction("Login");
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
    public IActionResult Register(RegisterRequestDto register)
    {
        register.Username = Password.DecryptStringAES(register.HiddenUsername);
        register.Password = Password.DecryptStringAES(register.HiddenPassword).Replace(register.HiddenChangePassword, "");
        
        var user = _genericRepository.GetFirstOrDefault<AppUser>(x => x.UserName == register.Username && x.EmailAddress == register.EmailAddress);

        if (user == null)
        {
            var imageUrl = register.Image != null ? _fileUploadService.UploadDocument(Constants.FilePath.UsersImagesFilePath, register.Image) : null;

            var player = _genericRepository.GetFirstOrDefault<AppRole>(x => x.Name == Constants.Roles.Player);
            
            var appUser = new AppUser
            {
                FullName = register.FullName,
                EmailAddress = register.EmailAddress,
                UserName = register.Username,
                Password = Password.CreatePasswordHash(register.Password, Password.CreateSalt(Password.PasswordSalt)),
                RoleId = player!.Id,
                CreatedAt = DateTime.Now,
                ImageURL = imageUrl
            };

            _genericRepository.Insert(appUser);

            TempData["Success"] = "You have been successfully registered to our system.";
            
            return RedirectToAction("Login");
        }
        
        TempData["Warning"] = "A user with the following username or email address has already been registered, please try with a new username.";
        
        return View(new RegisterRequestDto());
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