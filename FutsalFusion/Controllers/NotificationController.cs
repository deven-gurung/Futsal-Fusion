using FutsalFusion.Application.DTOs.Notification;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
using FutsalFusion.Domain.Constants;
using FutsalFusion.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class NotificationController : BaseController<NotificationController>
{
    private readonly IGenericRepository _genericRepository;

    public NotificationController(IGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var userId = UserDetail.UserId;

        var notifications = _genericRepository.Get<Notification>(x => x.ReceiverId == userId);

        var result = notifications.OrderByDescending(x => x.CreatedAt).GroupBy(x => x.CreatedAt.Date).Select(x => new NotificationResponseDto()
        {
            TimePeriod = x.Key.ToString("dd-MM-yyyy"),
            Notifications = x.Select(n => new Notifications()
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                IsSeen = n.IsSeen,
                AppointmentId = n.AppointmentId,
                SentByUser = _genericRepository.GetById<AppUser>(n.SenderId).FullName,
                SentImageUrl = _genericRepository.GetById<AppUser>(n.SenderId).ImageURL,
            }).ToList(),
        }).ToList();

        return View(result);
    }
    
    [HttpGet]
    public IActionResult GetNotifications()
    {
        var userId = UserDetail.UserId;

        var notifications = _genericRepository.Get<Notification>(x => x.ReceiverId == userId);

        var result = notifications.GroupBy(x => x.CreatedAt.Date).Select(x => new NotificationResponseDto()
        {
            TimePeriod = x.Key.ToString("dd-MM-yyyy"),
            Notifications = x.Select(n => new Notifications()
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                IsSeen = n.IsSeen,
                SentByUser = _genericRepository.GetById<AppUser>(n.SenderId).FullName,
                SentImageUrl = _genericRepository.GetById<AppUser>(n.SenderId).ImageURL,
            }).ToList(),
        }).ToList();

        return Json(new
        {
            htmlData = ConvertViewToString("_NotificationsList", result, true)
        });
    }

    [HttpPost]
    public IActionResult DeleteAllNotifications()
    {
        var userId = UserDetail.UserId;
        
        var notifications = _genericRepository.Get<Notification>(x => x.ReceiverId == userId);

        var notificationEntities = notifications as Notification[] ?? notifications.ToArray();
        
        if (notificationEntities.Any())
        {
            _genericRepository.RemoveMultipleEntity(notificationEntities);
        }

        return Json(new
        {
            data = true
        });
    }

    [HttpGet]
    public IActionResult NavigateNotification(Guid notificationId)
    {
        var notification = _genericRepository.GetById<Notification>(notificationId);

        notification.IsSeen = true;
            
        _genericRepository.Update(notification);

        TempData["Success"] = "Successfully Navigated";
        
        switch (notification.Title)
        {
            case "Booking Alert":
                return RedirectToAction("Index", "Appointments");
            case "Booking Action Alert":
                return RedirectToAction("Index", "Reservation");
        }

        TempData["Warning"] = "Could not find an associate notification";
        
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DeleteNotification(Guid notificationId, bool isReloaded)
    {
        var notification = _genericRepository.GetById<Notification>(notificationId);
        
        _genericRepository.Delete(notification);

        if (isReloaded)
        {
            TempData["Success"] = "Notification successfully deleted";
        }

        return Json(new
        {
            data = true
        });
    }

    [HttpGet]
    public IActionResult GetAppointmentDetails(Guid notificationId, Guid appointmentId)
    {
        var userId = UserDetail.UserId;

        var requestedUserDetails = _genericRepository.GetById<AppUser>(userId);
        
        var notification = _genericRepository.GetById<Notification>(notificationId);

        notification.IsSeen = true;
        
        _genericRepository.Update(notification);
        
        var appointment = _genericRepository.GetById<Appointment>(appointmentId);

        var appointmentDetails = _genericRepository.Get<AppointmentDetail>(x => x.AppointmentId == appointment.Id);

        var appointmentDetailsItem = appointmentDetails as AppointmentDetail[] ?? appointmentDetails.ToArray();
        
        var requestedPlayersDetails =
            _genericRepository.GetFirstOrDefault<AppointmentRequest>(x => x.AppointmentId == appointment.Id);
        
        var user = _genericRepository.GetById<AppUser>(appointment.CreatedBy);
        
        var court = _genericRepository.GetById<Court>(appointment.BookedCourtId);

        var futsal = _genericRepository.GetById<Futsal>(court.FutsalId);
        
        if (appointmentDetailsItem.Count(x => x is { PlayerStatus: "Requested Player", IsActive: true }) == requestedPlayersDetails?.RequestedPlayers)
        {
            TempData["Warning"] = "Sorry, the booking slot has already been full";
            
            return RedirectToAction("Index");
        }

        if (appointmentDetailsItem.Any(x => x.PlayerStatus == "Requested Player" && x.PlayerId == requestedUserDetails.Id))
        {
            TempData["Warning"] = "You have already requested for the following appointment.";
            
            return RedirectToAction("Index");
        }
        
        var teamMembers = _genericRepository.Get<Team>(x => x.PlayerId == user.Id || x.AssigneeId == user.Id);

        var uniqueUserIds = teamMembers
            .SelectMany(x => new List<Guid> { x.PlayerId, x.AssigneeId }) 
            .Distinct()
            .ToList();

        var isUserIncluded = uniqueUserIds.Where(x => x == user.Id);

        if (!isUserIncluded.Any())
        {
            uniqueUserIds.Add(user.Id);
        }
        
        var assignedPlayers = uniqueUserIds.Select(x => new AssignedPlayers()
        {
            PlayerId = x,
            PhoneNumber = _genericRepository.GetById<AppUser>(x).MobileNo ?? "",
            PlayerName = _genericRepository.GetById<AppUser>(x).FullName,
            IsSelected = user.Id == x || appointmentDetailsItem.Any(z => z.PlayerId == x)
        }).Where(x => x.IsSelected).ToList();
        
        var requestedPlayers = _genericRepository.GetFirstOrDefault<AppointmentRequest>(x => x.AppointmentId == appointment.Id && x.IsActive);

        var result = new AppointmentNotificationDto()
        {
            OrganizedBy = user.FullName,
            AppointedDate = appointment.AppointedDate.ToString("dd-MM-yyyy"),
            BookedDate = appointment.BookedDate.ToString("dd-MM-yyyy"),
            CourtName = court.Title,
            CourtType = court.Type == 1 ? "Indoor" : "Outdoor",
            FutsalName = futsal.Name,
            TimeSlot = $"{appointment.TimeSlotStartTime} - {appointment.TimeSlotEndTime}",
            AppointmentId = appointment.Id,
            AssignedPlayersList = assignedPlayers,
            NumberOfPlayers = requestedPlayers?.RequestedPlayers ?? 0,
            ConfirmedPlayers = assignedPlayers.Count()
        };

        return View(result);
    }

    [HttpGet]
    public IActionResult ApproveDisregardAppointmentRequest(Guid appointmentId, bool isApproved)
    {
        var userId = UserDetail.UserId;
        
        var appointment = _genericRepository.GetById<Appointment>(appointmentId);

        var user = _genericRepository.GetById<AppUser>(userId);

        var notification =
            _genericRepository.GetFirstOrDefault<Notification>(x =>
                x.ReceiverId == user.Id && x.AppointmentId == appointment.Id);

        if (!isApproved)
        {
            if (notification != null)
            {
                _genericRepository.Delete(notification);

                TempData["Success"] = "Appointment Request successfully cancelled.";

                return RedirectToAction("Index");
            }
        }

        var appointmentDetails = new AppointmentDetail()
        {
            AppointmentId = appointment.Id,
            PlayerId = user.Id,
            PlayerStatus = "Requested Player",
            CreatedAt = DateTime.Now,
            CreatedBy = user.Id,
            IsActive = false,
            PlayerTeam = ""
        };

        _genericRepository.Insert(appointmentDetails);

        var approvedNotification = new Notification()
        {
            Title = "Updated Player Request",
            Content = $"{user.FullName} is available for your {appointment.TimeSlotStartTime} - {appointment.TimeSlotEndTime} booking slot at {appointment.AppointedDate:dd-MM-yyyy}? Contact the personnel at (+977) {user.MobileNo}.",
            IsActive = true,
            CreatedBy = user.Id,
            ReceiverEntity = (int)Roles.Player,
            SenderEntity = (int)Roles.Player,
            SenderId = user.Id,
            ReceiverId = appointment.CreatedBy,
            AppointmentId = appointment.Id
        };

        _genericRepository.Insert(approvedNotification);
        
        TempData["Success"] = "Your request has successfully been notified.";

        return RedirectToAction("Index");
    }
}