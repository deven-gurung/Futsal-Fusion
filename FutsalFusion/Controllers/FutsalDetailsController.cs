using FutsalFusion.Application.DTOs.Appointment;
using FutsalFusion.Application.DTOs.Futsal;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FutsalFusion.Controllers;

public class FutsalDetailsController : BaseController<FutsalDetailsController>
{
    private readonly IGenericRepository _genericRepository;

    public FutsalDetailsController(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public IActionResult Index()
    {
        var userId = UserDetail.UserId;

        var user = _genericRepository.GetById<AppUser>(userId);

        var futsal = _genericRepository.GetFirstOrDefault<Futsal>(x => x.FutsalOwnerId == user.Id);
        
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
        
        return RedirectToAction("Index");
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

            return RedirectToAction("Index");
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
                            (int)x.TimeSlotEndTime.TotalHours == index + 1 && x.IsActive);

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
}