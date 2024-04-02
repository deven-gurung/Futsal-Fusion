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
    public IActionResult GetNotifications()
    {
        var userId = UserDetail.UserId;

        var notifications = _genericRepository.Get<Notification>(x => x.ReceiverId == userId);

        var result = notifications.GroupBy(x => x.CreatedAt.Date).Select(x => new NotificationResponseDto()
        {
            TimePeriod = x.Key.ToString("dd-MM-yyyy"),
            Notifications = x.Select(n => new Notifications()
            {
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
}