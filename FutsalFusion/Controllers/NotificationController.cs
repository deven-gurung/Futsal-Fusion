using FutsalFusion.Application.DTOs.Notification;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Controllers.Base;
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
}