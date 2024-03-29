using FutsalFusion.Application.DTOs.Futsal;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Application.Interfaces.Services;
using FutsalFusion.Attribute;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Constants;
using FutsalFusion.Domain.Entities;
using FutsalFusion.Domain.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class FutsalController : BaseController<FutsalController>
{
    private readonly IFileUploadService _fileUploadService;
    private readonly IGenericRepository _genericRepository;
    
    public FutsalController(IGenericRepository genericRepository, IFileUploadService fileUploadService)
    {
        _genericRepository = genericRepository;
        _fileUploadService = fileUploadService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var result = (from futsal in _genericRepository.Get<Futsal>()
                let owner = _genericRepository.GetById<AppUser>(futsal.FutsalOwnerId)
                let futsalImages = _genericRepository.GetFirstOrDefault<FutsalImage>(x => x.FutsalId == futsal.Id)
                let courts = _genericRepository.Get<Court>(x => x.FutsalId == futsal.Id)
                let appointments = _genericRepository.Get<Appointment>(x => courts.Select<Court, Guid>(z => z.Id).Contains(x.BookedCourtId))
                select new FutsalGridResponseDto()
                {
                    FutsalId = futsal.Id,
                    FutsalName = futsal.Name,
                    Slogan = futsal.Phrase,
                    OwnerName = owner.FullName,
                    ImageUrl = futsalImages!.ImageURL,
                    IsNew = futsal.CreatedAt.AddDays(15).Date >= DateTime.Today,
                    IsPopular = appointments.Count<Appointment>(x => x.CreatedAt.AddDays(-7).Date <= DateTime.Now.Date && x is { IsApproved: true, IsActionComplete: true }) > 20,
                    IsExotic = courts.Count<Court>() > 2,
                    OwnerImageUrl = owner.ImageURL ?? "sample-profile.png",
                    CurrentAppointments = appointments.Count<Appointment>(x => x is { IsActionComplete: false, IsApproved: false }),
                    TotalBookings = appointments.Count<Appointment>(),
                }).ToList();

        return View(result);
    }

    [HttpGet]
    public IActionResult Register()
    {
        var futsal = new FutsalRequestDto
        {
            Courts = new List<CourtRequestDto>
            {
                new()
            }
        };
        
        HttpContext.Session.SetComplexData("futsal", futsal);
        
        return View(futsal);
    }

    [HttpPost]
    public IActionResult Register(FutsalRequestDto futsal, string button)
    {
        if (button == "register")
        {
            var imageUrl = futsal.OwnerProfileImage != null ? _fileUploadService.UploadDocument(Constants.FilePath.UsersImagesFilePath, futsal.OwnerProfileImage) : null;

            var futsalRole = _genericRepository.GetFirstOrDefault<AppRole>(x => x.Name == Constants.Roles.Futsal);
            
            var owner = new AppUser
            {
                EmailAddress = futsal.OwnerEmail,
                FullName = futsal.OwnerName,
                UserName = futsal.OwnerUsername,
                MobileNo = futsal.OwnerPhoneNumber,
                Password = Password.CreatePasswordHash(Constants.Passwords.FutsalPassword, Password.CreateSalt(Password.PasswordSalt)),
                RoleId = futsalRole!.Id,
                CreatedAt = DateTime.Now,
                CreatedBy = UserDetail.UserId,
                ImageURL = imageUrl,
            };

            var userId = _genericRepository.Insert(owner);

            var futsalModel = new Futsal()
            {
                Name = futsal.FutsalName,
                Phrase = futsal.FutsalPhrase,
                City = futsal.FutsalCity,
                LocationAddress = futsal.FutsalLocationAddress,
                Description = futsal.FutsalDescription,
                CreatedBy = UserDetail.UserId,
                FutsalOwnerId = userId
            };

            var futsalId = _genericRepository.Insert(futsalModel);

            var images = HttpContext.Session.GetComplexData<List<string>?>("images") ?? [];

            foreach (var futsalImagesModel in images
                         .Select(uploadedImageUrl => new FutsalImage()
                     {
                         FutsalId = futsalId,
                         CreatedBy = UserDetail.UserId,
                         ImageURL = uploadedImageUrl,
                         ImageType = 1
                     }))
            {
                _genericRepository.Insert(futsalImagesModel);
            }

            foreach (var courtImageModel in from futsalCourt in futsal.Courts let futsalCourtModel = new Court()
                     {
                         FutsalId = futsalId,
                         Description = futsalCourt.Description,
                         Title = futsalCourt.Title,
                         Type = futsalCourt.Type,
                         CreatedBy = UserDetail.UserId
                     } 
                     let courtId = _genericRepository.Insert(futsalCourtModel) 
                                        from courtImageModel in futsalCourt.Images.Select(futsalCourtImage => 
                                                _fileUploadService.UploadDocument(Constants.FilePath.CourtImagesFilePath, futsalCourtImage))
                                        .Select(courtImageUrl => new CourtImage()
                                        {
                                             CourtId = courtId,
                                             ImageType = 1,
                                             ImageURL = courtImageUrl,
                                             CreatedBy = UserDetail.UserId
                                        }) select courtImageModel)
            {
                _genericRepository.Insert(courtImageModel);
            }

            TempData["Success"] = "Futsal Registration Successful";

            return RedirectToAction("Index");
        }

        futsal.Courts?.Add(new CourtRequestDto());
            
        HttpContext.Session.SetComplexData("futsal", futsal);

        return Json(new
        {
            result = 1,
            htmlData = ConvertViewToString("_Courts", futsal, true)
        });
    }

    [HttpPost]
    public IActionResult Upload()
    {
        var files = HttpContext.Request.Form.Files;
        
        if (files.Any())
        {
            foreach (var file in files)
            {
                var imageFilePath = _fileUploadService.UploadDocument(Constants.FilePath.FutsalImagesFilePath, file);

                var images = HttpContext.Session.GetComplexData<List<string>?>("images");

                images ??= [];
                
                images.Add(imageFilePath);
                
                HttpContext.Session.SetComplexData("images", images);
            }
        }

        return Json(new
        {
            success = 1
        });
    }

    [HttpGet]
    public IActionResult FutsalDetails(Guid futsalId)
    {
        var futsal = _genericRepository.GetById<Futsal>(futsalId);

        var futsalImages = _genericRepository.Get<FutsalImage>(x => x.FutsalId == futsal.Id);

        var courts = _genericRepository.Get<Court>(x => x.FutsalId == futsal.Id);

        var workingHours = _genericRepository.Get<WorkingHours>(x => x.FutsalId == futsal.Id);

        var owner = _genericRepository.GetById<AppUser>(futsal.FutsalOwnerId);
        
        var appointments = _genericRepository.Get<Appointment>(x => courts.Select(z => z.Id).Contains(x.BookedCourtId));
        
        var courtList = courts.Select(x => new CourtResponseDto()
        {
            Id = x.Id,
            Title = x.Title,
            Description = x.Description,
            Type = x.Type == 1 ? "Indoor" : "Outdoor",
            Images = _genericRepository.Get<CourtImage>(z => z.CourtId == x.Id).Select(z => z.ImageURL).ToList()
        }).ToList();

        var workingHoursList = new List<FutsalWorkingHours>();
        
        for (var i = 1; i <= 7; i++)
        {
            var day = i;

            var dayOfTheWeek = workingHours.Where(x => x.Day == day);

            var workingHoursEnumerable = dayOfTheWeek as WorkingHours[] ?? dayOfTheWeek.ToArray();

            if (workingHoursEnumerable.Length == 0) continue;
            {
                var startTime = workingHoursEnumerable.Min(x => x.OpenTime);
                var endTime = workingHoursEnumerable.Max(x => x.CloseTime);
                
                workingHoursList.Add(new FutsalWorkingHours()
                {
                    WeekDay = i,
                    Day = i switch
                    {
                        1 => "Monday",
                        2 => "Tuesday",
                        3 => "Wednesday",
                        4 => "Thursday",
                        5 => "Friday",
                        6 => "Saturday",
                        7 => "Sunday",
                        _ => "Other Days"
                    },
                    TimePeriod = $"{startTime} - {endTime}"
                });
            }
        }

        var result = new FutsalResponseDto()
        {
            FutsalId = futsal.Id,
            FutsalName = futsal.Name,
            FutsalPhrase = futsal.Phrase,
            OwnerName = owner.FullName,
            FutsalImages = futsalImages.Select(x => x.ImageURL).ToList(),
            IsNew = futsal.CreatedAt.AddDays(15).Date >= DateTime.Today,
            IsPopular = appointments.Count(x =>
                x.CreatedAt.AddDays(-7).Date <= DateTime.Now.Date &&
                x is { IsApproved: true, IsActionComplete: true }) > 20,
            IsExotic = courts.Count() > 2,
            OwnerImageUrl = owner.ImageURL ?? "sample-profile.png",
            Courts = courtList,
            OwnerEmail = owner.EmailAddress,
            WorkingHours = workingHoursList,
            FutsalLocationAddress = $"{futsal.LocationAddress}, {futsal.City}",
            FutsalOverview = futsal.Description,
            InitiatedDate = futsal.CreatedAt.ToString("dd-MMM-yyyy"),
            TotalEarnings = appointments.Where(x => x.IsApproved).Sum(x => x.TotalPrice)
        };

        return View(result);
    }
    
}