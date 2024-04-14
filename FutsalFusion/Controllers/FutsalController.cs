using ClosedXML.Excel;
using FutsalFusion.Application.DTOs.Appointment;
using FutsalFusion.Application.DTOs.Futsal;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Application.Interfaces.Services;
using FutsalFusion.Attribute;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Constants;
using FutsalFusion.Domain.Entities;
using FutsalFusion.Domain.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;

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
    public IActionResult Index(string? type = null)
    {
        var futsalDetailsList = (from futsal in _genericRepository.Get<Futsal>()
                let owner = _genericRepository.GetById<AppUser>(futsal.FutsalOwnerId)
                let futsalImages = _genericRepository.GetFirstOrDefault<FutsalImage>(x => x.FutsalId == futsal.Id)
                let courts = _genericRepository.Get<Court>(x => x.FutsalId == futsal.Id)
                let appointments = _genericRepository.Get<Appointment>(x => courts.Select<Court, Guid>(z => z.Id).Contains(x.BookedCourtId))
                select new FutsalDetails()
                {
                    FutsalId = futsal.Id,
                    FutsalName = futsal.Name,
                    Slogan = futsal.Phrase,
                    OwnerName = owner.FullName,
                    ImageUrl = futsalImages!.ImageURL,
                    IsActive = futsal.IsActive,
                    IsNew = futsal.CreatedAt.AddDays(15).Date >= DateTime.Today,
                    IsPopular = appointments.Count<Appointment>(x => x.CreatedAt.AddDays(-7).Date <= DateTime.Now.Date && x is { IsApproved: true, IsActionComplete: true }) > 20,
                    IsExotic = courts.Count<Court>() > 2,
                    OwnerImageUrl = owner.ImageURL ?? "sample-profile.png",
                    CurrentAppointments = appointments.Count<Appointment>(x => x is { IsActionComplete: false, IsApproved: false }),
                    TotalBookings = appointments.Count<Appointment>(),
                }).ToList();

        var futsalDetails = type switch
        {
            "All" => futsalDetailsList.Where(x => x.IsActive).ToList(),
            "Exotic" => futsalDetailsList.Where(x => x.IsExotic).ToList(),
            "Popular" => futsalDetailsList.Where(x => x.IsPopular).ToList(),
            "Inactive" => futsalDetailsList.Where(x => !x.IsActive).ToList(),
            _ => futsalDetailsList.Where(x => x.IsActive).ToList()
        };

        var result = new FutsalGridResponseDto()
        {
            Selection = type ?? "All",
            FutsalDetails = futsalDetails
        };
        
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

            var courts = _genericRepository.Get<Court>(x => x.FutsalId == futsalId);

            foreach (var court in courts)
            {
                var priceForMorning = new CourtPrice()
                {
                    CourtId = court.Id,
                    TimeTo = new TimeSpan(6, 0, 0),
                    TimeFrom = new TimeSpan(14, 0, 0),
                    IsDeleted = false,
                    IsActive = true,
                    CreatedBy = UserDetail.UserId,
                    CreatedAt = DateTime.Now,
                    PricePerHour = 1200,
                };
                
                var priceForDay = new CourtPrice()
                {
                    CourtId = court.Id,
                    TimeTo = new TimeSpan(15, 0, 0),
                    TimeFrom = new TimeSpan(20, 0, 0),
                    IsDeleted = false,
                    IsActive = true,
                    CreatedBy = UserDetail.UserId,
                    CreatedAt = DateTime.Now,
                    PricePerHour = 1500,
                };
                
                var priceForEvening = new CourtPrice()
                {
                    CourtId = court.Id,
                    TimeTo = new TimeSpan(21, 0, 0),
                    TimeFrom = new TimeSpan(22, 0, 0),
                    IsDeleted = false,
                    IsActive = true,
                    CreatedBy = UserDetail.UserId,
                    CreatedAt = DateTime.Now,
                    PricePerHour = 200,
                };
                
                _genericRepository.Insert(priceForMorning);
                _genericRepository.Insert(priceForDay);
                _genericRepository.Insert(priceForEvening);
            }
            
            for (var i = 1; i <= 7; i++)
            {
                var workingHours = new WorkingHours()
                {
                    FutsalId = futsalId,
                    Day = i,
                    OpenTime = new TimeSpan(6, 0, 0),
                    CloseTime = new TimeSpan(22, 0, 0),
                    IsDeleted = false,
                    IsActive = true,
                    CreatedBy = UserDetail.UserId,
                    CreatedAt = DateTime.Now,
                };

                _genericRepository.Insert(workingHours);
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
    public IActionResult ActivationStatus(Guid futsalId)
    {
        var futsal = _genericRepository.GetById<Futsal>(futsalId);

        futsal.IsActive = !futsal.IsActive;
        
        _genericRepository.Update(futsal);

        var owner = _genericRepository.GetById<AppUser>(futsal.FutsalOwnerId);

        owner.IsActive = !owner.IsActive;
        
        _genericRepository.Update(owner);

        TempData["Success"] = "The activation status of the following futsal has been successfully changed";
        
        return RedirectToAction("Index");
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
            IsActive = futsal.IsActive,
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

    [HttpGet]
    public IActionResult FutsalWorkingHours(Guid futsalId)
    {
        var futsal = _genericRepository.GetById<Futsal>(futsalId);

        var courts = _genericRepository.Get<Court>(x => x.FutsalId == futsal.Id);

        var workingHours = _genericRepository.Get<WorkingHours>(x => x.FutsalId == futsal.Id);
        
        var appointments = _genericRepository.Get<Appointment>(x => courts.Select(z => z.Id).Contains(x.BookedCourtId));

        var owner = _genericRepository.GetById<AppUser>(futsal.FutsalOwnerId);

        var startTimeSpanOptions = new List<SelectListItem>();

        for (var hours = 6; hours <= 22; hours++)
        {
            var timeSpan = new TimeSpan(hours, 0, 0);
            
            startTimeSpanOptions.Add(new SelectListItem
            {
                Text = timeSpan.ToString(), 
                Value = timeSpan.ToString(), 
            });
        }

        ViewBag.ddlStartTime = startTimeSpanOptions;
        
        var endTimeSpanOptions = new List<SelectListItem>();

        for (var hours = 7; hours <= 22; hours++)
        {
            var timeSpan = new TimeSpan(hours, 0, 0);
            
            endTimeSpanOptions.Add(new SelectListItem
            {
                Text = timeSpan.ToString(), 
                Value = timeSpan.ToString(),
            });
        }

        ViewBag.ddlEndTime = endTimeSpanOptions;

        var workingHoursList = new List<FutsalWorkingHoursRequestDto>();
        
        for (var i = 1; i <= 7; i++)
        {
            var day = i;

            var dayOfTheWeek = workingHours.Where(x => x.Day == day);

            var workingHoursEnumerable = dayOfTheWeek as WorkingHours[] ?? dayOfTheWeek.ToArray();

            if (workingHoursEnumerable.Length == 0) continue;
            {
                var startTime = workingHoursEnumerable.Min(x => x.OpenTime);
                var endTime = workingHoursEnumerable.Max(x => x.CloseTime);
                
                workingHoursList.Add(new FutsalWorkingHoursRequestDto()
                {
                    WorkingDay = i,
                    StartTime = startTime,
                    EndTime = endTime,
                });
            }
        }
        
        var result = new FutsalResponseDto()
        {
            FutsalId = futsal.Id,
            FutsalName = futsal.Name,
            FutsalPhrase = futsal.Phrase,
            OwnerName = owner.FullName,
            IsNew = futsal.CreatedAt.AddDays(15).Date >= DateTime.Today,
            IsPopular = appointments.Count(x =>
                x.CreatedAt.AddDays(-7).Date <= DateTime.Now.Date &&
                x is { IsApproved: true, IsActionComplete: true }) > 20,
            IsExotic = courts.Count() > 2,
            OwnerImageUrl = owner.ImageURL ?? "sample-profile.png",
            OwnerEmail = owner.EmailAddress,
            FutsalWorkingHours = workingHoursList,
            FutsalLocationAddress = $"{futsal.LocationAddress}, {futsal.City}",
            FutsalOverview = futsal.Description,
            InitiatedDate = futsal.CreatedAt.ToString("dd-MMM-yyyy"),
            TotalEarnings = appointments.Where(x => x.IsApproved).Sum(x => x.TotalPrice)
        };
        
        return View(result);
    }
    
    [HttpPost]
    public IActionResult FutsalWorkingHours(FutsalResponseDto futsalDetails)
    {
        var existingWorkingHours = _genericRepository.Get<WorkingHours>(x => x.FutsalId == futsalDetails.FutsalId);

        var workingHoursEnumerable = existingWorkingHours as WorkingHours[] ?? existingWorkingHours.ToArray();
        
        if (workingHoursEnumerable.Any())
        {
            _genericRepository.RemoveMultipleEntity(workingHoursEnumerable);
        }

        var workingHours = futsalDetails.FutsalWorkingHours.Select(x => new WorkingHours()
        {
            FutsalId = futsalDetails.FutsalId,
            Day = x.WorkingDay,
            OpenTime = x.StartTime,
            CloseTime = x.EndTime,
            IsActive = true,
            CreatedBy = UserDetail.UserId,
            CreatedAt = DateTime.Now,
        });

        _genericRepository.AddMultipleEntity(workingHours);

        TempData["Success"] = "Working Hours Successfully Updated";
        
        return RedirectToAction("FutsalDetails", new { futsalId = futsalDetails.FutsalId });
    }
    
    [HttpGet]
    public IActionResult CourtDetails(Guid futsalCourtId)
    {
        var court = _genericRepository.GetById<Court>(futsalCourtId);

        var courtImages = _genericRepository.Get<CourtImage>(x => x.CourtId == court.Id);
        
        var futsal = _genericRepository.GetById<Futsal>(court.FutsalId);

        var workingHours = _genericRepository.Get<WorkingHours>(x => x.FutsalId == futsal.Id);

        if (!workingHours.Any())
        {
            TempData["Warning"] = "Please enter the working days before viewing the court details";

            return RedirectToAction("FutsalDetails", new { futsalId = futsal.Id });
        }
        
        var hoursEnumerable = workingHours as WorkingHours[] ?? workingHours.ToArray();
        
        var startWorkingHours = hoursEnumerable.Min(x => x.OpenTime);

        var closeWorkingHours = hoursEnumerable.Max(x => x.CloseTime);
        
        var owner = _genericRepository.GetById<AppUser>(futsal.FutsalOwnerId);
        
        var workingHoursList = new List<FutsalWorkingHours>();
        
        for (var i = 1; i <= 7; i++)
        {
            var day = i;

            var dayOfTheWeek = hoursEnumerable.Where(x => x.Day == day);

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

        var courtPrices = _genericRepository.Get<CourtPrice>(x => x.CourtId == court.Id);

        var courtPriceDetails = courtPrices.Select(courtPrice => new CourtPriceDetails()
        {
            TimePeriod = $"{courtPrice.TimeFrom} - {courtPrice.TimeTo}", 
            Price = $"Rs {courtPrice.PricePerHour:N2}"
        }).ToList();

        var bookingSlots = GetCourtBookingSlots(court.Id);
        
        var result = new CourtDetailsResponseDto()
        {
            FutsalId = futsal.Id,
            FutsalName = futsal.Name,
            OwnerName = owner.FullName,
            CourtImageURL = courtImages.Select(x => x.ImageURL).ToList(),
            EmailAddress = owner.EmailAddress,
            WorkingHours = workingHoursList,
            Address = $"{futsal.LocationAddress}, {futsal.City}",
            CourtId = court.Id,
            CourtName = court.Title,
            CourtDescription = court.Description,
            PhoneNumber = owner.MobileNo ?? "Not Available",
            CourtPrices = courtPriceDetails,
            BookingSlots = bookingSlots,
            OpeningHours = (int)startWorkingHours.TotalHours,
            ClosingHours = (int)closeWorkingHours.TotalHours
        };

        return View(result);
    }

    [HttpGet]
    public IActionResult CourtPrices(Guid courtId)
    {
        var court = _genericRepository.GetById<Court>(courtId);
        
        var futsal = _genericRepository.GetById<Futsal>(court.FutsalId);

        var workingHours = _genericRepository.Get<WorkingHours>(x => x.FutsalId == futsal.Id);

        var owner = _genericRepository.GetById<AppUser>(futsal.FutsalOwnerId);
        
        var workingHoursList = new List<FutsalWorkingHours>();

        var hoursEnumerable = workingHours as WorkingHours[] ?? workingHours.ToArray();
        
        for (var i = 1; i <= 7; i++)
        {
            var day = i;

            var dayOfTheWeek = hoursEnumerable.Where(x => x.Day == day);

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

        var minStartTime = (int)(hoursEnumerable.Min(x => x.OpenTime).TotalHours);
        
        var maxCloseTime = (int)(hoursEnumerable.Max(x => x.CloseTime).TotalHours);

        var startTimeSpanOptions = new List<SelectListItem>();

        for (var hours = minStartTime; hours <= maxCloseTime; hours++)
        {
            var timeSpan = new TimeSpan(hours, 0, 0);
            
            startTimeSpanOptions.Add(new SelectListItem
            {
                Text = timeSpan.ToString(), 
                Value = timeSpan.ToString(), 
            });
        }

        ViewBag.ddlStartTime = startTimeSpanOptions;
        
        var endTimeSpanOptions = new List<SelectListItem>();

        for (var hours = minStartTime + 1; hours <= maxCloseTime; hours++)
        {
            var timeSpan = new TimeSpan(hours, 0, 0);
            
            endTimeSpanOptions.Add(new SelectListItem
            {
                Text = timeSpan.ToString(), 
                Value = timeSpan.ToString(),
            });
        }

        ViewBag.ddlEndTime = endTimeSpanOptions;

        var priceList = _genericRepository.Get<CourtPrice>(x => x.CourtId == court.Id);
        
        var result = new CourtDetailsRequestDto()
        {
            CourtId = court.Id,
            CourtName = court.Title,
            CourtDescription = court.Description,
            OwnerName = owner.FullName,
            OwnerImage = owner.ImageURL ?? "super-admin.jpg",
            EmailAddress = owner.EmailAddress,
            FutsalId = futsal.Id,
            FutsalName = futsal.Name,
            PhoneNumber = owner.MobileNo ?? "Not Available",
            Address = $"{futsal.LocationAddress}, {futsal.City}",
            WorkingHoursList = workingHoursList,
            CourtPrices = priceList.Select(x => new CourtPriceRange()
            {
                PricePerHour = x.PricePerHour,
                StartTime = x.TimeFrom,
                EndTime = x.TimeTo
            }).ToList()
        };

        return View(result);
    }

    [HttpPost]
    public IActionResult CourtPrices(CourtDetailsRequestDto courtPricesDetails)
    {
        var existingCourtPrice = _genericRepository.Get<CourtPrice>(x => x.CourtId == courtPricesDetails.CourtId);

        var removeEntityList = existingCourtPrice as CourtPrice[] ?? existingCourtPrice.ToArray();
        
        if (removeEntityList.Any())
        {
            _genericRepository.RemoveMultipleEntity(removeEntityList);
        }

        var courtPriceModel = courtPricesDetails.CourtPrices.Select(x => new CourtPrice()
        {
            TimeTo = x.EndTime,
            TimeFrom = x.StartTime,
            PricePerHour = x.PricePerHour,
            IsActive = true,
            CourtId = courtPricesDetails.CourtId,
            CreatedBy = UserDetail.UserId,
            CreatedAt = DateTime.Now,
        }).ToList();

        _genericRepository.AddMultipleEntity(courtPriceModel);
        
        return RedirectToAction("CourtPrices", new { courtId = courtPricesDetails.CourtId });
    }

    private List<BookingSlotDto> GetCourtBookingSlots(Guid courtId)
    {
        var court = _genericRepository.GetById<Court>(courtId);

        var futsal = _genericRepository.GetById<Futsal>(court.FutsalId);

        var workingHours = _genericRepository.Get<WorkingHours>(x => x.FutsalId == futsal.Id);
        
        var workingHoursEnumerable = workingHours as WorkingHours[] ?? workingHours.ToArray();

        var courtPrices = _genericRepository.Get<CourtPrice>(x => x.CourtId == court.Id);
        
        var pricesEnumerable = courtPrices as CourtPrice[] ?? courtPrices.ToArray();

        var appointments = _genericRepository.Get<Appointment>(x => x.BookedCourtId == court.Id);
        
        var result = new List<BookingSlotDto>();
        
        var todayDate = DateTime.Now;

        for (int i = 0; i <= 6; i++)
        {
            var dateOfTheWeek = todayDate.AddDays(i);

            var dayOfTheWeek = dateOfTheWeek.DayOfWeek.ToString();

            var dayOfTheWeekInDays = ((int)dateOfTheWeek.DayOfWeek + 6) % 7 + 1;

            var workingHoursInDay = workingHoursEnumerable.FirstOrDefault(x => x.Day == dayOfTheWeekInDays);

            var bookingSlot = new BookingSlotDto()
            {
                Day = dayOfTheWeek,
                AppointedDate = dateOfTheWeek
            };

            var slotsModel = new List<SlotsDto>();
            
            if (workingHoursInDay != null)
            {
                for (int j = (int)workingHoursInDay.OpenTime.TotalHours;
                     j < (int)workingHoursInDay.CloseTime.TotalHours;
                     j++)
                {
                    var index = j;

                    var price = pricesEnumerable.FirstOrDefault(x =>
                        (int)x.TimeFrom.TotalHours <= index && index <= (int)x.TimeTo.TotalHours);

                    if (price != null)
                    {
                        var appointment = appointments.FirstOrDefault(x =>
                            x.AppointedDate.Date == dateOfTheWeek.Date && (int)x.TimeSlotStartTime.TotalHours == index &&
                            (int)x.TimeSlotEndTime.TotalHours == index + 1);

                        var status = appointment switch
                        {
                            null => "Available",
                            { IsApproved: true, IsActionComplete: true } => "Busy",
                            { IsApproved: false, IsActionComplete: false } => "On Hold",
                            { IsApproved: false, IsActionComplete: true } => "Available",
                            _ => ""
                        };

                        var slots = new SlotsDto()
                        {
                            Price = price.PricePerHour,
                            AppointedDate = dateOfTheWeek.Date,
                            TimeSlot = TimeSpan.FromHours(index),
                            Status = status
                        };
                        
                        slotsModel.Add(slots);
                    }
                }
            }

            bookingSlot.Slots = slotsModel;
            
            result.Add(bookingSlot);

        }

        return result;
    }
    
    [HttpGet]
    public IActionResult DownloadSheet()
    {
        var futsals = _genericRepository.Get<Futsal>();

        var result = futsals.Select(futsal => new FutsalVenueDto()
            {
                Id = futsal.Id,
                Name = futsal.Name,
                Description = futsal.Description,
                Phrase = futsal.Phrase,
                LocationAddress = futsal.LocationAddress,
                City = futsal.City,
                IsActive = futsal.IsActive,
                RegisteredDate = futsal.CreatedAt.ToString("dd-MM-yyyy"),
                OwnerEmail = _genericRepository.GetById<AppUser>(futsal.FutsalOwnerId).EmailAddress,
                OwnerName = _genericRepository.GetById<AppUser>(futsal.FutsalOwnerId).FullName,
                NumberOfCourts = _genericRepository.Get<Court>(x => x.FutsalId == futsal.Id).Count(),
                NumberOfAppointments = _genericRepository.Get<Appointment>()
                    .Count(a => _genericRepository.Get<Court>()
                        .Any(c => c.Id == a.BookedCourtId && c.FutsalId == futsal.Id))
            })
            .ToList();

        var stream = CreateExcelFile(result);

        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Futsal-Venues.xlsx");
    }
    
    public MemoryStream CreateExcelFile(List<FutsalVenueDto> venues)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Futsal Venues");

        var headerRange = worksheet.Range("A1:K1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontName = "Arial";
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Font.FontColor = XLColor.DarkBlue;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        worksheet.Column(1).Width = 40; 
        worksheet.Column(2).Width = 30;
        worksheet.Column(3).Width = 30; 
        worksheet.Column(4).Width = 30; 
        worksheet.Column(5).Width = 25; 
        worksheet.Column(6).Width = 15; 
        worksheet.Column(7).Width = 20; 
        worksheet.Column(8).Width = 35; 
        worksheet.Column(9).Width = 20; 
        worksheet.Column(10).Width = 18; 
        worksheet.Column(11).Width = 25; 

        worksheet.Cell("A1").Value = "ID";
        worksheet.Cell("B1").Value = "Futsal Name";
        worksheet.Cell("C1").Value = "Description";
        worksheet.Cell("D1").Value = "Phrase";
        worksheet.Cell("E1").Value = "Location Address";
        worksheet.Cell("F1").Value = "City";
        worksheet.Cell("G1").Value = "Owner Name";
        worksheet.Cell("H1").Value = "Owner Email Address";
        worksheet.Cell("I1").Value = "Registered Date";
        worksheet.Cell("J1").Value = "Number of Courts";
        worksheet.Cell("K1").Value = "Number of Appointments";

        for (var i = 0; i < venues.Count; i++)
        {
            var row = i + 2; 
            var venue = venues[i];
            worksheet.Cell(row, 1).Value = venue.Id.ToString();
            worksheet.Cell(row, 2).Value = venue.Name;
            worksheet.Cell(row, 3).Value = venue.Description;
            worksheet.Cell(row, 4).Value = venue.Phrase;
            worksheet.Cell(row, 5).Value = venue.LocationAddress;
            worksheet.Cell(row, 6).Value = venue.City;
            worksheet.Cell(row, 7).Value = venue.OwnerName;
            worksheet.Cell(row, 8).Value = venue.OwnerEmail;
            worksheet.Cell(row, 9).Value = venue.RegisteredDate;
            worksheet.Cell(row, 10).Value = venue.NumberOfCourts;
            worksheet.Cell(row, 11).Value = venue.NumberOfAppointments;
            worksheet.Row(row).Style.Font.FontName = "Arial";
            
            for (var col = 1; col <= 11; col++)
            {
                worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            if (venue.IsActive) continue;
            {
                for (var col = 1; col <= 11; col++)
                {
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Red;
                }
            }

        }

        for (var col = 1; col <= 11; col++)
        {
            worksheet.Cell(1, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
        
        var stream = new MemoryStream();
        
        workbook.SaveAs(stream);
        
        stream.Position = 0;
        
        return stream;
    }
    
    
}