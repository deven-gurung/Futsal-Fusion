using System.Data;
using System.Text.Encodings.Web;
using FutsalFusion.Application.DTOs.Account;
using FutsalFusion.Application.DTOs.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FutsalFusion.Application.DTOs.Identity;
using FutsalFusion.Application.DTOs.User;
using FutsalFusion.Application.Interfaces.Identity;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Application.Interfaces.Services;
using FutsalFusion.Attribute;
using FutsalFusion.Domain.Constants;
using FutsalFusion.Domain.Entities;
using FutsalFusion.Domain.Helper;
using FutsalFusion.Domain.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;

namespace FutsalFusion.Controllers;

public class AccountController : Controller
{
    private readonly IEmailService _emailSender;
    private readonly IConfiguration _configuration;
    private readonly IGenericRepository _genericRepository;
    private readonly IFileUploadService _fileUploadService;
    private readonly IUserIdentityService _userIdentityService;

    public AccountController(IUserIdentityService userIdentityService, IEmailService emailSender, IGenericRepository genericRepository, IFileUploadService fileUploadService, IConfiguration configuration)
    {
        _userIdentityService = userIdentityService;
        _emailSender = emailSender;
        _genericRepository = genericRepository;
        _fileUploadService = fileUploadService;
        _configuration = configuration;
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
            if (!user.IsActive)
            {
                TempData["Warning"] = "You are currently inactive, please contact your administrator to verify the issue.";
        
                return View(new LoginRequestDto());
            }

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

        TempData["Success"] = "User Successfully Logged Out";
        
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
        register.Image = HttpContext.Request.Form.Files.FirstOrDefault();
        register.Username = Password.DecryptStringAES(register.HiddenUsername);
        register.Password = Password.DecryptStringAES(register.HiddenPassword).Replace(register.HiddenChangePassword, "");

        if (register.Image != null)
        {
            if (!ValidateFileMimeType(register.Image))
            {
                TempData["Warning"] = $"The file format of the {register.Image.FileName} file is incorrect.";
                
                return View();
            }
        }
        
        var user = _genericRepository.GetFirstOrDefault<AppUser>(x => x.UserName == register.Username || x.EmailAddress == register.EmailAddress);

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
        var captcha = GenerateAlphanumericCaseSensitiveCaptcha(6);
            
        ViewData["Captcha"] = captcha;
        
        HttpContext.Session.SetString("Captcha", captcha);
        
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

    [HttpGet]
    public IActionResult UsersList()
    {
        var users = _genericRepository.Get<AppUser>();

        var result = users.Select(x => new UserResponseDto()
        {
            Id = x.Id,
            FullName = x.FullName,
            Username = x.UserName,
            ImageURL = x.ImageURL ?? "sample-profile.png",
            EmailAddress = x.EmailAddress,
            IsActive = x.IsActive,
            RoleName = _genericRepository.GetById<AppRole>(x.RoleId).Name,
            RegisteredDate = x.CreatedAt.ToString("dd-MM-yyyy")
        }).ToList();

        return View(result);
    }

    [HttpGet]
    public IActionResult PlayersList()
    {
        var appointmentDetails = _genericRepository.Get<AppointmentDetail>();

        var userIds = appointmentDetails.Select(x => x.PlayerId).Distinct();

        var users = _genericRepository.Get<AppUser>(x => userIds.Contains(x.Id));
        
        var result = users.Select(x => new UserResponseDto()
        {
            Id = x.Id,
            FullName = x.FullName,
            Username = x.UserName,
            ImageURL = x.ImageURL ?? "sample-profile.png",
            EmailAddress = x.EmailAddress,
            IsActive = x.IsActive,
            RoleName = _genericRepository.GetById<AppRole>(x.RoleId).Name,
            RegisteredDate = x.CreatedAt.ToString("dd-MM-yyyy")
        }).ToList();

        return View(result);
    }
    
    [HttpGet]
    public IActionResult RoleRights()
    {
        var roles = _genericRepository.Get<AppRole>();
        
        var roleList = roles.Select(role => new SelectListItem()
        {
            Text = role.Name,
            Value = role.Id.ToString()
        }).ToList();

        ViewBag.ddlRoles = roleList;

        return View();
    }

    [HttpGet]
    public IActionResult LockUnlockUser(Guid userId)
    {
        var user = _genericRepository.GetById<AppUser>(userId);

        user.IsActive = !user.IsActive;
        
        _genericRepository.Update(user);

        TempData["Success"] = "The activation status of the selected user has been successfully changed.";
        
        return RedirectToAction("UsersList");
    }
    
    [HttpGet]
    public IActionResult GetRoleRights(Guid roleId)
    {
        var menus = _genericRepository.Get<Menu>(x => x.IsActive);

        var roleRights = _genericRepository.Get<RoleRights>(x => x.RoleId == roleId);

        var result = from m in menus
            join ur in roleRights
                on m.Id equals ur.MenuId into role
            from r in role.DefaultIfEmpty()
            orderby m.Name
            select new AccessRights()
            {
                MenuId = m.Id,
                ParentMenuId = m.ParentMenuId,
                MenuName = m.Name,
                RoleId = r?.RoleId ?? new Guid(),
                URL = m.URL,
            };
        
        var roleDetails = result.Select(l => new RoleRightsRequestDto()
        {
            MenuId = l.MenuId,
            ParentMenuId = l.ParentMenuId,  
            MenuName = l.MenuName,
            RoleId = l.RoleId,
            URL = l.URL 
        }).ToList();

        ViewBag.roleDetails = roleDetails;

        var htmlData = ConvertViewToString("_RoleRights", roleDetails, true);
        
        return Json(new
        {
            htmlData = htmlData
        });
    }
    
    [HttpPost]
    public IActionResult InsertRoleRights(Guid roleId, string menuIds)
    {
        var dtRoleRights = new DataTable();
        
        dtRoleRights.Columns.Add("RoleId", typeof(Guid));
        dtRoleRights.Columns.Add("MenuId", typeof(Guid));
        dtRoleRights.Columns.Add("CreatedBy", typeof(Guid));

        var arrMenuId = menuIds.Split(',');

        foreach (var menuId in arrMenuId)
        {
            var row = dtRoleRights.NewRow();
            row["RoleId"] = Convert.ToString(roleId);
            row["MenuId"] = Convert.ToString(menuId);
            row["CreatedBy"] = Convert.ToString("D1F1441C-E26A-4E89-81EE-08DC4ECE4B31");
            dtRoleRights.Rows.Add(row);
        }
        
        var dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        using (dbConnection)
        {
            var dbSqlCommand = new SqlCommand();
            
            dbSqlCommand.Connection = dbConnection;
            
            dbSqlCommand.CommandType = CommandType.StoredProcedure;
            
            dbSqlCommand.CommandText = "InsertRoleRights";

            var paramAttendanceDt = dbSqlCommand.Parameters.AddWithValue("@dtRoleRights", dtRoleRights);
            
            paramAttendanceDt.SqlDbType = SqlDbType.Structured;
            
            paramAttendanceDt.TypeName = "udtRoleRights";

            if (dbConnection.State == ConnectionState.Closed) dbConnection.Open();
            
            dbSqlCommand.ExecuteNonQuery();
            
            dbConnection.Close();
        }
        
        return Json(new 
        {
            success = 1,
            message = "Menu Rights and Privileges successfully assigned."
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto forgotPassword, string buttonType)
    {
        switch (buttonType)
        {
            case "OTP":
            {
                var captcha = HttpContext.Session.GetString("Captcha") ?? "";
        
                if (forgotPassword.Captcha != captcha)
                {
                    return Json(new
                    {
                        errorType = -1,
                        errorMessage = "Invalid captcha, please try again."
                    });
                }
                
                var userExists = _genericRepository.Exists<AppUser>(x => x.EmailAddress == forgotPassword.EmailAddress);
                
                if (userExists)
                {
                    // var otp = ExtensionMethod.GenerateOTP();
                    
                    var otp = "8VsfrT";

                    HttpContext.Session.SetComplexData("OTP", otp);
                    
                    forgotPassword.OTP = otp;
                    
                    // await _accountService.SendOTP(forgotPassword, MailProcess.OneTimePassword);
                    
                    return Json(new
                    {
                        successType = 1,
                        successMessage = "An email has been triggered with an OTP on your email, please check and fill the below details."
                    });
                }

                return Json(new
                {
                    errorType = -1,
                    errorMessage = "User with the following email address does not exist, please try again with a valid email address."
                });
            }
            case "ResendOTP":
            {
                var userExists = _genericRepository.Exists<AppUser>(x => x.EmailAddress == forgotPassword.EmailAddress);
                
                if (userExists)
                {
                    // var otp = ExtensionMethod.GenerateOTP();
                    
                    var otp = "8VsfrT";
                    
                    HttpContext.Session.SetComplexData("OTP", otp);
                    
                    forgotPassword.OTP = otp;
                    
                    // await _accountService.SendOTP(forgotPassword, MailProcess.OneTimePassword);
                    
                    return Json(new
                    {
                        successType = 1,
                        successMessage = "An email has been triggered with an OTP on your email, please check and fill the below details."
                    });
                }

                return Json(new
                {
                    errorType = -1,
                    errorMessage = "User with the following email address does not exist, please try again with a valid email address."
                });
            }
            case "VerifyOTP":
            {
                if (!string.IsNullOrEmpty(HttpContext.Session.GetComplexData<string>("OTP")))
                {
                    var otpValue = Convert.ToString(HttpContext.Session.GetComplexData<string>("OTP"));

                    switch (string.IsNullOrEmpty(forgotPassword.OTP))
                    {
                        case false when otpValue == forgotPassword.OTP:
                            return Json(new
                            {
                                successType = 2,
                                successMessage = "Your OTP matches with the provided OTP, please click on the below allocated button to reset your password."
                            });
                        
                        case false when otpValue != forgotPassword.OTP:
                            return Json(new
                            {
                                errorType = -1,
                                errorMessage = "Your OTP does not matches with the provided OTP, please provide a correct OTP or click to resent a OTP."
                            });
                    }
                }
    
                break;
            }
            case "Reset":
            {
                var user = _genericRepository.GetFirstOrDefault<AppUser>(x => x.EmailAddress == forgotPassword.EmailAddress);

                if (user == null)
                {
                    return Json(new
                    {
                        errorType = -1,
                        errorMessage = "User with the following email address does not exist, please try again with a valid email address."
                    });
                }
        
                user.Password = Password.CreatePasswordHash(Constants.Passwords.UserPassword, Password.CreateSalt(Password.PasswordSalt));

                _genericRepository.Update(user);
                
                HttpContext.Session.Remove("OTP");

                TempData["Success"] = "Your password has been successfully reset, please log in to access the system from the password sent at your email.";
                
                return Json(new
                {
                    successType = 3,
                    successMessage = "Your password has been successfully reset, please log in to access the system from the password sent at your email."
                });
            }
        }
        
        return Json(new
        {
            errorType = -1,
            errorMessage = "Invalid process, please try again."
        });
    }
    
    private string ConvertViewToString<TModel>(string viewName, TModel model, bool partial = false)
    {
        if (string.IsNullOrEmpty(viewName))
        {
            viewName = ControllerContext.ActionDescriptor.ActionName;
        }

        ViewData.Model = model;

        using var writer = new StringWriter();
        
        var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
        
        var viewResult = viewEngine?.FindView(this.ControllerContext, viewName, !partial);

        if (viewResult is { Success: false })
        {
            return $"A view with the name {viewName} could not be found";
        }

        if (viewResult?.View == null) return writer.ToString();
        
        var viewContext = new ViewContext(
            ControllerContext,
            viewResult.View,
            ViewData,
            TempData,
            writer,
            new HtmlHelperOptions()
        );

        viewResult.View.RenderAsync(viewContext);

        return writer.ToString();
    }
    
    private string GenerateAlphanumericCaseSensitiveCaptcha(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    [HttpGet]
    public IActionResult ResetCaptcha()
    {
        var captcha = GenerateAlphanumericCaseSensitiveCaptcha(6);
            
        ViewData["Captcha"] = captcha;
        
        HttpContext.Session.SetString("Captcha", captcha);

        return Json(new
        {
            data = captcha
        });
    }

    private static bool ValidateFileMimeType(IFormFile file)
    {
        var mimeType = string.Empty;
        
        var fileExtension = Path.GetExtension(file.FileName);
        
        using (var stream = file.OpenReadStream())
        {
            mimeType = FileHelper.GetMimeType(stream);
        }

        var disMType = FileHelper.GetFileMimeType(fileExtension);
        
        return mimeType == disMType;
    }
}